using System.Collections.Generic;

namespace WPFClient.Configuration;

public class ServiceConfiguration : IServiceConfiguration
{
    public string GetEndpoint(string endp_name)
    {
        return $"{ServiceAddress}{Endpoints[endp_name]}";
    }
    public string ServiceAddress { get; set; }
    public RedisDb RedisCache { get; set; }
    public Dictionary<string, string> Endpoints { get; set; }
}
