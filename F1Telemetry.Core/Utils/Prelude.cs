using System;
using System.Collections.Generic;
using System.Text;
using log4net.Appender;

namespace F1Telemetry.Core.Utils
{
    public static class Prelude
    {
        public static T With<T>(this T o, Action<T> action)
        {
            action(o);
            return o;
        }
    }
}
