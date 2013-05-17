using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public List<Course> Courses { get; set; }
    }
}
