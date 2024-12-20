using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Template.Web.Features.Richiesta
{
    public class RichiestaViewModel
    {
        // ID della richiesta (generato automaticamente, non visibile all'utente)
        [Key]
        public int Id { get; set; }

        // Utente che ha fatto la richiesta 
        [Required]
        [Display(Name = "Utente")]
        public int UtenteName { get; set; }

        // Tipologia della richiesta (permessi o ferie)
        [Required]
        [Display(Name = "Tipologia")]
        public string Tipologia { get; set; }

        // Data di inizio del permesso/ferie
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Inizio")]
        public DateTime DataInizio { get; set; }

        // Data di fine del permesso/ferie
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Fine")]
        public DateTime DataFine { get; set; }

        // Ora di inizio del permesso/ferie (opzionale per casi di permessi orari)
        [DataType(DataType.Time)]
        [Display(Name = "Ora Inizio")]
        public TimeSpan? OraInizio { get; set; }

        // Ora di fine del permesso/ferie (opzionale per casi di permessi orari)
        [DataType(DataType.Time)]
        [Display(Name = "Ora Fine")]
        public TimeSpan? OraFine { get; set; }

        // Stato della richiesta
        [Required]
        [Display(Name = "Stato")]
        public string Stato { get; set; } = "Da Approvare";



    }
}
