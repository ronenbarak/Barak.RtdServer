using System;
using System.Linq;
using System.Threading;
using RTDServer;

namespace SampleRTDServer
{
    internal class Proxy : ServerProxy
    {
        private IPublisher m_publisher;
        private Timer m_timer;

        protected override void Initialize(IPublisher publisher)
        {
            publisher.RegisterTopic<TimeZoneEntry>("Time", x => x.Id, Transformer)
                .SubTopic(x => x.Offset)
                .SubTopic(x => x.Location)
                .SubTopic(x => x.Time, "The current time", "Local Time")
                .SubTopic(x => x.PrevTime,"The time published before the current value");

            m_publisher = publisher;
            m_timer = new Timer(OnTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void Transformer(TimeZoneEntry newValue, TimeZoneEntry oldValue)
        {
            if (oldValue == null)
            {
                newValue.PrevTime = null;
            }
            else
            {
                newValue.PrevTime = oldValue.Time;
            }
        }

        protected override void Terminate()
        {
            m_timer.Dispose();
        }

        private void OnTimerTick(object state)
        {
            m_publisher.Publish(new TimeZoneEntry()
            {
                Id = 1,
                Location = "Baker Island",
                Offset = -12*60,
                InternalTime = DateTime.UtcNow - TimeSpan.FromMinutes(12 * 60)
            });
            
            m_publisher.Publish(new TimeZoneEntry()
            {
                Id = 2,
                Location = "Jamaica",
                Offset = -5*60,
                InternalTime = DateTime.UtcNow - TimeSpan.FromMinutes(-5 * 60)
            });
            
            m_publisher.Publish(new TimeZoneEntry() {Id = 3, Location = "Iceland", Offset = 0, InternalTime = DateTime.UtcNow});
            
            m_publisher.Publish(new TimeZoneEntry()
            {
                Id = 4,
                Location = "Afghanistan",
                Offset = (int) (4.5*60),
                InternalTime = DateTime.UtcNow + TimeSpan.FromMinutes(4.5*60)
            });
            
            m_publisher.Publish(new TimeZoneEntry()
            {
                Id = 5,
                Location = "Kiribati",
                Offset = 14*60,
                InternalTime = DateTime.UtcNow + TimeSpan.FromMinutes(14 * 60)
            });
        }
    }
}