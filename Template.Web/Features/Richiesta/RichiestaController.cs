﻿using Microsoft.AspNetCore.Authorization;
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
        public virtual async Task<IActionResult> RichiesteInArrivo()
        {
            try
            {
                // Retrieve the team name of the logged-in user
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
                    richieste = await _sharedService.GetManagerRequest(teamName);
                }
                else if (userRole == "CEO")
                {
                    richieste = await _sharedService.GetAllRequests();
                }
                else
                {
                    richieste = await _sharedService.GetUserRequest(user.Email); // For user with role = "Dipendente"
                }
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
                    var userName = User.Identity.Name;
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userName);
                    var userRole = user.Role;

                    AddRequestCommand cmd;
                    if (userRole == "CEO")
                    {
                        cmd = new AddRequestCommand
                        {
                            UserName = User.Identity.Name,
                            Tipologia = richiesta.Tipologia,
                            DataInizio = richiesta.DataInizio,
                            DataFine = richiesta.DataFine,
                            OraInizio = richiesta.OraInizio ?? TimeSpan.Zero,
                            OraFine = richiesta.OraFine ?? TimeSpan.Zero,
                            Stato = "Accettata"
                        };
                    }
                    else
                    {
                        cmd = new AddRequestCommand
                        {
                            UserName = User.Identity.Name,
                            Tipologia = richiesta.Tipologia,
                            DataInizio = richiesta.DataInizio,
                            DataFine = richiesta.DataFine,
                            OraInizio = richiesta.OraInizio ?? TimeSpan.Zero,
                            OraFine = richiesta.OraFine ?? TimeSpan.Zero,
                            Stato = richiesta.Stato
                        };
                    }
               
   
                    var id = await _sharedService.HandleRequest(cmd);


                        TempData["Message"] = "Richiesta inviata con successo!";
                    return RedirectToAction("Richiesta");
                }
                catch (Exception ex)
                {
                    // Manage the error and add an error message
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
        private async Task<IActionResult> ApproveOrRejectRequest(Guid id, string newStatus)
        {
            Console.WriteLine($"ID ricevuto: {id}");
            try
            {
                // Check if the id is valid
                if (id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "ID non valido." });
                }

                var success = await _sharedService.UpdateStatus(id, newStatus);
                if (!success)
                {
                    return BadRequest(new { success = false, message = "Impossibile aggiornare lo stato della richiesta." });
                }

                var richiesta = await _dbContext.Requests.FindAsync(id);
                if (richiesta == null)
                {
                    return BadRequest(new { success = false, message = "Richiesta non trovata." });
                }

                // Make sure the request email is not null or empty
                if (string.IsNullOrEmpty(richiesta.UserName))
                {
                    return BadRequest(new { success = false, message = "L'email dell'utente non è presente nella richiesta." });
                }

                // Search the user via his/her email
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == richiesta.UserName);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Utente non trovato per questa richiesta." });
                }

                // Verify that the user has events and that the list is not null
                if (user.Events == null)
                {
                    return BadRequest(new { success = false, message = "L'utente non ha eventi registrati." });
                }

                // Check if a similar event is already present for the user
                var existingEvent = user.Events
                    .FirstOrDefault(e => e.DataRichiesta == DateTime.Now &&
                                         e.Tipologia == richiesta.Tipologia &&
                                         e.Stato == richiesta.Stato);

                if (existingEvent != null)
                {
                    return BadRequest(new { success = false, message = "Un evento simile è già stato registrato." });
                }

                // Add the event for the corresponding user
                DataGenerator.AddEventForUser(_dbContext, richiesta);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante {newStatus.ToLower()} richiesta: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Approva(Guid id)
        {
            return await ApproveOrRejectRequest(id, "Accettata");
        }

        [HttpPost]
        public virtual async Task<IActionResult> Rifiuta(Guid id)
        {
            return await ApproveOrRejectRequest(id, "Rifiutata");
        }


        [HttpGet]
        public virtual async Task<IActionResult> Lista()
        {
            var userName = User.Identity.Name;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userName);
            var userRole = user.Role;
            var teamName = user.TeamName;


            List<Request> richieste;
            if (userRole == "Manager")
            {
                richieste = await _sharedService.GetManagerRequest(teamName);
            }
            else if (userRole == "CEO")
            {
                richieste = await _sharedService.GetAllRequests();
            }
            else
            {
                richieste = await _sharedService.GetUserRequest(user.Email);
            }
            return Json(richieste);
        }
    }
}
