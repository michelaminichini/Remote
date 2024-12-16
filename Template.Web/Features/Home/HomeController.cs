using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Template.Infrastructure;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Features.History;
using Template.Web.Infrastructure;

namespace Template.Web.Features.Home
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Alerts]
    public partial class HomeController : Controller
    {
        public readonly TemplateDbContext _dbContext;
        public static string LoginErrorModelStateKey = "LoginError";
        private readonly SharedService _sharedService;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public HomeController(TemplateDbContext dbContext, SharedService sharedService, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _dbContext = dbContext;
            _sharedService = sharedService;
            _sharedLocalizer = sharedLocalizer;
        }

        [HttpGet]
        public virtual IActionResult Home()
        {
            // Data di inizio del mese (puoi cambiarla a seconda delle tue esigenze)
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var model = new HomeViewModel(monthStart);
            return View(model);
        }

        // Azione per cambiare la lingua
        [HttpPost]
        public virtual IActionResult ChangeLanguageTo(string cultureName)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureName)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), Secure = true }
            );
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
    }
}
