using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorShould
    {
        private Elevator elevator;

        public ElevatorShould()
        {
            elevator = new Elevator(new Dictionary<int, string>
            {
                { -1, "Basement" },
                { 0, "Ground" },
                { 1, "First" },
                { 2, "Second" },
                { 3, "Penthouse Suite" },
            });
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
        public void GoToTheBasementInADownDirection()
        {
            const int basement = -1;

            var actualDirection = elevator.GoTo(basement);

            actualDirection.Should().Be(Direction.Down);
        }

        [Fact]
        public void StayOnTheGroundFloorWhenAlreadyOnTheGroundFloor()
        {
            const int groundFloor = 0;

            var actualDirection = elevator.GoTo(groundFloor);

            actualDirection.Should().Be(Direction.None);
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

            Action act = () => new Elevator(null);
            
            act.Should().Throw<ArgumentNullException>().WithMessage(expectedMessage);
        }

        [Fact]
        public void ThrowArgumentExceptionWhenEmptyFloorsAssigned()
        {
            Action act = () =>
            {
                var emptyFloors = new Dictionary<int, string>();
                new Elevator(emptyFloors);
            };

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DefaultCurrentFloorToZeroWhenNonExistentFloorIsAssignedOnTheConstructor()
        {
            var floors = new Dictionary<int, string> { { 0, "Ground Only" } };
            const int NonExistentFloor = 1;
            var expectedDefaultFloor = floors.First().Key;

            elevator = new Elevator(floors, NonExistentFloor);
            
            elevator.CurrentFloor.Should().Be(expectedDefaultFloor);
        }
    }
}
