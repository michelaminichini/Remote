using System.Collections.Generic;
using Template.Services.Shared;

namespace Template.Web.Features.Richiesta
{
    public class RichiestaPageViewModel
    {
        public List<RichiestaViewModel> Richieste { get; set; }
        public string TeamName { get; set; }
    }

}
