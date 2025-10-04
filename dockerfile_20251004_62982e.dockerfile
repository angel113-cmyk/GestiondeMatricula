# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Establecer variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos del proyecto y restaurar dependencias
COPY ["GestiondeMatricula.csproj", "."]
RUN dotnet restore "GestiondeMatricula.csproj"

# Copiar todo el código fuente
COPY . .
WORKDIR "/src"

# Compilar la aplicación
RUN dotnet build "GestiondeMatricula.csproj" -c Release -o /app/build

FROM build AS publish
# Publicar la aplicación
RUN dotnet publish "GestiondeMatricula.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copiar los archivos publicados
COPY --from=publish /app/publish .

# Crear directorio para la base de datos SQLite y dar permisos
RUN mkdir -p /app/Data && chmod 755 /app/Data

# Establecer el usuario no-root para seguridad
USER $APP_UID

# Comando de inicio
ENTRYPOINT ["dotnet", "GestiondeMatricula.dll"]