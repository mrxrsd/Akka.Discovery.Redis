using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using StackExchange.Redis;

namespace Akka.Discovery.Redis
{

    internal sealed class RedisServiceDiscovery : ServiceDiscovery
    {
        private readonly Configuration.Config _config;
        private readonly string _discoveryKey;
        private readonly string _connStr;
        private bool _isClustered;

        public RedisServiceDiscovery(ActorSystem system)
        {
            _config = system.Settings.Config.GetConfig("akka.discovery.redis");
            _discoveryKey = _config.GetString("discovery-key", "akka-cluster-discovery");
            _connStr = _config.GetString("configuration-string");
        }


        private IDatabase RedisClient
        {

            get
            {
                var redisConnection = ConnectionMultiplexer.Connect(_connStr);
                _isClustered = IsClustered(redisConnection);
                return redisConnection.GetDatabase();
            }
        }



        // https://github.com/akkadotnet/Akka.Persistence.Redis/blob/dev/src/Akka.Persistence.Redis/RedisExtensions.cs
        public static bool IsClustered(IConnectionMultiplexer connection)
        {
            return connection.GetEndPoints()
                .Select(endPoint => connection.GetServer(endPoint))
                .Any(server => server.ServerType == ServerType.Cluster);
        }

        public override async Task<Resolved> Lookup(Lookup lookup, TimeSpan resolveTimeout)
        {
            using (var cts = new CancellationTokenSource(resolveTimeout))
            {
                try
                {
                    return await Lookup(lookup, cts.Token);
                }
                catch (TaskCanceledException e)
                {
                    throw new TaskCanceledException($"Lookup for [{lookup}] timed out, within [{resolveTimeout}]", e);
                }
            }
        }

        private async Task<Resolved> Lookup(Lookup query, CancellationToken token)
        {
            var resolvedTargets = (await GetEntries()).Where(x=> x.ServiceName == query.ServiceName && IsValid(x))
                .Select(x => new ResolvedTarget(x.Host,
                                                x.Port,
                                                IPAddress.TryParse(x.Host, out var ipAddress) ? ipAddress : null)
                ).ToList();

            return new Resolved(query.ServiceName, resolvedTargets);
        }

        private Task<IList<DiscoveryEntry>> GetEntries()
        {
            throw new NotImplementedException();
        }

        private bool IsValid(DiscoveryEntry entry)
        {
            return true;
        }
    }


    internal class DiscoveryEntry
    {
        public string ServiceName { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public DateTime LastHeartBeat { get; set; }
    }
}
