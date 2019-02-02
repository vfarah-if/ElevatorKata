namespace ElevatorKata.Domain
{
    public interface IActivityState
    {
        bool IsActive { get; }
        bool IsEnabled { get; }
        bool CanExecute { get; }

        bool Execute();
    }
}