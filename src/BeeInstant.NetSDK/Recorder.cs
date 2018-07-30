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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BeeInstant.NetSDK.Abstractions;

namespace BeeInstant.NetSDK
{
    public class Recorder : IRecorder
    {
        private readonly ConcurrentQueue<decimal> _queue;
        private readonly Unit _unit;

        public Recorder(Unit unit)
        {
            _unit = unit;
            _queue = new ConcurrentQueue<decimal>();
        }

        public string FlushToString()
        {
            var values = new List<decimal>();

            while (_queue.Any() && _queue.TryDequeue(out decimal val))
            {
                values.Add(val);
            }

            if (values.Any())
            {
                return string.Join("+", values) + _unit;
            }

            return string.Empty;
        }

        public IRecorder Merge(IRecorder target)
        {
            if (target == null || target == this)
            {
                return this;
            }

            var recorder = (Recorder) target;
            while (recorder._queue.Any() && recorder._queue.TryDequeue(out var res))
            {
                Record(res, recorder._unit);
            }

            return this;
        }

        public void Record(decimal value, Unit unit)
        {
            if (_unit.Equals(unit))
            {
                _queue.Enqueue(Math.Max(0.0M, value));
            }
        }

        IMetric IMetric.Merge(IMetric target)
        {
            if (target is IRecorder recorder)
            {
                Merge(recorder);
            }

            return this;
        }
    }
}