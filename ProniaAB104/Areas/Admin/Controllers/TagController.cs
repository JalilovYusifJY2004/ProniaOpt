using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.Areas.Admin.ViewModels;
using ProniaAB104.DAL;
using ProniaAB104.Models;

namespace ProniaAB104.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Tag> tags = await _context.Tags.Include(t => t.ProductTags).ToListAsync();
            return View(tags);
        }
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM tagVM)
        {
            if (!ModelState.IsValid) return View();

            bool result = _context.Tags.Any(c => c.Name.Trim() == tagVM.Name.Trim());
            if (result)
            {
                ModelState.AddModelError("Name", "Bu Tag artiq movcuddur.");
                return View();
            }

            Tag tag=new Tag
            {
                Name = tagVM.Name
            };

            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();

            Tag tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (tag is null) return NotFound();

            UpdateTagVM updateTagVM = new UpdateTagVM
            {
                Name = tag.Name,
            };

            return View(updateTagVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateTagVM tagVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            bool result = _context.Tags.Any(t => t.Name == tagVM.Name && t.Id != id);

            if (result)
            {
                ModelState.AddModelError("Name", "Bu adda tag artiq movcuddur");
                return View();
            }

            existed.Name = tagVM.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Tag existed = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);

            if (existed is null) return NotFound();

            _context.Tags.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Tag tag = await _context.Tags.Include(c => c.ProductTags).
                ThenInclude(p => p.Product).
                ThenInclude(p => p.ProductImages).
                FirstOrDefaultAsync(s => s.Id == id);
            if (tag == null) return NotFound();

            return View(tag);
        }
    }
}
