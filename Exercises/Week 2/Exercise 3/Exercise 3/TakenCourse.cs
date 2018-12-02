using System;

namespace Exercise_3
{
    // A course actually taken by someone, such as a student.
    public class TakenCourse : Course
    {
        private PercentGrade grade;

        // Simple getters/setters for Taken Course's properties.

        // Grade set is normal, but Grade returned can be auto-zero
        // if the student is flagged for cheating.  In other words,
        // a student get's across-the-board F's and 0.0 GPA until
        // the an audit is complete and the flag is lifted.
        public PercentGrade Grade
        {
            get { return this.Flagged ? new PercentGrade( 0M ) : this.grade; }

            set { this.grade = value; }
        }

        // Flag for cheating.
        public Boolean Flagged { get; set; } = false;

        // Simple Constructor
        public TakenCourse( string name, PercentGrade grade ) : base( name )
        {
            this.Grade = grade;
        }
    }
}
