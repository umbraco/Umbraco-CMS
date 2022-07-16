using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Xml.XPath;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Navigable;

internal class NavigableContent : INavigableContent
{
    private readonly string?[] _builtInValues;
    private readonly PublishedContent _content;

    public NavigableContent(IPublishedContent content)
    {
        InnerContent = content;
        _content = PublishedContent.UnwrapIPublishedContent(InnerContent);

        var i = 0;
        _builtInValues = new[]
        {
            XmlString(i++, _content.Name), XmlString(i++, _content.ParentId), XmlString(i++, _content.CreateDate),
            XmlString(i++, _content.UpdateDate), XmlString(i++, true), // isDoc
            XmlString(i++, _content.SortOrder), XmlString(i++, _content.Level), XmlString(i++, _content.TemplateId),
            XmlString(i++, _content.WriterId), XmlString(i++, _content.CreatorId), XmlString(i++, _content.UrlSegment),
            XmlString(i, _content.IsDraft()),
        };
    }

    #region INavigableContent

    public IPublishedContent InnerContent { get; }

    private string? XmlString(int index, object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        INavigableFieldType field = Type.FieldTypes[index];
        return field.XmlStringConverter == null ? value.ToString() : field.XmlStringConverter(value);
    }

    public int Id => _content.Id;

    public int ParentId => _content.ParentId;

    public INavigableContentType Type => NavigableContentType.GetContentType(_content.ContentType);

    // returns all child ids, will be filtered by the source
    public IList<int>? ChildIds => _content.ChildIds;

    public object? Value(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index < NavigableContentType.BuiltinProperties.Length)
        {
            // built-in field, ie attribute
            // return XmlString(index, _builtInValues1[index]);
            return _builtInValues[index];
        }

        index -= NavigableContentType.BuiltinProperties.Length;
        IPublishedProperty[] properties = _content.PropertiesArray;
        if (index >= properties.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        // custom property, ie element
        return properties[index].GetXPathValue();
    }

    #endregion
}
