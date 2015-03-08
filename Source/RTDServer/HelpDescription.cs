using System.Collections.Generic;
using System.Text;

namespace RTDServer
{
    class HelpDescription
    {
        private Publisher m_publisher;
        public string Topic { get; private set; }

        public HelpDescription(Publisher publisher,string topic)
        {
            Topic = topic;
            m_publisher = publisher;
        }

        public string Help
        {
            get
            {
                return m_publisher.GetHelp(Topic);
            }
        }
    }
}