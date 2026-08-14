FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UmbracoProject/UmbracoProject.csproj", "UmbracoProject/"]
#if (UseCPM)
COPY ["UmbracoProject/Directory.Packages.props", "UmbracoProject/"]
#endif
RUN dotnet restore "UmbracoProject/"
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

# Copy entrypoint script
COPY UmbracoProject/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Make APP_UID available at runtime (inherits from .NET base image ARG)
ARG APP_UID
ENV APP_UID=${APP_UID}

# Create Umbraco directories as a fallback so they exist, and can be chowned by the entrypoint, for both bind-mounted and regular volumes
RUN mkdir -p umbraco/Logs && chown -R $APP_UID:$APP_UID umbraco

# Run as root initially - entrypoint will fix permissions and drop to APP_UID
ENTRYPOINT ["/entrypoint.sh"]
