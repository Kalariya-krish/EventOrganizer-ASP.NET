using EventOrganizer_ASP.NET.Areas.Admin.ViewModels;
using EventOrganizer_ASP.NET.DAL;
using EventOrganizer_ASP.NET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace EventOrganizer_ASP.NET.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                context.Result = RedirectToAction("Login", "Account");
            }

            base.OnActionExecuting(context);
        }
        private readonly DbHelper _dbHelper;

        public DashboardController(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            // Protect admin
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var model = new DashboardViewModel();
            model.RecentRegistrations = new List<RegistrationVM>();

            using (SqlConnection con = _dbHelper.GetConnection())
            {
                con.Open();

                // Total counts
                model.TotalEvents =
                    Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM Events", con).ExecuteScalar());

                model.TotalRegistrations =
                    Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM Registrations", con).ExecuteScalar());

                model.TotalUsers =
                    Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM Users", con).ExecuteScalar());

               var cmd = new SqlCommand(@"
    SELECT TOP 5 U.FullName, E.Title, R.RegistrationDate
    FROM Registrations R
    JOIN Users U ON R.UserId = U.UserId
    JOIN Events E ON R.EventId = E.EventId
    ORDER BY R.RegistrationDate DESC", con);

var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    model.RecentRegistrations.Add(new RegistrationVM
                    {
                        UserName = reader["FullName"].ToString(),
                        EventName = reader["Title"].ToString(),
                        Date = Convert.ToDateTime(reader["RegistrationDate"])
                    });
                }
            }

            return View("~/Areas/Admin/Views/Dashboard.cshtml", model);
        }
    }
}