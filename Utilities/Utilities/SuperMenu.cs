using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.General;

namespace Utilities.Terminal
{
    public class SuperMenu : List< MenuOption >, IRun< string >, IDisplay
    {
        	public string Prompt { get; set; }
		public string Error  { get; set; }

        public ConsoleColor TextColorEnabled     { get; set; } = ConsoleColor.Gray;
        public ConsoleColor TextColorDisabled    { get; set; } = ConsoleColor.DarkGray;
        public ConsoleColor LetterColorEnabled   { get; set; } = ConsoleColor.Blue;
        public ConsoleColor LetterColorDisabled  { get; set; } = ConsoleColor.DarkRed;
        public ConsoleColor NumberColorEnabled   { get; set; } = ConsoleColor.Blue;
        public ConsoleColor NumberColorDisabled  { get; set; } = ConsoleColor.DarkRed;
        public ConsoleColor BracketColorEnabled  { get; set; } = ConsoleColor.White;
        public ConsoleColor BracketColorDisabled { get; set; } = ConsoleColor.Gray;

        	public SuperMenu( string prompt = "Please choose from the available options."
                        , string error  = "That is not an available option.  Try again." )
		{
			this.Prompt = prompt;
			this.Error  = error;
		}

        public new void Add( MenuOption menuOption )
        {
            menuOption.Parent = this;
            menuOption.Number = ( Count + 1 ).ToString();

            string uniqueLetter = UniqueLetter( menuOption );

            if( !string.IsNullOrEmpty( uniqueLetter ) )
                menuOption.Letter = UniqueLetter( menuOption );

            base.Add( menuOption );
        }

        public new MenuOption this[ int index ]
        {
            get { return base[ index ]; }

            set
            {
                base[ index ].Parent = null;
                value.Parent         = this;
                value.Number         = this[ index ].Number;

                string uniqueLetter = UniqueLetter( value );

                if( !string.IsNullOrEmpty( uniqueLetter ) )
                    value.Letter         = uniqueLetter;

                base[ index ]        = value;
            }
        }

        public new void Insert( int index, MenuOption menuOption)
        {
            menuOption.Parent = this;

            string uniqueLetter = UniqueLetter( menuOption );

            if( !string.IsNullOrEmpty( uniqueLetter ) )
                menuOption.Letter = UniqueLetter( menuOption );

            base.Insert( index, menuOption );
            RenumberMenuOptions();
        }

        public new bool Remove( MenuOption menuOption)
        {
            if( base.Remove( menuOption ) )
            {
                menuOption.Parent = null;

                RenumberMenuOptions();

                return true;
            }

            return false;
        }

        public new void RemoveAt( int index )
        {
            this[ index ].Parent = null;

            base.RemoveAt( index );

            RenumberMenuOptions();
        }

        public new void RemoveRange( int index, int count )
        {
            this.GetRange( index, count ).ForEach( opt => opt.Parent = null );

            base.RemoveRange( index, count );

            RenumberMenuOptions();
        }

        public new void RemoveAll( Predicate< MenuOption > match )
        {
            this.Where( opt => match( opt) ).ToList().ForEach( opt => opt.Parent = null );

            base.RemoveAll( match );

            RenumberMenuOptions();
        }

        public new void Clear()
        {
            this.ForEach( opt => opt.Parent = null );

            base.Clear();
        }

        private void RenumberMenuOptions()
        {
            int number = 0;

            this.ForEach( opt => opt.Number = ( ++number ).ToString() );
        }

        private string UniqueLetter( MenuOption menuOption)
        {
            var existingLetters = this.Where( opt => opt.Letter != null)
                                      .Select( opt => opt.Letter.ToUpper() ).ToList();
            var lettersInOption = new string( menuOption.Text.Where( char.IsLetter ).ToArray() ).ToUpper();

            var chunksInOption  =
                from i in Enumerable.Range( 0, lettersInOption.Length )
                from j in Enumerable.Range( 0, lettersInOption.Length - i + 1 )
                where j >= 1
                orderby lettersInOption.Substring( i, j ).Length
                select lettersInOption.Substring( i, j );

            return chunksInOption.Except( existingLetters )?.FirstOrDefault() ?? null;
        }

        public string Run()
        {
            string     input;
            MenuOption menuOption;

            Console.Clear();
            Console.WriteLine( Prompt );
            Display();
            Console.Write( "\n\nYour choice: " );

            input      = Console.ReadLine().ToUpper();

            menuOption = this.Where( opt => opt.Number                      == input
                                         || ( opt.Letter?.ToUpper() ?? "" ) == input
                                         || opt.Text.ToUpper()              == input ).FirstOrDefault();

            Console.Clear();

            if( menuOption?.Run() ?? false )
                return menuOption.Text;
            else
            {
                Console.WriteLine( Error );
                return null;
            }
        }

        public void Display() => this.ForEach( opt => opt.Display() );
    }
}
