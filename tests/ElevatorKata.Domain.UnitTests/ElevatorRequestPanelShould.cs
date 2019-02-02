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

        private ElevatorRequestPanel elevatorRequestPanel;
        private readonly Mock<IClock> clockMock;
        private Elevator elevator1;
        private Elevator elevator2;
        private Floor callingFloor;

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
    }
}
