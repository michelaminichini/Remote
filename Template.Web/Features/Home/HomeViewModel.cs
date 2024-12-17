using System.Collections.Generic;
using Template.Infrastructure;

namespace Template.Web.Features.Home
{
    public class HomeViewModel
    {
        public string CurrentMonthName { get; set; }
        public int CurrentYear { get; set; }
        public List<List<DayViewModel>> Weeks { get; set; }
        public string UserProfileImage { get; set; } // Percorso immagine profilo
        public string UserEmail { get; set; } // visualizzata con logout
    }
}
