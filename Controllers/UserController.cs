using EventOrganizer_ASP.NET.DAL;
using EventOrganizer_ASP.NET.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace EventOrganizer_ASP.NET.Controllers
{
    public class UserController : Controller
    {
        private readonly DbHelper _dbHelper;
        public UserController(DbHelper dbHelper) { _dbHelper = dbHelper; }

        private int? GetUserId() =>
            int.TryParse(HttpContext.Session.GetString("UserId"), out var id) ? id : id;

        private bool IsUser() => HttpContext.Session.GetString("Role") == "User";

        // ================= USER HOME =================
        public IActionResult Index()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            return View("~/Views/User/Index.cshtml");
        }

        // ================= EVENTS (User version) =================
        public IActionResult Events(string category = "All")
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");

            var events = new List<EventCardVM>();
            var categories = new List<string> { "All" };

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();

                var catCmd = new SqlCommand("SELECT CategoryName FROM Categories ORDER BY CategoryName", con);
                var catReader = catCmd.ExecuteReader();
                while (catReader.Read())
                    categories.Add(catReader["CategoryName"].ToString());
                catReader.Close();

                var cmd = new SqlCommand(@"
                    SELECT E.EventId, E.Title, E.Description, E.EventDate, E.EventTime,
                           E.Location, C.CategoryName,
                           (SELECT COUNT(*) FROM Registrations R WHERE R.EventId = E.EventId) AS RegisteredCount
                    FROM Events E
                    LEFT JOIN Categories C ON E.CategoryId = C.CategoryId
                    WHERE (@category = 'All' OR C.CategoryName = @category)
                    ORDER BY E.EventDate ASC", con);
                cmd.Parameters.AddWithValue("@category", category);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    events.Add(new EventCardVM
                    {
                        EventId = Convert.ToInt32(reader["EventId"]),
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"]?.ToString() ?? "",
                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                        EventTime = reader["EventTime"] != DBNull.Value
                                              ? (TimeSpan)reader["EventTime"] : TimeSpan.Zero,
                        Location = reader["Location"]?.ToString() ?? "",
                        CategoryName = reader["CategoryName"]?.ToString() ?? "",
                        RegisteredCount = Convert.ToInt32(reader["RegisteredCount"])
                    });
                }
            }

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            return View("~/Views/User/Events.cshtml", events);
        }

        // ================= EVENT DETAIL (User version) =================
        public IActionResult EventDetail(int id)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");

            EventDetailVM vm = null;
            var userId = GetUserId();

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    SELECT E.EventId, E.Title, E.Description, E.EventDate, E.EventTime,
                           E.Location, C.CategoryName,
                           U.FullName AS OrganizerName,
                           (SELECT COUNT(*) FROM Registrations R WHERE R.EventId = E.EventId) AS RegisteredCount
                    FROM Events E
                    LEFT JOIN Categories C ON E.CategoryId = C.CategoryId
                    LEFT JOIN Users      U ON E.EventId    = U.UserId
                    WHERE E.EventId = @id", con);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    vm = new EventDetailVM
                    {
                        EventId = Convert.ToInt32(reader["EventId"]),
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"]?.ToString() ?? "",
                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                        EventTime = reader["EventTime"] != DBNull.Value
                                              ? (TimeSpan)reader["EventTime"] : TimeSpan.Zero,
                        Location = reader["Location"]?.ToString() ?? "",
                        CategoryName = reader["CategoryName"]?.ToString() ?? "",
                        OrganizerName = reader["OrganizerName"]?.ToString() ?? "Eventalk Team",
                        RegisteredCount = Convert.ToInt32(reader["RegisteredCount"])
                    };
                }
                reader.Close();

                if (vm != null)
                {
                    // Check if already registered
                    var regCmd = new SqlCommand(@"
                        SELECT COUNT(1) FROM Registrations
                        WHERE UserId=@uid AND EventId=@eid", con);
                    regCmd.Parameters.AddWithValue("@uid", userId);
                    regCmd.Parameters.AddWithValue("@eid", id);
                    vm.IsRegistered = Convert.ToInt32(regCmd.ExecuteScalar()) > 0;

                    vm.Reviews = new List<EventReviewVM>();
                    var revCmd = new SqlCommand(@"
                        SELECT U.FullName, R.Rating, R.Comment, R.CreatedAt
                        FROM Reviews R
                        INNER JOIN Users U ON R.UserId = U.UserId
                        WHERE R.EventId = @id
                        ORDER BY R.CreatedAt DESC", con);
                    revCmd.Parameters.AddWithValue("@id", id);
                    var revReader = revCmd.ExecuteReader();
                    while (revReader.Read())
                    {
                        vm.Reviews.Add(new EventReviewVM
                        {
                            FullName = revReader["FullName"].ToString(),
                            Rating = Convert.ToInt32(revReader["Rating"]),
                            Comment = revReader["Comment"]?.ToString() ?? "",
                            CreatedAt = Convert.ToDateTime(revReader["CreatedAt"])
                        });
                    }
                }
            }

            if (vm == null) return NotFound();
            return View("~/Views/User/EventDetail.cshtml", vm);
        }

        // ================= REGISTER FOR EVENT =================
        [HttpPost]
        public IActionResult RegisterEvent(int eventId)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            var userId = GetUserId();

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM Registrations WHERE UserId=@uid AND EventId=@eid)
                    INSERT INTO Registrations (UserId, EventId)
                    VALUES (@uid, @eid)", con);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@eid", eventId);
                cmd.ExecuteNonQuery();
            }

            TempData["RegisterSuccess"] = "You have successfully registered for this event!";
            return RedirectToAction("EventDetail", new { id = eventId });
        }

        // ================= MY EVENTS =================
        public IActionResult MyEvents()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            var userId = GetUserId();
            var list = new List<MyEventVM>();

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    SELECT R.RegistrationId, E.EventId, E.Title, E.Location,
                           E.EventDate, E.EventTime,
                           CASE WHEN E.EventDate >= CAST(GETDATE() AS DATE)
                                THEN 'Upcoming' ELSE 'Completed' END AS Status,
                           CASE WHEN EXISTS (
                               SELECT 1 FROM Reviews V
                               WHERE V.UserId = R.UserId AND V.EventId = E.EventId)
                                THEN 1 ELSE 0 END AS HasReview
                    FROM Registrations R
                    INNER JOIN Events E ON R.EventId = E.EventId
                    WHERE R.UserId = @uid
                    ORDER BY E.EventDate DESC", con);
                cmd.Parameters.AddWithValue("@uid", userId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MyEventVM
                    {
                        RegistrationId = Convert.ToInt32(reader["RegistrationId"]),
                        EventId = Convert.ToInt32(reader["EventId"]),
                        Title = reader["Title"].ToString(),
                        Location = reader["Location"]?.ToString() ?? "",
                        EventDate = Convert.ToDateTime(reader["EventDate"]),
                        EventTime = reader["EventTime"] != DBNull.Value
                                             ? (TimeSpan)reader["EventTime"] : TimeSpan.Zero,
                        Status = reader["Status"].ToString(),
                        HasReview = Convert.ToInt32(reader["HasReview"]) == 1
                    });
                }
            }
            return View("~/Views/User/MyEvents.cshtml", list);
        }

        // ================= PROFILE =================
        public IActionResult Profile()
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            var userId = GetUserId();
            UserProfileVM vm = null;

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    "SELECT UserId, FullName, Email, Phone FROM Users WHERE UserId = @id", con);
                cmd.Parameters.AddWithValue("@id", userId);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    vm = new UserProfileVM
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"]?.ToString() ?? ""
                    };
                }
            }
            if (vm == null) return RedirectToAction("Login", "Account");
            return View("~/Views/User/Profile.cshtml", vm);
        }

        // ================= UPDATE PROFILE =================
        [HttpPost]
        public IActionResult UpdateProfile(string fullName, string phone)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            var userId = GetUserId();

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(
                    "UPDATE Users SET FullName=@name, Phone=@phone WHERE UserId=@id", con);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@phone", phone ?? "");
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.ExecuteNonQuery();
            }
            HttpContext.Session.SetString("FullName", fullName);
            TempData["ProfileSuccess"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // ================= SHOW PASSWORD PANEL =================
        [HttpPost]
        public IActionResult ShowPasswordPanel()
        {
            TempData["ShowPassword"] = true;
            return RedirectToAction("Profile");
        }

        // ================= CHANGE PASSWORD =================
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword,
                                            string newPassword,
                                            string confirmPassword)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            var userId = GetUserId();

            if (newPassword != confirmPassword)
            {
                TempData["PasswordError"] = "New password and confirm password do not match.";
                TempData["ShowPassword"] = true;
                return RedirectToAction("Profile");
            }

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var checkCmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Users WHERE UserId=@id AND Password=@pwd", con);
                checkCmd.Parameters.AddWithValue("@id", userId);
                checkCmd.Parameters.AddWithValue("@pwd", currentPassword);
                var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

                if (!exists)
                {
                    TempData["PasswordError"] = "Current password is incorrect.";
                    TempData["ShowPassword"] = true;
                    return RedirectToAction("Profile");
                }

                var updateCmd = new SqlCommand(
                    "UPDATE Users SET Password=@pwd WHERE UserId=@id", con);
                updateCmd.Parameters.AddWithValue("@pwd", newPassword);
                updateCmd.Parameters.AddWithValue("@id", userId);
                updateCmd.ExecuteNonQuery();
            }

            TempData["PasswordSuccess"] = "Password changed successfully!";
            return RedirectToAction("Profile");
        }

        // ================= GIVE FEEDBACK =================
        [HttpPost]
        public IActionResult GiveFeedback(int eventId, int rating, string comment)
        {
            if (!IsUser()) return RedirectToAction("Login", "Account");
            var userId = GetUserId();

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM Reviews WHERE UserId=@uid AND EventId=@eid)
                    INSERT INTO Reviews (UserId, EventId, Rating, Comment)
                    VALUES (@uid, @eid, @rating, @comment)", con);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@eid", eventId);
                cmd.Parameters.AddWithValue("@rating", rating);
                cmd.Parameters.AddWithValue("@comment", comment ?? "");
                cmd.ExecuteNonQuery();
            }
            TempData["FeedbackSuccess"] = "Thank you for your feedback!";
            return RedirectToAction("MyEvents");
        }
    }
}