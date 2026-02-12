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
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: Messages
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        IQueryable<Message> query = _db.Messages
            .Include(m => m.FromUser)
            .Include(m => m.ToUser);

        if (!isAdmin)
        {
            query = query.Where(m => m.FromUserId == userId || m.ToUserId == userId);
        }

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    // GET: Messages/Sent
    public async Task<IActionResult> Sent()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var messages = await _db.Messages
            .Include(m => m.ToUser)
            .Where(m => m.FromUserId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    // GET: Messages/Received
    public async Task<IActionResult> Received()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var messages = await _db.Messages
            .Include(m => m.FromUser)
            .Where(m => m.ToUserId == userId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    // GET: Messages/Create
    public IActionResult Create(string? toEmail = null)
    {
        var model = new MessageCreateViewModel();
        if (!string.IsNullOrEmpty(toEmail))
        {
            model.ToUserEmail = toEmail;
        }
        return View(model);
    }

    // POST: Messages/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MessageCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var toUser = await _userManager.FindByEmailAsync(model.ToUserEmail);
        if (toUser == null)
        {
            ModelState.AddModelError("ToUserEmail", "User with this email not found.");
            return View(model);
        }

        var fromUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var message = new Message
        {
            FromUserId = fromUserId!,
            ToUserId = toUser.Id,
            Subject = model.Subject,
            Body = model.Body,
            SentAt = DateTime.UtcNow
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Message sent successfully!";
        return RedirectToAction(nameof(Sent));
    }

    // GET: Messages/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var message = await _db.Messages
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        // Only sender or recipient or Admin can view
        if (message.FromUserId != userId && message.ToUserId != userId && !isAdmin)
            return Forbid();

        return View(message);
    }

    // GET: Messages/Reply/5
    public async Task<IActionResult> Reply(Guid id)
    {
        var originalMessage = await _db.Messages
            .Include(m => m.FromUser)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (originalMessage == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Only recipient can reply
        if (originalMessage.ToUserId != userId)
            return Forbid();

        var model = new MessageCreateViewModel
        {
            ToUserEmail = originalMessage.FromUser?.Email ?? "",
            Subject = "Re: " + originalMessage.Subject
        };

        return View("Create", model);
    }

    // POST: Messages/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var message = await _db.Messages.FindAsync(id);
        if (message == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        // Only sender or recipient or Admin can delete
        if (message.FromUserId != userId && message.ToUserId != userId && !isAdmin)
            return Forbid();

        _db.Messages.Remove(message);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Message deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}

