using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class Country
    {
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public Uri Flag { get; set; }
        public string Abbrev { get; set; }
    }
}
