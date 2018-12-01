using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_3
{
    public static class ListExtensionMethods
    {
        // Here's a way to get GPA from a List of TakenCourses without
        // making TakenCourses a class that inherits from List< TakenCourse >.
        public static decimal ToGPA( this List< TakenCourse > takenCourses )
        {
            var points = new Dictionary< string, decimal>()
            {
                [ "A" ] = 4M
            ,   [ "B" ] = 3M
            ,   [ "C" ] = 2M
            ,   [ "D" ] = 1M
            ,   [ "F" ] = 0M
            };

            // GPA is the sum of points per Letter Grade over Courses Taken.
            return takenCourses.Select( tc => points[ tc.Grade.Letter ] ).Sum() / takenCourses.Count;
        }
    }
}
