var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5500",
                "http://localhost:8080",
                "https://tpi-messaging-frontend-isriwh72n-patricios-projects-3063c8f8.vercel.app",
                "https://tpi-messaging-frontend-fetasdbzg-patricios-projects-3063c8f8.vercel.app",
                "https://tpi-messaging-frontend-pq31dgayj-patricios-projects-3063c8f8.vercel.app",
                "https://tpi-messaging-frontend-gqzktpkfh-patricios-projects-3063c8f8.vercel.app",
                "https://tpi-messaging-frontend-cb4fykw0t-patricios-projects-3063c8f8.vercel.app",
                "https://tpi-messaging-frontend-2nf0aiotg-patricios-projects-3063c8f8.vercel.app",
                "https://uncranked-linelike-bryanna.ngrok-free.dev"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "PathPattern", "/api/auth/{**catch-all}" }
                    }
                }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "messages-route",
                ClusterId = "messages-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/api/messages/{**catch-all}"
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "PathPattern", "/api/messages/{**catch-all}" }
                    }
                }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "groups-route",
                ClusterId = "groups-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/api/groups/{**catch-all}"
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "PathPattern", "/api/groups/{**catch-all}" }
                    }
                }
            },
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "hubs-route",
                ClusterId = "messages-cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/hubs/{**catch-all}"
                },
                Transforms = new List<IReadOnlyDictionary<string, string>>
                {
                    new Dictionary<string, string>
                    {
                        { "PathPattern", "/hubs/{**catch-all}" }
                    }
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
