using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Threading.Tasks;
using Template.Infrastructure;
using Template.Services.Shared;
using Template.Web.Infrastructure;

namespace Template.Web.Features.Richiesta
{
    public partial class RichiestaController : Controller
    {
        private readonly SharedService _sharedService;

        // Costruttore che inietta il servizio SharedService
        public RichiestaController(SharedService sharedService)
        {
            _sharedService = sharedService ?? throw new ArgumentNullException(nameof(sharedService));
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

                    // Crea un oggetto Request
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

                    // Salva la richiesta usando il tuo servizio
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


    }
 }
