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

            // Verifica che la data 'dal' non sia successiva alla data 'al'
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            {
                TempData["ErrorMessage"] = "La data 'dal' non pu� essere successiva alla data 'al'.";
                return RedirectToAction("Home");
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
                    // Inizializza la lista degli eventi se non � gi� stata inizializzata
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

                        // Raccoglie solo gli utenti che hanno l'evento in quel giorno
                        var userNames = dayEvents
                                        .Where(e => e.Tipologia == eventInfo.Tipologia && e.Email == eventInfo.Email)
                                        .Select(e => e.Email)
                                        .Distinct()
                                        .ToList();

                        // Aggiungi l'evento con l'icona e il nome dell'utente specifico per quel giorno
                        day.Events.Add(new EventIconViewModel
                        {
                            Icon = eventIcon,
                            UserName = string.Join(", ", userNames) // Concateno l'email dell'utente per il tooltip
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

                    // Logica per Smartworking, Trasferta, Presenza (gestiti tramite DataGenerator)
                    if (eventType.Equals("Smartworking", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ValidateSmartworkingAvailability(userEmail, eventStartDate))
                        {
                            eventStartDate = eventStartDate.Date;
                            eventEndDate = eventStartDate.AddDays(1).AddSeconds(-1);

                            var richiesta = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                            DataGenerator.AddEventForUser(_dbContext, richiesta);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Hai raggiunto il limite massimo di giornate di Smartworking disponibili per questa settimana.";
                            return RedirectToAction("Home");
                        }
                    }
                    else if (eventType.Equals("Trasferta", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ValidateStartAndEndTime(startTime, endTime))
                        {
                            eventStartDate = selectedDate.Date.Add(startTime.Value);
                            eventEndDate = selectedDate.Date.Add(endTime.Value);

                            var richiesta = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                            DataGenerator.AddEventForUser(_dbContext, richiesta);
                        }
                    }
                    else if (eventType.Equals("Presenza", StringComparison.OrdinalIgnoreCase))
                    {
                        // Gestisce l'evento di "Presenza" come un evento che copre tutta la giornata
                        eventStartDate = selectedDate.Date;
                        eventEndDate = selectedDate.Date.AddDays(1).AddSeconds(-1); // Copre tutta la giornata

                        var richiesta = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                        DataGenerator.AddEventForUser(_dbContext, richiesta);
                    }
                    // Logica per Ferie e Permessi (gestiti tramite _dbContext.Requests.Add)
                    else if (eventType.Equals("Ferie", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ValidateFerieAvailability(userEmail, eventStartDate))
                        {
                            eventStartDate = eventStartDate.Date;
                            eventEndDate = eventStartDate.AddDays(1).AddSeconds(-1);

                            var request = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                            _dbContext.Requests.Add(request);
                            await _dbContext.SaveChangesAsync();

                            TempData["Message"] = "Richiesta inviata con successo!!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Hai raggiunto il limite massimo di giorni di ferie disponibili per questo mese.";
                            return RedirectToAction("Home");
                        }
                    }
                    else if (eventType.Equals("Permessi", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ValidateStartAndEndTime(startTime, endTime))
                        {
                            eventStartDate = selectedDate.Date.Add(startTime.Value);
                            eventEndDate = selectedDate.Date.Add(endTime.Value);

                            var request = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                            _dbContext.Requests.Add(request);
                            await _dbContext.SaveChangesAsync();

                            TempData["Message"] = "Richiesta inviata con successo!!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Ora di inizio e fine devono essere specificate per permessi.";
                            return RedirectToAction("Home");
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Si � verificato un errore durante l'aggiunta dell'evento.";
                }
            }

            return RedirectToAction("Home");
        }

        private bool ValidateSmartworkingAvailability(string userEmail, DateTime eventStartDate)
        {
            var startOfWeek = eventStartDate.AddDays(-(int)eventStartDate.DayOfWeek); // Start of the week
            var endOfWeek = startOfWeek.AddDays(6); // End of the week

            var existingSmartworkingEvents = _dbContext.Users
                .Where(u => u.Email == userEmail)
                .SelectMany(u => u.Events.Where(e => e.Tipologia == "Smartworking" &&
                                                     e.DataInizio.HasValue &&
                                                     e.DataInizio.Value.Date >= startOfWeek &&
                                                     e.DataInizio.Value.Date <= endOfWeek &&
                                                     e.Stato != "Rifiutata"))
                .ToList();

            return existingSmartworkingEvents.Count < 2;
        }

        private bool ValidateFerieAvailability(string userEmail, DateTime eventStartDate)
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
            return ferieDaysTaken < 7;
        }

        private bool ValidateStartAndEndTime(TimeSpan? startTime, TimeSpan? endTime)
        {
            if (startTime.HasValue && endTime.HasValue)
            {
                return endTime > startTime;
            }
            return false;
        }

        private Request CreateRequest(string userEmail, string eventType, DateTime eventStartDate, DateTime eventEndDate,
                              TimeSpan? startTime, TimeSpan? endTime, string userRole)
        {
            return new Request
            {
                UserName = userEmail,
                Tipologia = eventType,
                DataInizio = eventStartDate,
                DataFine = eventEndDate,
                Stato = (eventType.ToLower() == "smartworking" ||
                         eventType.ToLower() == "presenza" ||
                         eventType.ToLower() == "trasferta") ? "Approvata" : "Da Approvare",
                OraInizio = startTime, // Imposta l'orario di inizio
                OraFine = endTime, // Imposta l'orario di fine
                LogoPath = GetEventIcon(eventType),
                Role = userRole
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
