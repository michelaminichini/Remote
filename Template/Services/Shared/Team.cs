using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Template.Services.Shared
{
    public class Teams
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string TeamName { get; set; } 
        public string TeamManager { get; set; }
        public List<User> Employee { get; set; }
    }
}
