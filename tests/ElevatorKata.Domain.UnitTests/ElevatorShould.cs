using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorShould
    {
        private const int PenthouseSuite = 3;
        private const int GroundFloor = 0;
        private const int Basement = -1;

        private Elevator elevator;
        private readonly Mock<IClock> mockClock;

        public ElevatorShould()
        {
            mockClock = new Mock<IClock>();
            elevator = new Elevator(new Dictionary<int, string>
            {
                { -1, "1st Basement" },
                {  0, "Ground floor" },
                {  1, "1st floor" },
                {  2, "2nd floor" },
                {  3, "3rd floor" },
                {  4, "4th floor" },
                {  5, "5th floor" },
                {  6, "Penthouse Suite" },
            }, mockClock.Object);
            elevator.StateChanged += (sender, args) =>
            {
                Debug.WriteLine($"Elevator states are the following: '{elevator.States}'");
            };
        }

        [Fact]
        public void DefaultTheElevatorOnTheGroundFloor()
        {
            elevator.CurrentFloor.Should().Be(0);
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
        }

        [Fact]
        public void ConfigureSupportedFloorsFromTheBasementToThePenthouseSuite()
        {
            elevator.Floors.Should().NotBeEmpty();
            const string expectedBasementFloor = "1st Basement";
            const string expectedPenthouseSuite = "Penthouse Suite";

            elevator.Floors.First().Value.Should().Be(expectedBasementFloor);
            elevator.Floors.Last().Value.Should().Be(expectedPenthouseSuite);
        }

        [Fact]
        public void GoToThePenthouseSuite()
        {            
            elevator.GoTo(PenthouseSuite);

            elevator.CurrentFloor.Should().Be(PenthouseSuite);
        }

        [Fact]
        public void GoToThePenthouseSuiteInAnUpDirection()
        {
            var actualDirection = elevator.GoTo(PenthouseSuite);

            actualDirection.Should().Be(Direction.Up);
        }

        [Fact]
        public void GoToThePenthouseSuiteAndObserveEachFloorAsItChanges()
        {
            var changedFloors = new List<int>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args.CurrentFloor);
                elevator.States.Should().Be(ElevatorState.MovingWithDoorClosed);
            };
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);

            elevator.GoTo(PenthouseSuite);

            changedFloors.Should().Contain(1);
            changedFloors.Should().Contain(2);
            changedFloors.Should().Contain(3);
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Exactly(3));
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
        }

        [Fact]
        public void GoToTheBasementInADownDirection()
        {
            var actualDirection = elevator.GoTo(Basement);

            actualDirection.Should().Be(Direction.Down);
        }

        [Fact]
        public void GoToTheBasementAndObserveEachFloorAsItChanges()
        {            
            var changedFloors = new List<FloorChangedEventArgument>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args);
                elevator.States.Should().Be(ElevatorState.MovingWithDoorClosed);
            };

            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
            elevator.GoTo(Basement);

            var expectedFloor = changedFloors.Single(x => x.CurrentFloor == -1);
            expectedFloor.Should().NotBeNull();
            expectedFloor.Direction.Should().Be(Direction.Down);
            expectedFloor.Description.Should().Be("1st Basement");
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Once);
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
        }

        [Fact]
        public void StayOnTheGroundFloorWhenAlreadyOnTheGroundFloor()
        {
            var actualDirection = elevator.GoTo(GroundFloor);

            actualDirection.Should().Be(Direction.None);
        }

        [Fact]
        public void GoToTheSameFloorWithoutEmittingAnyChangedFloor()
        {
            var changedFloors = new List<int>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args.CurrentFloor);
                elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
            };

            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
            elevator.GoTo(GroundFloor);

            changedFloors.Should().BeEmpty();
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Never);
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(10)]
        public void ThrowAnOutOfRangeExceptionIfTheFloorDoesNotExist(int nonExistentFloor)
        {
            Action act = () => elevator.GoTo(nonExistentFloor);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ThrowArgumentNullExceptionWhenNullFloorsAssigned()
        {
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: supportedFloors";

            Action act = () => new Elevator(null, mockClock.Object);

            act.Should().Throw<ArgumentNullException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ThrowArgumentNullExceptionWhenNullClockIsAssigned()
        {
            var expectedMessage = $"Value cannot be null.{Environment.NewLine}Parameter name: clock";

            Action act = () => new Elevator(new Dictionary<int, string>{{0, "ground"}}, null);

            act.Should().Throw<ArgumentNullException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ThrowArgumentExceptionWhenEmptyFloorsAssigned()
        {
            Action act = () =>
            {
                var emptyFloors = new Dictionary<int, string>();
                new Elevator(emptyFloors, mockClock.Object);
            };

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DefaultCurrentFloorToZeroWhenNonExistentFloorIsAssignedOnTheConstructor()
        {
            var floors = new Dictionary<int, string> { { 0, "Ground Only" } };
            const int NonExistentFloor = 1;
            var expectedDefaultFloor = floors.First().Key;

            elevator = new Elevator(floors, mockClock.Object, NonExistentFloor);

            elevator.CurrentFloor.Should().Be(expectedDefaultFloor);
        }

        [Fact]
        public void OpenTheDoorAndPlaceTheLiftInAStoppedWithOpenDoorState()
        {
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);

            elevator.OpenTheDoor();

            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(3)), Times.Once);
        }

        [Fact]
        public void NotBeAbleToOpenTheDoorWhenTheLiftIsMoving()
        {
            elevator.FloorChanged += (sender, argument) =>
            {
                elevator.States.Should().Be(ElevatorState.MovingWithDoorClosed);
                elevator.OpenTheDoor().Should().BeFalse();
                elevator.States.Should().NotHaveFlag(ElevatorState.DoorOpen);
                elevator.States.Should().HaveFlag(ElevatorState.DoorClosed);
                elevator.States.Should().HaveFlag(ElevatorState.Moving);
            };
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);

            elevator.GoTo(PenthouseSuite);
            
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(3)), Times.Once);
        }
    }
}
