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
    public class ContactController : Controller
    {
        private readonly DbHelper _dbHelper;

        public ContactController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var list = new List<AdminContactVM>();
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    SELECT MessageId, FullName, Email, Subject, Message, CreatedAt
                    FROM ContactMessages
                    ORDER BY CreatedAt DESC", con);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new AdminContactVM
                    {
                        MessageId = Convert.ToInt32(reader["MessageId"]),
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Subject = reader["Subject"]?.ToString() ?? "",
                        Message = reader["Message"]?.ToString() ?? "",
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }
            return View("~/Areas/Admin/Views/ContactMessages.cshtml", list);
        }

        // ================= DELETE =================
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand("DELETE FROM ContactMessages WHERE MessageId=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}