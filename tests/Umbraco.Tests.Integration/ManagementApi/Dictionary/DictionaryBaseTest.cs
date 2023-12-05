using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

[TestFixture]
public abstract class DictionaryBaseTest<T> : ManagementApiUserGroupTestBase<T> where T : ManagementApiControllerBase
{
    protected override Expression<Func<T, object>> MethodSelector { get; }

    protected IDictionaryItemService DictionaryItemService => GetRequiredService<IDictionaryItemService>();

    protected IFileService FileService => GetRequiredService<IFileService>();

    protected Guid CreateDictionaryItem(string itemKey)
    {
        IDictionaryItem dictionaryItemModel = new DictionaryItem(itemKey);
        DictionaryItemService.CreateAsync(dictionaryItemModel, Constants.Security.SuperUserKey);
        return dictionaryItemModel.Key;
    }

}
