using System;

namespace ElevatorKata.Domain
{
    [Flags]
    public enum ElevatorState
    {
        Stopped = 0,
        Moving = 1,
        DoorOpened = 2,
        DoorClosed = 4,
        StoppedWithDoorOpened = Stopped | DoorOpened,
        StoppedWithDoorClosed = Stopped | DoorClosed,
        MovingWithDoorClosed = Moving | DoorClosed,        
    }
}
