using System;

namespace ElevatorKata.Domain
{
    public interface IClock
    {
        void RunFor(TimeSpan timeSpan);
    }
}