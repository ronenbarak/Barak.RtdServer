using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RTDServer
{
  public abstract class ServerProxy
  {
      private bool m_isOk = true;
      private Publisher m_publisher;

      protected abstract void Initialize(IPublisher publisher);
      protected abstract void Terminate();

      public int ServerStart(CallbackInvoker callbackObject)
      {
        m_publisher = new Publisher(callbackObject);

        m_publisher.RegisterTopic<HelpDescription>("-Help", x=>x.Topic).SubTopic(x=>x.Help,string.Empty,string.Empty);

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

      private List<string> emptyList = new List<string>();
      public object ConnectData(int TopicID, ref Array Strings, ref bool GetNewValues)
      {
        GetNewValues = true;
        return m_publisher.ConnectData(TopicID, Strings == null ? emptyList : new List<string>(Strings.Cast<string>()));
      }

      public Array RefreshData(ref int TopicCount)
      {
        object[,] data = m_publisher.GetChanges(out TopicCount);
        return data;
      }

      public void DisconnectData(int TopicID)
      {
        m_publisher.DisconnectData(TopicID);
      }

      public int Heartbeat()
      {
        return m_isOk ? 1 : 0;
      }

      public void ServerTerminate()
      {
        m_isOk = false;
        Terminate();
      }
  }
}
