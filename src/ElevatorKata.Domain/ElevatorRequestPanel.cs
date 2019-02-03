using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ElevatorKata.Domain
{
    /// <summary>
    ///     Elevator Request Panel requests a lift in the direction the lift user would like to go in,
    ///     enabling the direction requested until the lift arrives on the current floor
    /// </summary>
    /// <remarks>
    ///     On the very bottom floor, the only option would be up.
    ///     On the very top floor, the only option would be down.
    ///     In-between, both options can be supported
    /// </remarks>
    public class ElevatorRequestPanel
    {
        private readonly List<Elevator> elevators = new List<Elevator>();
        private ActivityButton upButton;
        private ActivityButton downButton;

        public ElevatorRequestPanel(Floor callingFloor, ElevatorRequestPanelOption elevatorRequestPanelOption, params Elevator[] elevatorItems)
        {
            CallingFloor = callingFloor ?? throw new ArgumentNullException(nameof(callingFloor));
            if (!elevatorItems.Any())
            {
                throw new ArgumentException(Errors.ElevatorsEmpty, nameof(elevatorItems));
            }                            
            elevators.AddRange(elevatorItems);
            elevators.ForEach(elevator => elevator.StateChanged += CheckStateChangesForElevatorOnTheSameFloorWithDoorOpened);
            
            switch (elevatorRequestPanelOption)
            {
                case ElevatorRequestPanelOption.DownOnly:
                    CreateUpButton(false);
                    CreateDownButton(true);
                    break;
                case ElevatorRequestPanelOption.UpOnly:
                    CreateUpButton(true);
                    CreateDownButton(false);
                    break;
                default:
                    CreateUpButton(true);
                    CreateDownButton(true);
                    break;
            }
        }

        public IReadOnlyList<Elevator> Elevators => elevators.AsReadOnly();
        public Floor CallingFloor { get; private set; }

        public IActivityState UpButton => upButton;
        public IActivityState DownButton => downButton;

        public event EventHandler Changed;

        private void RequestElevatorsToTheCurrentFloor()
        {            
            var closestStoppedElevator =
                (from elevator in elevators
                let floorDistanceAway = Math.Abs(this.CallingFloor.Number - elevator.CurrentFloor.Number)
                where elevator.IsElevatorStopped
                orderby floorDistanceAway
                select elevator).FirstOrDefault();

            if (closestStoppedElevator != null)
            {
                Debug.WriteLine($"{closestStoppedElevator.Name} requested.");
                closestStoppedElevator.GoTo(CallingFloor.Number);
                return;
            }
            CallAllLifts();
        }

        private void CallAllLifts()
        {
            Debug.WriteLine($"All lifts requested as they are busy with which ever stopping on this floor.");
            elevators.ForEach(elevator => elevator.GoTo(CallingFloor.Number));
        }

        private void CheckStateChangesForElevatorOnTheSameFloorWithDoorOpened(object sender, EventArgs e)
        {
            if (sender is Elevator callingElevator && 
                callingElevator.IsElevatorDoorOpened && 
                callingElevator.CurrentFloor.Number == CallingFloor.Number)
            {
                upButton.Deactivate();
                downButton.Deactivate();                        
            }
        }

        private void CreateDownButton(bool isEnabled)
        {
            downButton = ActivityButton.Create("Request lift to go down", false, isEnabled, RequestElevatorsToTheCurrentFloor);
            downButton.Changed += OnChanged;
        }

        private void CreateUpButton(bool isEnabled)
        {
            upButton = ActivityButton.Create("Request lift to go up", false, isEnabled, RequestElevatorsToTheCurrentFloor);
            upButton.Changed += OnChanged;
        }

        private void OnChanged(object sender, EventArgs e)
        {
            var handler = Changed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
