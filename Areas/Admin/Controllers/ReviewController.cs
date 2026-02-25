using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using EventOrganizer_ASP.NET.DAL;
using EventOrganizer_ASP.NET.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using EventOrganizer_ASP.NET.ViewModels;

namespace EventOrganizer_ASP.NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReviewController : Controller
    {
        private readonly DbHelper _dbHelper;

        public ReviewController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ================= LIST =================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var list = new List<AdminReviewVM>();

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();

                var cmd = new SqlCommand(@"
                    SELECT R.ReviewId,
                           U.FullName,
                           E.Title,
                           R.Rating,
                           R.Comment,
                           R.CreatedAt
                    FROM Reviews R
                    JOIN Users U ON R.UserId = U.UserId
                    JOIN Events E ON R.EventId = E.EventId
                    ORDER BY R.CreatedAt DESC", con);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new AdminReviewVM
                    {
                        ReviewId = Convert.ToInt32(reader["ReviewId"]),
                        UserName = reader["FullName"].ToString(),
                        EventTitle = reader["Title"].ToString(),
                        Rating = Convert.ToInt32(reader["Rating"]),
                        Comment = reader["Comment"]?.ToString() ?? "",
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }

            return View("~/Areas/Admin/Views/Reviews.cshtml", list);
        }

        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("DELETE FROM Reviews WHERE ReviewId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }
    }
}
