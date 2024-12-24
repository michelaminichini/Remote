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
                .SelectMany(u => u.Events.Where(e => e.Stato != "Rifiutata"),
                    (u, e) => new
                    {
                        u.Email,
                        u.Role,
                        u.TeamName,
                        e.Tipologia,
                        e.DataInizio,
                        e.DataFine,
                        e.Stato,
                        e.LogoPath // Non normalizzo qui, uso direttamente il valore salvato
                    })
                .ToList();

            if (currentUser?.Role == "Manager" || currentUser?.Role == "Dipendente") // Filtra gli eventi per il proprio team
            {
                events = events.Where(e => e.TeamName == currentUser.TeamName).ToList();
            }

            // Mappa gli eventi anonimi in una lista di EventIconViewModel
            var eventIcons = events.Select(e => new EventIconViewModel
            {
                Icon = e.LogoPath, // Usa il LogoPath per l'icona
                UserName = e.Email // Usa l'Email per la lista degli utenti
            }).ToList();

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
                Weeks = Calendar.GetWeeksInMonth(currentYear, currentMonth, dateFrom, dateTo, eventIcons)  // Passa gli eventi come EventIconViewModel
            };

            // Associa gli eventi ai giorni del calendario
            foreach (var week in model.Weeks)
            {
                foreach (var day in week) // Per ogni giorno della settimana
                {
                    // Inizializza la lista degli eventi se non è già stata inizializzata
                    if (day.Events == null)
                    {
                        day.Events = new List<EventIconViewModel>();
                    }

                    // Filtra gli eventi che si sovrappongono con il giorno specifico
                    var dayEvents = events.Where(e =>
                        e.DataInizio.HasValue && e.DataFine.HasValue &&
                        e.DataInizio.Value.Date <= day.Date.Date &&
                        e.DataFine.Value.Date >= day.Date.Date)
                        .Select(e => new
                        {
                            e.Tipologia,
                            e.LogoPath,
                            e.Email
                        })
                        .ToList();

                    foreach (var eventInfo in dayEvents)
                    {
                        var eventIcon = GetEventIcon(eventInfo.Tipologia);
                        var userNames = events.Where(e => e.Tipologia == eventInfo.Tipologia)
                                              .Select(e => e.Email)
                                              .Distinct()
                                              .ToList(); // Evitiamo duplicati

                        // Aggiungi l'evento con l'icona e i nomi degli utenti
                        day.Events.Add(new EventIconViewModel
                        {
                            Icon = eventIcon,
                            UserName = string.Join(", ", userNames) // Concateno gli utenti per il tooltip
                        });
                    }
                }
            }

            return View(model);
        }

        private string GetEventIcon(string eventType)
        {
            return eventType.ToLower() switch
            {
                "ferie" => "/images/Logo/ferie.png",
                "presenza" => "/images/Logo/inpresenza.png",
                "trasferta" => "/images/Logo/trasferta.png",
                "permessi" => "/images/Logo/permessi.png",
                "smartworking" => "/images/Logo/smartworking.png",
                _ => "/images/Logo/default.png", // Icona predefinita
            };
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
                    else if (eventType.Equals("Presenza", StringComparison.OrdinalIgnoreCase))
                    {
                        // Gestisce l'evento di "Presenza" come un evento che copre tutta la giornata
                        eventStartDate = selectedDate.Date;
                        eventEndDate = selectedDate.Date.AddDays(1).AddSeconds(-1); // Copre tutta la giornata
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
