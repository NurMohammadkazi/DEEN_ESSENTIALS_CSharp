using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Deen_Essentials.Models;
using Deen_Essentials.Web.Data;
using Deen_Essentials.ViewModels;

namespace Deen_Essentials.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDatabaseContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(MyDatabaseContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        public IActionResult Login(Admin admin)
        {
            if (admin == null || string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.Password))
            {
                ViewBag.ErrorMessage = "Please enter both email and password!";
                return View();
            }

            var existingAdmin = _context.Admins
                .FirstOrDefault(a => a.Email == admin.Email && a.Password == admin.Password);

            if (existingAdmin != null)
            {
                HttpContext.Session.SetString("AdminEmail", existingAdmin.Email); // Store admin email in session
                return RedirectToAction("Dashboard");
            }

            ViewBag.ErrorMessage = "Invalid login credentials!";
            return View();
        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminEmail")))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Admin/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process file upload
                string uniqueFileName = null;
                if (model.ProductImage != null)
                {
                    // Create upload directory if it doesn't exist
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProductImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProductImage.CopyToAsync(fileStream);
                    }
                }

                // Create new product
                var product = new Product
                {
                    ProductName = model.ProductName,
                    ProductPrice = model.ProductPrice,
                    ProductCategory = model.ProductCategory,
                    ProductDescription = model.ProductDescription,
                    ProductStock = model.ProductStock,
                    ImagePath = uniqueFileName != null ? "/images/products/" + uniqueFileName : null,
                    CreatedAt = DateTime.Now
                };

                // Add to database
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Success message
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Dashboard");
            }

            // If we reach here, something failed - redisplay form
            return View("Dashboard", model);
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session on logout
            return RedirectToAction("Login");
        }

        // GET: Admin/Signup
        public IActionResult Signup()
        {
            return View();
        }

        // POST: Admin/Signup
        [HttpPost]
        public IActionResult Signup(Admin admin)
        {
            if (admin == null || string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.Password))
            {
                ViewBag.ErrorMessage = "All fields are required!";
                return View();
            }

            // Check if the email already exists
            var existingAdmin = _context.Admins.FirstOrDefault(a => a.Email == admin.Email);
            if (existingAdmin != null)
            {
                ViewBag.ErrorMessage = "Email already exists!";
                return View();
            }

            // If the model is valid, add the new admin to the database
            if (ModelState.IsValid)
            {
                _context.Admins.Add(admin);
                _context.SaveChanges();
                return RedirectToAction("Login"); // Redirect to Login after successful signup
            }

            return View(admin); // If validation fails, return the view with errors
        }
    }
}