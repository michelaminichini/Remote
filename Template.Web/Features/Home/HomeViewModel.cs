using System.Collections.Generic;
using System;
using Template.Infrastructure;

namespace Template.Web.Features.Home
{
    public class HomeViewModel
    {
        public string CurrentMonthName { get; set; }
        public int CurrentYear { get; set; }
        public int CurrentMonth { get; set; }
        public List<List<DayViewModel>> Weeks { get; set; }
        public string UserProfileImage { get; set; }
        public string UserEmail { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}

