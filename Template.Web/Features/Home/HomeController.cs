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

            var events = _dbContext.Users
                .SelectMany(u => u.Events.Where(e => e.Stato != "Rifiutata"), // Seleziona gli eventi associati a ciascun utente, filtrando per stato
                    (u, e) => new
                    {
                        u.Email,
                        u.Role,
                        u.TeamName,
                        e.Tipologia,
                        e.DataInizio,
                        e.DataFine,
                        e.Stato,
                        e.LogoPath
                    })
                .ToList();

            if (currentUser?.Role == "Manager" || currentUser?.Role == "Dipendente") // filtra gli eventi per il rpoprio team
            {
                events = events.Where(e => e.TeamName == currentUser.TeamName).ToList();
            }

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

            foreach (var week in model.Weeks)
            {
                foreach (var day in week) // Filtra gli eventi per ogni giorno
                {
                    var dayEvents = events.Where(e =>
                        e.DataInizio.HasValue && e.DataFine.HasValue &&
                        e.DataInizio.Value.Date <= day.Date.Date &&
                        e.DataFine.Value.Date >= day.Date.Date)
                        .Select(e => e.Tipologia)
                        .ToList();

                    foreach (var eventType in dayEvents)
                    {
                        day.Events.Add(GetEventIcon(eventType));
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddEvent(DateTime selectedDate, string eventType, TimeSpan? startTime, TimeSpan? endTime)
        {
            var userEmail = User.Identity?.Name;
            var currentUser = _dbContext.Users
                .FirstOrDefault(u => u.Email == userEmail);

            if (currentUser != null)
            {
                try
                {
                    DateTime eventStartDate = selectedDate.Date;
                    DateTime eventEndDate = selectedDate.Date;

                    if (eventType.Equals("Smartworking", StringComparison.OrdinalIgnoreCase))
                    {
                        var startOfWeek = eventStartDate.AddDays(-(int)eventStartDate.DayOfWeek); //controllo che nella settimana non abbia già due giornate di smart
                        var endOfWeek = startOfWeek.AddDays(6); 

                        var existingSmartworkingEvents = _dbContext.Users
                            .Where(u => u.Email == userEmail)
                            .SelectMany(u => u.Events.Where(e => e.Tipologia == "Smartworking" &&
                                                                 e.DataInizio.HasValue &&
                                                                 e.DataInizio.Value.Date >= startOfWeek &&
                                                                 e.DataInizio.Value.Date <= endOfWeek &&
                                                                 e.Stato != "Rifiutata"))
                            .ToList();

                        if (existingSmartworkingEvents.Count >= 2)
                        {
                            TempData["ErrorMessage"] = "Hai raggiunto il limite massimo di giornate di Smartworking disponibili per questa settimana.";
                            return RedirectToAction("Home");
                        }

                        eventStartDate = eventStartDate.Date;
                        eventEndDate = eventStartDate.AddDays(1).AddSeconds(-1);
                    }
                    else if (eventType.Equals("Ferie", StringComparison.OrdinalIgnoreCase))
                    {
                        var startOfMonth = new DateTime(eventStartDate.Year, eventStartDate.Month, 1);
                        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                        var existingFerieEvents = _dbContext.Users
                            .Where(u => u.Email == userEmail)
                            .SelectMany(u => u.Events.Where(e => e.Tipologia == "Ferie" &&
                                                                 e.DataInizio.HasValue &&
                                                                 e.DataInizio.Value.Month == eventStartDate.Month &&
                                                                 e.DataInizio.Value.Year == eventStartDate.Year &&
                                                                 e.Stato != "Rifiutata"))
                            .ToList();

                        var ferieDaysTaken = existingFerieEvents.Sum(e => (e.DataFine ?? e.DataInizio).Value.Subtract(e.DataInizio.Value).Days + 1);

                        if (ferieDaysTaken >= 7)
                        {
                            TempData["ErrorMessage"] = "Hai raggiunto il limite massimo di giorni di ferie disponibili per questo mese.";
                            return RedirectToAction("Home");
                        }

                        eventStartDate = eventStartDate.Date;
                        eventEndDate = eventStartDate.AddDays(1).AddSeconds(-1);
                    }
                    else if (eventType.Equals("Trasferta", StringComparison.OrdinalIgnoreCase) ||
                             eventType.Equals("Permessi", StringComparison.OrdinalIgnoreCase))
                    {
                        if (startTime.HasValue && endTime.HasValue)
                        {
                            eventStartDate = selectedDate.Date.Add(startTime.Value);
                            eventEndDate = selectedDate.Date.Add(endTime.Value);

                            // Verifica che ora fine sia dopo ora inizio
                            if (eventEndDate <= eventStartDate)
                            {
                                TempData["ErrorMessage"] = "L'orario di fine deve essere successivo all'orario di inizio.";
                                return RedirectToAction("Home");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Ora di inizio e fine devono essere specificate per trasferta o permesso.";
                            return RedirectToAction("Home");
                        }
                    }

                    // Crea un oggetto Request
                    var request = new Request
                    {
                        UserName = userEmail,
                        Tipologia = eventType,
                        DataInizio = eventStartDate,
                        DataFine = eventEndDate,
                        Stato = "Da Approvare",
                        OraInizio = startTime, // Imposta l'orario di inizio
                        OraFine = endTime, // Imposta l'orario di fine
                        LogoPath = GetEventIcon(eventType),
                        Role = currentUser.Role
                    };

                    // Salva la richiesta nel database
                    _dbContext.Requests.Add(request);
                    await _dbContext.SaveChangesAsync();

                    TempData["Message"] = "Richiesta inviata con successo!!";
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
                "in presenza" => "/images/Logo/inpresenza.png",
                "trasferta" => "/images/Logo/trasferta.png",
                "permessi" => "/images/Logo/permessi.png",
                "smartworking" => "/images/Logo/smartworking.png",
                _ => "/images/Logo/default.png",
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
