using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Template.Services;
using Template.Services.Shared;


namespace Template.Web.Features.Team {
    public partial class TeamController : Controller
    {
        private readonly TemplateDbContext _context;

        public TeamController(TemplateDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public virtual async Task<IActionResult> Team()
        {
            var teams = await _context.Teams
                .Include(t => t.Employee)
                .Select(x => new
                {
                    x.TeamName,
                    x.TeamManager,
                    Employees = x.Employee.Select(e => new { e.FirstName, e.LastName })
                })
                .ToListAsync();

            var model = teams.Select(data => new TeamViewModel
            {
                TeamName = data.TeamName,
                TeamManager = data.TeamManager,
                Employee = data.Employees.Select(e => new User
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName
                }).ToList()
            }).ToList();

            if (model == null || !model.Any())
            {
                return NotFound("No teams found.");
            }

            // Send data to the view as JSON
            ViewData["TeamsData"] = JsonConvert.SerializeObject(teams);

            return View();
        }

    }

}