using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorShould
    {
        private readonly Elevator elevator;

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
    }
}
