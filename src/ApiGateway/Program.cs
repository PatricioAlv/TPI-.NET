using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Configurar CORS para permitir Vercel y ngrok
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// Proxy inverso simple
var httpClient = new HttpClient();

// Redirigir /api/auth/* al puerto 5001
app.Map("/api/auth", authApp =>
{
    authApp.Run(async context =>
    {
        var targetUrl = $"http://localhost:5001{context.Request.Path}{context.Request.QueryString}";
        var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUrl);
        
        // Copiar headers
        foreach (var header in context.Request.Headers)
        {
            if (!header.Key.StartsWith(":") && header.Key != "Host")
                request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
        
        // Copiar body si existe
        if (context.Request.ContentLength > 0)
        {
            request.Content = new StreamContent(context.Request.Body);
            if (context.Request.ContentType != null)
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
        }
        
        var response = await httpClient.SendAsync(request);
        
        context.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();
        
        await response.Content.CopyToAsync(context.Response.Body);
    });
});

// Redirigir /api/messages/* al puerto 5002
app.Map("/api/messages", messagesApp =>
{
    messagesApp.Run(async context =>
    {
        var targetUrl = $"http://localhost:5002{context.Request.Path}{context.Request.QueryString}";
        var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUrl);
        
        foreach (var header in context.Request.Headers)
        {
            if (!header.Key.StartsWith(":") && header.Key != "Host")
                request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
        
        if (context.Request.ContentLength > 0)
        {
            request.Content = new StreamContent(context.Request.Body);
            if (context.Request.ContentType != null)
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
        }
        
        var response = await httpClient.SendAsync(request);
        
        context.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();
        
        await response.Content.CopyToAsync(context.Response.Body);
    });
});

// Redirigir /api/groups/* al puerto 5003
app.Map("/api/groups", groupsApp =>
{
    groupsApp.Run(async context =>
    {
        var targetUrl = $"http://localhost:5003{context.Request.Path}{context.Request.QueryString}";
        var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUrl);
        
        foreach (var header in context.Request.Headers)
        {
            if (!header.Key.StartsWith(":") && header.Key != "Host")
                request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
        
        if (context.Request.ContentLength > 0)
        {
            request.Content = new StreamContent(context.Request.Body);
            if (context.Request.ContentType != null)
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
        }
        
        var response = await httpClient.SendAsync(request);
        
        context.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers)
            context.Response.Headers[header.Key] = header.Value.ToArray();
        
        await response.Content.CopyToAsync(context.Response.Body);
    });
});

// Proxy WebSocket para SignalR /hubs/chat
app.Map("/hubs", hubsApp =>
{
    hubsApp.Run(async context =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            // Para WebSockets, redirigir al puerto 5002
            context.Response.Redirect($"http://localhost:5002{context.Request.Path}");
        }
        else
        {
            var targetUrl = $"http://localhost:5002{context.Request.Path}{context.Request.QueryString}";
            var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUrl);
            
            foreach (var header in context.Request.Headers)
            {
                if (!header.Key.StartsWith(":") && header.Key != "Host")
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
            
            if (context.Request.ContentLength > 0)
            {
                request.Content = new StreamContent(context.Request.Body);
                if (context.Request.ContentType != null)
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
            }
            
            var response = await httpClient.SendAsync(request);
            
            context.Response.StatusCode = (int)response.StatusCode;
            foreach (var header in response.Headers)
                context.Response.Headers[header.Key] = header.Value.ToArray();
            
            await response.Content.CopyToAsync(context.Response.Body);
        }
    });
});

app.MapGet("/", () => "API Gateway corriendo - Puerto 8000");

app.Run("http://localhost:8000");
