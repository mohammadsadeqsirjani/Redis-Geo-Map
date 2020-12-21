using RedisGeo.ServiceModel;
using ServiceStack;
using ServiceStack.Redis;

namespace RedisGeo.ServiceInterface
{
    public class RedisGeoServices : Service
    {
        public object Any(FindGeoResults request)
        {
            var results = Redis.FindGeoResultsInRadius(request.State,
                request.Lng, request.Lat,
                request.WithinKm.GetValueOrDefault(20), RedisGeoUnit.Kilometers,
                sortByNearest: true);

            return results;
        }
    }
}