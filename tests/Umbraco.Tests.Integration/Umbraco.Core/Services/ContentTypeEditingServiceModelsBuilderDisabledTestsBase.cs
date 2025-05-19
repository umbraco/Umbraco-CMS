using Umbraco.Cms.Infrastructure.ModelsBuilder.Options;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Unlike <see cref="ContentTypeEditingServiceModelsBuilderEnabledTestsBase"/> this testbase does not configure the modelsbuilder based <see cref="ConfigurePropertySettingsOptions"/>
/// which has the same effect as disabling it completely as <see cref="ContentTypeEditingServiceModelsBuilderEnabledTestsBase"/> only loads in that part anyway.
/// </summary>
public class ContentTypeEditingServiceModelsBuilderDisabledTestsBase : ContentTypeEditingServiceTestsBase
{
}
