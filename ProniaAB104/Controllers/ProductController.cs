using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.DAL;
using ProniaAB104.Models;
using ProniaAB104.ViewModels;

namespace ProniaAB104.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Detail(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = _context.Products
                .Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.Category).FirstOrDefault(p => p.Id == id);

            List<Product> relatedProducts = _context.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id).ToList();

            ProductVM vm = new ProductVM()
            {
                Product = product,
                RelatedProducts = relatedProducts
            };
            if (product == null) return NotFound();

            return View(vm);
        }
    }
}
