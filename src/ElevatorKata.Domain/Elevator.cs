using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ElevatorKata.Domain
{
    public class Elevator
    {
        public Elevator(IDictionary<int, string> supportedFloors, int currentFloor = 0)
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

        public EventHandler<FloorChangedEventArgument> FloorChanged;

        public Direction GoTo(int targetFloor)
        {            
            if (!Floors.ContainsKey(targetFloor))
            {
                throw new ArgumentOutOfRangeException();
            }

            var result = Direction.None;

            if (CurrentFloor < targetFloor)
            {
                result = Direction.Up;
                MoveElevatorUpwards(targetFloor);
            }

            if (CurrentFloor > targetFloor)
            {
                result = Direction.Down;
                MoveElevatorDownwards(targetFloor);
            }

            return result;
        }

        private void MoveElevatorDownwards(int targetFloor)
        {
            for (var i = CurrentFloor - 1; i >= targetFloor; i--)
            {
                if (!Floors.ContainsKey(i)) continue;
                var previousFloor = CurrentFloor;
                CurrentFloor = i;
                OnFloorChanged(FloorChangedEventArgument.Create(previousFloor, CurrentFloor, Floors[CurrentFloor]));
            }
        }

        private void MoveElevatorUpwards(int targetFloor)
        {
            for (var i = CurrentFloor + 1; i <= targetFloor; i++)
            {
                if (!Floors.ContainsKey(i)) continue;
                var previousFloor = CurrentFloor;
                CurrentFloor = i;
                OnFloorChanged(FloorChangedEventArgument.Create(previousFloor, CurrentFloor, Floors[CurrentFloor]));
            }
        }

        protected virtual void OnFloorChanged(FloorChangedEventArgument currentFloorArgument)
        {
            var handler = FloorChanged;
            handler?.Invoke(this, currentFloorArgument);
        }
    }
}
