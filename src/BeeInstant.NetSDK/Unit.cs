namespace BeeInstant.NetSDK
{
    public sealed class Unit
    {
        private readonly string _unit;

        /* time units */
        public static Unit NanoSecond = new Unit("ns");
        public static Unit MicroSecond = new Unit("us");
        public static Unit MilliSecond = new Unit("ms");
        public static Unit Second = new Unit("s");
        public static Unit Minute = new Unit("m");
        public static Unit Hour = new Unit("h");

        /* byte units */
        public static Unit Byte = new Unit("b");
        public static Unit KiloByte = new Unit("kb");
        public static Unit MegaByte = new Unit("mb");
        public static Unit GigaByte = new Unit("gb");
        public static Unit TeraByte = new Unit("tb");

        /* rate units */
        public static Unit BitPerSecond = new Unit("bps");
        public static Unit KiloBitPerSecond = new Unit("kbps");
        public static Unit MegaBitPerSecond = new Unit("mbps");
        public static Unit GigaBitPerSecond = new Unit("gbps");
        public static Unit TeraBitPerSecond = new Unit("tbps");

        public static Unit Percent = new Unit("p");
        public static Unit None = new Unit(string.Empty);

        public Unit(string unit)
        {
            _unit = unit;
        }

        public override string ToString() => _unit;
    }
}