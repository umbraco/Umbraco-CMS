FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ConnectionStrings__umbracoDbDSN="Server=umb_database;Database=umbracoDb;User Id=sa;Password=Password1234;TrustServerCertificate=true;"
ENV ConnectionStrings__umbracoDbDSN_ProviderName="Microsoft.Data.SqlClient"

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UmbracoProject/UmbracoProject.csproj", "UmbracoProject/"]
# These are for local development, remove these please.
RUN dotnet nuget add source "https://www.myget.org/F/umbracoprereleases/api/v3/index.json" -n "Umbraco Prerelease"
COPY LocalFeed /packages
RUN dotnet nuget add source "/packages" -n "Local Feed"
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
# We need to make sure that the user running the app has access to the wwwroot and umbraco folder
# Since these are volumes they are created as root by the docker daemon
USER root
RUN mkdir umbraco
RUN mkdir umbraco/Logs
RUN chown $APP_UID umbraco --recursive
USER $APP_UID
ENTRYPOINT ["dotnet", "UmbracoProject.dll"]
