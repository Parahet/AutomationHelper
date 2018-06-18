using System;
using System.Diagnostics;
using System.Threading;

namespace AutomationHelper.Base
{
    public class Wait
    {
        public static void UntilTrue(Func<bool> p, string err, TimeSpan timeout)
        {
            var timer = new Stopwatch();
            timer.Start();
            while (true)
            {
                if (p())
                    return;
                if (timer.Elapsed > timeout)
                    throw new TimeoutException($"WaitUntilTrue TIMED OUT ({timeout}). {err}");
            }
        }

        public static void Sleep(TimeSpan time)
        {
            Thread.Sleep(time);
        }

        public static T UntilNoException<T>(Func<T> f, TimeSpan timeout)
        {
			var timer = new Stopwatch();
	        timer.Start();
	        var attemps = 0;
	        while (true)
	        {
		        try
		        {
			        attemps++;
			        var temp = f();
			        return temp;
		        }
		        catch (Exception ex)
		        {
			        if (timer.Elapsed > timeout)
				        throw new TimeoutException($"UntilNoException TIMED OUT ({timeout})(attemps:{attemps})(elapsed:{timer.Elapsed}). {ex.Message}");
			        Sleep(TimeSpan.FromMilliseconds(50));
		        }
	        }
		}

        public static void UntilNoException(Action f, TimeSpan timeout)
        {
			var timer = new Stopwatch();
	        timer.Start();
	        var attemps = 0;
	        while (true)
	        {
		        try
		        {
			        Thread.Yield();
			        attemps++;
			        f();
			        return;
		        }
		        catch (Exception ex)
		        {
			        if (timer.Elapsed > timeout)
				        throw new TimeoutException($"UntilNoException TIMED OUT ({timeout})(attemps:{attemps})(elapsed:{timer}). {ex.Message}");
		        }
	        }
		}

        public static void UntilNumberOfExceptions(Action f, int times = 3)
        {
            for (var i = 0; i < times; i++)
            {
                try
                {
                    f();
                    return;
                }
                catch (Exception ex)
                {
                    if (i == times - 1) throw ex;
                }
            }
        }

        public static T UntilNumberOfExceptions<T>(Func<T> f, int times = 3)
        {
            for (var i = 0; i < times; i++)
            {
                try
                {
                    return f();
                }
                catch (Exception ex)
                {
                    if (i == times - 1) throw ex;
                }
            }
            throw new Exception("UntilNumberOfExceptions " + times + " times");
        }
    }
}
