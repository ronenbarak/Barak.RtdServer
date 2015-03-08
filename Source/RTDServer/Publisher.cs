using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Text;
using System.Threading;

namespace RTDServer
{
  class Publisher : IPublisher
  {
    private CallbackInvoker m_callbackObject;
    private SynchronizationContext m_synchronizationContext;

    private object m_lockObject = new object();

    private Dictionary<string, PublisherTypeData> m_headTopicToPublisherData = new Dictionary<string, PublisherTypeData>();
    private Dictionary<int, PropertyTopicData> m_registerTopicIdToTopicData = new Dictionary<int, PropertyTopicData>();
    private Dictionary<Type,string> m_typeToTopic = new Dictionary<Type, string>();
    private HashSet<PropertyTopicData> m_changedObject = new HashSet<PropertyTopicData>();
    private bool m_isPublishedNotify = false;

    public Publisher(CallbackInvoker callbackObject)
    {
      m_callbackObject = callbackObject;
      m_synchronizationContext = SynchronizationContext.Current;
      if (m_synchronizationContext == null)
      {
        m_synchronizationContext = new SynchronizationContext();
      }
    }

    public object[,] GetChanges(out int size)
    {
      lock (m_lockObject)
      {
        List<Tuple<int, object>> values = new List<Tuple<int, object>>();
        foreach (PropertyTopicData messageEnvelope in m_changedObject)
        {
          foreach (var topicId in messageEnvelope.SubscribedTopics)
          {
            values.Add(Tuple.Create(topicId, messageEnvelope.Value));
          }
        }

        object[,] arrayValues = new object[2, values.Count];
        int index = 0;
        foreach (var value in values)
        {
          arrayValues[0, index] = value.Item1;
          arrayValues[1, index] = value.Item2;
          index++;
        }

        m_changedObject.Clear();
        m_isPublishedNotify = false;
        
        size = index;
        return arrayValues;
      }
    }

    private string GetUpperTopicValue(List<string> enumerable, int index)
    {
        if (enumerable.Count > index)
        {
            var type = enumerable[index];
            if (type == null)
            {
                type = string.Empty;
            }
            else
            {
                type = type.ToUpper();
            }

            return type;   
        }
        return string.Empty;
    }

    public object ConnectData(int topicId, List<string> enumerable)
    {
      lock (m_lockObject)
      {
        if (enumerable.Count() > 0)
        {
          var type = GetUpperTopicValue(enumerable, 0);

          PublisherTypeData ptd;
          if (m_headTopicToPublisherData.TryGetValue(type, out ptd))
          {
            var key = GetUpperTopicValue(enumerable, 1);

            InstanceAndProperties backendInstance;
            if (!ptd.Snapshot.TryGetValue(key, out backendInstance))
            {
                ptd.Snapshot.Add(key, backendInstance = new InstanceAndProperties());
            }

            var propertyName = GetUpperTopicValue(enumerable, 2);

            PropertyTopicData backendProperty;
            if (!backendInstance.Properties.TryGetValue(propertyName, out backendProperty))
            {
                backendInstance.Properties.Add(propertyName, backendProperty = new PropertyTopicData());       
            }

            if (m_registerTopicIdToTopicData.ContainsKey(topicId))
            {
                // This is a bug
            }
            else
            {
                m_registerTopicIdToTopicData.Add(topicId,backendProperty);
            }

            backendProperty.SubscribedTopics.Add(topicId);
            return backendProperty.Value ?? string.Empty;
          }
        }
        
        // if you are here there is some thing wrong
        return null;
      }
    }


    public IPublisherTopic<T> RegisterTopic<T>(string topic, Func<T, object> key) where T : class
    {
      return RegisterTopic(topic, key, null, null);
    }

    public IPublisherTopic<T> RegisterTopic<T>(string topic, Func<T, object> key, EntryTransformer<T> transformer) where T : class
    {
        return RegisterTopic(topic, key, transformer, null);
    }

    public IPublisherTopic<T> RegisterTopic<T>(string topic, Func<T, object> key, string description) where T : class
    {
      return RegisterTopic(topic, key, null, null);
    }

