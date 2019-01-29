using System;

namespace ElevatorKata.Domain
{
    public interface IClock
    {
        void PauseFor(TimeSpan timeSpan);
    }
}