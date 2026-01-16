using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Data;
using WebApplication4.Models;

namespace WebApplication4.Controllers;

[Authorize(Roles = "Admin,Agent")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: Admin Dashboard
    public async Task<IActionResult> Index()
    {
        var stats = new
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalProperties = await _db.Properties.CountAsync(),
            TotalRequests = await _db.Requests.CountAsync(),
            TotalMessages = await _db.Messages.CountAsync(),
            PendingRequests = await _db.Requests.CountAsync(r => r.Status == "Pending"),
            AvailableProperties = await _db.Properties.CountAsync(p => p.Status == "Available"),
            SoldProperties = await _db.Properties.CountAsync(p => p.Status == "Sold")
        };

        ViewBag.Stats = stats;

        // Recent activities
        var recentProperties = await _db.Properties
            .Include(p => p.Owner)
            .OrderByDescending(p => p.Id)
            .Take(5)
            .ToListAsync();

        var recentRequests = await _db.Requests
            .Include(r => r.Property)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentProperties = recentProperties;
        ViewBag.RecentRequests = recentRequests;

        return View();
    }

    // GET: Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    // GET: Admin/UserDetails/id
    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var properties = await _db.Properties
            .Where(p => p.OwnerId == id)
            .ToListAsync();
        var requests = await _db.Requests
            .Include(r => r.Property)
            .Where(r => r.UserId == id)
            .ToListAsync();

        ViewBag.Roles = roles;
        ViewBag.Properties = properties;
        ViewBag.Requests = requests;

        return View(user);
    }

    // POST: Admin/ToggleUserRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        // Ensure role exists
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        var isInRole = await _userManager.IsInRoleAsync(user, role);

        if (isInRole)
        {
            await _userManager.RemoveFromRoleAsync(user, role);
            TempData["Success"] = $"Removed {role} role from user.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = $"Added {role} role to user.";
        }

        return RedirectToAction(nameof(UserDetails), new { id = userId });
    }

    // POST: Admin/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction(nameof(Users));
        }

        TempData["Error"] = "Failed to delete user.";
        return RedirectToAction(nameof(UserDetails), new { id });
    }

    // GET: Admin/Properties
    public async Task<IActionResult> Properties()
    {
        var properties = await _db.Properties
            .Include(p => p.Owner)
            .Include(p => p.Images)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return View(properties);
    }

    // GET: Admin/Requests
    public async Task<IActionResult> Requests()
    {
        var requests = await _db.Requests
            .Include(r => r.Property)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    // GET: Admin/Messages
    public async Task<IActionResult> Messages()
    {
        var messages = await _db.Messages
            .Include(m => m.FromUser)
            .Include(m => m.ToUser)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    // GET: Admin/Statistics
    public async Task<IActionResult> Statistics()
    {
        var propertyStats = await _db.Properties
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var requestStats = await _db.Requests
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var userStats = new List<object>();
        var roles = await _roleManager.Roles.ToListAsync();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            userStats.Add(new { Role = role.Name, Count = usersInRole.Count });
        }

        ViewBag.PropertyStats = propertyStats;
        ViewBag.RequestStats = requestStats;
        ViewBag.UserStats = userStats;

        return View();
    }

    // POST: Admin/SeedRoles
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedRoles()
    {
        var roles = new[] {  "Client", "Agent" };
        
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        TempData["Success"] = "Roles seeded successfully!";
        return RedirectToAction(nameof(Index));
    }
}
