using System;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace ElevatorKata.Domain.UnitTests
{
    public class ElevatorShould
    {
        private const int PenthouseSuite = 15;
        private const int GroundFloor = 0;
        private const int FirstFloor = 1;
        private const int SeventhFloor = 7;
        private const int Basement = -1;
        private const int UnluckyThirteenthFloor = 13;

        private Elevator elevator;
        private readonly Mock<IClock> mockClock;

        public ElevatorShould()
        {
            mockClock = new Mock<IClock>();
            elevator = new Elevator(new List<Floor>
            {
                Floor.Create(-1, "1st Basement" ),
                Floor.Create( 0, "Ground floor" ),
                Floor.Create( 1, "1st floor" ),
                Floor.Create( 3, "3rd floor" ),
                Floor.Create( 2, "2nd floor" ),
                Floor.Create( 4, "4th floor" ),
                Floor.Create( 5, "5th floor" ),
                Floor.Create( 6, "6th floor" ),
                Floor.Create( 7, "7th floor" ),
                Floor.Create( 8, "8th floor" ),
                Floor.Create( 9, "9th floor" ),
                Floor.Create( 10, "10th floor" ),
                Floor.Create( 11, "11th floor" ),
                Floor.Create( 12, "12th floor" ),
                Floor.Create( 14, "14th floor" ),
                Floor.Create( 15, "Penthouse Suite" ),
            }, mockClock.Object);
            elevator.StateChanged += (sender, args) =>
            {
                Debug.WriteLine($"Elevator states are the following: '{elevator.States}'");
            };
        }

        [Fact]
        public void DefaultTheElevatorOnTheGroundFloor()
        {
            elevator.CurrentFloor.Number.Should().Be(0);
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
        }

        [Fact]
        public void ConfigureSupportedFloorsFromTheBasementToThePenthouseSuite()
        {
            elevator.Floors.Should().NotBeEmpty();
            const string expectedBasementFloor = "1st Basement";
            const string expectedPenthouseSuite = "Penthouse Suite";

            elevator.Floors.First().Description.Should().Be(expectedBasementFloor);
            elevator.Floors.Last().Description.Should().Be(expectedPenthouseSuite);
        }

        [Theory]
        [InlineData(-2)]
        [InlineData(UnluckyThirteenthFloor)]
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
        
            Action act = () => new Elevator(new List<Floor>{Floor.Create(0, "ground") }, null);
        
            act.Should().Throw<ArgumentNullException>().WithMessage(expectedMessage);
        }

        
        [Fact]
        public void ThrowArgumentExceptionWhenEmptyFloorsAssigned()
        {
            Action act = () =>
            {
                var emptyFloors = new List<Floor>();
                new Elevator(emptyFloors, mockClock.Object);
            };
        
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GoToThePenthouseSuite()
        {
            elevator.GoTo(PenthouseSuite);
        
            elevator.CurrentFloor.Number.Should().Be(PenthouseSuite);
        }

        [Fact]
        public void GoToThePenthouseSuiteInAnUpDirection()
        {
            var expectTheFloorChangedEventToHaveBeenCalled = false;
            elevator.FloorChanged += (sender, argument) =>
            {
                argument.Direction.Should().HaveFlag(Direction.Up);
                expectTheFloorChangedEventToHaveBeenCalled = true;
            }; 
            
            elevator.GoTo(PenthouseSuite);

            expectTheFloorChangedEventToHaveBeenCalled.Should().BeTrue();
        }

        [Fact]
        public void GoToThePenthouseSuiteAndObserveEachFloorAsItChanges()
        {
            var changedFloors = new List<Floor>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args.Floor);
                elevator.IsElevatorDoorClosed.Should().BeTrue();
                elevator.IsElevatorMoving.Should().BeTrue();
            };
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
            var callCountForTransitionTimeBetweenFloors = 14;

            elevator.GoTo(PenthouseSuite);
        
            changedFloors.Exists(x => x.Number == FirstFloor).Should().BeTrue();
            changedFloors.Exists(x => x.Number == UnluckyThirteenthFloor).Should().BeFalse();
            changedFloors.Exists(x => x.Number == SeventhFloor).Should().BeTrue();
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Exactly(callCountForTransitionTimeBetweenFloors));
            elevator.IsElevatorStopped.Should().BeTrue();
            elevator.IsElevatorDoorOpened.Should().BeTrue();
        }

        [Fact]
        public void GoToTheBasementInADownDirection()
        {
            var expectTheFloorChangedEventToHaveBeenCalled = false;
            elevator.FloorChanged += (sender, argument) =>
            {
                argument.Direction.Should().HaveFlag(Direction.Down);
                expectTheFloorChangedEventToHaveBeenCalled = true;
            };

            elevator.GoTo(Basement);

            expectTheFloorChangedEventToHaveBeenCalled.Should().BeTrue();
        }

        [Fact]
        public void GoToTheBasementAndObserveEachFloorAsItChangedExposingValidFloorDataOnTheFloorChangedEvent()
        {            
            var changedFloors = new List<FloorChangedEventArgument>();
            elevator.FloorChanged += (sender, args) =>
            {
                changedFloors.Add(args);
                elevator.States.Should().Be(ElevatorState.MovingWithDoorClosed);
            };
        
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
            elevator.GoTo(Basement);
        
            var expectedFloor = changedFloors.Single(x => x.Floor.Number == Basement);
            expectedFloor.Should().NotBeNull();
            expectedFloor.Direction.Should().Be(Direction.Down);
            expectedFloor.Floor.Description.Should().Be("1st Basement");
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Once);
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
        }

        [Fact]
        public void GoToGroundFloorAndStayOnTheGroundFloorOpeningTheDoor()
        {
            var expectTheFloorChangedEventNotToHaveBeenCalled = true;
            elevator.FloorChanged += (sender, args) => { expectTheFloorChangedEventNotToHaveBeenCalled = false; };
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);

            elevator.GoTo(GroundFloor);

            expectTheFloorChangedEventNotToHaveBeenCalled.Should().BeTrue();
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(5)), Times.Never);
        }       
        
        [Fact]
        public void StopAndOpenTheDoor()
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
                elevator.States.Should().NotHaveFlag(ElevatorState.DoorOpened);
                elevator.States.Should().HaveFlag(ElevatorState.DoorClosed);
                elevator.States.Should().HaveFlag(ElevatorState.Moving);
            };
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorClosed);
        
            elevator.GoTo(PenthouseSuite);
            
            elevator.States.Should().Be(ElevatorState.StoppedWithDoorOpened);
            mockClock.Verify(x => x.PauseFor(TimeSpan.FromSeconds(3)), Times.Once);
        }

        [Fact]
        public void StartOnTheGroundGoingToPenthouseStoppingFirstOnTheSeventhFloorEndingAtTheBasement()
        {
            const int expectedFinalFloor = Basement;
            var expectedFloorsForLiftToOpenOn = new List<Floor>();
            elevator.StateChanged += (sender, argument) =>
            {
                if (elevator.IsElevatorDoorOpened)
                {
                    expectedFloorsForLiftToOpenOn.Add(elevator.CurrentFloor);
                }
            };

            elevator.OpenTheDoor();
            elevator.GoTo(PenthouseSuite, SeventhFloor, Basement);

            expectedFloorsForLiftToOpenOn.Count.Should().Be(4);
            expectedFloorsForLiftToOpenOn.First().Number.Should().Be(GroundFloor);
            expectedFloorsForLiftToOpenOn.ElementAt(1).Number.Should().Be(SeventhFloor);
            expectedFloorsForLiftToOpenOn.ElementAt(2).Number.Should().Be(PenthouseSuite);
            expectedFloorsForLiftToOpenOn.ElementAt(3).Number.Should().Be(Basement);
            var actualFinalFloor = elevator.CurrentFloor.Number;
            actualFinalFloor.Should().Be(expectedFinalFloor);
        }
    }
}
