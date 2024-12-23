using Template.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Template.Services.Shared;
using System.Threading.Tasks;
using Template.Infrastructure;
using System.Data;
using System.Diagnostics;

namespace Template.Web.Features.Login
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Alerts]
    [ModelStateToTempData]
    public partial class LoginController : Controller
    {
        public static string LoginErrorModelStateKey = "LoginError";
        private readonly SharedService _sharedService;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public LoginController(SharedService sharedService, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _sharedService = sharedService;
            _sharedLocalizer = sharedLocalizer;
        }

        private ActionResult LoginAndRedirect(UserDetailDTO utente, string returnUrl, bool rememberMe)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, utente.Id.ToString()),
            new Claim(ClaimTypes.Name, utente.Email),
            new Claim(ClaimTypes.Email, utente.Email),
            new Claim(ClaimTypes.Role, utente.Role, utente.Role ?? "User")
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var userPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Debug: Verifica il ruolo dopo la creazione del ClaimsPrincipal
            var userRole = userPrincipal.FindFirstValue(ClaimTypes.Role);
            // Console.WriteLine($"Role: {userRole}"); // Debug del ruolo dell'utente

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
            {
                ExpiresUtc = (rememberMe) ? DateTimeOffset.UtcNow.AddMonths(3) : null,
                IsPersistent = rememberMe,
            });

            // Redirect to the requested URL or Home/Index if no return URL
            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Home", "Home"); // Explicit redirect to "Home"
        }

        [HttpGet]
        public virtual IActionResult Login(string returnUrl)
        {
            if (HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated)
            {
                // If already authenticated, redirect to Home
                return RedirectToAction("Home", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
            };

            return View(model);
        }

        [HttpPost]
        public async virtual Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var utente = await _sharedService.Query(new CheckLoginCredentialsQuery
                    {
                        Email = model.Email,
                        Password = model.Password,
                    });

                    //var userRole = User.FindFirstValue(ClaimTypes.Role);
                    //Console.WriteLine($"Role: {userRole}");

                    

                    return LoginAndRedirect(utente, model.ReturnUrl, model.RememberMe);
                }
                catch (LoginException e)
                {
                    ModelState.AddModelError(LoginErrorModelStateKey, e.Message);
                }
            }

            return View(model); // Return to login view if authentication fails
        }

        [HttpPost]
        public virtual IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            Alerts.AddSuccess(this, "Utente scollegato correttamente");
            return RedirectToAction("Login", "Login");  // Redirect alla pagina di login
        }
    }

}
