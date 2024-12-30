using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Template.Infrastructure;
using Template.Services;
using Template.Services.Shared;
using Template.Web.Infrastructure;

namespace Template.Web.Features.Richiesta
{
    public partial class RichiestaController : Controller
    {
        private readonly SharedService _sharedService;
        private readonly TemplateDbContext _dbContext;



        // Costruttore che inietta il servizio SharedService
        public RichiestaController(SharedService sharedService, TemplateDbContext dbContext)
        {
            _sharedService = sharedService ?? throw new ArgumentNullException(nameof(sharedService));
            _dbContext = dbContext;
        }


        [HttpGet]
        public virtual IActionResult Richiesta(RichiestaViewModel model)
        {
            return View(model);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Storico()
        {
            try
            {
                // Recupera il nome del team dell'utente loggato
                var userName = User.Identity.Name;
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userName);
                var userRole = user.Role;
                var teamName = user.TeamName;

                if (string.IsNullOrEmpty(teamName))
                {
                    TempData["ErrorMessage"] = "Impossibile determinare il team dell'utente loggato.";
                    return RedirectToAction("Richiesta");
                }

                List<Request> richieste;
                if (userRole == "Manager")
                {
                    // Recupera solo le richieste relative al team dell'utente
                    richieste = await _sharedService.GetRequestByTeam(teamName);
                    Console.WriteLine($"Richieste trovate per il team {teamName}: {richieste.Count}");
                }
                else
                {
                    richieste = await _dbContext.Requests
                   .Where(r => r.UserName == user.Email) // Usa il nome dell'utente per filtrare
                   .ToListAsync();
                }

                // Passa i dati alla vista
                return View(richieste);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante il recupero delle richieste: " + ex.Message);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il recupero delle richieste.";
                return RedirectToAction("Richiesta");
            }
        }


        //[HttpGet]
        //public virtual async Task<IActionResult> Storico()
        //{
        //    try
        //    {
        //        // Recupera tutte le richieste dal database usando SharedService
        //        var richieste = await _sharedService.GetAllRequests();

        //        // Passa i dati alla vista
        //        return View(richieste);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Errore durante il recupero delle richieste: " + ex.Message);
        //        TempData["ErrorMessage"] = "Si è verificato un errore durante il recupero delle richieste.";
        //        return RedirectToAction("Richiesta");
        //    }
        //}

        [HttpPost]
        public virtual async Task<IActionResult> PostRequest(RichiestaViewModel richiesta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var cmd = new AddRequestCommand
                    {
                        UserName = User.Identity.Name,
                        Tipologia = richiesta.Tipologia,
                        DataInizio = richiesta.DataInizio,
                        DataFine = richiesta.DataFine,
                        OraInizio = richiesta.OraInizio ?? TimeSpan.Zero,
                        OraFine = richiesta.OraFine ?? TimeSpan.Zero,
                        Stato = richiesta.Stato
                    };
   
                    var id = await _sharedService.HandleRequest(cmd);

                    TempData["Message"] = "Richiesta inviata con successo!";
                    return RedirectToAction("Richiesta");
                }
                catch (Exception ex)
                {
                    // Gestisci l'errore e aggiungi un messaggio di errore
                    Console.WriteLine("Errore durante l'elaborazione della richiesta: " + ex.Message);
                    TempData["ErrorMessage"] = "Si è verificato un errore durante l'invio della richiesta: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "I dati inseriti non sono validi.";
            }
            return View(richiesta);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Approva(Guid id)  
        {
            Console.WriteLine("ID ricevuto: " + id);
            try
            {
                // Verifica se l'id è valido
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "ID non valido." });
                }

                var success = await _sharedService.UpdateStatus(id, "Approvata");
                if (!success)
                {
                    return BadRequest(new { success = false, message = "Impossibile approvare la richiesta." });
                }

                var richiesta = await _dbContext.Requests.FindAsync(id);
                if (richiesta == null)
                {
                    return BadRequest(new { success = false, message = "Richiesta non trovata." });
                }

                // Assicurati che l'email della richiesta non sia null o vuota
                if (string.IsNullOrEmpty(richiesta.UserName))
                {
                    return BadRequest(new { success = false, message = "L'email dell'utente non è presente nella richiesta." });
                }

                // Cerca l'utente tramite la sua email
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == richiesta.UserName);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Utente non trovato per questa richiesta." });
                }

                // Verifica che l'utente abbia eventi e che la lista non sia null
                if (user.Events == null)
                {
                    return BadRequest(new { success = false, message = "L'utente non ha eventi registrati." });
                }

                // Controllo se un evento simile è già presente per l'utente
                var existingEvent = user.Events
                    .FirstOrDefault(e => e.DataRichiesta == DateTime.Now &&
                                         e.Tipologia == richiesta.Tipologia &&
                                         e.Stato == richiesta.Stato);

                if (existingEvent != null)
                {
                    return BadRequest(new { success = false, message = "Un evento simile è già stato registrato." });
                }

                // Aggiungi l'evento per l'utente corrispondente
                DataGenerator.AddEventForUser(_dbContext, richiesta);

                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante approvazione richiesta: " + ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Rifiuta(Guid id)
        {
            var success = await _sharedService.UpdateStatus(id, "Rifiutata");
            if (!success)
            {
                return BadRequest(new { success = false, message = "Errore durante operazione" });
            }
            return Json(new { success = true });
        }


        [HttpGet]
        public virtual async Task<IActionResult> Lista()
        {
            var richieste = await _dbContext.Requests.ToListAsync(); 
            return Json(richieste);
        }


        //[HttpGet]
        //public virtual async Task<IActionResult> RichiesteDipendenti()
        //{
        //    try
        //    {
        //        var userName = User.Identity.Name;
        //        var utenteLoggato = await _sharedService.GetUserByName(userName);

        //        if(utenteLoggato == null || !string.Equals(utenteLoggato.Role, "Manager", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return Unauthorized();
        //        }

        //        var richieste = await _sharedService.GetRequestByTeam(utenteLoggato.TeamName);

        //        var model = new RichiesteDipendentiViewModel
        //        {
        //            Richieste = richieste
        //        };
        //        return View(model);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("errore durante recupero delle richieste del team: " + ex.Message);
        //        TempData["ErrorMessage"] = "Si è verificato un errore durante il recupero delle richieste del team.";
        //        return RedirectToAction("Richiesta");

        //    }
        //}


        //[HttpGet]
        //public virtual async Task<IActionResult> RichiestaPage()
        //{
        //    try
        //    {
        //        var richiesteEsistenti = await _sharedService.GetAllRequests();
        //        var userName = User.Identity.Name;
        //        var teamName = await _dbContext.Users
        //                                             .Where(u => u.Email == User.Identity.Name)
        //                                             .Select(u => u.TeamName)
        //                                             .FirstOrDefaultAsync();
        //        var model = new RichiestaViewModel
        //        {
        //            Richieste = richiesteEsistenti.Select(r => new RichiestaViewModel
        //            {
        //                Id = r.Id,
        //                Tipologia = r.Tipologia,
        //                DataInizio = r.DataInizio,
        //                DataFine = r.DataFine,
        //                OraInizio = r.OraInizio,
        //                OraFine = r.OraFine,
        //                Stato = r.Stato,
        //                Role = r.Role,
        //                TeamName = teamName
        //            }).ToList(),

        //            TeamName = teamName ?? "Unknown"
        //        };

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Errore durante il caricamento della pagina Richiesta: " + ex.Message);
        //        TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento della pagina.";
        //        return RedirectToAction("Richiesta");
        //    }
        //}
    }
}
