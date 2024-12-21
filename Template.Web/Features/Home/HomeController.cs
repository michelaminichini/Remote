using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual IActionResult Home(int? year, int? month, DateTime? dateFrom, DateTime? dateTo)
        {
            var currentMonth = month ?? DateTime.Now.Month;
            var currentYear = year ?? DateTime.Now.Year;

            if (currentMonth < 1)
            {
                currentMonth = 12;
                currentYear--;
            }
            else if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
            }

            var userEmail = User.Identity?.Name;
            var currentUser = _dbContext.Users
                .FirstOrDefault(u => u.Email == userEmail);

            // Recupera gli eventi (Tipologia) relativi all'utente
            var events = _dbContext.Users
                .Where(u => u.Email == userEmail && u.Stato != "Rifiutata")  // Filtra l'utente per email
                .Select(u => new
                {
                    u.Tipologia,
                    u.DataInizio,
                    u.DataFine
                })
                .ToList();

            // Crea il modello per la vista
            var model = new HomeViewModel
            {
                UserEmail = currentUser?.Email,
                CurrentMonthName = new DateTime(currentYear, currentMonth, 1).ToString("MMMM"),
                CurrentYear = currentYear,
                CurrentMonth = currentMonth,
                DateFrom = dateFrom,
                DateTo = dateTo,
                UserProfileImage = currentUser?.Img,
                Weeks = Calendar.GetWeeksInMonth(currentYear, currentMonth, dateFrom, dateTo)
            };

            // Aggiungi eventi per ciascun giorno della settimana
            foreach (var week in model.Weeks)
            {
                foreach (var day in week)
                {
                    // Filtra gli eventi per ogni giorno
                    var dayEvents = events.Where(e =>
                        e.DataInizio.HasValue && e.DataFine.HasValue &&  // Verifica la presenza delle date
                        e.DataInizio.Value.Date <= day.Date.Date && // Verifica che la data di inizio sia prima del giorno corrente
                        e.DataFine.Value.Date >= day.Date.Date) // Verifica che la data di fine sia successiva o uguale al giorno corrente
                        .Select(e => e.Tipologia) // Ottieni la tipologia dell'evento
                        .ToList();

                    foreach (var eventType in dayEvents)
                    {
                        // Aggiungi icone per ogni evento
                        day.Events.Add(GetEventIcon(eventType));
                    }
                }
            }

            return View(model);
        }

        // Metodo per aggiungere un evento tramite POST
        [HttpPost]
        public virtual async Task<IActionResult> AddEvent(DateTime selectedDate, string eventType)
        {
            var userEmail = User.Identity?.Name;
            var currentUser = _dbContext.Users
                .FirstOrDefault(u => u.Email == userEmail);

            if (currentUser != null)
            {
                try
                {
                    var eventStartDate = selectedDate.Date;
                    var eventEndDate = selectedDate.Date;

                    var request = new Request
                    {
                        Tipologia = eventType,
                        DataInizio = eventStartDate,
                        DataFine = eventEndDate,
                        OraInizio = TimeSpan.Zero, // Ora di inizio, modificabile in base alle necessità
                        OraFine = TimeSpan.Zero // Ora di fine, modificabile in base alle necessità
                    };

                    await _sharedService.HandleRequest(new AddRequestCommand
                    {
                        Tipologia = eventType,
                        DataInizio = eventStartDate,
                        DataFine = eventEndDate,
                        OraInizio = TimeSpan.Zero,
                        OraFine = TimeSpan.Zero,
                    });

                    TempData["Message"] = "Evento aggiunto con successo!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Si è verificato un errore durante l'aggiunta dell'evento.";
                }
            }
            return RedirectToAction("Home");
        }

        private string GetEventIcon(string eventType)
        {
            return eventType switch
            {
                "ferie" => "/images/Logo/ferie.png",
                "in presenza" => "/images/Logo/in_presenza.png",
                "trasferta" => "/images/Logo/trasferta.png",
                "permessi" => "/images/Logo/permessi.png",
                "smartworking" => "/images/Logo/smartworking.png",
                _ => "/images/Logo/default.png", // Icona di default
            };
        }

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