/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2017 BeeInstant
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

namespace BeeInstant.NetSDK
{
    public sealed class Unit
    {
        private readonly string _unit;

        public static Unit NanoSecond = new Unit("ns");
        public static Unit MicroSecond = new Unit("us");
        public static Unit MilliSecond = new Unit("ms");
        public static Unit Second = new Unit("s");
        public static Unit Minute = new Unit("m");
        public static Unit Hour = new Unit("h");

        public static Unit Byte = new Unit("b");
        public static Unit KiloByte = new Unit("kb");
        public static Unit MegaByte = new Unit("mb");
        public static Unit GigaByte = new Unit("gb");
        public static Unit TeraByte = new Unit("tb");

        public static Unit BitPerSecond = new Unit("bps");
        public static Unit KiloBitPerSecond = new Unit("kbps");
        public static Unit MegaBitPerSecond = new Unit("mbps");
        public static Unit GigaBitPerSecond = new Unit("gbps");
        public static Unit TeraBitPerSecond = new Unit("tbps");

        public static Unit Percent = new Unit("p");
        public static Unit None = new Unit(string.Empty);

        public Unit(string unit) => _unit = unit;

        public override string ToString() => _unit;

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Unit))
            {
                return false;
            }

            return ((Unit) obj)._unit.Equals(_unit);
        }

        public override int GetHashCode() => _unit.GetHashCode();
    }
}