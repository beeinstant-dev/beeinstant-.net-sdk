using System;
using System.Threading.Tasks;
using BeeInstant.NetSDK;

namespace BeeInstant.NetSDK.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            MetricsManager.Initialize("SampleApp", "Development", "localhost");

            using(var timer = MetricsManager.GetRootMetricsLogger().StartTimer("Timer"))
            {
                Task.Delay(500).Wait();
            }

            Parallel.For(0, 1000, (i) => {
                MetricsManager.GetRootMetricsLogger().IncrementCounter("MyCounter", 1);
                MetricsManager.GetRootMetricsLogger().Record("MyRecorder", 1.0M, Unit.Second);
            });         

            Console.ReadKey();
        }
    }
}
