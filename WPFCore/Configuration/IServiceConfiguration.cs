using System.Collections.Generic;
using System.Security.RightsManagement;

namespace WPFClient.Configuration;

public interface IServiceConfiguration
{
    string GetEndpoint(string endp_name);
    string ServiceAddress { get; set; }
    RedisDb RedisCache { set; get; }
    Dictionary<string, string> Endpoints { get; set; }
}

public class RedisDb
{
    public string ConnectionString { set; get; }
    public int DbId { set; get; } = 0;
}
