using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CMCSWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CMCSWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        { 
            ViewData["ReturnUrl"] = returnUrl;

            ModelState.Remove("returnUrl");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe?? true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var userRole = roles.FirstOrDefault() ?? "Lecturer";

                        // Set session based on role
                        HttpContext.Session.SetString("UserRole", userRole);
                        HttpContext.Session.SetString("UserName", user.FullName ?? "User");

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        // Redirect based on role
                        return userRole switch
                        {
                            "HR" => RedirectToAction("Dashboard", "HR"),
                            "Coordinator" => RedirectToAction("Manage", "Coordinator"),
                            "Manager" => RedirectToAction("Manage", "Manager"),
                            _ => RedirectToAction("Track", "Lecturer")
                        };
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        //[EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool? RememberMe { get; set; }
    }
}