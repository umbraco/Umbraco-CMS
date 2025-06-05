FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Umbraco.Web.UI/Umbraco.Web.UI.csproj", "Umbraco.Web.UI/"]
RUN dotnet restore "Umbraco.Web.UI/Umbraco.Web.UI.csproj"
COPY . .
WORKDIR "/src/Umbraco.Web.UI"
RUN dotnet build "Umbraco.Web.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Umbraco.Web.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# We need to make sure that the user running the app has write access to the umbraco folder, in order to write logs and other files.
# Since these are volumes they are created as root by the docker daemon.
USER root
RUN mkdir umbraco
RUN mkdir umbraco/Logs
RUN chown $APP_UID umbraco --recursive
#if (UmbracoRelease = 'LTS')
RUN chown $APP_UID wwwroot/umbraco --recursive
#endif
USER $APP_UID
ENTRYPOINT ["dotnet", "Umbraco.Web.UI.dll"]
