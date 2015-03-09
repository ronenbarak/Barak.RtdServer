using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RTDServer;

namespace RTDServerTests
{
    internal class Callback : IRTDUpdateEvent
    {
        private IRtdServer m_rtdServer;

        public Callback(IRtdServer rtdServer)
        {
            m_rtdServer = rtdServer;
            HeartbeatInterval = 5;
        }

        public void UpdateNotify()
        {
            int refCount = 0;
            object[,] array = (object[,])m_rtdServer.RefreshData(ref refCount);

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
            IRtdServer rtdServer = Activator.CreateInstance(rtd) as IRtdServer;

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
