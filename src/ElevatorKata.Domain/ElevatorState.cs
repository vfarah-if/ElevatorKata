using System;

namespace ElevatorKata.Domain
{
    [Flags]
    public enum ElevatorState
    {
        Stopped = 1,
        Moving = 2,
        DoorOpened = 4,
        DoorClosed = 8,
        StoppedWithDoorOpened = Stopped | DoorOpened,
        StoppedWithDoorClosed = Stopped | DoorClosed,
        MovingWithDoorClosed = Moving | DoorClosed,        
    }
}
