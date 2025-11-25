var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "auth-route",
                ClusterId = "auth-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/api/auth/{**catch-all}"
                }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "messages-route",
                ClusterId = "messages-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/api/messages/{**catch-all}"
                }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "groups-route",
                ClusterId = "groups-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/api/groups/{**catch-all}"
                }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "hubs-route",
                ClusterId = "messages-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/hubs/{**catch-all}"
                }
            }
        },
        new[]
        {
            new Yarp.ReverseProxy.Configuration.ClusterConfig
            {
                ClusterId = "auth-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "destination1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://localhost:5001" } }
                }
            },
            new Yarp.ReverseProxy.Configuration.ClusterConfig
            {
                ClusterId = "messages-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "destination1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://localhost:5002" } }
                }
            },
            new Yarp.ReverseProxy.Configuration.ClusterConfig
            {
                ClusterId = "groups-cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "destination1", new Yarp.ReverseProxy.Configuration.DestinationConfig { Address = "http://localhost:5003" } }
                }
            }
        });

var app = builder.Build();

app.UseCors();
app.MapReverseProxy();

app.MapGet("/health", () => "OK");

app.Run("http://localhost:8000");
