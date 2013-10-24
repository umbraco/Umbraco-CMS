using System;

namespace umbraco.interfaces
{
    [Obsolete("Use the TagService to query tags or the UmbracoHelper on the front-end and use the SetTags, RemoveTags extension methods on IContentBase to manipulate tags")]
	public interface ITag
	{
		int Id { get; }
		string TagCaption { get; }
		string Group { get; }
	}
}