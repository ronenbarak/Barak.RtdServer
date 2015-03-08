using System;

namespace SampleRTDServer
{
  class TimeZoneEntry
  {
    public int Offset { get; set; }
    public string Location { get; set; }
    public DateTime InternalTime { get; set; }
    public string Time { get { return InternalTime.ToString(); }}
    public int Id { get; set; }
    public string PrevTime { get; set; }
  }
}