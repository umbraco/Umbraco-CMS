FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UmbracoProject/UmbracoProject.csproj", "UmbracoProject/"]
RUN dotnet restore "UmbracoProject/UmbracoProject.csproj"
COPY . .
WORKDIR "/src/UmbracoProject"
RUN dotnet build "UmbracoProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "UmbracoProject.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install gosu for dropping privileges and openssl for certificate generation
USER root
RUN apt-get update && apt-get install -y --no-install-recommends gosu openssl && rm -rf /var/lib/apt/lists/*

# Generate self-signed certificate for HTTPS
RUN mkdir -p /https && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
      -keyout /https/aspnetcore.key \
      -out /https/aspnetcore.crt \
      -subj "/CN=localhost" \
      -addext "subjectAltName=DNS:localhost,DNS:myproject" && \
    openssl pkcs12 -export -out /https/aspnetcore.pfx \
      -inkey /https/aspnetcore.key \
      -in /https/aspnetcore.crt \
      -password pass:DevOnlyPassword && \
    chmod 644 /https/aspnetcore.pfx

# Copy entrypoint script
COPY UmbracoProject/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Create directories that may be needed (for non-bind-mount volumes)
RUN mkdir -p umbraco/Logs && chown -R $APP_UID:$APP_UID umbraco

# Run as root initially - entrypoint will fix permissions and drop to APP_UID
ENTRYPOINT ["/entrypoint.sh"]
