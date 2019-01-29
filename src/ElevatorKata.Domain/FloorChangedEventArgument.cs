
namespace ElevatorKata.Domain
{
    public class FloorChangedEventArgument
    {        
        public FloorChangedEventArgument(int previousFloor, int currentFloor, string description)
        {
            PreviousFloor = previousFloor;
            CurrentFloor = currentFloor;
            Description = description;
        }

        public int PreviousFloor { get; }
        public int CurrentFloor { get; }
        public string Description { get; }

        public static FloorChangedEventArgument Create(int previousFloor, int currentFloor, string description)
        {
            return new FloorChangedEventArgument(previousFloor, currentFloor, description);
        }
    }
}