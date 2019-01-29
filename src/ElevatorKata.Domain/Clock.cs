using System;

namespace ElevatorKata.Domain
{
    public class Clock: IClock
    {
        public void PauseFor(TimeSpan timeSpan)
        {
            System.Threading.Thread.Sleep(timeSpan);
        }
    }
}
