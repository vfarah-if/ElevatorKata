using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ElevatorKata.Domain
{
    public class Elevator
    {
        private readonly IClock clock;

        public Elevator(IDictionary<int, string> supportedFloors, IClock clock, int currentFloor = 0)
        {
            if (supportedFloors == null)
            {
                throw new ArgumentNullException(nameof(supportedFloors));
            }

            if (supportedFloors.Count == 0)
            {
                throw new ArgumentException(Errors.FloorsEmpty, nameof(supportedFloors));
            }

            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));

            Floors = supportedFloors.ToImmutableSortedDictionary();
            CurrentFloor = Floors.ContainsKey(currentFloor) ? currentFloor : Floors.First().Key;
                UpdateElevatorState(ElevatorState.StoppedWithDoorClosed);
        }

        public int CurrentFloor { get; private set; }
        public ElevatorState States { get; private set; }
        public IImmutableDictionary<int, string> Floors { get; }

        public EventHandler<FloorChangedEventArgument> FloorChanged;
        public EventHandler StateChanged;

        public Direction GoTo(int targetFloor)
        {
            if (!Floors.ContainsKey(targetFloor))
            {
                throw new ArgumentOutOfRangeException();
            }

            var result = Direction.None;
            try
            {
                CloseTheDoor();

                if (IsMovingUp(targetFloor))
                {
                    result = Direction.Up;
                    UpdateElevatorState(ElevatorState.MovingWithDoorClosed);
                    MoveElevatorUpwards(targetFloor);
                }

                if (IsMovingDown(targetFloor))
                {
                    result = Direction.Down;
                    UpdateElevatorState(ElevatorState.MovingWithDoorClosed);
                    MoveElevatorDownwards(targetFloor);
                }
            }
            finally
            {
                UpdateElevatorState(ElevatorState.StoppedWithDoorClosed);
                OpenTheDoor();
            }
            
            return result;
        }

        private bool IsMovingUp(int targetFloor)
        {
            return CurrentFloor < targetFloor;
        }

        private bool IsMovingDown(int targetFloor)
        {
            return CurrentFloor > targetFloor;
        }

        protected virtual void OnFloorChanged(FloorChangedEventArgument currentFloorArgument)
        {
            clock.PauseFor(TimeSpan.FromSeconds(5));
            var handler = FloorChanged;
            handler?.Invoke(this, currentFloorArgument);
        }

        private void MoveElevatorDownwards(int targetFloor)
        {
            for (var i = CurrentFloor - 1; i >= targetFloor; i--)
            {
                if (!Floors.ContainsKey(i)) continue;

                CurrentFloor = i;
                OnFloorChanged(FloorChangedEventArgument.Create(CurrentFloor, Floors[CurrentFloor], Direction.Down));
            }
        }

        private void MoveElevatorUpwards(int targetFloor)
        {
            for (var i = CurrentFloor + 1; i <= targetFloor; i++)
            {
                if (!Floors.ContainsKey(i)) continue;

                CurrentFloor = i;
                OnFloorChanged(FloorChangedEventArgument.Create(CurrentFloor, Floors[CurrentFloor], Direction.Up));
            }
        }

        public bool OpenTheDoor()
        {
            if ((States & ElevatorState.Moving) == ElevatorState.Moving) return false;
            clock.PauseFor(TimeSpan.FromSeconds(3));
            UpdateElevatorState(ElevatorState.StoppedWithDoorOpened);
            return true;
        }

        private void UpdateElevatorState(ElevatorState elevatorState)
        {
            States = elevatorState;
            OnStatedChanged();
        }

        private void OnStatedChanged()
        {
            var handler = StateChanged;
            handler?.Invoke(this, EventArgs.Empty);            
        }

        private void CloseTheDoor()
        {
            States = ElevatorState.StoppedWithDoorClosed;
        }
    }
}
