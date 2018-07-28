using System;
using System.Threading;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class TimerMetric : IDisposable
    {
        private IMetricsComposer _composer;
        private string _timerName; 

        private bool disposedValue = false;
        private int closed = 0;

        public TimerMetric(IMetricsComposer metricsComposer, string timerName)
        {
            _composer = metricsComposer;
            _timerName = timerName;
        }

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _composer.StopTimer(_timerName);
                }

                disposedValue = true;
            }
        }
    }
}