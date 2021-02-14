using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Providers;
using Umbraco.Core.Configuration.Models;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.Common.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Image Sharp with Umbraco settings
        /// </summary>
        public static IServiceCollection AddUmbracoImageSharp(this IServiceCollection services, IConfiguration configuration)
        {
            var imagingSettings = configuration.GetSection(Core.Constants.Configuration.ConfigImaging)
                .Get<ImagingSettings>() ?? new ImagingSettings();

            services.AddImageSharp(options =>
                    {
                        options.Configuration = SixLabors.ImageSharp.Configuration.Default;
                        options.BrowserMaxAge = imagingSettings.Cache.BrowserMaxAge;
                        options.CacheMaxAge = imagingSettings.Cache.CacheMaxAge;
                        options.CachedNameLength = imagingSettings.Cache.CachedNameLength;
                        options.OnParseCommandsAsync = context =>
                        {
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Width, imagingSettings.Resize.MaxWidth);
                            RemoveIntParamenterIfValueGreatherThen(context.Commands, ResizeWebProcessor.Height, imagingSettings.Resize.MaxHeight);

                            return Task.CompletedTask;
                        };
                        options.OnBeforeSaveAsync = _ => Task.CompletedTask;
                        options.OnProcessedAsync = _ => Task.CompletedTask;
                        options.OnPrepareResponseAsync = _ => Task.CompletedTask;
                    })
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetMemoryAllocator(provider => ArrayPoolMemoryAllocator.CreateWithMinimalPooling())
                .Configure<PhysicalFileSystemCacheOptions>(options =>
                {
                    options.CacheFolder = imagingSettings.Cache.CacheFolder;
                })
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .AddProvider<PhysicalFileSystemProvider>()
                .AddProcessor<ResizeWebProcessor>()
                .AddProcessor<FormatWebProcessor>()
                .AddProcessor<BackgroundColorWebProcessor>();

            return services;
        }

        /// <summary>
        /// Adds the services required for using Members Identity
        /// </summary>
        public static void AddMembersIdentity(this IServiceCollection services) =>
            services.BuildMembersIdentity()
                .AddDefaultTokenProviders()
                .AddUserStore<MembersUserStore>()
                .AddMembersManager<IMemberManager, MemberManager>();


        private static MembersIdentityBuilder BuildMembersIdentity(this IServiceCollection services)
        {
            // Services used by Umbraco members identity
            services.TryAddScoped<IUserValidator<MembersIdentityUser>, UserValidator<MembersIdentityUser>>();
            services.TryAddScoped<IPasswordValidator<MembersIdentityUser>, PasswordValidator<MembersIdentityUser>>();
            services.TryAddScoped<IPasswordHasher<MembersIdentityUser>, PasswordHasher<MembersIdentityUser>>();
            return new MembersIdentityBuilder(services);
        }

        private static void RemoveIntParamenterIfValueGreatherThen(IDictionary<string, string> commands, string parameter, int maxValue)
        {
            if (commands.TryGetValue(parameter, out var command))
            {
                if (int.TryParse(command, out var i))
                {
                    if (i > maxValue)
                    {
                        commands.Remove(parameter);
                    }
                }
            }
        }
    }
}
