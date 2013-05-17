using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class Shot
    {
        public Player Player { get; set; }
        public Round Round { get; set; }
        public Hole Hole { get; set; }
        public int ShotNum { get; set; }
        public string ShotLocation { get; set; }
        public int DistanceTraveled { get; set; }
        public int DistanceToPin { get; set; }
        public string VirtualLocation { get; set; }
        public string VirtualDistance { get; set; }
        public string TemplateData { get; set; }
    }
}
