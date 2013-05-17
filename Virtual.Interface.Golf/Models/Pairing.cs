using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Virtual.Interface.Golf.Models
{
    public class Pairing
    {
        public int PairingID { get; set; }
        public int CourseID { get; set; }
        public DateTime TeeTime { get; set; }
        public string FormattedTeeTime { get; set; }
        public int StartHole { get; set; }
        public int Round { get; set; }
        public string OnCourse { get; set; }
        public List<Player> Players { get; set; }
        public string PlayerNames { get; set; }
    }
}
