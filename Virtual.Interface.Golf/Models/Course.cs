using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Virtual.Interface.Golf.Models
{
    public class Course
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string CourseGpsLocation { get; set; }
        public double CourseGpsHeading { get; set; }
        public List<Hole> Holes { get; set; }
    }
}
