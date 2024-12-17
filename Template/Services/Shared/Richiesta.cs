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

        //[ForeignKey("User")]
        //public Guid UserId { get; set; }  // Chiave esterna che punta a User
        //public User User { get; set; }    // Proprietà di navigazione

        public string Tipologia { get; set; }  // Tipo di richiesta

        public DateTime DataInizio { get; set; } // Data inizio
        public DateTime DataFine { get; set; }   // Data fine

        public TimeSpan OraInizio { get; set; }  // Ora inizio
        public TimeSpan OraFine { get; set; }    // Ora fine
    }
}
