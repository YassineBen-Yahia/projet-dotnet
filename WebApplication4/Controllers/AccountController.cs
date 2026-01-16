using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Data;
using WebApplication4.Models;
using WebApplication4.ViewModels;

namespace WebApplication4.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _db;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Ensure role exists
        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.FindByIdAsync(userId!);
        
        if (user == null)
            return NotFound();

        // Get user's owned properties
        var ownedProperties = await _db.Properties
            .Include(p => p.Images)
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        // Get user's requests
        var userRequests = await _db.Requests
            .Include(r => r.Property)
            .ThenInclude(p => p!.Images)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Get requests for user's properties
        var propertyRequests = await _db.Requests
            .Include(r => r.Property)
            .Include(r => r.User)
            .Where(r => r.Property!.OwnerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // Get user's received messages
        var receivedMessages = await _db.Messages
            .Include(m => m.FromUser)
            .Where(m => m.ToUserId == userId)
            .OrderByDescending(m => m.SentAt)
            .Take(10)
            .ToListAsync();

        // Get user's sent messages
        var sentMessages = await _db.Messages
            .Include(m => m.ToUser)
            .Where(m => m.FromUserId == userId)
            .OrderByDescending(m => m.SentAt)
            .Take(10)
            .ToListAsync();

        ViewBag.User = user;
        ViewBag.Roles = await _userManager.GetRolesAsync(user);
        ViewBag.OwnedProperties = ownedProperties;
        ViewBag.UserRequests = userRequests;
        ViewBag.PropertyRequests = propertyRequests;
        ViewBag.ReceivedMessages = receivedMessages;
        ViewBag.SentMessages = sentMessages;

        return View();
    }
}
