/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2018 BeeInstant
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
 * documentation files (the "Software"), to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
 * to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions
 * of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

using System;
using System.Threading.Tasks;
using BeeInstant.NetSDK;

namespace BeeInstant.NetSDK.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initialize the metrics manager. This method should be called only once!
            MetricsManager.Initialize("SampleApp", "Development", "localhost");

            //Imitate long-running taks and get the timings
            //should be resolved in ~500ms 
            using(var timer = MetricsManager.GetRootMetricsLogger().StartTimer("Timer"))
            {
                Task.Delay(500).Wait();
            }

            //service also allows you to extend the logging experience by grouping metrics
            //together via dimensions. The line below creates a new logger with additional 
            //property `api`.
            var logger = MetricsManager.GetMetricsLogger("api=Counters");
            //and then extends an existing logger by adding more specifi property
            var extendedLogger = logger.ExtendDimensions("subApi=Recorders");
            //Imitate concurrent environment, create counter and record metrics and update them
            Parallel.For(0, 1000, (i) => {
                logger.IncrementCounter("MyCounter", 1);
                extendedLogger.Record("MyRecorder", 1.0M, Unit.Second);
            });         

            //If the 'IsManualFlush' config key is set to false, then the next line is redundant
            //as metrics manager will automatically flush things 
            // MetricsManager.FlushAll();

            Console.ReadKey();
        }
    }
}
