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
            CurrentFloor = currentFloor;
            Floors = supportedFloors.ToImmutableSortedDictionary();
        }

        public int CurrentFloor { get; private set; }
        public IImmutableDictionary<int, string> Floors { get; private set; }

        public void GoTo(int floor)
        {
            CurrentFloor = floor;
        }
    }
}
