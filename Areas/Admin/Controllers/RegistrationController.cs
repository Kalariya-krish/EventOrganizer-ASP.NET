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
    public class RegistrationController : Controller
    {
        private readonly DbHelper _dbHelper;

        public RegistrationController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var list = new List<AdminRegistrationVM>();
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    SELECT 
                        R.RegistrationId,
                        E.Title        AS EventTitle,
                        U.FullName,
                        U.Email,
                        R.RegistrationDate
                    FROM Registrations R
                    INNER JOIN Events E ON R.EventId = E.EventId
                    INNER JOIN Users  U ON R.UserId  = U.UserId
                    ORDER BY R.RegistrationDate DESC", con);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new AdminRegistrationVM
                    {
                        RegistrationId = Convert.ToInt32(reader["RegistrationId"]),
                        EventTitle = reader["EventTitle"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"].ToString(),
                        RegistrationDate = Convert.ToDateTime(reader["RegistrationDate"])
                    });
                }
            }
            return View("~/Areas/Admin/Views/Registration.cshtml", list);
        }
    }
}