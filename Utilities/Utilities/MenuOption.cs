using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.General;

namespace Utilities.Terminal
{
    public abstract class MenuOption : IRun< bool >, IEnable, ISignal, IDisplay, IParent< SuperMenu >
    {
        public SuperMenu Parent { get; set; }

        public string Text      { get; set; }

        public string Number    { get; set; }
        public string Letter    { get; set; }

        public virtual bool Run() { return false; }

        public bool Enabled     { get; set; }

        public void Disable() => Enabled = false;
        public void Enable()  => Enabled = true;
        public void Toggle()  => Enabled = !Enabled;

        public List< Func< bool > > Conditions { get; set; }

        public bool Signal
        {
            get { return Conditions?.All( cond => cond() == true ) ?? true; }
        }

        public void Display()
        {
            DisplayNumber();
            
            if( Letter == null ) DisplayTextOnly(); else DisplayLetterAndText();
        }

        private void DisplayNumber()
        {
            Console.ForegroundColor = Enabled && Signal ? Parent.BracketColorEnabled
                                                        : Parent.BracketColorDisabled;
            Console.WriteLine();
            Console.Write( "[" );

            Console.ForegroundColor = Enabled && Signal ? Parent.NumberColorEnabled
                                                        : Parent.NumberColorDisabled;
            Console.Write( Number );

            Console.ForegroundColor = Enabled && Signal ? Parent.BracketColorEnabled
                                                        : Parent.BracketColorDisabled;
            Console.Write( "]" );
            Console.Write( " - " );
        }

        private void DisplayTextOnly()
        {
            Console.ForegroundColor = Enabled && Signal ? Parent.TextColorEnabled
                                                        : Parent.TextColorDisabled;
            Console.Write( Text );
        }

        private void DisplayLetterAndText()
        {
                int letterIndex = Text.ToUpper().IndexOf( Letter );

                Console.ForegroundColor = Enabled && Signal ? Parent.TextColorEnabled
                                                            : Parent.TextColorDisabled;

                Console.Write( Text.Substring( 0, letterIndex     ) );

                Console.ForegroundColor = Enabled && Signal ? Parent.BracketColorEnabled
                                                            : Parent.BracketColorDisabled;
                Console.Write( "[" );

                Console.ForegroundColor = Enabled && Signal ? Parent.LetterColorEnabled
                                                            : Parent.LetterColorDisabled;
                Console.Write( Text.Substring(    letterIndex,  Letter.Length ) );

                Console.ForegroundColor = Enabled && Signal ? Parent.BracketColorEnabled
                                                            : Parent.BracketColorDisabled;
                Console.Write( "]" );

                Console.ForegroundColor = Enabled && Signal ? Parent.TextColorEnabled
                                                            : Parent.TextColorDisabled;
                Console.Write( Text.Substring(    letterIndex + Letter.Length ) );
        }

        public MenuOption( string text, bool enabled, List< Func< bool > > conditions )
        {
            this.Text       = text;
            this.Enabled    = enabled;
            this.Conditions = conditions;
        }
    }
}
