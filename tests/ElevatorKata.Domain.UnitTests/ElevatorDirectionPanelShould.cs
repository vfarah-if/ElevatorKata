using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorDirectionPanelShould
    {
        private const int TopFloor = 3;
        private const int GroundFloor = 0;
        private const int BasementFloor = -1;

        private readonly Mock<IClock> clockMock;
        private readonly ElevatorDirectionPanel elevatorDirectionPanel;
        private Elevator elevator;

        public ElevatorDirectionPanelShould()
        {
            var floors = new List<Floor>
            {
                Floor.Create(-1, "Basement"),
                Floor.Create(0, "Ground floor"),
                Floor.Create(1, "1st floor"),                
                Floor.Create(2, "2nd floor"),
                Floor.Create(3, "3rd floor")
            };

            clockMock = new Mock<IClock>();
            elevator = new Elevator(floors, clockMock.Object);
            elevatorDirectionPanel = new ElevatorDirectionPanel(elevator);
        }

        [Fact]
        public void ThrowArgumentNullExceptionWhenNullElevatorAssigned()
        {
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: elevator";

            Action act = () => new ElevatorDirectionPanel(null);

            act.Should().Throw<ArgumentNullException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void CreateAPanelWithDefaultSettings()
        {
            elevatorDirectionPanel.Should().NotBeNull();
            elevatorDirectionPanel.Elevator.Should().NotBeNull();
            elevatorDirectionPanel.DownArrowButton.Should().NotBeNull();            
            elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeFalse();
            elevatorDirectionPanel.UpArrowButton.Should().NotBeNull();
            elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeFalse();
        }

        [Fact]
        public void ShowUpButtonArrowActiveWhileElevatorMovesUpAndDeactivateWhenFinished()
        {
            var elevatorFloorChangedCalled = false;
            var elevatorFinishedCalled = false;

            elevator.FloorChanged += (sender, argument) =>
            {
                elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeTrue();
                elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeFalse();
                elevatorFloorChangedCalled = true;
            };
            elevator.Finished += (sender, args) =>
            {
                elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeFalse();
                elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeFalse();
                elevatorFinishedCalled = true;
            };

            elevator.GoTo(TopFloor);

            elevatorFloorChangedCalled.Should().BeTrue();
            elevatorFinishedCalled.Should().BeTrue();
        }

        [Fact]
        public void ShowDownArrowButtonActiveWhileElevatorMovesDownAndDeactivateWhenFinished()
        {
            var elevatorFloorChangedCalled = false;
            var elevatorFinishedCalled = false;

            elevator.FloorChanged += (sender, argument) =>
            {
                elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeFalse();
                elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeTrue();
                elevatorFloorChangedCalled = true;
            };
            elevator.Finished += (sender, args) =>
            {
                elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeFalse();
                elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeFalse();
                elevatorFinishedCalled = true;
            };

            elevator.GoTo(BasementFloor);

            elevatorFloorChangedCalled.Should().BeTrue();
            elevatorFinishedCalled.Should().BeTrue();
        }

        [Fact]
        public void ShowInActiveUpDownArrowsIfTheLiftGoesToTheSameFloor()
        {
            elevator.GoTo(GroundFloor);

            elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeFalse();
            elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeFalse();
        }

        [Fact]
        public void ShowCorrectButtonStateChangesBasedOnSeveralCallsToDifferentFloors()
        {
            var elevatorFloorChangedCalled = true;
            elevator.FloorChanged += (sender, argument) =>
            {
                var isElevatorGoingUp = elevator.Direction == ElevatorDirection.Up;
                var isElevatorGoingDown = elevator.Direction == ElevatorDirection.Down;
                elevatorDirectionPanel.UpArrowButton.IsActive.Should().Be(isElevatorGoingUp);
                elevatorDirectionPanel.DownArrowButton.IsActive.Should().Be(isElevatorGoingDown);
                if (elevator.CurrentFloor.Number == BasementFloor)
                {
                    elevator.GoTo(GroundFloor);
                }
                elevatorFloorChangedCalled = true;
            };
            elevator.Finished += (sender, args) =>
            {
                elevatorDirectionPanel.UpArrowButton.IsActive.Should().BeFalse();
                elevatorDirectionPanel.DownArrowButton.IsActive.Should().BeFalse();
            };

            elevator.GoTo(TopFloor, BasementFloor);

            elevatorFloorChangedCalled.Should().BeTrue();
        }
    }
}
