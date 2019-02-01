
namespace ElevatorKata.Domain
{
    public class FloorChangedEventArgument
    {        
        public FloorChangedEventArgument(Floor currentFloor, ElevatorDirection direction)
        {
            Floor = currentFloor;
            Direction = direction;
        }

        public Floor Floor { get; }
        public ElevatorDirection Direction { get; }

        public static FloorChangedEventArgument Create(Floor currentFloor, ElevatorDirection direction)
        {
            return new FloorChangedEventArgument(currentFloor, direction);
        }
    }
}