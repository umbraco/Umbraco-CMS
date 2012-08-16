using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Data;

namespace Umbraco.Core.Dynamics
{
	internal class DynamicBackingItem
	{
		private readonly IDocument _content;
		private IEnumerable<DynamicBackingItem> _cachedChildren;

		//internal ExamineBackedMedia media;
		public DynamicBackingItemType Type;

		public DynamicBackingItem(IDocument iNode)
		{
			this._content = iNode;
			this.Type = DynamicBackingItemType.Content;
		}
		//public DynamicBackingItem(ExamineBackedMedia media)
		//{
		//    this.media = media;
		//    this.Type = DynamicBackingItemType.Media;
		//}
		//public DynamicBackingItem(int Id)
		//{
		//    NodeFactory.Node baseNode = new NodeFactory.Node(Id);

		//    this._content = baseNode;
		//    this.Type = DynamicBackingItemType.Content;
		//    if (baseNode.Id == 0 && Id != 0)
		//    {
		//        this.media = ExamineBackedMedia.GetUmbracoMedia(Id);
		//        this.Type = DynamicBackingItemType.Media;
		//        if (this.media == null)
		//        {
		//            this.Type = DynamicBackingItemType.Content;
		//        }
		//        return;
		//    }

		//}
		//public DynamicBackingItem(int Id, DynamicBackingItemType Type)
		//{
		//    NodeFactory.Node baseNode = new NodeFactory.Node(Id);
		//    if (Type == DynamicBackingItemType.Media)
		//    {
		//        this.media = ExamineBackedMedia.GetUmbracoMedia(Id);
		//        this.Type = Type;
		//    }
		//    else
		//    {
		//        this._content = baseNode;
		//        this.Type = Type;
		//    }
		//}

		//public DynamicBackingItem(CMSNode node)
		//{
		//    this._content = (INode)node;
		//    this.Type = DynamicBackingItemType.Content;
		//}

		public bool IsNull()
		{
			//return (_content == null && media == null);
			return (_content == null);
		}
		public IEnumerable<DynamicBackingItem> Children
		{
			get
			{
				if (_cachedChildren == null)
				{
					if (IsNull()) return null;
					if (Type == DynamicBackingItemType.Content)
					{
						var children = _content.Children;
						if (children != null)
						{
							_cachedChildren = children.Select(c => new DynamicBackingItem(c));
						}
					}
					else
					{
						//var children = media.ChildrenAsList.Value;
						//if (children != null)
						//{
						//    return children.ToList().ConvertAll(m => new DynamicBackingItem(m));
						//}

						_cachedChildren = new List<DynamicBackingItem>();
					}
					
				}
				return _cachedChildren;
			}
		}

		public PropertyResult GetProperty(string alias)
		{
			if (IsNull()) return null;
			if (Type == DynamicBackingItemType.Content)
			{
				return GetPropertyInternal(alias, _content);
			}
			//else
			//{
			//    return GetPropertyInternal(alias, media);
			//}
			return null;
		}

