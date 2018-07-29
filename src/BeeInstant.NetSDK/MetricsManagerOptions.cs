namespace BeeInstant.NetSDK
{
    internal class MetricsManagerOptions
    {
        public int FlushInSeconds { get; set; }
        public int FlushStartDelayInSeconds { get; set; }
        public bool IsManualFlush { get; set; }
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string EndPoint { get; set; }
    }
}