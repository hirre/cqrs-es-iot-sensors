using IoT.Interfaces;
using IoT.Persistence.Events;

namespace IoT.Domain.Sensor.Aggregates
{
    public class SensorAggregateRoot(string aggregateId) : IAggregateRoot
    {
        public string AggregateId { get; } = aggregateId;

        public SensorDayAggregate[]? Sensor30DayAggregates { get; set; }

        public SensorHourAggregate[]? Sensor24HourAggregates { get; set; }

        public void ApplyEvent(DomainEvent e)
        {

        }
    }
}
