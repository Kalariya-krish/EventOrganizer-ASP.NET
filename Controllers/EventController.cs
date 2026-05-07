using EventOrganizer_ASP.NET.DAL;
using EventOrganizer_ASP.NET.Services;
using EventOrganizer_ASP.NET.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventOrganizer_ASP.NET.Controllers
{
    public class EventController : Controller
    {
        private readonly DbHelper _dbHelper;
        private readonly UnsplashService _unsplashService;

        // ✅ FIXED CONSTRUCTOR
        public EventController(DbHelper dbHelper, UnsplashService unsplashService)
        {
            _dbHelper = dbHelper;
            _unsplashService = unsplashService;
        }

        // ================= EVENT LIST =================
        public async Task<IActionResult> Index(string category = "All")
        {
            var events = new List<EventCardVM>();
            var categories = new List<string> { "All" };

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();

                // categories
                var catCmd = new SqlCommand("SELECT CategoryName FROM Categories ORDER BY CategoryName", con);
                var catReader = catCmd.ExecuteReader();
                while (catReader.Read())
                    categories.Add(catReader["CategoryName"].ToString());
                catReader.Close();

                // events
                var cmd = new SqlCommand(@"
    SELECT E.EventId, E.Title, E.Description, E.EventDate, E.EventTime,
           E.Location, C.CategoryName,
           (SELECT COUNT(*) FROM Registrations R WHERE R.EventId = E.EventId) AS RegisteredCount
    FROM Events E
    LEFT JOIN Categories C ON E.CategoryId = C.CategoryId
    WHERE 
        (@category = 'All' OR C.CategoryName = @category)
        AND E.EventDate >= CAST(GETDATE() AS DATE)
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

            // ✅ FETCH IMAGES FROM API
            foreach (var ev in events)
            {
                var query = string.IsNullOrEmpty(ev.CategoryName) ? ev.Title : ev.CategoryName;

                var img = await _unsplashService.GetEventImage(query);

                ev.ImageUrl = img ?? "/images/default-event.jpg"; // fallback
            }

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            return View("~/Views/Event/Index.cshtml", events);
        }

        // ================= EVENT DETAIL =================
        public async Task<IActionResult> Detail(int id)
        {
            EventDetailVM vm = null;

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
                    LEFT JOIN Users U ON E.EventId = U.UserId
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

                // reviews
                if (vm != null)
                {
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

            // ✅ ADD IMAGE HERE ALSO
            var queryText = string.IsNullOrEmpty(vm.CategoryName) ? vm.Title : vm.CategoryName;
            var image = await _unsplashService.GetEventImage(queryText);
            vm.ImageUrl = image ?? "/images/default-event.jpg";

            return View("~/Views/Event/Detail.cshtml", vm);
        }

        // ================= REGISTER =================
        [HttpPost]
        public IActionResult Register(int eventId)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "User" || userId == null)
                return RedirectToAction("Login", "Account");

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();

                var cmd = new SqlCommand(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM Registrations WHERE UserId=@uid AND EventId=@eid)
                    INSERT INTO Registrations (UserId, EventId)
                    VALUES (@uid, @eid)", con);

                cmd.Parameters.AddWithValue("@uid", int.Parse(userId));
                cmd.Parameters.AddWithValue("@eid", eventId);

                cmd.ExecuteNonQuery();
            }

            TempData["RegisterSuccess"] = "You have successfully registered!";
            return RedirectToAction("Detail", new { id = eventId });
        }
    }
}