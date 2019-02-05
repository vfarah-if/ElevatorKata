using System;

namespace ElevatorKata.Domain
{
    public class Clock: IClock
    {
        public void RunFor(TimeSpan timeSpan)
        {
            System.Threading.Thread.Sleep(timeSpan);
        }
    }
}