		private PropertyResult GetPropertyInternal(string alias, IDocument content)
		{
			var prop = content.GetProperty(alias);
			if (prop != null)
			{
				return new PropertyResult(prop) { ContextAlias = content.DocumentTypeAlias, ContextId = content.Id };
			}
			else
			{
				if (alias.Substring(0, 1).ToUpper() == alias.Substring(0, 1))
				{
					prop = content.GetProperty(alias.Substring(0, 1).ToLower() + alias.Substring((1)));
					if (prop != null)
					{
						return new PropertyResult(prop) { ContextAlias = content.DocumentTypeAlias, ContextId = content.Id };
					}
					else
					{
						//reflect
						object result = null;
						try
						{
							result = content.GetType().InvokeMember(alias,
													  System.Reflection.BindingFlags.GetProperty |
													  System.Reflection.BindingFlags.Instance |
													  System.Reflection.BindingFlags.Public |
													  System.Reflection.BindingFlags.NonPublic,
													  null,
													  content,
													  null);
						}
						catch (MissingMethodException)
						{

						}
						if (result != null)
						{
							return new PropertyResult(alias, string.Format("{0}", result), Guid.Empty) { ContextAlias = content.DocumentTypeAlias, ContextId = content.Id };
						}
					}
				}
			}
			return null;
		}
		//private PropertyResult GetPropertyInternal(string alias, ExamineBackedMedia content)
		//{
		//    bool propertyExists = false;
		//    var prop = content.GetProperty(alias, out propertyExists);
		//    if (prop != null)
		//    {
		//        return new PropertyResult(prop) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
		//    }
		//    else
		//    {
		//        if (alias.Substring(0, 1).ToUpper() == alias.Substring(0, 1) && !propertyExists)
		//        {
		//            prop = content.GetProperty(alias.Substring(0, 1).ToLower() + alias.Substring((1)), out propertyExists);
		//            if (prop != null)
		//            {
		//                return new PropertyResult(prop) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
		//            }
		//            else
		//            {
		//                object result = null;
		//                try
		//                {
		//                    result = content.GetType().InvokeMember(alias,
		//                                              System.Reflection.BindingFlags.GetProperty |
		//                                              System.Reflection.BindingFlags.Instance |
		//                                              System.Reflection.BindingFlags.Public |
		//                                              System.Reflection.BindingFlags.NonPublic,
		//                                              null,
		//                                              content,
		//                                              null);
		//                }
		//                catch (MissingMethodException)
		//                {
		//                }
		//                if (result != null)
		//                {
		//                    return new PropertyResult(alias, string.Format("{0}", result), Guid.Empty) { ContextAlias = content.NodeTypeAlias, ContextId = content.Id };
		//                }
		//            }
		//        }
		//    }
		//    return null;
		//}
		public PropertyResult GetProperty(string alias, out bool propertyExists)
		{
			if (IsNull())
			{
				propertyExists = false;
				return null;
			}
			PropertyResult property = null;
			IDocumentProperty innerProperty = null;
			if (Type == DynamicBackingItemType.Content)
			{
				innerProperty = _content.GetProperty(alias);
				propertyExists = innerProperty != null;
				if (innerProperty != null)
				{
					property = new PropertyResult(innerProperty);
					property.ContextAlias = _content.DocumentTypeAlias;
					property.ContextId = _content.Id;
				}
			}
			else
			{
				//string[] internalProperties = new string[] {
				//    "id", "nodeName", "updateDate", "writerName", "path", "nodeTypeAlias",
				//    "parentID", "__NodeId", "__IndexType", "__Path", "__NodeTypeAlias", 
				//    "__nodeName", "umbracoBytes","umbracoExtension","umbracoFile","umbracoWidth",
				//    "umbracoHeight"
				//};
				//if (media.WasLoadedFromExamine && !internalProperties.Contains(alias) && !media.Values.ContainsKey(alias))
				//{
				//    //examine doesn't load custom properties
				//    innerProperty = media.LoadCustomPropertyNotFoundInExamine(alias, out propertyExists);
				//    if (innerProperty != null)
				//    {
				//        property = new PropertyResult(innerProperty);
				//        property.ContextAlias = media.NodeTypeAlias;
				//        property.ContextId = media.Id;
				//    }
				//}
				//else
				//{
				//    innerProperty = media.GetProperty(alias, out propertyExists);
				//    if (innerProperty != null)
				//    {
				//        property = new PropertyResult(innerProperty);
				//        property.ContextAlias = media.NodeTypeAlias;
				//        property.ContextId = media.Id;
				//    }
				//}
				propertyExists = false;
			}
			return property;
		}

		public PropertyResult GetProperty(string alias, bool recursive)
		{
			bool propertyExists = false;
			return GetProperty(alias, recursive, out propertyExists);
		}
		public PropertyResult GetProperty(string alias, bool recursive, out bool propertyExists)
		{
			if (!recursive)
			{
				return GetProperty(alias, out propertyExists);
			}
			if (IsNull())
			{
				propertyExists = false;
				return null;
			}
			DynamicBackingItem context = this;
			PropertyResult prop = this.GetProperty(alias, out propertyExists);
			while (prop == null || string.IsNullOrEmpty(prop.Value))
			{
				context = context.Parent;
				if (context == null) break;
				prop = context.GetProperty(alias, out propertyExists);
			}
			if (prop != null)
			{
				return prop;
			}
			return null;
		}
		public string GetPropertyValue(string alias)
		{
			var prop = GetProperty(alias);
			if (prop != null) return prop.Value;
			return null;
		}
		public string GetPropertyValue(string alias, bool recursive)
		{
			var prop = GetProperty(alias, recursive);
			if (prop != null) return prop.Value;
			return null;
		}
		public IEnumerable<IDocumentProperty> Properties
		{
			get
			{
				if (IsNull()) return null;
				if (Type == DynamicBackingItemType.Content)
				{
					return _content.Properties;
				}
				//else
				//{
				//    return media.PropertiesAsList;
				//}
				return null;
			}
		}
		//public DataTable ChildrenAsTable()
		//{
		//    if (IsNull()) return null;
		//    if (Type == DynamicBackingItemType.Content)
		//    {
		//        return _content.ChildrenAsTable();
		//    }
		//    else
		//    {
		//        //sorry
		//        return null;
		//    }

