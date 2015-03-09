using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RTDServer
{
  public interface IPublisherTopic<T>
  {
    IPublisherTopic<T> SubTopic(Func<T, object> property, string description, string name);
    IPublisherTopic<T> SubTopic(Expression<Func<T, object>> property, string description);
    IPublisherTopic<T> SubTopic(Expression<Func<T, object>> property);
  }

  public interface IPublisher
  {
    IPublisherTopic<T> RegisterTopic<T>(string topic,Func<T, object> key) where T : class;
    IPublisherTopic<T> RegisterTopic<T>(string topic,Func<T, object> key,string description) where T : class;

    void Publish<T>(T instances) where T : class;
  }
}