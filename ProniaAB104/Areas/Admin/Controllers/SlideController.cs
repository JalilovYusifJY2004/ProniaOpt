using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaAB104.Areas.Admin.ViewModels;
using ProniaAB104.DAL;
using ProniaAB104.Models;
using ProniaAB104.Utilities.Extensions;

namespace ProniaAB104.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Slide> slides=await _context.Slides.ToListAsync();

            return View(slides);
        }
        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM slideVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //if (slideVM.Photo is null)
            //{
            //    ModelState.AddModelError("Photo", "Shekil mutleq secilmelidir");
            //    return View();
            //}
            if (!slideVM.Photo.ValidateType("image/"))
            {
                ModelState.AddModelError("Photo", "File-in type sehvdir");
                return View();
            }
            if (slideVM.Photo.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Photo", "Olcusu 2MB dan cox olmamalidir");
                return View();
            }

            string fileExtension = Path.GetExtension(slideVM.Photo.FileName);

            if (string.IsNullOrEmpty(fileExtension) || !IsImageFileExtension(fileExtension))
            {
                ModelState.AddModelError("Photo", "Fayl tipi sehvdir");
                return View();
            }

            string fileName = Guid.NewGuid().ToString() + fileExtension;

            string path = Path.Combine(_env.WebRootPath, @"assets\images\website-images\", fileName);
            using (FileStream file = new FileStream(path, FileMode.Create))
            {
                await slideVM.Photo.CopyToAsync(file);
            }

            string fileName2 = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");

            Slide slide = new Slide
            {
                Image = fileName2,
                Title = slideVM.Title,
                Description = slideVM.Description,
                SubTitle = slideVM.SubTitle,
                Order = slideVM.Order
            };

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool IsImageFileExtension(string fileExtension)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            return allowedExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            Slide slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null) return NotFound();

            return View(slide);
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if(id<=0) return BadRequest();
            Slide existed = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();

            UpdateSlideVM slideVM = new UpdateSlideVM
            {
                Image = existed.Image,
                Title = existed.Title,
                SubTitle = existed.SubTitle,
                Description = existed.Description,
                Order = existed.Order
            };

            return View(slideVM);
        }

        [HttpPost]

        public async Task<IActionResult> Update(int id, UpdateSlideVM slideVM)
        {
            if(!ModelState.IsValid)
            {
                return View(slideVM);
            }
            Slide existed =await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);

            if (existed is null) return NotFound();


            if (slideVM.Photo is not null)
            {
                if (!slideVM.Photo.ValidateType("image/"))
                {
                    ModelState.AddModelError("Photo", "File-in type sehvdir");
                    return View(slideVM);
                }
                if (slideVM.Photo.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("Photo", "Olcusu 2MB dan cox olmamalidir");
                    return View(slideVM);
                }

                string newImage = await slideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.Image = newImage;
            }
            
            existed.Title = slideVM.Title;
            existed.SubTitle = slideVM.SubTitle;
            existed.Description = slideVM.Description;
            existed.Order = slideVM.Order;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <=0) return BadRequest();
            Slide slide=await _context.Slides.FirstOrDefaultAsync(s=> s.Id == id);
            if (slide is null) return NotFound();

            slide.Image.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
