namespace Utilities.General
{
    public interface IEnable
    {
        bool Enabled { get; set; }

        void Enable();
        void Disable();
        void Toggle();
    }
}
