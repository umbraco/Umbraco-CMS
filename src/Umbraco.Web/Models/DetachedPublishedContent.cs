using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
	public class DetachedPublishedContent : PublishedContentBase
	{
		private readonly string _name;
		private readonly PublishedContentType _contentType;
		private readonly IEnumerable<IPublishedProperty> _properties;
		private readonly bool _isPreviewing;

		public DetachedPublishedContent(string name,
			PublishedContentType contentType,
			IEnumerable<IPublishedProperty> properties,
			bool isPreviewing = false)
		{
			_name = name;
			_contentType = contentType;
			_properties = properties;
			_isPreviewing = isPreviewing;
		}

		public override int Id
		{
			get { return 0; }
		}

		public override string Name
		{
			get { return _name; }
		}

		public override bool IsDraft
		{
			get { return _isPreviewing; }
		}

		public override PublishedItemType ItemType
		{
			get { return PublishedItemType.Content; }
		}

		public override PublishedContentType ContentType
		{
			get { return _contentType; }
		}

		public override string DocumentTypeAlias
		{
			get { return _contentType.Alias; }
		}

		public override int DocumentTypeId
		{
			get { return _contentType.Id; }
		}

		public override ICollection<IPublishedProperty> Properties
		{
			get { return _properties.ToArray(); }
		}

		public override IPublishedProperty GetProperty(string alias)
		{
			return _properties.FirstOrDefault(x => x.PropertyTypeAlias.InvariantEquals(alias));
		}

		public override IPublishedProperty GetProperty(string alias, bool recurse)
		{
			if (recurse)
				throw new NotSupportedException();
			
			return GetProperty(alias);
		}

		public override IPublishedContent Parent
		{
			get { return null; }
		}

		public override IEnumerable<IPublishedContent> Children
		{
			get { return Enumerable.Empty<IPublishedContent>(); }
		}

		public override int TemplateId
		{
			get { return 0; }
		}

		public override int SortOrder
		{
			get { return 0; }
		}

		public override string UrlName
		{
			get { return null; }
		}

		public override string WriterName
		{
			get { return null; }
		}

		public override string CreatorName
		{
			get { return null; }
		}

		public override int WriterId
		{
			get { return 0; }
		}

		public override int CreatorId
		{
			get { return 0; }
		}

		public override string Path
		{
			get { return null; }
		}

		public override DateTime CreateDate
		{
			get { return DateTime.MinValue; }
		}

		public override DateTime UpdateDate
		{
			get { return DateTime.MinValue; }
		}

		public override Guid Version
		{
			get { return Guid.Empty; }
		}

		public override int Level
		{
			get { return 0; }
		}
	}
}
