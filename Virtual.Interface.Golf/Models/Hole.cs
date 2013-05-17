using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class Hole
    {
        public int HoleID { get; set; }
        public int HoleNum { get; set; }
        public int Par { get; set; }
        public int Yardage { get; set; }
        public string TeeLocation { get; set; }
        public string PinLocation { get; set; }
        public string ApproachLocation { get; set; }
    }
}
