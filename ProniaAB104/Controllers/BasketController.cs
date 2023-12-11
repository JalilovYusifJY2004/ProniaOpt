using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaAB104.DAL;
using ProniaAB104.Interfaces;
using ProniaAB104.Models;
using ProniaAB104.ViewModels;
using System.Security.Claims;

namespace ProniaAB104.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {
            List<BasketItemVM> basketVM = new List<BasketItemVM>();

            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .ThenInclude(bi => bi.Product)
                    .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                foreach (BasketItem item in user.BasketItems)
                {
                    basketVM.Add(new BasketItemVM()
                    {
                        Name = item.Product.Name,
                        Price = item.Product.Price,
                        Count = item.Count,
                        SubTotal = item.Count * item.Product.Price,
                        Image = item.Product.ProductImages.FirstOrDefault().Url,
                        Id = item.Product.Id
                    });
                }
            }
            else
            {
                if (Request.Cookies["Basket"] is not null)
                {
                    List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    foreach (BasketCookieItemVM basketCookieItem in basket)
                    {
                        Product product = await _context.Products.Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true)).FirstOrDefaultAsync(p => p.Id == basketCookieItem.Id);

                        if (product is not null)
                        {
                            BasketItemVM basketItemVM = new BasketItemVM
                            {
                                Id = product.Id,
                                Name = product.Name,
                                Image = product.ProductImages.FirstOrDefault().Url,
                                Price = product.Price,
                                Count = basketCookieItem.Count,
                                SubTotal = product.Price * basketCookieItem.Count,
                            };

                            basketVM.Add(basketItemVM);

                        }
                    }
                }
            }

            return View(basketVM);
        }

        public async Task<IActionResult> AddBasket(int id, string plus)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {  

                AppUser user = await _userManager.Users.Include(u => u.BasketItems.Where(bi => bi.OrderId == null)).FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (user is null) return NotFound();

                BasketItem item = user.BasketItems.FirstOrDefault(b => b.ProductId == id);

                if (item is null)
                {
                    item = new BasketItem
                    {
                        AppUserId = user.Id,
                        ProductId = product.Id,
                        Price = product.Price,
                        Count = 1,
                    };
                    user.BasketItems.Add(item);
                }
                else
                {
                    item.Count++;

                 
                }

                await _context.SaveChangesAsync();
            }
            else
            {
              

                List<BasketCookieItemVM> basket;

                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM itemVM = basket.FirstOrDefault(b => b.Id == id);
                    if (itemVM is null)
                    {
                        BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                        {
                            Id = id,
                            Count = 1
                        };

                        basket.Add(basketCookieItemVM);
                    }
                    else
                    {
                        itemVM.Count++;
                    }
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();

                    BasketCookieItemVM basketCookieItemVM = new BasketCookieItemVM
                    {
                        Id = id,
                        Count = 1
                    };

                    basket.Add(basketCookieItemVM);
                }

                string json = JsonConvert.SerializeObject(basket);

                Response.Cookies.Append("Basket", json);
            }

        

            return RedirectToAction(nameof(Index), "Home");
        }
        public async Task<IActionResult> MinusBasket(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            List<BasketCookieItemVM> basket;
            if (Request.Cookies["Basket"] is not null)
            {
                basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);
                if (item is not null)
                {
                    item.Count--;

                    if (item.Count == 0)
                    {
                        basket.Remove(item);
                    }
                    string json = JsonConvert.SerializeObject(basket);
                    Response.Cookies.Append("Basket", json);
                }
            }
            else
            {
                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);

                    if (item is not null)
                    {
                        item.Count--;

                        if (item.Count == 0)
                        {
                            basket.Remove(item);
                        }
                        string json = JsonConvert.SerializeObject(basket);
                        Response.Cookies.Append("Basket", json);
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> RemoveBasket(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            List<BasketCookieItemVM> basket;
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (user is null) return NotFound();

                BasketItem basketItem = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (basketItem is null)
                {
                    return NotFound();
                }
                else
                {
                    user.BasketItems.Remove(basketItem);
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                if (Request.Cookies["Basket"] is not null)
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(Request.Cookies["Basket"]);

                    BasketCookieItemVM item = basket.FirstOrDefault(b => b.Id == id);

                    if (item is not null)
                    {
                        basket.Remove(item);

                        string json = JsonConvert.SerializeObject(basket);
                        Response.Cookies.Append("Basket", json);
                    }
                }
            }

            return Redirect(Request.Headers["Referer"]);
        }
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.Users
                .Include(u => u.BasketItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            OrderVM orderVM = new OrderVM
            {
                BasketItems = user.BasketItems
            };

            return View(orderVM);
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM)
        {
            var user = await _userManager.Users
                .Include(u => u.BasketItems)
                .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (!ModelState.IsValid)
            {
                orderVM.BasketItems = user.BasketItems;
                return View(orderVM);
            }


            decimal total = 0;
            foreach (BasketItem item in user.BasketItems)
            {
                item.Price = item.Product.Price;
                total = item.Count * item.Price;
            }

            Order order = new Order
            {
                Status = null,
                Address = orderVM.Address,
                PurchaseAt = DateTime.Now,
                AppUserId = user.Id,
                BasketItems = user.BasketItems,
                TotalPrice = total
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            string body = @"<table border=""1"">
                              <thead>
                                <tr>
                                  <th>Name</th>
                                  <th>Price</th>
                                  <th>Count</th>
                                </tr>
                              </thead>
                              <tbody>";

            foreach (var item in order.BasketItems)
            {
                body += @$"<tr>
                           <td>{item.Product.Name}</td>
                           <td>{item.Price}</td>
                           <td>{item.Count}</td>
                         </tr>";
            }

            body += @"  </tbody>
                     </table>";

            await _emailService.SendMailAsync(user.Email, " Order", body, true);

            return RedirectToAction("Index", "Home");
        }
    }
}
