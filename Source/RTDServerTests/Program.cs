using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RTDServerTests
{
    [Guid("A43788C1-D91B-11D3-8F39-00C04F3651B8")]
    public interface IRTDUpdateEvent
    {
        void UpdateNotify();

        int HeartbeatInterval { get; set; }

        void Disconnect();
    }

    [Guid("EC0E6191-DB51-11D3-8F3E-00C04F3651B8")]
    public interface IRtdServer
    {
        int ServerStart(IRTDUpdateEvent callback);

        object ConnectData(int topicId,
                           [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] ref Array strings,
                           ref bool newValues);

        [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
        Array RefreshData(ref int topicCount);

        void DisconnectData(int topicId);

        int Heartbeat();

        void ServerTerminate();
    }

    internal class Callback : IRTDUpdateEvent
    {
        private dynamic m_rtdServer;

        public Callback(dynamic rtdServer)
        {
            m_rtdServer = rtdServer;
            HeartbeatInterval = 5;
        }

        public void UpdateNotify()
        {
            int refCount = 0;
            object[,] array = m_rtdServer.RefreshData(ref refCount);

            for (int i = 0; i < refCount; i++)
            {
                Console.WriteLine("Topic: " + array[0, i] + " Value: " + array[1, i]);
            }
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnect was called");
        }

        public int HeartbeatInterval { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create the RTD server.
            Type rtd;
            //Object rtdServer = null;
            rtd = Type.GetTypeFromProgID("SampleRtdServer");
            dynamic rtdServer = Activator.CreateInstance(rtd);
            Console.WriteLine("rtdServer = {0}", rtdServer.ToString());

            rtdServer.ServerStart(new Callback(rtdServer));

            bool newValues = false;
            var topics = (Array) new object[] {"Time", "4", "Local Time"};
            Console.WriteLine("ConnectDataValue: {0}", rtdServer.ConnectData(1, ref topics, ref newValues));

            Console.ReadLine();
            
            topics = (Array)new object[] { "Time", "3", "Local Time" };
            Console.WriteLine("ConnectDataValue: {0}", rtdServer.ConnectData(2, ref topics, ref newValues));
            Console.ReadLine();
        }
    }
}
