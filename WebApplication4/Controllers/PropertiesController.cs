using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Data;
using WebApplication4.Models;
using WebApplication4.ViewModels;
using System.Security.Claims;

namespace WebApplication4.Controllers;

public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropertiesController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Properties
            .Include(p => p.Images)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var p = await _db.Properties
            .Include(x => x.Images)
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return View(p);
    }

    [Authorize]
    public IActionResult Create() => View(new PropertyCreateViewModel());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var property = new Property
        {
            Title = model.Title,
            Description = model.Description,
            Address = model.Address,
            Price = model.Price,
            Bedrooms = model.Bedrooms,
            Bathrooms = model.Bathrooms,
            Area = model.Area,
            Status = model.Status,
            OwnerId = userId
        };

        _db.Properties.Add(property);
        await _db.SaveChangesAsync();

        // Handle image uploads
        if (model.Images != null && model.Images.Any())
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "properties");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var image in model.Images)
            {
                if (image.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    var propertyImage = new PropertyImage
                    {
                        PropertyId = property.Id,
                        Url = "/uploads/properties/" + uniqueFileName
                    };
                    _db.PropertyImages.Add(propertyImage);
                }
            }
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Property created successfully!";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var p = await _db.Properties.FindAsync(id);
        if (p == null) return NotFound();

        // All authenticated users can edit as per user request (all user types)

        var model = new PropertyCreateViewModel
        {
            Title = p.Title,
            Description = p.Description,
            Address = p.Address,
            Price = p.Price,
            Bedrooms = p.Bedrooms,
            Bathrooms = p.Bathrooms,
            Area = p.Area,
            Status = p.Status
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PropertyCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var property = await _db.Properties.FindAsync(id);
        if (property == null) return NotFound();

        // All authenticated users can edit as per user request (all user types)

        property.Title = model.Title;
        property.Description = model.Description;
        property.Address = model.Address;
        property.Price = model.Price;
        property.Bedrooms = model.Bedrooms;
        property.Bathrooms = model.Bathrooms;
        property.Area = model.Area;
        property.Status = model.Status;

        await _db.SaveChangesAsync();

        // Handle new image uploads
        if (model.Images != null && model.Images.Any())
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "properties");
            Directory.CreateDirectory(uploadsFolder);

            foreach (var image in model.Images)
            {
                if (image.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    var propertyImage = new PropertyImage
                    {
                        PropertyId = property.Id,
                        Url = "/uploads/properties/" + uniqueFileName
                    };
                    _db.PropertyImages.Add(propertyImage);
                }
            }
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Property updated successfully!";
        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        // All authenticated users can delete as per user request (all user types)

        // Delete associated images from disk
        foreach (var image in p.Images)
        {
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.Url.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        _db.Properties.Remove(p);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Property deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(Guid imageId, Guid propertyId)
    {
        var image = await _db.PropertyImages.FindAsync(imageId);
        if (image == null) return NotFound();

        var property = await _db.Properties.FindAsync(propertyId);
        if (property == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (property.OwnerId != userId && !isAdmin)
            return Forbid();

        // Delete image file from disk
        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.Url.TrimStart('/'));
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }

        _db.PropertyImages.Remove(image);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Image deleted successfully!";
        return RedirectToAction(nameof(Edit), new { id = propertyId });
    }
}
