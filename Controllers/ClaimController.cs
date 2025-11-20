using Microsoft.AspNetCore.Mvc;
using CMCSWeb.Data;
using CMCSWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CMCSWeb.Controllers
{
    [Authorize] // Require login
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Claim/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Pre-populate with user data
            var claim = new Claim
            {
                UserId = user.Id,
                HourlyRate = (double)(user.HourlyRate ?? 0)
            };

            ViewBag.UserName = user.FullName;
            ViewBag.UserHourlyRate = user.HourlyRate;

            return View(claim);
        }

        // POST: Claim/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile document)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate hours worked
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked cannot exceed 180 hours per month.");
            }

            if (ModelState.IsValid)
            {
                // Auto-populate user data
                claim.UserId = user.Id;
                claim.Status = ClaimStatus.Pending;
                claim.SubmittedAt = DateTime.Now;
                claim.ClaimMonth = DateTime.Now.Month;
                claim.ClaimYear = DateTime.Now.Year;

                // Handle file upload
                if (document != null && document.Length > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(document.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("DocumentPath", "Only .pdf, .docx, .xlsx, .jpg, or .jpeg files are allowed.");
                        return View(claim);
                    }

                    const long maxFileSize = 5 * 1024 * 1024;
                    if (document.Length > maxFileSize)
                    {
                        ModelState.AddModelError("DocumentPath", "File size cannot exceed 5 MB.");
                        return View(claim);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(document.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await document.CopyToAsync(stream);
                    }

                    claim.DocumentPath = uniqueFileName;
                }

                // Save to DB
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Claim submitted successfully! Awaiting coordinator verification.";
                return RedirectToAction(nameof(Status));
            }

            // If validation fails, reload with user data
            ViewBag.UserName = user.FullName;
            ViewBag.UserHourlyRate = user.HourlyRate;
            return View(claim);
        }

        // GET: Claim/Status
        [HttpGet]
        public async Task<IActionResult> Status()
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

            if (!claims.Any())
            {
                ViewBag.InfoMessage = "You have not submitted any claims yet.";
            }

            return View(claims);
        }
    }
}