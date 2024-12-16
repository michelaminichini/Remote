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

        [HttpGet]
        public async virtual Task<IActionResult> History()
        {
            
            var userEmail = User.Identity.Name; // ottengo email dell'utente loggato

            if (string.IsNullOrEmpty(userEmail))
            {
                return NotFound("Email dell'utente non trovata.");
            }

            var user = await _dbContext.Users
                .Where(x => x.Email == userEmail)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();


            if (user == null)
            {
                return NotFound("Utente non trovato");
            }

            var allData = await _dbContext.Users
                .Where(x => x.Email == userEmail)
                .Select(x => new
                {
                    x.Email,
                    x.FirstName,
                    x.TeamName,
                    x.Role,
                    x.DataRichiesta,
                    x.Tipologia,
                    x.DataInizio,
                    x.DataFine,
                    x.Durata
                }).ToListAsync();



            //var userData = allData.Where(data => data.Email == userEmail).ToList();


            var model = allData.Select(data => new HistoryViewModel
            {
                Nome = data.FirstName,
                NomeTeam = data.TeamName,
                Ruolo = data.Role,
                Email = data.Email,
                DataRichiesta = data.DataRichiesta,
                Tipologia = data.Tipologia,
                DataInizio = data.DataInizio,
                DataFine = data.DataFine,

                // Calcolo della durata in ore e la conversione in stringa
                Durata = (data.DataFine - data.DataInizio).TotalHours.ToString("F2")
                
            }).ToList();

            return View(model);
        }


    }
}
