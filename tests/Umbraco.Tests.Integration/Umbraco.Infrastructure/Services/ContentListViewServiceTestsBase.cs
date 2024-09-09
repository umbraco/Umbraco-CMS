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

        Assert.AreEqual(expectedCollectionPropertyAliases.Length, actualCollectionPropertyAliases.Length);
        Assert.IsTrue(expectedCollectionPropertyAliases.SequenceEqual(actualCollectionPropertyAliases));
    }

    private async Task<ListViewConfiguration> GetListViewConfigurationFromListViewDataType(Guid dataTypeKey)
    {
        IDataType? dataType = await DataTypeService.GetAsync(dataTypeKey);
        var listViewConfiguration = dataType.ConfigurationObject;
        Assert.IsTrue(listViewConfiguration is ListViewConfiguration);

        return listViewConfiguration as ListViewConfiguration;
    }
}
