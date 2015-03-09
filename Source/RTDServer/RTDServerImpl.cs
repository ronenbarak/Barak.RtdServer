using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RTDServer
{
  public abstract class RTDServerImpl : IRtdServer
  {
      private bool m_isOk = true;
      private Publisher m_publisher;

      protected abstract void Initialize(IPublisher publisher);
      protected abstract void Terminate();

      private List<string> emptyList = new List<string>();
      public int ServerStart(IRTDUpdateEvent callback)
      {
          m_publisher = new Publisher(callback);

          m_publisher.RegisterTopic<HelpDescription>("-Help", x => x.Topic).SubTopic(x => x.Help, string.Empty, string.Empty);

          m_publisher.Publish(new HelpDescription(m_publisher, string.Empty));

          try
          {
              Initialize(m_publisher);
          }
          catch (Exception)
          {
              m_isOk = false;
          }

          return m_isOk ? 1 : 0;
      }

      object IRtdServer.ConnectData(int TopicID, ref Array Strings, ref bool GetNewValues)
      {
        GetNewValues = true;
        return m_publisher.ConnectData(TopicID, Strings == null ? emptyList : new List<string>(Strings.Cast<string>()));
      }

      Array IRtdServer.RefreshData(ref int TopicCount)
      {
        object[,] data = m_publisher.GetChanges(out TopicCount);
        return data;
      }

      void IRtdServer.DisconnectData(int TopicID)
      {
        m_publisher.DisconnectData(TopicID);
      }

      int IRtdServer.Heartbeat()
      {
        return m_isOk ? 1 : 0;
      }

      void IRtdServer.ServerTerminate()
      {
          try
          {
              Terminate();
          }
          finally
          {
              m_isOk = false;
          }
      }
  }
}
