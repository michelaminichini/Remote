using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Template.Services.Shared;

namespace Template.Web.Features.Team
{
    public class TeamViewModel
    {
        public string TeamName { get; set; }
        public string TeamManager { get; set; }
        public List<User> Employee { get; set; }

    }
}
