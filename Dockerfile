FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar y restaurar UI
COPY src/UI/UI.csproj UI/
RUN dotnet restore "UI/UI.csproj"

# Copiar código y compilar
COPY src/UI/ UI/
WORKDIR "/src/UI"
RUN dotnet publish "UI.csproj" -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UI.dll"]
