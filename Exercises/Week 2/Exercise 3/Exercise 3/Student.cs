using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_3
{
    // No need to instroduce inheritance from Person since
    // there is no Teacher class in this exercise.
    public class Student
    {
        // Simple Name Getters/Setters.
        public string FirstName { get; set; }
        public string LastName  { get; set; }

        // More exotic Name Getters.
        public string FullName  { get => $"{this.FirstName} {this.LastName}";  }
        public string SortName  { get => $"{this.LastName}, {this.FirstName}"; }

        // Simple Taken Courses Getters/Setters.
        public List< TakenCourse > TakenCourses = new List< TakenCourse >();

        // Simple Constructor.
        public Student( string firstName, string lastName, List< TakenCourse > takenCourses)
        {
            this.FirstName    = firstName;
            this.LastName     = lastName;
            this.TakenCourses = takenCourses;
        }
    }
}