		//}
		//public DataTable ChildrenAsTable(string nodeTypeAlias)
		//{
		//    if (IsNull()) return null;
		//    if (Type == DynamicBackingItemType.Content)
		//    {
		//        return _content.ChildrenAsTable(nodeTypeAlias);
		//    }
		//    else
		//    {
		//        //sorry
		//        return null;
		//    }

		//}
		public int Level
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.Level : media.Level; }
			get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.Level : 0; }
		}


		public int Id
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.Id : media.Id; }
			get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.Id : 0; }
		}

		public string NodeTypeAlias
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.NodeTypeAlias : media.NodeTypeAlias; }
			get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.DocumentTypeAlias : null; }
		}

		public DynamicBackingItem Parent
		{
			get
			{
				if (IsNull()) return null;
				if (Type == DynamicBackingItemType.Content)
				{
					var parent = _content.Parent;
					if (parent != null)
					{

						return new DynamicBackingItem(parent);
					}

				}
				//else
				//{
				//    var parent = media.Parent;
				//    if (parent != null && parent.Value != null)
				//    {
				//        return new DynamicBackingItem(parent.Value);
				//    }
				//}
				return null;
			}
		}
		public DateTime CreateDate
		{
			//get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? _content.CreateDate : media.CreateDate; }
			get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? _content.CreateDate : DateTime.MinValue; }
		}
		public DateTime UpdateDate
		{
			//get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? _content.UpdateDate : media.UpdateDate; }
			get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? _content.UpdateDate : DateTime.MinValue; }
		}

		public string WriterName
		{
			get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.WriterName : null; }
		}

		public string Name
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Name : media.Name; }
			get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Name : null; }
		}
		public string nodeName
		{
			get { return Name; }
		}
		public string pageName
		{
			get { return Name; }
		}
		public Guid Version
		{
			//get { if (IsNull()) return Guid.Empty; return Type == DynamicBackingItemType.Content ? _content.Version : media.Version; }
			get { if (IsNull()) return Guid.Empty; return Type == DynamicBackingItemType.Content ? _content.Version : Guid.Empty; }
		}

		//public string Url
		//{
		//    //get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Url : media.Url; }
		//    get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Url : null; }
		//}

		//public string NiceUrl
		//{
		//    //get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.NiceUrl : media.NiceUrl; }
		//    get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.NiceUrl : null; }
		//}

		public string UrlName
		{
			get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.UrlName : null; }
		}

		public int TemplateId
		{
			get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.TemplateId : 0; }
		}

		public int SortOrder
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.SortOrder : media.SortOrder; }
			get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.SortOrder : 0; }
		}


		public string CreatorName
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.CreatorName : media.CreatorName; }
			get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.CreatorName : null; }
		}

		public int WriterId
		{
			get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.WriterId : 0; }
		}

		public int CreatorId
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.CreatorID : media.CreatorID; }
			get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.CreatorId : 0; }
		}

		public string Path
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Path : media.Path; }
			get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Path : null; }
		}

		public IEnumerable<DynamicBackingItem> GetChildren
		{
			get
			{
				if (Type == DynamicBackingItemType.Content)
				{
					var children = _content.Children.ToArray();
					//testing
					if (!children.Any() && _content.Id == 0)
					{
						return new List<DynamicBackingItem>(new DynamicBackingItem[] { this });
					}
					return children.Select(n => new DynamicBackingItem(n));
				}
				//else
				//{
				//    List<ExamineBackedMedia> children = media.ChildrenAsList.Value;
				//    //testing
				//    if (children.Count == 0 && _content.Id == 0)
				//    {
				//        return new List<DynamicBackingItem>(new DynamicBackingItem[] { this });
				//    }
				//    return children.ConvertAll(n => new DynamicBackingItem(n));
				//}
				return Enumerable.Empty<DynamicBackingItem>();
			}
		}


	}
}