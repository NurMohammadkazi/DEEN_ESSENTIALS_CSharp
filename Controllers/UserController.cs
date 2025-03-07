using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Deen_Essentials.Models;
using Deen_Essentials.Web.Data;
using System.Security.Cryptography;
using System.Text;

namespace Deen_Essentials.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDatabaseContext _context;

        public UserController(MyDatabaseContext context)
        {
            _context = context;
        }

        // ✅ GET: User/Signup
        public IActionResult Signup()
        {
            return View();
        }

        // ✅ POST: User/Signup
        [HttpPost]
        public IActionResult Signup(User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ViewBag.ErrorMessage = "Email already registered!";
                    return View();
                }

                // 🔥 Fix: Explicitly set CreatedAt before saving
                user.CreatedAt = DateTime.Now;

                // 🔐 Hash Password Before Storing
                user.Password = HashPassword(user.Password);

                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(user);
        }


        // ✅ GET: User/Login
        public IActionResult Login()
        {
            return View();
        }

        // ✅ POST: User/Login
        [HttpPost]
        public IActionResult Login(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null && VerifyPassword(user.Password, existingUser.Password))
            {
                HttpContext.Session.SetString("UserEmail", existingUser.Email);
                HttpContext.Session.SetString("UserName", existingUser.Name);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid email or password!";
            return View();
        }

        // ✅ Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // 🔐 Password Hashing
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // 🔐 Verify Password
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashedPassword = HashPassword(enteredPassword);
            return hashedPassword == storedHash;
        }
    }
}
