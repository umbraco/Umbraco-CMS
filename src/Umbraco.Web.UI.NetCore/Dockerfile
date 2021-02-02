#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["NuGet.Config", ""]
COPY ["Umbraco.Web.UI.NetCore/Umbraco.Web.UI.NetCore.csproj", "Umbraco.Web.UI.NetCore/"]
#COPY ["Umbraco.Persistence.SqlCe/Umbraco.Persistence.SqlCe.csproj", "Umbraco.Persistence.SqlCe/"]
COPY ["Umbraco.Infrastructure/Umbraco.Infrastructure.csproj", "Umbraco.Infrastructure/"]
COPY ["Umbraco.Core/Umbraco.Core.csproj", "Umbraco.Core/"]
COPY ["Umbraco.ModelsBuilder.Embedded/Umbraco.ModelsBuilder.Embedded.csproj", "Umbraco.ModelsBuilder.Embedded/"]
COPY ["Umbraco.Web.BackOffice/Umbraco.Web.BackOffice.csproj", "Umbraco.Web.BackOffice/"]
COPY ["Umbraco.Web.Common/Umbraco.Web.Common.csproj", "Umbraco.Web.Common/"]
COPY ["Umbraco.PublishedCache.NuCache/Umbraco.PublishedCache.NuCache.csproj", "Umbraco.PublishedCache.NuCache/"]
#COPY ["Umbraco.Examine.Lucene/Umbraco.Examine.Lucene.csproj", "Umbraco.Examine.Lucene/"]
COPY ["Umbraco.Web.Website/Umbraco.Web.Website.csproj", "Umbraco.Web.Website/"]
RUN dotnet restore "Umbraco.Web.UI.NetCore/Umbraco.Web.UI.NetCore.csproj"
COPY . .
WORKDIR "/src/Umbraco.Web.UI.NetCore"
RUN dotnet build "Umbraco.Web.UI.NetCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Umbraco.Web.UI.NetCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Umbraco.Web.UI.NetCore.dll"]
