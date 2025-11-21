using Microsoft.AspNetCore.Mvc;
using CMCSWeb.Data;
using CMCSWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace CMCSWeb.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinatorController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Display all claims pending verification
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            // Safe session setting
            var userName = User.Identity?.Name ?? "Coordinator";
            HttpContext.Session.SetString("UserRole", "Coordinator");
            HttpContext.Session.SetString("UserName", userName);

            var pendingClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            if (pendingClaims == null || !pendingClaims.Any())
            {
                ViewBag.InfoMessage = "There are no pending claims awaiting verification.";
            }

            return View(pendingClaims ?? new List<Claim>());
        }

        // Verify (approve for manager review)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            // Safe session check
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "Coordinator")
            {
                TempData["ErrorMessage"] = "Access denied. Please log in as Coordinator.";
                return RedirectToAction("Login", "Account");
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(Manage));
            }

            if (claim.Status == ClaimStatus.Pending)
            {
                claim.Status = ClaimStatus.Verified;
                claim.VerifiedAt = DateTime.Now;
                claim.VerifiedBy = User.Identity?.Name ?? "Coordinator";

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Claim #{claim.Id} verified successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error saving changes: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Only pending claims can be verified.";
            }

            return RedirectToAction(nameof(Manage));
        }

        // Reject claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            // Safe session check
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "Coordinator")
            {
                TempData["ErrorMessage"] = "Access denied. Please log in as Coordinator.";
                return RedirectToAction("Login", "Account");
            }

            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(Manage));
            }

            if (claim.Status == ClaimStatus.Pending)
            {
                claim.Status = ClaimStatus.Rejected;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["ErrorMessage"] = $"Claim #{claim.Id} has been rejected.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error saving changes: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Only pending claims can be rejected.";
            }

            return RedirectToAction(nameof(Manage));
        }

        // View verified claims
        [HttpGet]
        public async Task<IActionResult> Verified()
        {
            var verifiedClaims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.Status == ClaimStatus.Verified)
                .OrderByDescending(c => c.VerifiedAt)
                .ToListAsync();

            return View(verifiedClaims ?? new List<Claim>());
        }

        // Claim Details
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