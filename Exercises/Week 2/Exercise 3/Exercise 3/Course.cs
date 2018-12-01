using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_3
{
    // Named course instead of class to avoid clashing
    // with C# reserved words.
    public class Course
    {
        // Simple getter/setter for Course's single property.
        public string Name { get; set; }

        // Simple Constructor
        public Course( string name )
        {
            this.Name = name;
        }
    }
}
