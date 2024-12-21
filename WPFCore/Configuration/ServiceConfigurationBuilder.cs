using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace WPFClient.Configuration;

public class ServiceConfigurationBuilder
{
    public IServiceConfiguration Config { set; get; }
    public ServiceConfigurationBuilder()
    {
        var c = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("Endpoints.json").Build();

        Config = new ServiceConfiguration();
        var redis = c.GetSection("RedisCache");
        Config.RedisCache = new RedisDb()
        {
            ConnectionString = redis["ConnectionString"],
            DbId = Convert.ToInt32(redis["Database"])
        };
        var services = c.GetSection("Services");
        Config.ServiceAddress = services["ServiceAddress"];
        Config.Endpoints = new Dictionary<string, string>();
        foreach (var ep in services.GetSection("Endpoints").GetChildren())
        {
            Config.Endpoints.Add(ep.Key,ep.Value);
        }

    }
}