    public IPublisherTopic<T> RegisterTopic<T>(string topic, Func<T, object> key, EntryTransformer<T> transformer, string description) where T : class
    {
        var properties = new Dictionary<string, PropertyValueExtractor>();
        m_typeToTopic[typeof(T)] = topic.ToUpper();
        Action<object, object> tranAction = null;
        if (transformer != null)
        {
            tranAction = (o, o1) => transformer.Invoke((T) o, (T) o1);
        }

        m_headTopicToPublisherData[topic.ToUpper()] = new PublisherTypeData(o => key.Invoke((T)o), properties)
        {
            Description = description,
            TransformMethod = tranAction,
        };

        this.Publish(new HelpDescription(this, string.Empty));
        return new PublisherTopic<T>(properties, this, m_lockObject, topic);
    }

    public void Publish<T>(T instance) where T : class
    {
        lock (m_lockObject)
        {
            string typeTopic;
            if (m_typeToTopic.TryGetValue(typeof (T), out typeTopic))
            {
                PublisherTypeData pubTypeData;
                if (m_headTopicToPublisherData.TryGetValue(typeTopic, out pubTypeData))
                {
                    var key = pubTypeData.KeyExtractor.Invoke(instance);

                    InstanceAndProperties instanceAndProperties;
                    var stringkey = key.ToString().ToUpper();
                    if (!pubTypeData.Snapshot.TryGetValue(stringkey, out instanceAndProperties))
                    {
                        pubTypeData.Snapshot.Add(stringkey, instanceAndProperties = new InstanceAndProperties());
                    }

                    if (pubTypeData.TransformMethod != null)
                    {
                        pubTypeData.TransformMethod.Invoke(instance, instanceAndProperties.FullInstance);
                    }
                    instanceAndProperties.FullInstance = instance;

                    foreach (PropertyValueExtractor propertyValueExtractor in pubTypeData.Topics.Values)
                    {
                        bool publish = false;
                        var propertyValue = propertyValueExtractor.ValueExtractor.Invoke(instance);

                        PropertyTopicData propertyTopicData;
                        if (!instanceAndProperties.Properties.TryGetValue(propertyValueExtractor.PropertyTopic, out propertyTopicData))
                        {
                            instanceAndProperties.Properties.Add(propertyValueExtractor.PropertyTopic,
                                propertyTopicData = new PropertyTopicData());
                        }

                        if (propertyTopicData.SubscribedTopics.Count == 0)
                        {
                            propertyTopicData.Value = propertyValue;
                            propertyTopicData.IsSet = true;
                        }
                        else
                        {
                            if (!propertyTopicData.IsSet)
                            {
                                propertyTopicData.IsSet = true;
                                publish = true;
                            }

                            if (!object.Equals(propertyValue, propertyTopicData.Value))
                            {
                                propertyTopicData.Value = propertyValue;
                                publish = true;
                            }

                            if (publish)
                            {
                                m_changedObject.Add(propertyTopicData);
                            }
                        }
                    }

                    if (m_changedObject.Count != 0)
                    {
                        if (!m_isPublishedNotify)
                        {
                            m_isPublishedNotify = true;
                            m_synchronizationContext.Post(state => m_callbackObject.UpdateNotify(), null);
                        }
                    }
                }
            }
        }
    }

    public void DisconnectData(int topicId)
    {
      lock (m_lockObject)
      {
        PropertyTopicData topicData;
        if (m_registerTopicIdToTopicData.TryGetValue(topicId, out topicData))
        {
          m_registerTopicIdToTopicData.Remove(topicId);
          topicData.SubscribedTopics.Remove(topicId);
        }
      }
    }

    public string GetHelp(string topic)
    {
        if (string.IsNullOrEmpty(topic))
        {
            return string.Join(",",m_headTopicToPublisherData.Select(p=>p.Key));
        }
        else
        {
            PublisherTypeData publisherData;
            if (m_headTopicToPublisherData.TryGetValue(topic.ToUpper(), out publisherData))
            {
                return string.Join(",", publisherData.Topics.Select(p => p.Value.PropertyTopicNiceName));
            }
        }
        return string.Empty;
    }
  }
}