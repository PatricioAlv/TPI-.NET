FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/UI/UI.csproj", "UI/"]
RUN dotnet restore "UI/UI.csproj"
COPY src/UI/ UI/
WORKDIR "/src/UI"
RUN dotnet build "UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Usar configuración de producción para las URLs de los servicios
RUN if [ -f wwwroot/config.production.js ]; then \
    cp wwwroot/config.production.js wwwroot/config.js; \
    fi

ENTRYPOINT ["dotnet", "UI.dll"]
