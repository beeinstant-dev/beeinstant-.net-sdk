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

        public Unit(string unit) =>  _unit = unit;

        public override string ToString() => _unit;

        public override bool Equals(object obj)
        {
            if(obj == null || obj.GetType() != typeof(Unit))
            {
                return false;
            }

            return (obj as Unit)._unit.Equals(_unit);
        }

        public override int GetHashCode() => _unit.GetHashCode();
    }
}