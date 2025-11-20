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
            _context = context;
        }

        // Display all claims pending verification
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            // Set session for coordinator
            HttpContext.Session.SetString("UserRole", "Coordinator");
            HttpContext.Session.SetString("UserName", User.Identity.Name ?? "Coordinator");

            var pendingClaims = await _context.Claims
                .Include(c => c.User) // CRUCIAL: This includes the User data
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            if (!pendingClaims.Any())
                ViewBag.InfoMessage = "There are no pending claims awaiting verification.";

            return View(pendingClaims);
        }

        // Verify (approve for manager review)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
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
                claim.VerifiedBy = User.Identity.Name;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Claim #{claim.Id} verified successfully.";
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
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
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
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Claim #{claim.Id} has been rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Only pending claims can be rejected.";
            }

            return RedirectToAction(nameof(Manage));
        }
    }
}