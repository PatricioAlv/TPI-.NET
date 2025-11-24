FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar todos los proyectos
COPY ["src/Services/Auth.Service/Auth.Service.csproj", "Services/Auth.Service/"]
COPY ["src/Services/Messages.Service/Messages.Service.csproj", "Services/Messages.Service/"]
COPY ["src/Services/Groups.Service/Groups.Service.csproj", "Services/Groups.Service/"]
COPY ["src/UI/UI.csproj", "UI/"]
COPY ["src/Shared/Shared.csproj", "Shared/"]

# Restaurar dependencias
RUN dotnet restore "Services/Auth.Service/Auth.Service.csproj"
RUN dotnet restore "Services/Messages.Service/Messages.Service.csproj"
RUN dotnet restore "Services/Groups.Service/Groups.Service.csproj"
RUN dotnet restore "UI/UI.csproj"

# Copiar código fuente
COPY src/ .

# Compilar todos los servicios
WORKDIR "/src/Services/Auth.Service"
RUN dotnet publish "Auth.Service.csproj" -c Release -o /app/auth

WORKDIR "/src/Services/Messages.Service"
RUN dotnet publish "Messages.Service.csproj" -c Release -o /app/messages

WORKDIR "/src/Services/Groups.Service"
RUN dotnet publish "Groups.Service.csproj" -c Release -o /app/groups

WORKDIR "/src/UI"
RUN dotnet publish "UI.csproj" -c Release -o /app/ui

# Imagen final
FROM base AS final
WORKDIR /app

# Copiar todos los servicios compilados
COPY --from=build /app/auth ./auth
COPY --from=build /app/messages ./messages
COPY --from=build /app/groups ./groups
COPY --from=build /app/ui ./ui

# Crear script de inicio directamente
RUN echo '#!/bin/sh' > start.sh && \
    echo 'cd /app/auth && dotnet Auth.Service.dll --urls "http://0.0.0.0:5001" &' >> start.sh && \
    echo 'cd /app/messages && dotnet Messages.Service.dll --urls "http://0.0.0.0:5002" &' >> start.sh && \
    echo 'cd /app/groups && dotnet Groups.Service.dll --urls "http://0.0.0.0:5003" &' >> start.sh && \
    echo 'sleep 5' >> start.sh && \
    echo 'cd /app/ui && exec dotnet UI.dll --urls "http://0.0.0.0:8080"' >> start.sh && \
    chmod +x start.sh

ENTRYPOINT ["/bin/sh", "/app/start.sh"]
