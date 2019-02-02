using System;
using System.Diagnostics;

namespace ElevatorKata.Domain
{
    public class ElevatorDirectionPanel
    {
        private readonly ActivityButton downArrowButton;
        private readonly ActivityButton upArrowButton;

        public ElevatorDirectionPanel(Elevator elevator)
        {
            Elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
            downArrowButton = ActivityButton.Create("Down Arrow");
            upArrowButton = ActivityButton.Create("Up Arrow");
            downArrowButton.Changed += OnButtonStateChanged;
            upArrowButton.Changed += OnButtonStateChanged;
            Elevator.StateChanged += ElevatorStateChanged;
            Elevator.Finished += ElevatorStateChanged;
        }
        
        public Elevator Elevator { get; }
        public IActivityState DownArrowButton => downArrowButton;
        public IActivityState UpArrowButton => upArrowButton;

        public event EventHandler Changed;

        public override string ToString()
        {
            return $"Direction Elevator Panel for {Elevator.Name} on floor {Elevator.CurrentFloor} with '{DownArrowButton}' and '{UpArrowButton}'";
        }

        private void ElevatorStateChanged(object sender, EventArgs e)
        {
            switch (Elevator.Direction)
            {
                case ElevatorDirection.Up:
                    upArrowButton.Activate();
                    downArrowButton.Deactivate();
                    break;
                case ElevatorDirection.Down:
                    upArrowButton.Deactivate();
                    downArrowButton.Activate();
                    break;
                default:
                    upArrowButton.Deactivate();
                    downArrowButton.Deactivate();
                    break;
            }
        }

        private void OnButtonStateChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(this.ToString());
            var handler = Changed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
