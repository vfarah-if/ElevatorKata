using System;

namespace ElevatorKata.Domain
{
    [Flags]
    public enum ElevatorState
    {
        Stopped = 0,
        Moving = 1,
        DoorOpen = 2,
        DoorClosed = 4,
        StoppedWithDoorOpened = Stopped | DoorOpen,
        StoppedWithDoorClosed = Stopped | DoorClosed,
        MovingWithDoorClosed = Moving | DoorClosed,        
    }
}
