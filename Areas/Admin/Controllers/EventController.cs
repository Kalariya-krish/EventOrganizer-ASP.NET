using EventOrganizer_ASP.NET.Areas.Admin.ViewModels;
using EventOrganizer_ASP.NET.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace EventOrganizer_ASP.NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EventController : Controller
    {
        private readonly DbHelper _dbHelper;

        public EventController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ================= HELPER: Load Categories =================
        private List<CategoryVM> GetCategories()
        {
            var categories = new List<CategoryVM>();
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("SELECT CategoryId, CategoryName FROM Categories ORDER BY CategoryName", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new CategoryVM
                    {
                        CategoryId = Convert.ToInt32(reader["CategoryId"]),
                        CategoryName = reader["CategoryName"].ToString()
                    });
                }
            }
            return categories;
        }

        // ================= LIST =================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var list = new List<AdminEventVM>();
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    SELECT E.EventId, E.Title, C.CategoryName, E.CategoryId,
                           E.EventDate, E.EventTime, E.Location, E.Description
                    FROM Events E
                    LEFT JOIN Categories C ON E.CategoryId = C.CategoryId
                    ORDER BY E.EventDate DESC", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var date = Convert.ToDateTime(reader["EventDate"]);
                    list.Add(new AdminEventVM
                    {
                        EventId = Convert.ToInt32(reader["EventId"]),
                        Title = reader["Title"].ToString(),
                        CategoryName = reader["CategoryName"]?.ToString() ?? "",
                        CategoryId = Convert.ToInt32(reader["CategoryId"]),
                        EventDate = date,
                        EventTime = reader["EventTime"] != DBNull.Value
                                           ? (TimeSpan)reader["EventTime"] : TimeSpan.Zero,
                        Location = reader["Location"]?.ToString() ?? "",
                        Description = reader["Description"]?.ToString() ?? "",
                        Status = date >= DateTime.Today ? "Active" : "Completed"
                    });
                }
            }

            // Pass categories to ViewBag for the Edit modal dropdown
            ViewBag.Categories = GetCategories();

            return View("~/Areas/Admin/Views/ManageEvents.cshtml", list);
        }

        // ================= ADD SCREEN =================
        public IActionResult Add()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var vm = new AddEventVM { Categories = GetCategories() };
            return View("~/Areas/Admin/Views/AddEvent.cshtml", vm);
        }

        // ================= ADD POST =================
        [HttpPost]
        public IActionResult Add(AddEventVM model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO Events (Title, Description, EventDate, EventTime, Location, CategoryId)
                    VALUES (@title, @desc, @date, @time, @loc, @cat)", con);
                cmd.Parameters.AddWithValue("@title", model.Title);
                cmd.Parameters.AddWithValue("@desc", model.Description ?? "");
                cmd.Parameters.AddWithValue("@date", model.EventDate);
                cmd.Parameters.AddWithValue("@time", model.EventTime);
                cmd.Parameters.AddWithValue("@loc", model.Location);
                cmd.Parameters.AddWithValue("@cat", model.CategoryId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // ================= EDIT POST =================
        [HttpPost]
        public IActionResult Edit(EditEventVM model)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    UPDATE Events SET
                        Title       = @title,
                        Description = @desc,
                        EventDate   = @date,
                        EventTime   = @time,
                        Location    = @loc,
                        CategoryId  = @cat
                    WHERE EventId = @id", con);
                cmd.Parameters.AddWithValue("@id", model.EventId);
                cmd.Parameters.AddWithValue("@title", model.Title);
                cmd.Parameters.AddWithValue("@desc", model.Description ?? "");
                cmd.Parameters.AddWithValue("@date", model.EventDate);
                cmd.Parameters.AddWithValue("@time", model.EventTime);
                cmd.Parameters.AddWithValue("@loc", model.Location);
                cmd.Parameters.AddWithValue("@cat", model.CategoryId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("DELETE FROM Events WHERE EventId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}