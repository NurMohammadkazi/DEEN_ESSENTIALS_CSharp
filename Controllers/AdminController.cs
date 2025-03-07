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
using System.Diagnostics;

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
            var products = _context.Products.ToList(); // Fetch all products
            return View(products); // Pass products to dashboard view
        }

        //  Edit Product (GET)
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();
            return View(product);
        }

        //  Edit Product (POST)
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product, IFormFile newImage)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _context.Products.Find(product.Id);
                if (existingProduct == null) return NotFound();

                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductPrice = product.ProductPrice;
                existingProduct.ProductCategory = product.ProductCategory;
                existingProduct.ProductDescription = product.ProductDescription;
                existingProduct.ProductStock = product.ProductStock;

                // Handle Image Upload if changed
                if (newImage != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + newImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await newImage.CopyToAsync(fileStream);
                    }

                    existingProduct.ImagePath = "/images/products/" + uniqueFileName;
                }

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Dashboard");
            }
            return View(product);
        }


        // Delete Product
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction("Dashboard");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Failed to add product. Please check your inputs.";
                var products = _context.Products.ToList();
                return View("Dashboard", products);
            }

            string uniqueFileName = null;
            if (model.ProductImage != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProductImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProductImage.CopyToAsync(fileStream);
                }
            }

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

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("Dashboard");
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