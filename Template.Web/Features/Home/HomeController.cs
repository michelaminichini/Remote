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

            // Recupera gli eventi (Tipologia) per ogni utente, includendo gli eventi relativi all'utente
            var events = _dbContext.Users
                .SelectMany(u => u.Events.Where(e => e.Stato != "Rifiutata"), // Seleziona gli eventi associati a ciascun utente, filtrando per stato
                    (u, e) => new // Creazione di un nuovo oggetto anonimo con i dettagli richiesti
                    {
                        u.Email,
                        u.Role,
                        u.TeamName,
                        e.Tipologia,
                        e.DataInizio,
                        e.DataFine,
                        e.Stato, // Stato dell'evento
                        e.LogoPath // Logo dell'evento, se disponibile
                    })
                .ToList();
            ;

            // Filtra gli eventi in base al ruolo dell'utente
            if (currentUser?.Role == "Dipendente")
            {
                // Se l'utente è un Dipendente, filtra per lo stesso team
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

                    // Se l'evento è di tipo "smartworking", controlliamo quanti eventi di questo tipo ci sono già nella settimana
                    if (eventType.Equals("Smartworking", StringComparison.OrdinalIgnoreCase))
                    {
                        var startOfWeek = eventStartDate.AddDays(-(int)eventStartDate.DayOfWeek); // Lunedì della settimana
                        var endOfWeek = startOfWeek.AddDays(6); // Domenica della settimana

                        // Recupera gli eventi di tipo "smartworking" già presenti nella settimana
                        var existingSmartworkingEvents = _dbContext.Users
                            .Where(u => u.Email == userEmail)
                            .SelectMany(u => u.Events.Where(e => e.Tipologia == "Smartworking" &&
                                                                 e.DataInizio.HasValue &&
                                                                 e.DataInizio.Value.Date >= startOfWeek &&
                                                                 e.DataInizio.Value.Date <= endOfWeek &&
                                                                 e.Stato != "Rifiutata")) // Solo eventi non rifiutati
                            .ToList();

                        // Se ci sono già 2 eventi di "smartworking", impedisci l'aggiunta
                        if (existingSmartworkingEvents.Count >= 2)
                        {
                            TempData["ErrorMessage"] = "Hai raggiunto il limite massimo di giornate di Smartworking disponibili per questa settimana.";
                            return RedirectToAction("Home");
                        }
                    }else if (eventType.Equals("Ferie", StringComparison.OrdinalIgnoreCase))
                    {
                        // Logica per limitare i giorni di ferie nel mese
                        var startOfMonth = new DateTime(eventStartDate.Year, eventStartDate.Month, 1); // Primo giorno del mese
                        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Ultimo giorno del mese

                        // Recupera gli eventi di tipo "ferie" già presenti nel mese
                        var existingFerieEvents = _dbContext.Users
                            .Where(u => u.Email == userEmail)
                            .SelectMany(u => u.Events.Where(e => e.Tipologia == "Ferie" &&
                                                                 e.DataInizio.HasValue &&
                                                                 e.DataInizio.Value.Month == eventStartDate.Month &&
                                                                 e.DataInizio.Value.Year == eventStartDate.Year &&
                                                                 e.Stato != "Rifiutata")) // Solo eventi non rifiutati
                            .ToList();

                        // Controlla quanti giorni di ferie sono già stati presi nel mese
                        var ferieDaysTaken = existingFerieEvents.Sum(e => (e.DataFine ?? e.DataInizio).Value.Subtract(e.DataInizio.Value).Days + 1);

                        // Se sono già stati presi 7 o più giorni di ferie, impedisci l'aggiunta
                        if (ferieDaysTaken >= 7)
                        {
                            TempData["ErrorMessage"] = "Hai raggiunto il limite massimo di giorni di ferie disponibili per questo mese";
                            return RedirectToAction("Home");
                        }
                    }

                    // Crea un oggetto Request per l'evento
                    var cmd = new AddRequestCommand
                    {
                        UserName = userEmail,
                        Tipologia = eventType,
                        DataInizio = eventStartDate,
                        DataFine = eventEndDate,
                        OraInizio = TimeSpan.Zero, // Ora di inizio, modificabile in base alle necessità
                        OraFine = TimeSpan.Zero, // Ora di fine, modificabile in base alle necessità
                        Stato = "Da Approvare" // Stato di default
                    };

                    // Usa il servizio SharedService per gestire la richiesta, delegando l'elaborazione al controller Richiesta
                    await _sharedService.HandleRequest(cmd);

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