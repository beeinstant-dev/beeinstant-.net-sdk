using System.Diagnostics;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class Timer : ITimer
    {
        private readonly Recorder Recorder = new Recorder(Unit.MilliSecond);
        private Stopwatch _timer;

        public string FlushToString()
        {
            return this.Recorder.FlushToString();
        }

        public ITimer Merge(ITimer target)
        {   
            if(target == null || target == this)
            {
                return this;
            }

            var targetTimer = (Timer)target;
            this.Recorder.Merge(targetTimer.Recorder);
            return this;
        }

        public void StartTimer()
        {
            if (_timer != null && _timer.IsRunning)
            {
                this.StopTimer();
            }

            _timer = Stopwatch.StartNew();
        }

        public void StopTimer()
        {
            if (_timer == null)
            {
                return;
            }

            _timer.Stop();
            Recorder.Record(_timer.ElapsedMilliseconds, Unit.MilliSecond);
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if(target is ITimer)
            {
                this.Merge(target as ITimer);
            }

            return this;
        }
    }
}