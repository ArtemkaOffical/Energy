using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Timers;

namespace Energy.Extensions.Additions
{
    static class Additions
    {
        public static string GetFileNameFromPath(this string path) 
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        public static Timer Timer(Action action, double seconds, bool repeat = false)
        {
            if (seconds == 0) seconds = 0.001;
            Timer timer = new Timer();
            timer.AutoReset = repeat;
            timer.Interval = seconds * 1000;
            timer.Elapsed += delegate (object o, ElapsedEventArgs e) { action(); };
            timer.Start();
            return timer;
        }
        public static Timer Every(Action action, double seconds, bool repeat = true)
        {
            if (seconds == 0) seconds = 0.001;
            return Timer(action, seconds, repeat);
        }
    }
}
