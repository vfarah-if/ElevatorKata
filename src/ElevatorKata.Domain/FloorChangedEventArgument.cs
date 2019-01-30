
namespace ElevatorKata.Domain
{
    public class FloorChangedEventArgument
    {        
        public FloorChangedEventArgument(int currentFloor, string description, Direction direction)
        {
            CurrentFloor = currentFloor;
            Description = description;
            Direction = direction;
        }

        public int CurrentFloor { get; }
        public string Description { get; }
        public Direction Direction { get; }

        public static FloorChangedEventArgument Create(int currentFloor, string description, Direction direction)
        {
            return new FloorChangedEventArgument(currentFloor, description, direction);
        }
    }
}