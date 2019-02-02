using System;
using System.Collections.Generic;
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

        private void RequestElevatorToTheCurrentFloor()
        {
            var nearestStoppedElevatorOnTheSameFloor = elevators.SingleOrDefault(x =>
                x.IsElevatorStopped && x.CurrentFloor.Number == this.CallingFloor.Number);
            if (nearestStoppedElevatorOnTheSameFloor != null)
            {
                nearestStoppedElevatorOnTheSameFloor.StateChanged += CheckStateChangesForElevatorOnTheSameFloorWithDoorOpened;
                nearestStoppedElevatorOnTheSameFloor.GoTo(this.CallingFloor.Number);                
            }
        }

        private void CheckStateChangesForElevatorOnTheSameFloorWithDoorOpened(object sender, EventArgs e)
        {
            if (sender is Elevator callingElevator && 
                callingElevator.IsElevatorDoorOpened && 
                callingElevator.CurrentFloor.Number == CallingFloor.Number)
            {
                //TODO: Calculate which button was called first instead of deactivating both
                upButton.Deactivate();
                downButton.Deactivate();                        
            }
        }

        private void CreateDownButton(bool isEnabled)
        {
            downButton = ActivityButton.Create("Request lift to go down", false, isEnabled, RequestElevatorToTheCurrentFloor);
            downButton.Changed += OnChanged;
        }

        private void CreateUpButton(bool isEnabled)
        {
            upButton = ActivityButton.Create("Request lift to go up", false, isEnabled, RequestElevatorToTheCurrentFloor);
            upButton.Changed += OnChanged;
        }

        private void OnChanged(object sender, EventArgs e)
        {
            var handler = Changed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
