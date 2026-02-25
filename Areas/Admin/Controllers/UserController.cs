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
    public class UserController : Controller
    {
        private readonly DbHelper _dbHelper;

        public UserController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ================= LIST =================
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var list = new List<AdminUserVM>();
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    SELECT UserId, FullName, Email, Phone, Password, Role
                    FROM Users
                    WHERE Role != 'Admin'
                    ORDER BY UserId", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new AdminUserVM
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"]?.ToString() ?? "",
                        Password = reader["Password"].ToString(),
                        Role = reader["Role"].ToString()
                    });
                }
            }
            return View("~/Areas/Admin/Views/UserList.cshtml", list);
        }
    }
}