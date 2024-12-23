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

    public class EventViewModel
    {
        public string IconPath { get; set; }
        public string EventType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
