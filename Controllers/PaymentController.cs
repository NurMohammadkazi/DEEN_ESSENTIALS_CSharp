using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

public class PaymentController : Controller
{
    private readonly StripeSettings _stripeSettings;

    public PaymentController(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
    }

    public IActionResult Checkout()
    {
        ViewBag.PublishableKey = _stripeSettings.PublishableKey;
        return View();
    }

    [HttpPost]
    public IActionResult CreateCheckoutSession()
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = 5000, // $50.00
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Sample Product"
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = Url.Action("Success", "Payment", null, Request.Scheme),
            CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme)
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Json(new { id = session.Id });
    }

    public IActionResult Success()
    {
        return View();
    }

    public IActionResult Cancel()
    {
        return View();
    }
}
