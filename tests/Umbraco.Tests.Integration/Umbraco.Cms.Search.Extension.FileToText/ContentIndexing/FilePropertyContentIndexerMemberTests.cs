using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using CoreConstants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Extension.FileToText.Tests.ContentIndexing;

internal class FilePropertyContentIndexerMemberTests : FilePropertyContentIndexerTestBase
{
    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private IMemberTypeService MemberTypeService => GetRequiredService<IMemberTypeService>();

    private IMemberService MemberService => GetRequiredService<IMemberService>();

    [Test]
    public async Task Ignores_Files_On_Members()
    {
        IDataType uploadDataType = await DataTypeService.GetAsync(CoreConstants.DataTypes.Guids.UploadGuid)
                                   ?? throw new InvalidOperationException("Could not find the default file upload data type.");

        IMemberType memberType = new MemberTypeBuilder()
            .WithAlias("fileUploadMember")
            .WithName("File Upload Member")
            .AddPropertyGroup()
                .WithAlias("files")
                .WithName("Files")
                .AddPropertyType()
                    .WithAlias(FirstFilePropertyAlias)
                    .WithName("First File")
                    .WithDataTypeId(uploadDataType.Id)
                    .WithPropertyEditorAlias(uploadDataType.EditorAlias)
                    .WithValueStorageType(uploadDataType.DatabaseType)
                    .Done()
                .Done()
            .Build();

        Attempt<ContentTypeOperationStatus> result = await MemberTypeService.CreateAsync(memberType, CoreConstants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True, $"Could not create the member type: {result.Result}");

        IMember member = new MemberBuilder()
            .WithMemberType(memberType)
            .WithName("File Upload Member")
            .WithEmail("member@local")
            .WithLogin("member@local", "Test123456")
            .Build();
        member.SetValue(FirstFilePropertyAlias, await AddLoremIpsumFileToMediaFileSystem(".md"));
        MemberService.Save(member);

        IndexField[] fields = (await FilePropertyContentIndexer.GetIndexFieldsAsync(member, [], false, CancellationToken.None)).ToArray();
        Assert.That(fields, Has.Length.EqualTo(0));
    }
}
