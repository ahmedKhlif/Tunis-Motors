using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TP2.Models;
using TP2.Models.Help;
using TP2.Models.Repositories;
using TP2.Services;
using TP2.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TP2.Controllers
{
    public class PanierController : Controller
    {
        readonly IProductRepository productRepository;
        readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IPaymentService paymentService;
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;
        private readonly AppDbContext _context;

        public PanierController(IProductRepository productRepository, IOrderRepository orderRepository,
                               UserManager<IdentityUser> userManager, IPaymentService paymentService,
                               IEmailService emailService, IConfiguration configuration, AppDbContext context)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
            this.paymentService = paymentService;
            this.emailService = emailService;
            this.configuration = configuration;
            _context = context;
        }
        public ActionResult Index()
        {
            ViewBag.Liste = ListeCart.Instance.Items;
            ViewBag.total = ListeCart.Instance.GetSubTotal();
            return View();
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cartCount = ListeCart.Instance.Items.Sum(item => item.quantite);
            return Json(new { cartCount = cartCount });
        }
        [HttpPost]
        public IActionResult AddProduct(int id)
        {
            CarListing pp = productRepository.GetById(id);

            // Check if product is in stock
            if (pp.QuantityInStock <= 0)
            {
                return Json(new { success = false, message = "This car is currently out of stock." });
            }

            // Check if adding this item would exceed available stock
            var existingItem = ListeCart.Instance.Items.FirstOrDefault(item => item.CarListing.ProductId == id);
            int currentQuantityInCart = existingItem?.quantite ?? 0;

            if (currentQuantityInCart + 1 > pp.QuantityInStock)
            {
                return Json(new { success = false, message = $"Only {pp.QuantityInStock} units available in stock." });
            }

            ListeCart.Instance.AddItem(pp);
            var cartCount = ListeCart.Instance.Items.Sum(item => item.quantite);
            return Json(new { success = true, message = "Car added to cart", cartCount = cartCount });
        }
        [HttpPost]
        public ActionResult PlusProduct(int id)
        {
            CarListing pp = productRepository.GetById(id);

            // Check stock availability
            var existingItem = ListeCart.Instance.Items.FirstOrDefault(item => item.CarListing.ProductId == id);
            int currentQuantityInCart = existingItem?.quantite ?? 0;

            if (currentQuantityInCart + 1 > pp.QuantityInStock)
            {
                return Json(new
                {
                    ct = 0,
                    message = $"Cannot add more items. Only {pp.QuantityInStock} units available in stock."
                });
            }

            ListeCart.Instance.AddItem(pp);
            Item trouve = null;
            foreach (Item a in ListeCart.Instance.Items)
            {
                if (a.CarListing.ProductId == pp.ProductId)
                    trouve = a;
            }
            var results = new
            {
                ct = 1,
                Total = (float)ListeCart.Instance.GetSubTotal(),
                Quatite = trouve.quantite,
                TotalRow = (float)trouve.TotalPrice
            };
            return Json(results);
        }
        [HttpPost]
        public ActionResult MinusProduct(int id)
        {
            CarListing pp = productRepository.GetById(id);
            ListeCart.Instance.SetLessOneItem(pp);
            Item trouve = null;
            foreach (Item a in ListeCart.Instance.Items)
            {
                if (a.CarListing.ProductId == pp.ProductId)
                    trouve = a;
            }
            if (trouve != null)
            {
                var results = new
                {
                    Total = (float)ListeCart.Instance.GetSubTotal(),
                    Quatite = trouve.quantite,
                    TotalRow = (float)trouve.TotalPrice,
                    ct = 1
                };
                return Json(results);
            }
            else
            {
                var results = new
                {
                    ct = 0
                };
                return Json(results);
            }
            return null;
        }
        [HttpPost]
        public ActionResult RemoveProduct(int id)
        {
            CarListing pp = productRepository.GetById(id);
            ListeCart.Instance.RemoveItem(pp);
            var results = new
            {
                Total = (float)ListeCart.Instance.GetSubTotal(),
            };
            return Json(results);
        }

        [Authorize]
        // GET: /Order/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }

            var cartItems = ListeCart.Instance.Items.ToList();

            // Validate stock availability for all cart items
            var outOfStockItems = new List<string>();
            foreach (var item in cartItems)
            {
                var product = productRepository.GetById(item.CarListing.ProductId);
                if (product.QuantityInStock < item.quantite)
                {
                    if (product.QuantityInStock == 0)
                    {
                        outOfStockItems.Add($"{product.Name} is out of stock");
                        ListeCart.Instance.RemoveItem(product); // Remove from cart
                    }
                    else
                    {
                        outOfStockItems.Add($"{product.Name} only has {product.QuantityInStock} units available");
                        // Adjust quantity in cart to available stock
                        while (item.quantite > product.QuantityInStock)
                        {
                            ListeCart.Instance.SetLessOneItem(product);
                            item.quantite--;
                        }
                    }
                }
            }

            if (outOfStockItems.Any())
            {
                TempData["WarningMessage"] = "Some items in your cart had stock issues and were adjusted: " + string.Join(", ", outOfStockItems);
            }

            // Recalculate after stock validation
            cartItems = ListeCart.Instance.Items.ToList();
            var totalAmount = ListeCart.Instance.GetSubTotal();

            var viewModel = new OrderViewModel
            {
                CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    ProductName = item.CarListing.Name,
                    Quantity = item.quantite,
                    Price = (float)item.CarListing.Price
                }).ToList(),
                TotalAmount = totalAmount
            };

            // Pre-fill delivery address from user profile
            var userProfile = _context.UserProfiles.FirstOrDefault(p => p.Id == user.Id);
            if (userProfile != null && !string.IsNullOrEmpty(userProfile.Address))
            {
                var addressParts = new List<string>();
                if (!string.IsNullOrEmpty(userProfile.Address)) addressParts.Add(userProfile.Address);
                if (!string.IsNullOrEmpty(userProfile.City)) addressParts.Add(userProfile.City);
                if (!string.IsNullOrEmpty(userProfile.Country)) addressParts.Add(userProfile.Country);

                viewModel.Address = string.Join(", ", addressParts);
            }

            // Pass Stripe publishable key to view
            ViewBag.StripePublishableKey = configuration["Stripe:PublishableKey"];

            return View(viewModel);
        }

        // POST : /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel model, string StripePaymentMethodId = null)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur non authentifié.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
                }

                // Recharger les articles du panier depuis la source (ListeCart)
                var cartItems = ListeCart.Instance.Items.ToList();

                // Final stock validation before processing order
                foreach (var item in cartItems)
                {
                    var product = productRepository.GetById(item.CarListing.ProductId);
                    if (product.QuantityInStock < item.quantite)
                    {
                        TempData["ErrorMessage"] = $"Insufficient stock for {product.Name}. Only {product.QuantityInStock} units available.";
                        return RedirectToAction("Index");
                    }
                }

                // Handle Stripe payment if selected
                if (model.PaymentMethod == "Stripe" && !string.IsNullOrEmpty(StripePaymentMethodId))
                {
                    // Check Stripe amount limit ($999,999.99 = 99,999,999 cents)
                    const decimal maxStripeAmount = 999999.99m;
                    if ((decimal)model.TotalAmount > maxStripeAmount)
                    {
                        TempData["ErrorMessage"] = $"Stripe payments are limited to ${maxStripeAmount:N0}. For larger purchases, please use Bank Transfer or Cash on Delivery.";
                        return View(model);
                    }

                    try
                    {
                        // Create payment intent
                        var clientSecret = await paymentService.CreatePaymentIntent((decimal)model.TotalAmount);
                        // In a real implementation, you'd confirm the payment here
                        // For demo purposes, we'll assume payment succeeds
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Payment processing failed: " + ex.Message;
                        return View(model);
                    }
                }

                model.CartItems = cartItems.Select(item => new CartItemViewModel
                {
                    ProductName = item.CarListing.Name,
                    Quantity = item.quantite,
                    Price = (float)item.CarListing.Price
                }).ToList();
                model.TotalAmount = (float)ListeCart.Instance.GetSubTotal();

                var order = new Order
                {
                    CustomerName = user.UserName,
                    Email = user.Email,
                    Address = model.Address,
                    PaymentMethod = model.PaymentMethod,
                    TotalAmount = model.TotalAmount,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending, // Start with Pending status
                    UserId = user.Id,
                    Items = model.CartItems.Select(item => new OrderItem
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                orderRepository.Add(order);

                // Decrease stock quantities after successful order creation
                foreach (var item in cartItems)
                {
                    var product = productRepository.GetById(item.CarListing.ProductId);
                    product.QuantityInStock -= item.quantite;
                    product.UpdatedAt = DateTime.Now;
                    productRepository.Update(product);
                }

                // Send order confirmation email to buyer
                try
                {
                    await emailService.SendOrderConfirmationAsync(user.Email, user.UserName, order.Id, (decimal)order.TotalAmount);
                }
                catch (Exception ex)
                {
                    // Log email error but don't fail the order
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                }

                // Send notifications to sellers
                foreach (var item in cartItems)
                {
                    var product = productRepository.GetById(item.CarListing.ProductId);
                    if (product.Seller != null && !string.IsNullOrEmpty(product.Seller.Email))
                    {
                        try
                        {
                            await emailService.SendCarSoldNotificationAsync(
                                product.Seller.Email,
                                product.Seller.UserName,
                                product.Name,
                                product.Price * item.quantite
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Seller notification failed: {ex.Message}");
                        }
                    }
                }

                ListeCart.Instance.Items.Clear();
                TempData["SuccessMessage"] = "Votre commande a été passée avec succès.";
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }
            TempData["ErrorMessage"] = "Une erreur est survenue. Veuillez vérifier les informations.";
            return View(model);
        }

        // GET: /Order/Confirmation
        public IActionResult Confirmation(int orderId)
        {
            var order = orderRepository.GetById(orderId);
            return View(order);
        }

    }
}
