using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class ShotItem
    {
        public string Description { get; set; }
        public string Template { get; set; }
        public List<Shot> Shots { get; set; }
        public string Type { get; set; }
    }
}
