FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/Auth.Service/Auth.Service.csproj", "Services/Auth.Service/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Services/Auth.Service/Auth.Service.csproj"
COPY src/ .
WORKDIR "/src/Services/Auth.Service"
RUN dotnet build "Auth.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.Service.dll"]
