// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Tests.Integration.Implementations;

namespace Umbraco.Cms.Tests.Integration.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     These services need to be manually added because they do not get added by the generic host
    /// </summary>
    public static void AddRequiredNetCoreServices(this IServiceCollection services, TestHelper testHelper, IWebHostEnvironment webHostEnvironment)
    {
        services.AddSingleton(x => testHelper.GetHttpContextAccessor());

        // The generic host does add IHostEnvironment but not this one because we are not actually in a web context
        services.AddSingleton(x => webHostEnvironment);

        // Replace the IHostEnvironment that generic host created too
        services.AddSingleton<IHostEnvironment>(x => webHostEnvironment);
    }
}
