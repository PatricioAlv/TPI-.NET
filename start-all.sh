#!/bin/sh

# Iniciar todos los servicios en background
cd /app/auth && dotnet Auth.Service.dll --urls "http://0.0.0.0:5001" &
cd /app/messages && dotnet Messages.Service.dll --urls "http://0.0.0.0:5002" &
cd /app/groups && dotnet Groups.Service.dll --urls "http://0.0.0.0:5003" &

# Esperar un momento para que los servicios backend inicien
sleep 5

# Iniciar UI en foreground (Railway necesita un proceso principal)
cd /app/ui && exec dotnet UI.dll --urls "http://0.0.0.0:8080"
