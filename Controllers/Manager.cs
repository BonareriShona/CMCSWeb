using Microsoft.AspNetCore.Mvc;
using CMCSWeb.Data;
using CMCSWeb.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;

namespace CMCSWeb.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all verified claims for approval/rejection
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            // Set session for manager
            HttpContext.Session.SetString("UserRole", "Manager");
            HttpContext.Session.SetString("UserName", User.Identity?.Name ?? "Manager");

            var verifiedClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Verified)
                .OrderByDescending(c => c.VerifiedAt)
                .ToListAsync();

            return View(verifiedClaims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            // Check session
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied. Please log in as Manager.";
                return RedirectToAction("Login", "Account");
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim != null && claim.Status == ClaimStatus.Verified)
            {
                claim.Status = ClaimStatus.Approved;
                claim.ApprovedAt = DateTime.Now;
                claim.ApprovedBy = User.Identity?.Name ?? "Manager";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{claim.Id} approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found or not in verified status.";
            }

            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            // Check session
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied. Please log in as Manager.";
                return RedirectToAction("Login", "Account");
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim != null && claim.Status == ClaimStatus.Verified)
            {
                claim.Status = ClaimStatus.Rejected;
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Claim #{claim.Id} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found or not in verified status.";
            }

            return RedirectToAction(nameof(Manage));
        }

        // View all approved claims
        [HttpGet]
        public async Task<IActionResult> Approved()
        {
            var approvedClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Approved)
                .OrderByDescending(c => c.ApprovedAt)
                .ToListAsync();

            return View(approvedClaims);
        }

        // Claim Details for Manager
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(Manage));
            }

            return View(claim);
        }
    }
}