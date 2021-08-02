using System;
using System.Collections.Generic;
using System.Text;

namespace Game2
{
    public class TimeUtils
    {
        public static TimeSpan ConvertMillisecondsToTimeSpan(double dt)
        {
            long ticks = (long) (dt * 10000);
            TimeSpan time = new TimeSpan(ticks);
            return time;
        }


    }
}
