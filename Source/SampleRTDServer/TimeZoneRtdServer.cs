using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RTDServer;

namespace SampleRTDServer
{
  class TimeZoneEntry
  {
      public int Offset { get; set; }// Excel handle int just fine
      public string Location { get; set; } // Excel handle string just fine
      public DateTime InternalTime { get; set; } // Excel has harder time to handle DateTime
      public string Time { get { return InternalTime.ToString(); } } // we use string to publish the time
      public int Id { get; set; } 
  }

  [Guid("F6FC1817-6F2C-4F2B-AF31-13E84FB18F36")] // Replace this GUID for your own server
  [ProgId("SampleRtdServer")] // This text will be used in the excel function replace it with you own =RTD("samplertdserver",,"time","1","local time")
  public class TimeZoneRtdServer : RTDServerImpl
  {
      private IPublisher m_publisher;
      private Timer m_timer;

      public TimeZoneRtdServer()
      {
          // Dont do any thing here. use the Initialize function
      }

      protected override void Initialize(IPublisher publisher)
      {
          publisher.RegisterTopic<TimeZoneEntry>("Time", x => x.Id) // The id is used as the second topic paramater to identify the row
              .SubTopic(x => x.Offset) // The topic is the name of the property
              .SubTopic(x => x.Location) // Topic is not case sensitive
              .SubTopic(x => x.Time, "The current time", "Local Time"); // Topic can be overwritten to be what ever like this: Local Time

          m_publisher = publisher;
          m_timer = new Timer(OnTimerTick, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
      }

      protected override void Terminate()
      {
          m_timer.Dispose();
      }

      private void OnTimerTick(object state)
      {
          // Publish is thread safe so feel free to call it from any thread 
          m_publisher.Publish(new TimeZoneEntry()
          {
              Id = 1,
              Location = "Baker Island", // Location is never changed and there for will not cause update event to be raised.
              Offset = -12 * 60,
              InternalTime = DateTime.UtcNow - TimeSpan.FromMinutes(12 * 60) // We only notify excel on topics how has changed.
          });

          m_publisher.Publish(new TimeZoneEntry()
          {
              Id = 2,
              Location = "Jamaica",
              Offset = -5 * 60,
              InternalTime = DateTime.UtcNow - TimeSpan.FromMinutes(-5 * 60)
          });

          m_publisher.Publish(new TimeZoneEntry() { Id = 3, Location = "Iceland", Offset = 0, InternalTime = DateTime.UtcNow });

          m_publisher.Publish(new TimeZoneEntry()
          {
              Id = 4,
              Location = "Afghanistan",
              Offset = (int)(4.5 * 60),
              InternalTime = DateTime.UtcNow + TimeSpan.FromMinutes(4.5 * 60)
          });

          m_publisher.Publish(new TimeZoneEntry()
          {
              Id = 5,
              Location = "Kiribati",
              Offset = 14 * 60,
              InternalTime = DateTime.UtcNow + TimeSpan.FromMinutes(14 * 60)
          });
      }
  }
}
