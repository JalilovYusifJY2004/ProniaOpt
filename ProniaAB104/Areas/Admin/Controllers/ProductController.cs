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
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary == true))
                .Include(pt => pt.ProductTags).ThenInclude(pt => pt.Tag)
                .ToListAsync();

            return View(products);
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Create()
        {
      

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Tags = await _context.Tags.ToListAsync();
            ViewBag.Sizes = await _context.Sizes.ToListAsync();
            ViewBag.Colors = await _context.Colors.ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                return View();
            }
            bool result = await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);
            if (!result)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bu adli category movcuddur");
                return View();
            }

            foreach (int tagId in productVM.TagIds)
            {
                bool tagResult = await _context.Tags.AllAsync(t => t.Id == tagId);

                if (tagResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ViewBag.Colors = await _context.Colors.ToListAsync();
                    ViewBag.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("TagIds", "Yanlish id melumati gonderilib");
                    return View();
                }
            }

            foreach (int id in productVM.ColorIds)
            {
                bool colorResult = await _context.Colors.AnyAsync(t => t.Id == id);
                if (!colorResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ViewBag.Colors = await _context.Colors.ToListAsync();
                    ViewBag.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("ColorIds", "Yanlish id melumati gonderilib");
                    return View(productVM);
                }
            }

            foreach (int id in productVM.SizeIds)
            {
                bool sizeResult = await _context.Sizes.AnyAsync(t => t.Id == id);
                if (!sizeResult)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Tags = await _context.Tags.ToListAsync();
                    ViewBag.Colors = await _context.Colors.ToListAsync();
                    ViewBag.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("SizeIds", "There is no such size");
                    return View(productVM);
                }
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ModelState.AddModelError("MainPhoto", "File tipi uygun deyil");
                return View();
            }

            if (!productVM.MainPhoto.ValidateSize(500))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ModelState.AddModelError("MainPhoto", "File olcusu uygun deyil:500Kb");
                return View();
            }

            if (!productVM.HoverPhoto.ValidateType("image/"))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ModelState.AddModelError("HoverPhoto", "File tipi uygun deyil");
                return View();
            }

            if (!productVM.HoverPhoto.ValidateSize(500))
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                ViewBag.Sizes = await _context.Sizes.ToListAsync();
                ViewBag.Colors = await _context.Colors.ToListAsync();
                ModelState.AddModelError("HoverPhoto", "File olcusu uygun deyil:500Kb");
                return View();
            }

            ProductImage mainImage = new ProductImage
            {
                Alternative = productVM.Name,
                IsPrimary = true,
                Url = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
            };

            ProductImage hoverImage = new ProductImage
            {
                Alternative = productVM.Name,
                IsPrimary = false,
                Url = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
            };

            Product product = new Product
            {
                Name = productVM.Name,
                SKU = productVM.SKU,
                Description = productVM.Description,
                Price = productVM.Price,
                CategoryId = (int)productVM.CategoryId,
                ProductTags = new List<ProductTag>(),
                ProductColors = new List<ProductColor>(),
                ProductSizes = new List<ProductSize>(),
                ProductImages = new List<ProductImage> { mainImage,hoverImage }
            };

            foreach (int tagId in productVM.TagIds)
            {
                ProductTag productTag = new ProductTag
                {
                    TagId = tagId,
                };
                product.ProductTags.Add(productTag);
            }

            foreach (int colorId in productVM.ColorIds)
            {
                ProductColor productColor = new ProductColor
                {
                    ColorId = colorId,
                };
                product.ProductColors.Add(productColor);
            }

            foreach (int sizeID in productVM.SizeIds)
            {
                ProductSize productSize = new ProductSize
                {
                    SizeId = sizeID,
                };
                product.ProductSizes.Add(productSize);
            }

            TempData["Message"] = "";
            foreach (IFormFile photo in productVM.Photos)
            {
                if (!photo.ValidateType("image/"))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file tipi uygun deyil</p>";
                    continue;
                }

                if (!photo.ValidateSize(500))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file olcusu uygun deyil</p>";
                    continue;
                }
                product.ProductImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = null,
                    Url = await photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                });
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            Product product = await _context.Products
                .Include(pt=>pt.ProductTags)
                .Include(s=>s.ProductSizes)
				.Include(p=>p.ProductImages)
                .Include(c=>c.ProductColors)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (product is null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                Price = product.Price,
                CategoryId = (int)product.CategoryId,
                TagIds = product.ProductTags.Select(t => t.TagId).ToList(),
                ProductImages = product.ProductImages,
                SizeIds = product.ProductSizes.Select(s => s.SizeId).ToList(),
                ColorIds = product.ProductColors.Select(c => c.ColorId).ToList(),
                Categories = await _context.Categories.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Sizes = await _context.Sizes.ToListAsync(),
                Colors = await _context.Colors.ToListAsync()
            };

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id,UpdateProductVM productVM)
        {
            Product existed = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductSizes)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.Id == id);

            productVM.ProductImages = existed.ProductImages;
            if (!ModelState.IsValid)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags= await _context.Tags.ToListAsync();
                productVM.Colors= await _context.Colors.ToListAsync();
                productVM.Sizes= await _context.Sizes.ToListAsync();
                return View(productVM);
            }

            if (existed is null) return NotFound();

            bool result=await _context.Categories.AnyAsync(c => c.Id == productVM.CategoryId);

            if (!result)
            {
                productVM.Categories = await _context.Categories.ToListAsync();
                productVM.Tags = await _context.Tags.ToListAsync();
                productVM.Colors = await _context.Colors.ToListAsync();
                productVM.Sizes = await _context.Sizes.ToListAsync();
                ModelState.AddModelError("CategoryId", "Bu adda category movcud deyil");
                return View(productVM);
            }

            if (productVM.MainPhoto is not null)
            {
                if (!productVM.MainPhoto.ValidateType("image/"))
                {
                    productVM.Categories = await _context.Categories.ToListAsync(); 
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("MainPhoto", "Fayl novu uygun deyil");
                    return View(productVM);
                }
                if (!productVM.MainPhoto.ValidateSize(600))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("MainPhoto", "Fayl olcusu uygun deyil:600kB");
                    return View(productVM);
                }
            }

            if (productVM.HoverPhoto is not null)
            {
                if (!productVM.HoverPhoto.ValidateType("image/"))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("HoverPhoto", "Fayl novu uygun deyil");
                    return View(productVM);
                }
                if (!productVM.HoverPhoto.ValidateSize(600))
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("HoverPhoto", "Fayl olcusu uygun deyil:600kB");
                    return View(productVM);
                }
            }

            if (productVM.MainPhoto is not null)
            {
                string fileName = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage mainImage = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                mainImage.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                _context.ProductImages.Remove(mainImage);

                existed.ProductImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = true,
                    Url = fileName
                });
            }
            if (productVM.HoverPhoto is not null)
            {
                string fileName = await productVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");
                ProductImage hoverImage = existed.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true);
                hoverImage.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                _context.ProductImages.Remove(hoverImage);

                existed.ProductImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = false,
                    Url = fileName
                });
            }

           

  
            List<ProductImage> removeable =existed.ProductImages.Where(pi=>!productVM.ImageIds.Exists(imgId=>imgId==pi.Id)&& pi.IsPrimary==null).ToList();
            foreach (ProductImage pImage in removeable)
            {
                pImage.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
                existed.ProductImages.Remove(pImage);
            }

            TempData["Message"] = "";
            if (productVM.Photos is not null)
            {
            foreach (IFormFile photo in productVM.Photos)
            {
                if (!photo.ValidateType("image/"))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file tipi uygun deyil</p>";
                    continue;
                }

                if (!photo.ValidateSize(500))
                {
                    TempData["Message"] += $"<p class=\"text-danger\">{photo.FileName} file olcusu uygun deyil</p>";
                    continue;
                }
                existed.ProductImages.Add(new ProductImage
                {
                    Alternative = productVM.Name,
                    IsPrimary = null,
                    Url = await photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
                });
            }
            }

            existed.ProductTags.RemoveAll(pt => !productVM.TagIds.Exists(tId => tId == pt.TagId));
            List<int> tagCreatable = productVM.TagIds.Where(tId => !existed.ProductTags.Exists(pt => pt.TagId == tId)).ToList();
            foreach (int tagId in tagCreatable)
            {
                bool tagResult=await _context.Tags.AnyAsync(t=>t.Id==tagId);
                if (!tagResult)
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("TagId", "Bu adda tag movcud deyil");
                    return View(productVM);
                }
                existed.ProductTags.Add(new ProductTag
                {
                    TagId = tagId
                });
            }

            //COLOR

            existed.ProductColors.RemoveAll(pc => !productVM.ColorIds.Exists(cId => cId == pc.ColorId));

            List<int> colorCreatable = productVM.ColorIds.Where(cId => !existed.ProductColors.Exists(pc => pc.ColorId == cId)).ToList();

            foreach (int colorId in colorCreatable)
            {
                bool sizeResult = await _context.Sizes.AnyAsync(c => c.Id == colorId);
                if (!sizeResult)
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("ColorId", "Bu adda color movcud deyil");
                    return View();
                }
                existed.ProductColors.Add(new ProductColor
                {
                    ColorId = colorId
                });
            }

           

            existed.ProductSizes.RemoveAll(pt => !productVM.SizeIds.Exists(sId => sId == pt.SizeId));


            List<int> sizeCreatable = productVM.SizeIds.Where(sId => !existed.ProductSizes.Exists(ps => ps.SizeId == sId)).ToList();

            foreach (int sizeId in sizeCreatable)
            {
                bool sizeResult = await _context.Sizes.AnyAsync(s => s.Id == sizeId);
                if (!sizeResult)
                {
                    productVM.Categories = await _context.Categories.ToListAsync();
                    productVM.Tags = await _context.Tags.ToListAsync();
                    productVM.Colors = await _context.Colors.ToListAsync();
                    productVM.Sizes = await _context.Sizes.ToListAsync();
                    ModelState.AddModelError("SizeId", "Bu adda size movcud deyil");
                    return View();
                }
                existed.ProductSizes.Add(new ProductSize
                {
                    SizeId = sizeId
                });
            }


            existed.Name= productVM.Name;
            existed.Description= productVM.Description;
            existed.Price= productVM.Price;
            existed.SKU= productVM.SKU;
            existed.CategoryId=(int)productVM.CategoryId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(c => c.Id == id);

            if (product is null) return NotFound();

            foreach (ProductImage image in product.ProductImages)
            {
                image.Url.DeleteFile(_env.WebRootPath, "assets", "images", "website-images");
            };

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductSizes).ThenInclude(ps => ps.Size)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null) return NotFound();

            return View(product);
        }
    }
}
