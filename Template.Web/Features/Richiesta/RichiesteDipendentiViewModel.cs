using System.Collections.Generic;
using Template.Services.Shared;
using Template.Infrastructure;

namespace Template.Web.Features.Richiesta
{
    public class RichiesteDipendentiViewModel
    {
        public List<Event> Richieste { get; set; }
        public string TeamName { get; set; }
    }
}
