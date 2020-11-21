using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorRequestPanelShould
    {
        private const int GroundFloor = 0;

        private readonly ElevatorRequestPanel elevatorRequestPanel;
        private readonly Mock<IClock> clockMock;
        private readonly Elevator elevator1;
        private readonly Elevator elevator2;
        private readonly Floor callingFloor;

        public ElevatorRequestPanelShould()
        {
            var floors = new List<Floor>
            {
                Floor.Create(-1, "Basement"),
                Floor.Create(0, "Ground floor"),
                Floor.Create(1, "1st floor"),
                Floor.Create(2, "2nd floor"),
                Floor.Create(3, "3rd floor")
            };

            callingFloor = floors.ElementAt(1);
            clockMock = new Mock<IClock>();
            elevator1 = new Elevator(floors, clockMock.Object, 0, "Lift 1");
            elevator2 = new Elevator(floors, clockMock.Object, 1, "Lift 2");
            elevatorRequestPanel = new ElevatorRequestPanel(callingFloor, ElevatorRequestPanelOption.UpAndDown, elevator1, elevator2);
        }

        [Fact]
        public void ThrowArgumentNullExceptionWhenNullCallingFloorAssigned()
        {
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: callingFloor";

            Action act = () => new ElevatorRequestPanel(null, ElevatorRequestPanelOption.UpAndDown, elevator1);

            act.Should().Throw<ArgumentNullException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ThrowArgumentExceptionWhenWhenEmptyElevatorListAssigned()
        {
            var expectedMessage = $"{Errors.ElevatorsEmpty}{Environment.NewLine}Parameter name: elevatorItems";

            Action act = () => new ElevatorRequestPanel(callingFloor, ElevatorRequestPanelOption.UpAndDown, Enumerable.Empty<Elevator>().ToArray());

            act.Should().Throw<ArgumentException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ExistOnAFloorWithTheAbilityToCallMultipleElevatorsToGoDownOrUp()
        {
            elevatorRequestPanel.Elevators.Should().NotBeNull();
            elevatorRequestPanel.Elevators.Count.Should().Be(2);
            elevatorRequestPanel.CallingFloor.Should().NotBeNull();
            elevatorRequestPanel.CallingFloor.Number.Should().Be(GroundFloor);

            elevatorRequestPanel.UpButton.Should().NotBeNull();
            elevatorRequestPanel.UpButton.IsActive.Should().BeFalse();
            elevatorRequestPanel.UpButton.IsEnabled.Should().BeTrue();
            elevatorRequestPanel.UpButton.CanExecute.Should().BeTrue();

            elevatorRequestPanel.DownButton.Should().NotBeNull();
            elevatorRequestPanel.DownButton.IsActive.Should().BeFalse();
            elevatorRequestPanel.DownButton.IsEnabled.Should().BeTrue();
            elevatorRequestPanel.DownButton.CanExecute.Should().BeTrue();
        }

        [Fact]
        public void RequestElevatorOneOnlyAsItIsTheClosestInactiveElevator()
        {
            var elevator1StateChangedCall = false;
            elevator1.StateChanged += (sender, args) =>
            {
                elevator1StateChangedCall = true;
                elevator1.CurrentFloor.Number.Should().Be(GroundFloor);
                elevator1.IsElevatorDoorOpened.Should().BeTrue();
            };
            var elevator2StateChangedCall = false;
            elevator2.StateChanged += (sender, args) => { elevator2StateChangedCall = true; };
            var upButtonIsActiveStates = new List<bool>();
            elevatorRequestPanel.Changed += (sender, args) =>
            {
                upButtonIsActiveStates.Add(elevatorRequestPanel.UpButton.IsActive);
            };

            var actual = elevatorRequestPanel.UpButton.Execute();

            actual.Should().BeTrue();
            elevator2StateChangedCall.Should().BeFalse();
            elevator1StateChangedCall.Should().BeTrue();
            upButtonIsActiveStates.Count.Should().Be(2);
            upButtonIsActiveStates.First().Should().BeTrue();
            upButtonIsActiveStates.Last().Should().BeFalse();
        }

        [Fact]
        public void RequestElevator2WhenElevator1IsAtTheTopFloorOrFurtherAway()
        {
            var elevator1FinishedCalled = false;
            elevator1.Finished += (sender, args) =>
            {
                // Elevator request when Lift 2 is on the third floor
                elevator1.CurrentFloor.Number.Should().Be(3);
                elevatorRequestPanel.UpButton.Execute();
                elevator1FinishedCalled = true;
            };

            var elevator2FinishedCalled = false;
            elevator2.Finished += (sender, args) =>
            {
                elevator2.CurrentFloor.Number.Should().Be(elevatorRequestPanel.CallingFloor.Number);
                elevator2FinishedCalled = true;
            };

            elevator1.GoTo(3);

            elevator1FinishedCalled.Should().BeTrue();
            elevator2FinishedCalled.Should().BeTrue();
        }

        [Fact]
        public void RequestBothLiftsWhenAllLiftsAreBusy()
        {
            var elevator1FloorChanged = false;
            elevator1.FloorChanged += (sender, argument) =>
            {
                if (!elevator1FloorChanged)
                {
                    // Call Elevator 2 while Elevator 1 is busy
                    elevator2.GoTo(3);
                    elevator1FloorChanged = true;
                }                
            };
            var elevator2FloorChanged = false;
            elevator2.FloorChanged += (sender, argument) =>
            {
                if (!elevator2FloorChanged)
                {
                    // Request a lift on the ground floor while both lifts are busy
                    elevatorRequestPanel.UpButton.Execute();
                    elevator2FloorChanged = true;
                }
            };

            var finishedOrder = new List<Elevator>();
            elevator2.Finished += (sender, args) =>
            {
                elevator2.CurrentFloor.Number.Should().Be(elevatorRequestPanel.CallingFloor.Number);
                finishedOrder.Add(elevator2);
            };
            elevator1.Finished += (sender, args) =>
            {
                elevator1.CurrentFloor.Number.Should().Be(elevatorRequestPanel.CallingFloor.Number);
                finishedOrder.Add(elevator1);
            };

            elevator1.GoTo(3);

            elevator1FloorChanged.Should().BeTrue();
            elevator2FloorChanged.Should().BeTrue();
            finishedOrder.Count.Should().Be(2);
        }
    }
}
