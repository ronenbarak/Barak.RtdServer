using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RTDServer;

namespace SampleRTDServer
{
  [Guid("F6FC1817-6F2C-4F2B-AF31-13E84FB18F36")]
  [ProgId("SampleRtdServer")]
  [ComVisible(true)]
  public class SampleRtdServer : IRtdServer
  {
      private Proxy m_proxy;

      public SampleRtdServer()
      {
          m_proxy = new Proxy();
      }

      public int ServerStart(IRTDUpdateEvent CallbackObject)
      {
          return m_proxy.ServerStart(new CallbackInvoker(() => CallbackObject.UpdateNotify(),() => CallbackObject.Disconnect()));
      }

      public object ConnectData(int TopicID, ref Array Strings, ref bool GetNewValues)
      {
          return m_proxy.ConnectData(TopicID, ref Strings, ref GetNewValues);
      }

      public Array RefreshData(ref int TopicCount)
      {
          return m_proxy.RefreshData(ref TopicCount);
      }

      public void DisconnectData(int TopicID)
      {
          m_proxy.DisconnectData(TopicID);
      }

      public int Heartbeat()
      {
          return m_proxy.Heartbeat();
      }

      public void ServerTerminate()
      {
          m_proxy.ServerTerminate();
      }
  }
}
