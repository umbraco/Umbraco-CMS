using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.HealthChecks;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Extensions;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core Umbraco services required to run which may be replaced later in the pipeline.
    /// </summary>
    public static IUmbracoBuilder AddSearchServices(this IUmbracoBuilder builder)
    {
        // populators are not a collection: one cannot remove ours, and can only add more
        // the container can inject IEnumerable<IIndexPopulator> and get them all
        builder.Services.AddSingleton<IIndexPopulator, MemberIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, ContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, PublishedContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, MediaIndexPopulator>();
        return builder;
    }
}
