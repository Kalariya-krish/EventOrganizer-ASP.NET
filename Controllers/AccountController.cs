using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using EventOrganizer_ASP.NET.DAL;
using EventOrganizer_ASP.NET.Models;

namespace EventOrganizer_ASP.NET.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserDAL _userDAL;

        public AccountController(UserDAL userDAL)
        {
            _userDAL = userDAL;
        }

        // ================= REGISTER =================

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            user.Role = "User";

            _userDAL.InsertUser(user);

            return RedirectToAction("Login");
        }

        // ================= LOGIN =================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _userDAL.LoginUser(email, password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid Email or Password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
