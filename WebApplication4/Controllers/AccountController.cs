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
    private readonly IWebHostEnvironment _env;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _db = db;
        _env = env;
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
            ModelState.AddModelError(string.Empty, "Incorrect email or password. Please check your credentials and try again.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Incorrect email or password. Please check your credentials and try again.");
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the form for errors.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        
        TempData["Error"] = "Failed to update profile.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the password form for errors.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            TempData["Success"] = "Password changed successfully!";
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index");
        }

        TempData["Error"] = "Failed to change password. Please check your current password.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var userId = user.Id;

        // 1. Cleanup Properties and Images
        var properties = await _db.Properties.Include(p => p.Images).Where(p => p.OwnerId == userId).ToListAsync();
        foreach (var property in properties)
        {
            foreach (var image in property.Images)
            {
                var imagePath = Path.Combine(_env.WebRootPath, image.Url.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
        }
        _db.Properties.RemoveRange(properties);

        // 2. Cleanup Requests (sent or received)
        var requests = await _db.Requests
            .Where(r => r.UserId == userId || r.Property!.OwnerId == userId)
            .ToListAsync();
        _db.Requests.RemoveRange(requests);

        // 3. Cleanup Messages (sent or received)
        var messages = await _db.Messages
            .Where(m => m.FromUserId == userId || m.ToUserId == userId)
            .ToListAsync();
        _db.Messages.RemoveRange(messages);

        await _db.SaveChangesAsync();

        await _signInManager.SignOutAsync();
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        // If delete fails, redirect to index with error (unlikely after cleanup)
        TempData["Error"] = "Failed to delete account.";
        return RedirectToAction("Index");
    }
}
