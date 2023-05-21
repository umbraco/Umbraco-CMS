using Microsoft.AspNetCore.Builder;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.Common.ApplicationBuilder;

/// <summary>
///     Services used during the Umbraco building phase.
/// </summary>
public interface IUmbracoApplicationBuilderServices
{
    IApplicationBuilder AppBuilder { get; }

    IServiceProvider ApplicationServices { get; }

    IRuntimeState RuntimeState { get; }
}
