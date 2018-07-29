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
using System.Diagnostics;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class Timer : ITimer
    {
        private readonly Recorder _recorder = new Recorder(Unit.MilliSecond);
        private Stopwatch _timer;

        public string FlushToString()
        {
            return _recorder.FlushToString();
        }

        public ITimer Merge(ITimer target)
        {
            if (target == null || target == this)
            {
                return this;
            }

            var targetTimer = (Timer) target;
            _recorder.Merge(targetTimer._recorder);
            return this;
        }

        public void StartTimer()
        {
            if (_timer != null && _timer.IsRunning)
            {
                StopTimer();
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

            //multiplying by 1.0M to get decimal formatted as *.0 by default
            _recorder.Record(Convert.ToDecimal(_timer.ElapsedMilliseconds) * 1.0M, Unit.MilliSecond);
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if (target is ITimer timer)
            {
                Merge(timer);
            }

            return this;
        }
    }
}