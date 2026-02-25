using EventOrganizer_ASP.NET.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EventOrganizer_ASP.NET.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbHelper _dbHelper;
        public HomeController(DbHelper dbHelper) { _dbHelper = dbHelper; }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role == "Admin")
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            return View();
        }

        public IActionResult About() => View();

        [HttpGet]
        public IActionResult Contact() => View();

        [HttpPost]
        public IActionResult Contact(string fullName, string email, string subject, string message)
        {
            using (var con = _dbHelper.GetConnection())
            {
                con.Open();
                var cmd = new SqlCommand(@"
                    INSERT INTO ContactMessages (FullName, Email, Subject, Message)
                    VALUES (@name, @email, @subject, @message)", con);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@subject", subject ?? "");
                cmd.Parameters.AddWithValue("@message", message ?? "");
                cmd.ExecuteNonQuery();
            }
            TempData["Success"] = "Your message has been sent successfully!";
            return RedirectToAction("Contact");
        }
    }
}