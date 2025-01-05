using System;
using System.ComponentModel.DataAnnotations;

namespace Template.Web.Features.Richiesta
{
    public class RichiestaViewModel
    {
        // Request ID (automatically generated, the user can't see it)
        [Key]
        public Guid Id { get; set; }

        // Type of request (permessi/ferie)
        [Required]
        [Display(Name = "Tipologia")]
        public string Tipologia { get; set; }

        // Start date
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Inizio")]
        public DateTime DataInizio { get; set; }

        // End date
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Fine")]
        public DateTime DataFine { get; set; }

        // Start time
        [DataType(DataType.Time)]
        [Display(Name = "Ora Inizio")]
        public TimeSpan? OraInizio { get; set; }

        // End time
        [DataType(DataType.Time)]
        [Display(Name = "Ora Fine")]
        public TimeSpan? OraFine { get; set; }

        [Display(Name = "Durata")]
        public TimeSpan Durata { get; set; }

        // Request status
        [Required]
        [Display(Name = "Stato")]
        public string Stato { get; set; } = "Da Approvare";



    }
}
