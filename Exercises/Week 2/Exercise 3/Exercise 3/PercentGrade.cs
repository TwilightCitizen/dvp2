using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_3
{
    public class PercentGrade
    {
        // Sane Minimum and Maximum Percent Grade Values.
        public const decimal MinPercent = 0M;
        public const decimal MaxPercent = 100M;

        // The Percent Grade's Value, defaults to minimum.
        private decimal value = MinPercent;

        // Letter Grades and their Percent Grade Floor.
        private static List< ( string, decimal ) > letterGrades = new List< ( string, decimal ) > ()
        {
            ( "A", 89.5M )
        ,   ( "B", 79.5M )
        ,   ( "C", 72.5M )
        ,   ( "D", 69.5M )
        ,   ( "F", 0M    )
        };

        // Apply sensible update controls over Value.
        public  decimal Value
        {
            // Defaults to zero, so no problem
            get { return this.value; }

            // Could be beyond minimum or maximum.
            set
            {
                // When within range,
                if( value >= MinPercent && value <= MaxPercent )
                    // store it.
                    this.value = value;
                else
                    // Panic otherwise.
                    throw new Exception( $"Percent Grade Out of Range ({value})" );
            }
        }

        // Convert to Letter Grade.
        public  string Letter
        {
            // Return the first Letter Grade where the Percent Grade is at or above
            // the associated Percent Grade Floor.
            get => letterGrades.Where( pair => this.value >= pair.Item2 ).First().Item1;
        }

        // Simple Constructor.
        public PercentGrade( decimal value )
        {
            this.Value = value;
        }
    }
}
