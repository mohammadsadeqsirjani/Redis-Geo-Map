using Funq;
using RedisGeo.ServiceInterface;
using ServiceStack;
using ServiceStack.Redis;
using System.Collections.Generic;
using System.IO;

namespace RedisGeo
{
    public class AppHost : AppHostBase
    {
        /// <summary>
        /// Configure your ServiceStack AppHost singleton instance:
        /// Call base constructor with App Name and assembly where Service classes are located
        /// </summary>
        public AppHost()
            : base("RedisGeo", typeof(RedisGeoServices).Assembly) { }

        public override void Configure(Container container)
        {
            container.Register<IRedisClientsManager>(c =>
                new RedisManagerPool(AppSettings.Get("REDIS_HOST", "localhost")));

            ImportCountry(container.Resolve<IRedisClientsManager>(), "US");
        }

        public void ImportCountry(IRedisClientsManager redisManager, string countryCode)
        {
            using var redis = redisManager.GetClient();
            using var reader = new StreamReader(File.OpenRead(MapProjectPath($"~/Data/{countryCode}.txt")));
            string line, lastState = null, lastCity = null;
            var results = new List<ServiceStack.Redis.RedisGeo>();
            while ((line = reader.ReadLine()) != null)
            {
                var parts = line.Split('\t');
                var city = parts[2];
                var state = parts[4];
                var latitude = double.Parse(parts[9]);
                var longitude = double.Parse(parts[10]);

                if (city == lastCity)
                    continue;

                lastCity = city;

                lastState ??= state;

                if (state != lastState)
                {
                    redis.AddGeoMembers(lastState, results.ToArray());
                    lastState = state;
                    results.Clear();
                }

                results.Add(new ServiceStack.Redis.RedisGeo(longitude, latitude, city));
            }
        }
    }
}