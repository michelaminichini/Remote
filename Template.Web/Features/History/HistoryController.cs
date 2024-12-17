using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Services.Shared;
using Template.Web.Features.History;
using System.Linq;
using System.Threading.Tasks;
using Template.Services;
using System;

namespace Template.Web.Features.History
{
    public partial class HistoryController : Controller
    {
        private readonly TemplateDbContext _dbContext;

        public HistoryController(TemplateDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async virtual Task<IActionResult> History()
        {
            var userEmail = User.Identity.Name; // Ottieni l'email dell'utente loggato

            if (string.IsNullOrEmpty(userEmail))
            {
                Console.WriteLine("User.Identity.Name è null o vuoto");
            }
            else
            {
                Console.WriteLine($"User.Identity.Name: {userEmail}");
            }

            var user = await _dbContext.Users
                .Where(x => x.Email == userEmail)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound("Utente non trovato");
            }

            // Recupera tutte le richieste dell'utente dallo storico
            var allData = await _dbContext.Users
                .Where(x => x.Email == userEmail)
                .Select(x => new
                {
                    x.Email,
                    x.FirstName,
                    x.TeamName,
                    x.Role,
                    x.DataRichiesta,
                    x.Tipologia,
                    x.DataInizio,
                    x.DataFine,
                    x.Durata
                })
                .ToListAsync();
            
            // Crea il modello per la vista
            var model = allData.Select(data => new HistoryViewModel
            {
                Nome = data.FirstName,
                NomeTeam = data.TeamName,
                Ruolo = data.Role,
                Email = data.Email,
                DataRichiesta = data.DataRichiesta != DateTime.MinValue ? data.DataRichiesta : (DateTime?)null,
                Tipologia = data.Tipologia,
                DataInizio = data.DataInizio != DateTime.MinValue ? data.DataInizio : (DateTime?)null,
                DataFine = data.DataFine != DateTime.MinValue ? data.DataFine : (DateTime?)null,

                // Calcola la durata in ore
                Durata = (data.DataInizio != DateTime.MinValue && data.DataFine != DateTime.MinValue)
             ? (data.DataFine - data.DataInizio).TotalHours.ToString("F2") : "N/A"

            }).ToList();

            return View(model); // Restituisce la vista con il modello
        }
    }
}
