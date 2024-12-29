using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
                // Recupera tutte le richieste dal database usando SharedService
                var richieste = await _sharedService.GetAllRequests();
                //var richieste = DataGenerator.Requests;

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
   
                    //var newRequest = new Request
                    //{
                    //    Id = Guid.NewGuid(),
                    //    UserName = User.Identity.Name,
                    //    Tipologia = richiesta.Tipologia,
                    //    DataInizio = richiesta.DataInizio,
                    //    DataFine = richiesta.DataFine,
                    //    OraInizio = richiesta.OraInizio ?? TimeSpan.Zero,
                    //    OraFine = richiesta.OraFine ?? TimeSpan.Zero,
                    //    Stato = richiesta.Stato
                    //};

                    // Salva la richiesta
                    var id = await _sharedService.HandleRequest(cmd);
                    //DataGenerator.AddRequest(newRequest);

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

            // In caso di errore o se il modello non è valido, ritorna alla vista con il messaggio di errore
            return View(richiesta);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Approva(Guid id)  // id viene passato come parametro di query
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
                return BadRequest(new { success = false, message = "Impossibile rifiutare la richiesta." });
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
