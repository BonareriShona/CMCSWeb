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
                HourlyRate = user.HourlyRate 
            };

            ViewBag.UserName = user.FullName;
            ViewBag.HourlyRate = user.HourlyRate.ToString("F2");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim claim, IFormFile? document)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Server-side validation
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked cannot exceed 180 hours per month.");
            }

            if (claim.HoursWorked <= 0)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked must be greater than 0.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Auto-populate user data
                    claim.UserId = user.Id;
                    claim.HourlyRate = user.HourlyRate ;

                    // Calculate total amount - using the computed property
                    // TotalAmount is computed automatically in the getter

                    // Handle file upload
                    if (document != null && document.Length > 0)
                    {
                        const long maxFileSize = 10 * 1024 * 1024;
                        if (document.Length > maxFileSize)
                        {
                            ModelState.AddModelError("DocumentPath", "File size cannot exceed 10 MB.");
                            return View(claim);
                        }

                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

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
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error submitting claim: {ex.Message}");
                }
            }

            // If validation fails, reload with user data
            ViewBag.UserName = user.FullName;
            ViewBag.HourlyRate = user.HourlyRate.ToString("F2");
            return View(claim);
        }

        [HttpGet]
        public async Task<IActionResult> Track()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        // API endpoint for auto-calculation (for JavaScript)
        [HttpPost]
        public IActionResult CalculateTotal(decimal hoursWorked, decimal hourlyRate)
        {
            try
            {
                if (hoursWorked > 180)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Hours worked cannot exceed 180 hours per month."
                    });
                }

                if (hoursWorked <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Hours worked must be greater than 0."
                    });
                }

                var total = hoursWorked * hourlyRate;
                return Json(new
                {
                    success = true,
                    total = total.ToString("C"),
                    totalRaw = total
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = $"Calculation error: {ex.Message}"
                });
            }
        }
    }
}