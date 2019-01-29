using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ElevatorKata.Domain
{
    public class Elevator
    {
        public Elevator(Dictionary<int, string> supportedFloors, int currentFloor = 0)
        {
            if (supportedFloors == null)
            {
                throw new ArgumentNullException(nameof(supportedFloors));
            }

            if (supportedFloors.Count == 0)
            {
                throw new ArgumentException(Errors.FloorsEmpty, nameof(supportedFloors));
            }
                
            Floors = supportedFloors.ToImmutableSortedDictionary();
            CurrentFloor = Floors.ContainsKey(currentFloor) ? currentFloor : Floors.First().Key;
        }

        public int CurrentFloor { get; private set; }
        public IImmutableDictionary<int, string> Floors { get; }

        public Direction GoTo(int floor)
        {
            var result = Direction.None;
            ValidateFloorExists(floor);
            if (CurrentFloor < floor)
            {
                result = Direction.Up;
            }

            if (CurrentFloor > floor)
            {
                result = Direction.Down;
            }
            CurrentFloor = floor;
            return result;
        }

        private void ValidateFloorExists(int floor)
        {
            if (!Floors.ContainsKey(floor))
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
