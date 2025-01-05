using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace Template.Services.Shared
{
    public class Request
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }  // Primary key

        public string UserName { get; set; }  

        public string Tipologia { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }

        public TimeSpan? OraInizio { get; set; }
        public TimeSpan? OraFine { get; set; }
        public TimeSpan? Durata { get; set; }
        public string Stato { get; set; }
        public string LogoPath { get; set; }
        public string Role { get; set; }
    }
}
