
namespace ElevatorKata.Domain
{
    public class FloorChangedEventArgument
    {        
        public FloorChangedEventArgument(Floor currentFloor, Direction direction)
        {
            Floor = currentFloor;
            Direction = direction;
        }

        public Floor Floor { get; }
        public Direction Direction { get; }

        public static FloorChangedEventArgument Create(Floor currentFloor, Direction direction)
        {
            return new FloorChangedEventArgument(currentFloor, direction);
        }
    }
}