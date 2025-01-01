using System;

namespace Template.Services.Shared
{
    public class Event
    {
        public Guid EventId { get; set; }
        public string Tipologia { get; set; }  // e.g: Permesso, Ferie, Trasferta, etc.
        public DateTime? DataRichiesta { get; set; }
        public DateTime? DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public TimeSpan? Durata { get; set; }
        public string Stato { get; set; } // Status of the event (Accettata, Rifiutata, etc.)
        public string LogoPath { get; set; }
        public string TeamName { get; set; }
    }

    public class EventIconViewModel // Display username when clicking on an event
    {
        public string Icon { get; set; }
        public string UserName { get; set; }
    }

}
