using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Data;
using WebApplication4.Models;

using WebApplication4.ViewModels;

namespace WebApplication4.Controllers;

[Authorize(Roles = "Admin")]
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



    // GET: Admin/Statistics
    public async Task<IActionResult> Statistics()
    {
        var model = new DashboardStatisticsViewModel();

        // 1. Property Status Distribution
        var propertyStats = await _db.Properties
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        model.PropertyStatusLabels = propertyStats.Select(s => s.Status).ToList();
        model.PropertyStatusData = propertyStats.Select(s => s.Count).ToList();

        // 2. Monthly Request Trend (Last 6 months)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var requestStats = await _db.Requests
            .Where(r => r.CreatedAt >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
            .ToListAsync();

        for (int i = 0; i < 6; i++)
        {
            var month = sixMonthsAgo.AddMonths(i);
            var label = month.ToString("MMM yyyy");
            var count = requestStats.Count(r => r.CreatedAt.Year == month.Year && r.CreatedAt.Month == month.Month);
            model.RequestTrendLabels.Add(label);
            model.RequestTrendData.Add(count);
        }

        // 3. User Distribution by Role
        var roles = await _roleManager.Roles.ToListAsync();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            model.RoleLabels.Add(role.Name!);
            model.RoleData.Add(usersInRole.Count);
        }

        // 4. Top 5 Most Requested Properties
        var topProperties = await _db.Requests
            .GroupBy(r => r.PropertyId)
            .Select(g => new { PropertyId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        foreach (var item in topProperties)
        {
            var property = await _db.Properties.FindAsync(item.PropertyId);
            if (property != null)
            {
                model.TopPropertyLabels.Add(property.Title);
                model.TopPropertyData.Add(item.Count);
            }
        }

        // 5. Monthly Message Trend (Last 6 months)
        var messageStats = await _db.Messages
            .Where(m => m.SentAt >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
            .ToListAsync();

        for (int i = 0; i < 6; i++)
        {
            var month = sixMonthsAgo.AddMonths(i);
            var label = month.ToString("MMM yyyy");
            var count = messageStats.Count(m => m.SentAt.Year == month.Year && m.SentAt.Month == month.Month);
            model.MessageTrendLabels.Add(label);
            model.MessageTrendData.Add(count);
        }

        return View(model);
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
