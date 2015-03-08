using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RTDServer
{
    class PublisherTopic<T> : IPublisherTopic<T>
    {
        private Dictionary<string, PropertyValueExtractor> m_valueExtractors;
        private readonly object m_lockObject;
        private Publisher m_publisher;
        private string m_topicParent;

        public PublisherTopic(Dictionary<string, PropertyValueExtractor> valueExtractors, Publisher publisher, object lockObject,string topicParent)
        {
            m_topicParent = topicParent;
            m_publisher = publisher;
            m_valueExtractors = valueExtractors;
            m_lockObject = lockObject;
        }

        public IPublisherTopic<T> SubTopic(Func<T, object> property, string description, string name)
        {
            lock (m_lockObject)
            {
                m_valueExtractors.Add(name.ToUpper(), new PropertyValueExtractor()
                {
                    PropertyTopic = name.ToUpper(),
                    PropertyTopicNiceName = name,
                    Description = description,
                    ValueExtractor = o => property.Invoke((T)o),
                });

                m_publisher.Publish(new HelpDescription(m_publisher, m_topicParent));
            }
            return this;
        }

        public IPublisherTopic<T> SubTopic(Expression<Func<T, object>> property, string description)
        {
            return SubTopic(property.Compile(), description, ObjectHelper.GetMemberName(property));
        }

        public IPublisherTopic<T> SubTopic(Expression<Func<T, object>> property)
        {
            return SubTopic(property.Compile(), string.Empty, ObjectHelper.GetMemberName(property));
        }
    }
}