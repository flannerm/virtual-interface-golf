using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class Tournament
    {
        public string TournamentName { get; set; }
        public int CurrentRound { get; set; }
        public string ProjectedCut { get; set; }
        public string TournamentLogo { get; set; }
        public Event Event { get; set; }
    }
}
