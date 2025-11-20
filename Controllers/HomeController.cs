using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CMCSWeb.Controllers
{
    public class HomeController : Controller
    {
        // Landing page
        [HttpGet]
        public IActionResult Index()
        {
            // Redirect authenticated users to their dashboard
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("HR"))
                    return RedirectToAction("Dashboard", "HR");
                else if (User.IsInRole("Coordinator"))
                    return RedirectToAction("Manage", "Coordinator");
                else if (User.IsInRole("Manager"))
                    return RedirectToAction("Manage", "Manager");
                else
                    return RedirectToAction("Track", "Lecturer");
            }

            ViewData["Title"] = "Welcome to CMCS";
            ViewData["SystemName"] = "Contract Monthly Claim System";
            return View();
        }

        // About page
        [HttpGet]
        public IActionResult About()
        {
            ViewData["Title"] = "About CMCS";
            ViewData["Description"] = "The Contract Monthly Claim System (CMCS) streamlines the claim submission and approval process for lecturers, coordinators, and managers. Lecturers can submit claims with supporting documents, coordinators verify them, and managers approve payments.";
            return View();
        }

        // Help or contact page
        [HttpGet]
        public IActionResult Help()
        {
            ViewData["Title"] = "Help & Support";
            ViewData["Message"] = "If you experience any issues, please contact your Programme Coordinator or Academic Manager for assistance.";
            return View();
        }

        // Error page
        [HttpGet]
        public IActionResult Error()
        {
            ViewData["Title"] = "Error";
            return View();
        }

        // Dashboard based on user role
        [Authorize]
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (User.IsInRole("HR"))
                return RedirectToAction("Dashboard", "HR");
            else if (User.IsInRole("Coordinator"))
                return RedirectToAction("Manage", "Coordinator");
            else if (User.IsInRole("Manager"))
                return RedirectToAction("Manage", "Manager");
            else
                return RedirectToAction("Track", "Lecturer");
        }
    }
}