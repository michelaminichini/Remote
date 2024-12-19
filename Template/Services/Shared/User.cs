using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace Template.Services.Shared
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string TeamName { get; set; }

        public DateTime? DataRichiesta { get; set; }
        public string Tipologia { get; set; }
        public DateTime? DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public TimeSpan? Durata { get; set; }
        public string Stato { get; set; }

        public string Img { get; set; } // Campo per immagine profilo

        public bool IsMatchWithPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var sha256 = SHA256.Create();
            var testPassword = System.Convert.ToBase64String(sha256.ComputeHash(Encoding.ASCII.GetBytes(password)));

            return this.Password == testPassword;
        }
    }
}
