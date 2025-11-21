using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using CMCSWeb.Models;
using CMCSWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CMCSWeb.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HRController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // HR Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalClaims = await _context.Claims.CountAsync(),
                PendingClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Pending),
                ApprovedClaims = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Approved)
            };
            return View(stats);
        }

        // List all users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.UserRole = roles.FirstOrDefault() ?? "Lecturer";
                userList.Add(user);
            }

            return View(userList);
        }

        // Create new user
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(ApplicationUser model, string password, string role)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                    FullName = model.FullName ?? string.Empty,
                    PhoneNumber = model.PhoneNumber,
                    Department = model.Department,
                    HourlyRate = model.HourlyRate,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Ensure role exists
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }

                    // Add user to role
                    await _userManager.AddToRoleAsync(user, role);

                    TempData["SuccessMessage"] = $"User {user.FullName} created successfully!";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Edit user
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's current role
            var roles = await _userManager.GetRolesAsync(user);
            user.UserRole = roles.FirstOrDefault() ?? "Lecturer";

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, ApplicationUser model, string role)
        {
            if (string.IsNullOrEmpty(id) || id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName ?? string.Empty;
                user.Email = model.Email ?? string.Empty;
                user.UserName = model.Email ?? string.Empty;
                user.PhoneNumber = model.PhoneNumber;
                user.Department = model.Department;
                user.HourlyRate = model.HourlyRate;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, role);

                    TempData["SuccessMessage"] = $"User {user.FullName} updated successfully!";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.FullName} deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Users));
        }

        // Reset password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            // Generate a reset token and set new password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Password for {user.FullName} reset successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Error resetting password: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Users));
        }

        // Generate Reports
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var claims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Approved)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyReport(int month, int year)
        {
            var claims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.ClaimMonth == month && c.ClaimYear == year && c.Status == ClaimStatus.Approved)
                .OrderBy(c => c.User != null ? c.User.FullName : string.Empty)
                .ToListAsync();

            ViewBag.Month = month;
            ViewBag.Year = year;
            ViewBag.TotalAmount = claims.Sum(c => c.TotalAmount);

            return View("MonthlyReport", claims);
        }
    }
}