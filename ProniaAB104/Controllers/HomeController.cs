using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.DAL;
using ProniaAB104.Models;
using ProniaAB104.Services;
using ProniaAB104.ViewModels;

namespace ProniaAB104.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {

            List<Slide> slides = _context.Slides.OrderBy(s => s.Order).ToList();

            List<Product> products = _context.Products.Include(p => p.ProductImages).ToList();

            HomeVM home = new HomeVM
            {
                Slides = slides,
                Products = products
            };
            return View(home);
        }
        public IActionResult About()
        {
            return View();
        }
    }
}
