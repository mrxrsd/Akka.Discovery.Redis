# Redis Discovery
This module can be used as a discovery method for an Redis Discovery based cluster.

To use `Akka.Discovery.Redis` in your project, you must also include `Akka.Discovery` in your project nuget package dependency.
You will also need to include these HOCON settings in your HOCON configuration:
```
akka.discovery {
  # Set the following in your application.conf if you want to use this discovery mechanism:
  # method = redis
  redis {
    class = "Akka.Discovery.Redis, Akka.Discovery.Redis"

    # connection string, as described here: https://stackexchange.github.io/StackExchange.Redis/Configuration#basic-configuration-strings
    configuration-string = ""

    # Redis discovery key. Leave it for default or change it to appropriate value. WARNING: don't change it on production instances.
    discovery-key = ""
    }    
}

```
