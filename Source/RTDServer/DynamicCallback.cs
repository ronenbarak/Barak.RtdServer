using System;
using System.Reflection;

namespace RTDServer
{
    public class CallbackInvoker
    {
        private Action m_updateNotify;
        private Action m_disconnect;

        public CallbackInvoker(Action updateNotify, Action disconnect)
        {
            m_disconnect = disconnect;
            m_updateNotify = updateNotify;
        }

        public void UpdateNotify()
        {
            try
            {
                m_updateNotify.Invoke();
            }
            catch (Exception)
            {
            }
        }

        public int HeartbeatInterval { get { return 0; } set {  } }
        
        public void Disconnect()
        {
            m_disconnect.Invoke();
        }
    }
}