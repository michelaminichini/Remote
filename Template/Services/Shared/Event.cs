using System;

namespace Template.Services.Shared
{
    public class Event
    {
        public Guid EventId { get; set; }
        public string Tipologia { get; set; }  // Es: Permesso, Ferie, Trasferta, etc.
        public DateTime? DataRichiesta { get; set; }
        public DateTime? DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public TimeSpan? Durata { get; set; }
        public string Stato { get; set; } // Stato dell'evento (Accettata, Rifiutata, etc.)
        public string LogoPath { get; set; } // Percorso relativo al logo
        public string TeamName { get; set; }
    }

    public class EventIconViewModel //per visualizzare nome user quando passa freccia su evento
    {
        public string Icon { get; set; }
        public string UserName { get; set; }
    }

}
