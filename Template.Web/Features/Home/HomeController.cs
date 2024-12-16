using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Template.Web.Features.Home
{
    public partial class HomeController : Controller
    {
        // Azione GET per visualizzare la Home
        [HttpGet]
        public  virtual IActionResult Index()
        {
            // Data di inizio del mese (puoi cambiarla a seconda delle tue esigenze)
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Crea il modello HomeViewModel
            var model = new HomeViewModel(monthStart); // Passa la data di inizio del mese al modello

            // Ritorna la view passando il modello
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
