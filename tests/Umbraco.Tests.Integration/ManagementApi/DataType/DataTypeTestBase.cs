using System.Linq.Expressions;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi;

[TestFixture]
public abstract class DataTypeTestBase<T> : ManagementApiUserGroupTestBase<T> where T : ManagementApiControllerBase
{
    protected override Expression<Func<T, object>> MethodSelector { get; }

    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    protected IDataValueEditorFactory DataValueEditorFactory => GetRequiredService<IDataValueEditorFactory>();

    protected IIOHelper IOHelper => Services.GetRequiredService<IIOHelper>();

    protected IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer =>
        GetRequiredService<IConfigurationEditorJsonSerializer>();

    protected IEditorConfigurationParser EditorConfigurationParser => GetRequiredService<IEditorConfigurationParser>();

    protected IDataTypeContainerService DataTypeContainerService => GetRequiredService<IDataTypeContainerService>();

    protected async Task<Guid> CreateDataType()
    {
        IDataType dataTypeModel = new Core.Models.DataType(new LabelPropertyEditor(DataValueEditorFactory, IOHelper, EditorConfigurationParser), ConfigurationEditorJsonSerializer)
        {
            Name = "TestText",
            DatabaseType = ValueStorageType.Ntext
        };
        await DataTypeService.CreateAsync(dataTypeModel, Constants.Security.SuperUserKey);
        return dataTypeModel.Key;
    }

    protected async Task<Guid> CreateDataTypeFolder()
    {
        var folderId = Guid.NewGuid();
        await DataTypeContainerService.CreateAsync(folderId, "TestFolderName", null, Constants.Security.SuperUserKey);
        return folderId;
    }
}
