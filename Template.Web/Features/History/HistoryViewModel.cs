using System;

namespace Template.Web.Features.History
{
    public class HistoryViewModel
    {
        public string Nome { get; set; }
        public string NomeTeam { get; set; }
        public string Ruolo { get; set; }
        public string Email { get; set; }

        public DateTime? DataRichiesta { get; set; }
        public string Tipologia { get; set; }
        public DateTime? DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public string Durata { get; set; }

       
    }

}
