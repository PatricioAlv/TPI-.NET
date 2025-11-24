FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/Messages.Service/Messages.Service.csproj", "Services/Messages.Service/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Services/Messages.Service/Messages.Service.csproj"
COPY src/ .
WORKDIR "/src/Services/Messages.Service"
RUN dotnet build "Messages.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Messages.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Messages.Service.dll"]
