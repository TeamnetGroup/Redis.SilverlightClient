Redis.SilverlightClient
=======================

##Redis.Silverlight client
  is a library for subscribing to Redis PUB/SUB channels listening on a port
  from range 4502-4534 due to Silverlight network access restrictions.

##SilverlightPolicyServer
  is a component for delivering ClientAccessPolicy file to Silverlight clients on port 943.

---

### SocketConnection as publisher

    using (var connection = new SocketConnection("127.0.0.1", 4525, Scheduler.Immediate))
    {
      var publisher = connection.AsPublisher();

      await publisher.PublishMessage("alert1", textBoxMessage.Text);
      await publisher.PublishMessage("alert2", textBoxMessage.Text);
    }

### SocketConnection as cache client

    using (var connection = new SocketConnection("127.0.0.1", 4525, Scheduler.Immediate))
    {
      var cacheClient = connection.AsCacheClient();

      await cacheClient.SetValue("key1", "value1");
      await cacheClient.SetValue("key2", "value2");

      var value1 = await cacheClient.GetValue("key1");
      var value2 = await cacheClient.GetValue("key2");

      var dictionary = new Dictionary<string, string>();
      dictionary.Add("hash1", "value1");
      dictionary.Add("hash2", "value2");

      await cacheClient.SetValues(dictionary);
      var values = await cacheClient.GetValues(dictionary.Keys);
    }

### SocketConnection as subscriber

    var connection = new SocketConnection("127.0.0.1", 4525, Scheduler.Immediate))
    var subscriber = connection.AsSubscriber();

    var channelsSubscription = await subscriber.Subscribe("alert1", "alert2");
    channelsSubscription.Subscribe(message =>
    {
    },
    ex =>
    {
    });

    var channelsPatternSubscription = await subscriber.PSubscribe("alert*");
    channelsPatternSubscription.Subscribe(message =>
    {
    },
    ex =>
    {
    });

[![Build status](https://ci.appveyor.com/api/projects/status/b6w8dqd56iqh0uc5)](https://ci.appveyor.com/project/vgrigoriu/redis-silverlightclient)
