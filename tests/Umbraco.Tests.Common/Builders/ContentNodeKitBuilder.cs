using System;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

namespace Umbraco.Cms.Tests.Common.Builders;

public class ContentNodeKitBuilder : BuilderBase<ContentNodeKit>
{
    private ContentNode _contentNode;
    private int _contentTypeId;
    private ContentData _draftData;
    private ContentData _publishedData;

    public ContentNodeKitBuilder WithContentNode(ContentNode contentNode)
    {
        _contentNode = contentNode;
        return this;
    }

    public ContentNodeKitBuilder WithContentNode(int id, Guid uid, int level, string path, int sortOrder, int parentContentId, DateTime createDate, int creatorId)
    {
        _contentNode = new ContentNode(id, uid, level, path, sortOrder, parentContentId, createDate, creatorId);
        return this;
    }

    public ContentNodeKitBuilder WithContentTypeId(int contentTypeId)
    {
        _contentTypeId = contentTypeId;
        return this;
    }

    public ContentNodeKitBuilder WithDraftData(ContentData draftData)
    {
        _draftData = draftData;
        return this;
    }

    public ContentNodeKitBuilder WithPublishedData(ContentData publishedData)
    {
        _publishedData = publishedData;
        return this;
    }

    public override ContentNodeKit Build()
    {
        var data = new ContentNodeKit(_contentNode, _contentTypeId, _draftData, _publishedData);
        return data;
    }

    /// <summary>
    ///     Creates a ContentNodeKit
    /// </summary>
    /// <param name="contentTypeId"></param>
    /// <param name="id"></param>
    /// <param name="path"></param>
    /// <param name="sortOrder"></param>
    /// <param name="level">
    ///     Optional. Will get calculated based on the path value if not specified.
    /// </param>
    /// <param name="parentContentId">
    ///     Optional. Will get calculated based on the path value if not specified.
    /// </param>
    /// <param name="creatorId"></param>
    /// <param name="uid"></param>
    /// <param name="createDate"></param>
    /// <param name="draftData"></param>
    /// <param name="publishedData"></param>
    /// <returns></returns>
    public static ContentNodeKit CreateWithContent(
        int contentTypeId,
        int id,
        string path,
        int? sortOrder = null,
        int? level = null,
        int? parentContentId = null,
        int creatorId = -1,
        Guid? uid = null,
        DateTime? createDate = null,
        ContentData draftData = null,
        ContentData publishedData = null)
    {
        var pathParts = path.Split(',');
        if (pathParts.Length >= 2)
        {
            parentContentId ??= int.Parse(pathParts[^2]);
        }

        return new ContentNodeKitBuilder()
            .WithContentTypeId(contentTypeId)
            .WithContentNode(id, uid ?? Guid.NewGuid(), level ?? pathParts.Length - 1, path, sortOrder ?? 0, parentContentId.Value, createDate ?? DateTime.Now, creatorId)
            .WithDraftData(draftData)
            .WithPublishedData(publishedData)
            .Build();
    }
}
