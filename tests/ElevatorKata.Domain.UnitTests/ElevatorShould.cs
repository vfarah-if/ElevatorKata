using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorShould
    {
        private Elevator elevator;
        private readonly Mock<IClock> mockClock;

        public ElevatorShould()
        {
            mockClock = new Mock<IClock>();
            elevator = new Elevator(new Dictionary<int, string>
            {
                { -1, "Basement" },
                {  0, "Ground" },
                {  1, "First" },
                {  2, "Second" },
                {  3, "Penthouse Suite" },
            }, mockClock.Object);
        }

        [Fact]
        public void DefaultTheElevatorOnTheGroundFloor()
        {
            elevator.CurrentFloor.Should().Be(0);
        }

        [Fact]
        public void ConfigureSupportedFloorsFromTheBasementToThePenthouseSuite()
        {
            elevator.Floors.Should().NotBeEmpty();
            const string ExpectedBasementFloor = "Basement";
            const string ExpectedPenthouseSuite = "Penthouse Suite";

            elevator.Floors.First().Value.Should().Be(ExpectedBasementFloor);
            elevator.Floors.Last().Value.Should().Be(ExpectedPenthouseSuite);
        }

        [Fact]
        public void GoToThePenthouseSuite()
        {
            const int penthouseSuite = 3;

            elevator.GoTo(penthouseSuite);

            elevator.CurrentFloor.Should().Be(penthouseSuite);
        }

        [Fact]
        public void GoToThePenthouseSuiteInAnUpDirection()
        {
            const int penthouseSuite = 3;

            var actualDirection = elevator.GoTo(penthouseSuite);

            actualDirection.Should().Be(Direction.Up);
        }

        [Fact]
        public void GoToThePenthouseSuiteAndObserveEachFloorAsItChanges()
        {
            const int penthouseSuite = 3;
            var changedFloors = new List<int>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args.CurrentFloor);
            };

            elevator.GoTo(penthouseSuite);

            changedFloors.Should().Contain(1);
            changedFloors.Should().Contain(2);
            changedFloors.Should().Contain(3);
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Exactly(3));
        }

        [Fact]
        public void GoToTheBasementInADownDirection()
        {
            const int basement = -1;

            var actualDirection = elevator.GoTo(basement);

            actualDirection.Should().Be(Direction.Down);
        }

        [Fact]
        public void GoToTheBasementAndObserveEachFloorAsItChanges()
        {
            const int Basement = -1;
            const int GroundFloor = 0;
            var changedFloors = new List<FloorChangedEventArgument>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args);
            };

            elevator.GoTo(Basement);

            var expectedFloor = changedFloors.Single(x => x.CurrentFloor == -1);
            expectedFloor.Should().NotBeNull();
            expectedFloor.PreviousFloor.Should().Be(GroundFloor);
            expectedFloor.Description.Should().Be("Basement");
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Once);
        }

        [Fact]
        public void StayOnTheGroundFloorWhenAlreadyOnTheGroundFloor()
        {
            const int groundFloor = 0;

            var actualDirection = elevator.GoTo(groundFloor);

            actualDirection.Should().Be(Direction.None);
        }

        [Fact]
        public void GoToTheSameFloorWithoutEmittingAnyChangedFloor()
        {
            const int groundFloor = 0;
            var changedFloors = new List<int>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args.CurrentFloor);
            };

            elevator.GoTo(groundFloor);

            changedFloors.Should().BeEmpty();
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Never);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(4)]
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
    }
}
