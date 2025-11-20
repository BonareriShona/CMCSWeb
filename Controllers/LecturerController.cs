using Microsoft.AspNetCore.Mvc;
using CMCSWeb.Data;
using CMCSWeb.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;

namespace CMCSWeb.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public LecturerController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        // Display the claim submission form with auto-populated data
        [HttpGet]
        public async Task<IActionResult> Submit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new Claim
            {
                UserId = user.Id,
                HourlyRate = (double)(user.HourlyRate ?? 0)
            };

            ViewBag.UserName = user.FullName;
            ViewBag.HourlyRate = user.HourlyRate;

            return View(model);
        }

        // Handle claim submission with auto-calculation and validation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim claim, IFormFile? document)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate hours worked (max 180 hours)
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked cannot exceed 180 hours per month.");
            }

            if (ModelState.IsValid)
            {
                // Auto-populate user data
                claim.UserId = user.Id;
                claim.HourlyRate = (double)(user.HourlyRate ?? 0);

                // Handle file upload - ACCEPT ANY DOCUMENT TYPE
                if (document != null && document.Length > 0)
                {
                    // REMOVED file type restrictions - accept any file type
                    // Only check file size for security
                    const long maxFileSize = 10 * 1024 * 1024; // Increased to 10 MB

                    if (document.Length > maxFileSize)
                    {
                        ModelState.AddModelError("DocumentPath", "File size cannot exceed 10 MB.");
                        return View(claim);
                    }

                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Keep original filename but make it unique
                    var originalFileName = Path.GetFileName(document.FileName);
                    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(originalFileName)}_{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await document.CopyToAsync(stream);
                    }

                    claim.DocumentPath = uniqueFileName;
                }

                // Set claim properties
                claim.Status = ClaimStatus.Pending;
                claim.SubmittedAt = DateTime.Now;
                claim.ClaimMonth = DateTime.Now.Month;
                claim.ClaimYear = DateTime.Now.Year;

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your claim has been submitted successfully!";
                return RedirectToAction(nameof(Track));
            }

            // If validation fails, reload with user data
            ViewBag.UserName = user.FullName;
            ViewBag.HourlyRate = user.HourlyRate;
            return View(claim);
        }

        // Lecturer can view and track all their claims
        [HttpGet]
        public async Task<IActionResult> Track()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = await _context.Claims
                .Include(c => c.User) // Include user data for display
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        // Auto-calculation endpoint for AJAX
        [HttpPost]
        public IActionResult CalculateTotal(double hoursWorked, double hourlyRate)
        {
            if (hoursWorked > 180)
            {
                return Json(new { error = "Hours worked cannot exceed 180 hours per month." });
            }

            var total = hoursWorked * hourlyRate;
            return Json(new { total = total.ToString("C") });
        }
    }

}