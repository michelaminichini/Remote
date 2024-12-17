using System;
using System.Collections.Generic;

namespace Template.Infrastructure
{
    public class Calendar
    {
        public static List<List<DayViewModel>> GetWeeksInMonth(int year, int month)
        {
            var weeks = new List<List<DayViewModel>>();
            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Trova il primo lunedì della settimana del primo giorno del mese
            var startOfWeek = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek + (int)DayOfWeek.Monday);
            if (startOfWeek > firstDayOfMonth)
            {
                startOfWeek = startOfWeek.AddDays(-7); // Se il primo lunedì è dopo il 1° del mese, torna indietro di una settimana
            }

            var currentDay = startOfWeek;

            while (currentDay <= lastDayOfMonth)
            {
                var week = new List<DayViewModel>();

                // Crea la settimana da lunedì a domenica
                for (int i = 0; i < 7; i++)
                {
                    bool isCurrentMonth = currentDay.Month == month;

                    week.Add(new DayViewModel
                    {
                        Date = currentDay,
                        IsCurrentMonth = isCurrentMonth,
                        Events = new List<string>()
                    });

                    currentDay = currentDay.AddDays(1);
                }

                weeks.Add(week);
            }

            return weeks;
        }
    }

    public class DayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; } // Aggiunto per determinare se il giorno è nel mese corrente
        public List<string> Events { get; set; }
    }
}
