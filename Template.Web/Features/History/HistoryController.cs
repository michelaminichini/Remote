using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Services.Shared;
using Template.Web.Features.History;
using System.Linq;
using System.Threading.Tasks;
using Template.Services;
using System;
using Microsoft.Extensions.Logging;

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
            var userEmail = User.Identity.Name; // Get user email

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

            // Retrieves all user requests from history
            var allData = await _dbContext.Users
                .Where(x => x.Email == userEmail)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.TeamName,
                    x.Role,
                    x.Img,
                    Events = x.Events.Select(e => new
                    {
                        e.DataRichiesta,
                        e.Tipologia,
                        e.DataInizio,
                        e.DataFine,
                        e.Durata,
                        e.Stato
                    }).ToList()
                })
                .ToListAsync();

            // Create the template for the view
            var model = allData.Select(userData => new HistoryViewModel
            {
                Nome = userData.FirstName,
                NomeTeam = userData.TeamName,
                Ruolo = userData.Role,
                Email = userEmail,
                Img = userData.Img,
                Events = userData.Events.Select(e => new Event
                {
                    Tipologia = e.Tipologia,
                    DataRichiesta = e.DataRichiesta ?? (DateTime?)null, // Handle null values for nullable DateTime
                    DataInizio = e.DataInizio ?? (DateTime?)null,
                    DataFine = e.DataFine ?? (DateTime?)null,
                    Durata = e.Durata,
                    Stato = e.Stato
                }).ToList()
            }).ToList();

            return View(model);
        }
    }
}
