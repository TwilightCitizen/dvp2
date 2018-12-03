using System.Collections.Generic;

namespace TimeTracker
{
    public class User
    {
        public int    ID            { get; set; }
        public string FirstName     { get; set; }
        public string LastName      { get; set; }
        public string Password      { get; set; }
        public List< LogEntry > Log { get; set; } = new List< LogEntry >();
    }
}
