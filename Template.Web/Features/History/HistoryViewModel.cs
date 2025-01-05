using System.Collections.Generic;
using Template.Services.Shared;

namespace Template.Web.Features.History
{
    public class HistoryViewModel
    {
        public string Nome { get; set; }
        public string NomeTeam { get; set; }
        public string Ruolo { get; set; }
        public string Email { get; set; }
        public string Img { get; set; }

        public List<Event> Events { get; set; } = new List<Event>(); // Events list
    }

}
