using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public abstract class ContentListViewServiceTestsBase : UmbracoIntegrationTest
{
    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    protected IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    protected IUserService UserService => GetRequiredService<IUserService>();

    protected async Task<IUser> GetSuperUser()
        => await UserService.GetAsync(Constants.Security.SuperUserKey);

    protected async Task AssertListViewConfiguration(ListViewConfiguration actualConfiguration, Guid expectedListViewDataTypeKey)
    {
        var actualCollectionPropertyAliases = actualConfiguration
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        // The configured list view
        var expectedContentListViewConfig = await GetListViewConfigurationFromListViewDataType(expectedListViewDataTypeKey);
        var expectedCollectionPropertyAliases = expectedContentListViewConfig
            .IncludeProperties
            .Select(p => p.Alias)
            .WhereNotNull()
            .ToArray();

        Assert.That(actualCollectionPropertyAliases, Has.Length.EqualTo(expectedCollectionPropertyAliases.Length));
        Assert.That(expectedCollectionPropertyAliases.SequenceEqual(actualCollectionPropertyAliases), Is.True);
    }

    private async Task<ListViewConfiguration> GetListViewConfigurationFromListViewDataType(Guid dataTypeKey)
    {
        IDataType? dataType = await DataTypeService.GetAsync(dataTypeKey);
        var listViewConfiguration = dataType.ConfigurationObject;
        Assert.That(listViewConfiguration is ListViewConfiguration, Is.True);

        return listViewConfiguration as ListViewConfiguration;
    }
}
