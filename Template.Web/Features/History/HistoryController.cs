using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Services.Shared;
using Template.Web.Features.History;
using System.Linq;
using System.Threading.Tasks;
using Template.Services;

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
                return NotFound("Email dell'utente non trovata.");
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
                DataRichiesta = data.DataRichiesta,
                Tipologia = data.Tipologia,
                DataInizio = data.DataInizio,
                DataFine = data.DataFine,

                // Calcola la durata in ore
                Durata = (data.DataFine - data.DataInizio).TotalHours.ToString("F2") // La durata in ore come stringa
            }).ToList();

            return View(model); // Restituisce la vista con il modello
        }
    }
}
