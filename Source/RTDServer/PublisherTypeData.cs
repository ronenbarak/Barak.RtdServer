using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RTDServer
{
  class PublisherTypeData
  {
    public PublisherTypeData(Func<object, object> keyExtractor, Dictionary<string, PropertyValueExtractor> topics)
    {
      Topics = topics;
      KeyExtractor = keyExtractor;
      Snapshot = new Dictionary<string, InstanceAndProperties>();
    }

    public Func<object,object> KeyExtractor { get; private set; }
    public Dictionary<string, PropertyValueExtractor> Topics { get; private set; }

    public Dictionary<string, InstanceAndProperties> Snapshot { get; private set; }
    public Action<object,object> TransformMethod { get; set; }
    public string Description { get; set; }
  }

  class InstanceAndProperties
  {
    public InstanceAndProperties()
    {
        Properties = new Dictionary<string,PropertyTopicData>();
    }

    public object FullInstance { get; set; }
    public Dictionary<string,PropertyTopicData> Properties { get; set; }

  }
  class PropertyValueExtractor
  {
    public string PropertyTopic { get; set; }
    public Func<object, object> ValueExtractor { get; set; }
    public string PropertyTopicNiceName { get; set; }
    public string Description { get; set; }
  }

  class PropertyTopicData
  {
    public PropertyTopicData()
    {
      SubscribedTopics = new HashSet<int>();
    }
    
    public bool IsSet { get; set; }
    public object Value { get; set; }
    public HashSet<int> SubscribedTopics { get; set; }
  }
}