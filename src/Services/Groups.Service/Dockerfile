FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/Groups.Service/Groups.Service.csproj", "Services/Groups.Service/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Services/Groups.Service/Groups.Service.csproj"
COPY src/ .
WORKDIR "/src/Services/Groups.Service"
RUN dotnet build "Groups.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Groups.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Groups.Service.dll"]
