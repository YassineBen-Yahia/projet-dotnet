using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Data;
using WebApplication4.Models;
using WebApplication4.ViewModels;
using System.Security.Claims;

namespace WebApplication4.Controllers;

[Authorize]
public class RequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RequestsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: Requests
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        IQueryable<Request> query = _db.Requests
            .Include(r => r.Property)
            .Include(r => r.User);

        if (!isAdmin)
        {
            // Show user's own requests
            query = query.Where(r => r.UserId == userId);
        }

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    // GET: Requests/MyPropertyRequests
    public async Task<IActionResult> MyPropertyRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get requests for properties owned by current user
        var requests = await _db.Requests
            .Include(r => r.Property)
            .Include(r => r.User)
            .Where(r => r.Property!.OwnerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    // GET: Requests/Create
    public async Task<IActionResult> Create(Guid? propertyId)
    {
        var model = new RequestCreateViewModel();

        if (propertyId.HasValue)
        {
            var property = await _db.Properties.FindAsync(propertyId.Value);
            if (property != null)
            {
                model.PropertyId = propertyId.Value;
                model.PropertyTitle = property.Title;
            }
        }

        ViewBag.Properties = await _db.Properties
            .Where(p => p.Status == "Available")
            .Select(p => new { p.Id, p.Title })
            .ToListAsync();

        return View(model);
    }

    // POST: Requests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Properties = await _db.Properties
                .Where(p => p.Status == "Available")
                .Select(p => new { p.Id, p.Title })
                .ToListAsync();
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var request = new Request
        {
            PropertyId = model.PropertyId,
            UserId = userId!,
            Notes = model.Notes,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Your request has been submitted successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Requests/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var request = await _db.Requests
            .Include(r => r.Property)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        // Check authorization: user owns request or owns property or is admin
        if (request.UserId != userId && request.Property?.OwnerId != userId && !isAdmin)
            return Forbid();

        return View(request);
    }

    // POST: Requests/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        var request = await _db.Requests
            .Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        // Only property owner or admin can update status
        if (request.Property?.OwnerId != userId && !isAdmin)
            return Forbid();

        request.Status = status;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Request status updated to {status}!";
        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    // POST: Requests/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var request = await _db.Requests.FindAsync(id);
        if (request == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        // Only request owner or admin can delete
        if (request.UserId != userId && !isAdmin)
            return Forbid();

        _db.Requests.Remove(request);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Request deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

