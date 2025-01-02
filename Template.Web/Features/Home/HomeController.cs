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

            // Check that the date 'from' is not later than the date 'to'
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom.Value > dateTo.Value)
            {
                TempData["ErrorMessage"] = "La data 'dal' non può essere successiva alla data 'al'.";
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
                        e.LogoPath
                    })
                .ToList();

            if (currentUser?.Role == "Manager" || currentUser?.Role == "Dipendente")
            {
                events = events.Where(e => e.TeamName == currentUser.TeamName).ToList();
            }

            // Map anonymous events to a list of EventIconViewModel
            var eventIcons = events.Select(e => new EventIconViewModel
            {
                Icon = e.LogoPath, // Use LogoPath for the icon
                UserName = e.Email // Use email for users list
            }).ToList();

            // Create the template for the view
            var model = new HomeViewModel
            {
                UserEmail = currentUser?.Email,
                CurrentMonthName = new DateTime(currentYear, currentMonth, 1).ToString("MMMM"),
                CurrentYear = currentYear,
                CurrentMonth = currentMonth,
                DateFrom = dateFrom,
                DateTo = dateTo,
                UserProfileImage = currentUser?.Img,
                Weeks = Calendar.GetWeeksInMonth(currentYear, currentMonth, dateFrom, dateTo, eventIcons)  // Passes events as EventIconViewModel
            };

            // Associate events with calendar days
            foreach (var week in model.Weeks)
            {
                foreach (var day in week) // For each day of the week
                {
                    // Initialize the event list if it has not already been initialized
                    if (day.Events == null)
                    {
                        day.Events = new List<EventIconViewModel>();
                    }

                    //  Filters events that overlap with the specific day
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

                        // Only collects users who have the event on that day
                        var userNames = dayEvents
                                        .Where(e => e.Tipologia == eventInfo.Tipologia && e.Email == eventInfo.Email)
                                        .Select(e => e.Email)
                                        .Distinct()
                                        .ToList();

                        // Add the event with the icon and name of the specific user for that day
                        day.Events.Add(new EventIconViewModel
                        {
                            Icon = eventIcon,
                            UserName = string.Join(", ", userNames) // Connect the user’s email for the tooltip
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
                _ => "/images/Logo/default.png",
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

                    // Logic for Smartworking, Trasferta, Presenza
                    if (eventType.Equals("Smartworking", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ValidateSmartworkingAvailability(userEmail, eventStartDate))
                        {
                            eventStartDate = eventStartDate.Date;
                            eventEndDate = eventStartDate.AddDays(1).AddSeconds(-1);

                            var richiesta = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                            AddEventForUserBasedOnRole(richiesta, currentUser.Role);
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
                            AddEventForUserBasedOnRole(richiesta, currentUser.Role);
                        }
                    }
                    else if (eventType.Equals("Presenza", StringComparison.OrdinalIgnoreCase))
                    {
                        // Manages the "Presence" event as a full day event
                        eventStartDate = selectedDate.Date;
                        eventEndDate = selectedDate.Date.AddDays(1).AddSeconds(-1);

                        var richiesta = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                        AddEventForUserBasedOnRole(richiesta, currentUser.Role);
                    }
                    // Logic for Ferie and Permessi (managed by _dbContext.Requests.Add)
                    else if (eventType.Equals("Ferie", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ValidateFerieAvailability(userEmail, eventStartDate))
                        {
                            eventStartDate = eventStartDate.Date;
                            eventEndDate = eventStartDate.AddDays(1).AddSeconds(-1);

                            var request = CreateRequest(userEmail, eventType, eventStartDate, eventEndDate, startTime, endTime, currentUser.Role);
                            AddEventForUserBasedOnRole(request, currentUser.Role);

                            // Only if user is not a CEO, show the message of success
                            if (!currentUser.Role.Equals("CEO", StringComparison.OrdinalIgnoreCase))
                            {
                                TempData["Message"] = "Richiesta inviata con successo!!";
                            }
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
                            AddEventForUserBasedOnRole(request, currentUser.Role);

                            // Only if user is not a CEO, show the message of success
                            if (!currentUser.Role.Equals("CEO", StringComparison.OrdinalIgnoreCase))
                            {
                                TempData["Message"] = "Richiesta inviata con successo!!";
                            }
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
                    TempData["ErrorMessage"] = "Si è verificato un errore durante l'aggiunta dell'evento.";
                }
            }

            return RedirectToAction("Home");
        }

        // Support function to manage the addition of event based on role
        private void AddEventForUserBasedOnRole(Request request, string userRole)
        {
            // Always set the status of the application as "Accepted" for the role "CEO"
            if (userRole.Equals("CEO", StringComparison.OrdinalIgnoreCase))
            {
                request.Stato = "Accettata"; // Always approved for CEO
                DataGenerator.AddEventForUser(_dbContext, request);
            }
            else
            {
                _dbContext.Requests.Add(request);
                _dbContext.SaveChanges();
            }
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
                         eventType.ToLower() == "trasferta") ? "Accettata" : "Da Approvare",
                OraInizio = startTime,
                OraFine = endTime,
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
