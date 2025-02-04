namespace IoT.Common
{
    public abstract class AbstractAggregate
    {
        public double Value { get; set; }

        public SensorValueUnit Unit { get; set; }

        public abstract Period Period { get; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
