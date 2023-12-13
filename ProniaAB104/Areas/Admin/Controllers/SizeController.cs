using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.Areas.Admin.ViewModels;
using ProniaAB104.DAL;
using ProniaAB104.Models;

namespace ProniaAB104.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeController : Controller
    {
        private readonly AppDbContext _context;

        public SizeController(AppDbContext context)
        {
            _context = context;
        }
        //[Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            var sizes = await _context.Sizes.Include(s => s.ProductSizes).ToListAsync();
            return View(sizes);
        }
        //[Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSizeVM sizeVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool result = _context.Sizes.Any(c => c.Name.ToLower().Trim() == sizeVM.Name.ToLower().Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Bu olcude artiq movcuddur");
                return View();
            }

            Size size = new Size
            {
                Name = sizeVM.Name
            };
            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        //[Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Size size = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);
            if (size is null) return NotFound();

            UpdateSizeVM sizeVM = new UpdateSizeVM
            {
                Name = size.Name
            };

            return View(sizeVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateSizeVM sizeVM)
        {
            if (!ModelState.IsValid) return View();

            Size existed = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();
            bool result = _context.Sizes.Any(c => c.Name == sizeVM.Name && c.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "Bu olcuden artiq movcuddur");
                return View();
            }

            existed.Name = sizeVM.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            Size existed = await _context.Sizes.FirstOrDefaultAsync(c => c.Id == id);
            if (existed is null) return NotFound();
            _context.Sizes.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Size size = await _context.Sizes
                .Include(c => c.ProductSizes)
                .ThenInclude(pc => pc.Product)
                .ThenInclude(p => p.ProductImages).
                FirstOrDefaultAsync(s => s.Id == id);
            if (size == null) return NotFound();

            return View(size);
        }
    }
}
