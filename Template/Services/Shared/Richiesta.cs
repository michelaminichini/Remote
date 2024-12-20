using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Template.Services.Shared
{
    public class Request
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }  // Chiave primaria

        public String UserName { get; set; }  

        public string Tipologia { get; set; }  // Tipo di richiesta

        public DateTime DataInizio { get; set; } // Data inizio
        public DateTime DataFine { get; set; }   // Data fine

        public TimeSpan? OraInizio { get; set; }  // Ora inizio
        public TimeSpan? OraFine { get; set; }    // Ora fine
        public string Stato { get; set; }
    }
}
