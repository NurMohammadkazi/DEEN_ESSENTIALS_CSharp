using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Deen_Essentials.Models;
using Deen_Essentials.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Deen_Essentials.Controllers
{
    public class CartController : Controller
    {
        private readonly MyDatabaseContext _context;

        public CartController(MyDatabaseContext context)
        {
            _context = context;
        }

        // ✅ Add to Cart (User Must Be Logged In)
       [HttpPost]
public async Task<IActionResult> AddToCart(int productId, int quantity)
{
    var userEmail = HttpContext.Session.GetString("UserEmail");

    if (string.IsNullOrEmpty(userEmail))
    {
                Debug.WriteLine(" User not logged in. Redirecting to login.");
        return Json(new { message = "You must log in to add items to your cart!" });
    }

    var product = await _context.Products.FindAsync(productId);
    if (product == null || product.ProductStock < quantity)
    {
                Debug.WriteLine(" Product not found or out of stock.");
        return Json(new { message = "Product is out of stock or unavailable." });
    }

            Debug.WriteLine($" Adding Product to Cart: {product.ProductName} | Quantity: {quantity}");

    var cartItem = await _context.CartItems
        .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == productId);

    if (cartItem != null)
    {
                Debug.WriteLine("Product already in cart, updating quantity.");
        cartItem.Quantity += quantity;
    }
    else
    {
                Debug.WriteLine(" New product added to cart.");
        cartItem = new CartItem
        {
            UserEmail = userEmail,
            ProductId = productId,
            Quantity = quantity,
            Price = product.ProductPrice
        };
        _context.CartItems.Add(cartItem);
    }

    await _context.SaveChangesAsync();
            Debug.WriteLine(" Product successfully saved to cart.");
    return Json(new { message = "Product added to cart successfully!" });
}



        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            var cartItems = _context.CartItems
                .Where(c => c.UserEmail == userEmail)
                .Include(c => c.Product)  // Ensure we include product details
                .Select(c => new
                {
                    c.Id,
                    c.ProductId,
                    c.Quantity,
                    c.Price,
                    ProductName = c.Product.ProductName,
                    ImagePath = c.Product.ImagePath
                })
                .ToList();

            return View(cartItems);
        }

        // ✅ Remove Item from Cart
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ConfirmOrder()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "User");
            }

            var cartItems = _context.CartItems.Where(c => c.UserEmail == userEmail).ToList();
            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.ProductStock -= item.Quantity;
                }
            }

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Index", "Home");
        }

    }
}
