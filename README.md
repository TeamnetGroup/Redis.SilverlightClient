Redis.SilverlightClient
=======================

##Redis.Silverlight client
  is a library for subscribing to Redis PUB/SUB channels listening on a port
  from range 4502-4534 due to Silverlight network access restrictions.

##SilverlightPolicyServer
  is a component for delivering ClientAccessPolicy file to Silverlight clients on port 943.

    RedisSubscriber
      .SubscribeToChannel("127.0.0.1", 4525, "test-alert")
      .Subscribe(message =>
      {
        //do something with a message
      },
      ex =>
      {
        //handle exception
      });

    RedisSubscriber
      .SubscribeToChannelPattern("127.0.0.1", 4525, "test-*")
      .Subscribe(message =>
      {
        //do something with a message
      },
      ex =>
      {
        //handle exception
      });
