using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ElevatorKata.Domain
{
    public class Elevator
    {
        private readonly IClock clock;
        private readonly List<Floor> floorRequests = new List<Floor>();
        private readonly List<Floor> floors;

        public Elevator(IList<Floor> supportedFloors, IClock clock, int currentFloorNo = 0, string name = "Lift 1")
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
            Name = name;
            floors = supportedFloors.OrderBy(x => x.Number).ToList();
            CurrentFloor = Floors.SingleOrDefault(x => x.Number == currentFloorNo) ?? Floors.First();
            UpdateElevatorState(ElevatorState.StoppedWithDoorClosed);
        }

        public Floor CurrentFloor { get; private set; }
        public ElevatorState States { get; private set; }
        public IReadOnlyList<Floor> Floors => floors.AsReadOnly();
        public bool IsElevatorDoorOpened => (States & ElevatorState.DoorOpened) == ElevatorState.DoorOpened;
        public bool IsElevatorMoving => (States & ElevatorState.Moving) == ElevatorState.Moving;
        public bool IsElevatorDoorClosed => (States & ElevatorState.DoorClosed) == ElevatorState.DoorClosed;
        public bool IsElevatorStopped => (States & ElevatorState.Stopped) == ElevatorState.Stopped;
        public string Name { get; }

        public EventHandler<FloorChangedEventArgument> FloorChanged;
        public EventHandler StateChanged;
        
        public void GoTo(params int[] targetFloorNumbers)
        {
            ValidateFloorsExist(targetFloorNumbers);
           
            if (IsElevatorMoving) return;

            do
            {
                if (floorRequests.Contains(CurrentFloor))
                {
                    StopTheElevator();
                    OpenTheDoor();
                    floorRequests.Remove(CurrentFloor);
                }

                if (floorRequests.Any())
                {
                    CloseTheDoor();
                    MoveElevatorToNextFloor();
                }

            } while (floorRequests.Any());
        }

        public bool OpenTheDoor()
        {
            if (IsElevatorMoving || IsElevatorDoorOpened) return false;
            clock.PauseFor(TimeSpan.FromSeconds(3));
            UpdateElevatorState(ElevatorState.StoppedWithDoorOpened);
            Debug.WriteLine("Make DING SOUND");
            return true;
        }

        public bool CloseTheDoor()
        {
            if (IsElevatorDoorClosed) return false;

            UpdateElevatorState(ElevatorState.StoppedWithDoorClosed);
            return true;
        }

        protected virtual void OnFloorChanged(FloorChangedEventArgument currentFloorArgument)
        {
            clock.PauseFor(TimeSpan.FromSeconds(5));
            var handler = FloorChanged;
            handler?.Invoke(this, currentFloorArgument);
        }

        protected virtual void OnStateChanged()
        {
            var handler = StateChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected bool StopTheElevator()
        {
            if (!IsElevatorMoving) return false;

            UpdateElevatorState(ElevatorState.StoppedWithDoorClosed);
            return true;
        }

        private void ValidateFloorsExist(int[] targetFloorNumbers)
        {
            foreach (var targetFloor in targetFloorNumbers)
            {
                var existingFloor = Floors.SingleOrDefault(x => x.Number == targetFloor);
                if (existingFloor == null)
                {
                    throw new ArgumentOutOfRangeException($"{targetFloor} does not exist!");
                }

                floorRequests.Add(existingFloor);
            }
        }

        private void MoveElevatorToNextFloor()
        {
            UpdateElevatorState(ElevatorState.MovingWithDoorClosed);
            var targetFloor = floorRequests.First();
            UpdateElevatorState(ElevatorState.MovingWithDoorClosed);
            Direction targetFloorDirection = CanMoveUp(targetFloor.Number) ? Direction.Up : Direction.Down;
            var nextFloor = targetFloorDirection == Direction.Up ? GetNextFloor() : GetPreviousFloor();
            if (nextFloor != null)
            {
                CurrentFloor = nextFloor;
                OnFloorChanged(FloorChangedEventArgument.Create(CurrentFloor, targetFloorDirection));
            }
        }

        private Floor GetPreviousFloor()
        {
            var indexOfPreviousFloor = floors.FindIndex(x => x.Equals(CurrentFloor)) - 1;
            return indexOfPreviousFloor >= 0 ? Floors.ElementAt(indexOfPreviousFloor) : null;
        }

        private Floor GetNextFloor()
        {
            var indexOfNextFloor = floors.FindIndex(x => x.Equals(CurrentFloor)) + 1;
            return indexOfNextFloor <= Floors.Count ? Floors.ElementAt(indexOfNextFloor) : null;
        }

        private bool CanMoveUp(int targetFloorNo)
        {
            return CurrentFloor.Number < targetFloorNo;
        }

        private void UpdateElevatorState(ElevatorState elevatorState)
        {
            States = elevatorState;
            OnStateChanged();
        }
    }
}
