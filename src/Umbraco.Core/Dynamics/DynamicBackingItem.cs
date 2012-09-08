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

		public DynamicBackingItem(IDocument document)
		{
			if (document == null) throw new ArgumentNullException("document");

			this._content = document;
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

		public IEnumerable<DynamicBackingItem> Children
		{
			get
			{
				if (_cachedChildren == null)
				{
					if (Type == DynamicBackingItemType.Content)
					{
						var children = _content.Children.ToArray();
						//testing, think this must be a special case for the root node ?
						if (!children.Any() && _content.Id == 0)
						{
							return new List<DynamicBackingItem>(new[] { this });
						}
						return children.Select(n => new DynamicBackingItem(n));
					}
					else
					{
						//var children = media.ChildrenAsList.Value;
						//if (children != null)
						//{
						//    return children.ToList().ConvertAll(m => new DynamicBackingItem(m));
						//}

						_cachedChildren = Enumerable.Empty<DynamicBackingItem>();
					}
					
				}
				return _cachedChildren;
			}
		}		

		private PropertyResult GetPropertyInternal(string alias, IDocument content, bool checkUserProperty = true)
		{
			if (alias.IsNullOrWhiteSpace()) throw new ArgumentNullException("alias");
			if (content == null) throw new ArgumentNullException("content");

			//if we're looking for a user defined property
			if (checkUserProperty)
			{
				var prop = content.GetProperty(alias)
					   ?? (alias[0].IsUpperCase() //if it's null, try to get it with a different casing format (pascal vs camel)
							? content.GetProperty(alias.ConvertCase(StringAliasCaseType.CamelCase))
							: content.GetProperty(alias.ConvertCase(StringAliasCaseType.PascalCase)));

				return prop == null
				       	? null
				       	: new PropertyResult(prop, PropertyResultType.UserProperty)
				       		{
				       			DocumentTypeAlias = content.DocumentTypeAlias,
				       			DocumentId = content.Id
				       		};				
			}

			//reflect
			
			Func<string, Attempt<object>> getMember =
				memberAlias =>
					{
						try
						{
							return new Attempt<object>(true,
							                           content.GetType().InvokeMember(memberAlias,
							                                                          System.Reflection.BindingFlags.GetProperty |
							                                                          System.Reflection.BindingFlags.Instance |
							                                                          System.Reflection.BindingFlags.Public							                                                          ,
							                                                          null,
							                                                          content,
							                                                          null));
						}
						catch (MissingMethodException ex)
						{
							return new Attempt<object>(ex);
						}
					};

			//try with the current casing
			var attempt = getMember(alias);
			if (!attempt.Success)
			{
				//if we cannot get with the current alias, try changing it's case
				attempt = alias[0].IsUpperCase()
					? getMember(alias.ConvertCase(StringAliasCaseType.CamelCase))
					: getMember(alias.ConvertCase(StringAliasCaseType.PascalCase));
			}

			return !attempt.Success
			       	? null
					: new PropertyResult(alias, attempt.Result, Guid.Empty, PropertyResultType.ReflectedProperty)
			       		{
			       			DocumentTypeAlias = content.DocumentTypeAlias,
			       			DocumentId = content.Id
			       		};
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
		//public PropertyResult GetProperty(string alias, out bool propertyExists)
		//{
		//    if (IsNull())
		//    {
		//        propertyExists = false;
		//        return null;
		//    }
		//    PropertyResult property = null;
		//    if (Type == DynamicBackingItemType.Content)
		//    {
		//        var innerProperty = _content.GetProperty(alias);
		//        propertyExists = innerProperty != null;
		//        if (innerProperty != null)
		//        {
		//            property = new PropertyResult(innerProperty)
		//                {
		//                    ContextAlias = _content.DocumentTypeAlias, 
		//                    ContextId = _content.Id
		//                };
		//        }
		//    }
		//    else
		//    {
		//        //string[] internalProperties = new string[] {
		//        //    "id", "nodeName", "updateDate", "writerName", "path", "nodeTypeAlias",
		//        //    "parentID", "__NodeId", "__IndexType", "__Path", "__NodeTypeAlias", 
		//        //    "__nodeName", "umbracoBytes","umbracoExtension","umbracoFile","umbracoWidth",
		//        //    "umbracoHeight"
		//        //};
		//        //if (media.WasLoadedFromExamine && !internalProperties.Contains(alias) && !media.Values.ContainsKey(alias))
		//        //{
		//        //    //examine doesn't load custom properties
		//        //    innerProperty = media.LoadCustomPropertyNotFoundInExamine(alias, out propertyExists);
		//        //    if (innerProperty != null)
		//        //    {
		//        //        property = new PropertyResult(innerProperty);
		//        //        property.ContextAlias = media.NodeTypeAlias;
		//        //        property.ContextId = media.Id;
		//        //    }
		//        //}
		//        //else
		//        //{
		//        //    innerProperty = media.GetProperty(alias, out propertyExists);
		//        //    if (innerProperty != null)
		//        //    {
		//        //        property = new PropertyResult(innerProperty);
		//        //        property.ContextAlias = media.NodeTypeAlias;
		//        //        property.ContextId = media.Id;
		//        //    }
		//        //}
		//        propertyExists = false;
		//    }
		//    return property;
		//}

		//public PropertyResult GetProperty(string alias, bool recursive)
		//{
		//    bool propertyExists = false;
		//    return GetProperty(alias, recursive, out propertyExists);
		//}

		/// <summary>
		/// Returns a property defined on the document object as a member property using reflection
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		public PropertyResult GetReflectedProperty(string alias)
		{
			return GetPropertyInternal(alias, _content, false);
		}

		/// <summary>
		/// Return a user defined property
		/// </summary>
		/// <param name="alias"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		public PropertyResult GetUserProperty(string alias, bool recursive = false)
		{
			if (Type == DynamicBackingItemType.Content)
			{
				if (!recursive)
				{
					return GetPropertyInternal(alias, _content);
				}
				var context = this;
				var prop = GetPropertyInternal(alias, _content);
				while (prop == null || !prop.HasValue())
				{
					context = context.Parent;
					if (context == null) break;
					prop = context.GetPropertyInternal(alias, context._content);
				}
				return prop;
			}
			else
			{
				//return GetPropertyInternal(alias, media);
				return null;
			}
		}
		
		public IEnumerable<IDocumentProperty> Properties
		{
			get
			{
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
			get { return Type == DynamicBackingItemType.Content ? _content.Level : 0; }
		}


		public int Id
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.Id : media.Id; }
			get { return Type == DynamicBackingItemType.Content ? _content.Id : 0; }
		}

		public string NodeTypeAlias
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.NodeTypeAlias : media.NodeTypeAlias; }
			get { return Type == DynamicBackingItemType.Content ? _content.DocumentTypeAlias : null; }
		}

		public DynamicBackingItem Parent
		{
			get
			{
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
			get { return Type == DynamicBackingItemType.Content ? _content.CreateDate : DateTime.MinValue; }
		}
		public DateTime UpdateDate
		{
			//get { if (IsNull()) return DateTime.MinValue; return Type == DynamicBackingItemType.Content ? _content.UpdateDate : media.UpdateDate; }
			get { return Type == DynamicBackingItemType.Content ? _content.UpdateDate : DateTime.MinValue; }
		}

		public string WriterName
		{
			get { return Type == DynamicBackingItemType.Content ? _content.WriterName : null; }
		}

		public string Name
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Name : media.Name; }
			get { return Type == DynamicBackingItemType.Content ? _content.Name : null; }
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
			get { return Type == DynamicBackingItemType.Content ? _content.Version : Guid.Empty; }
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
			get { return Type == DynamicBackingItemType.Content ? _content.UrlName : null; }
		}

		public int TemplateId
		{
			get { return Type == DynamicBackingItemType.Content ? _content.TemplateId : 0; }
		}

		public int SortOrder
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.SortOrder : media.SortOrder; }
			get { return Type == DynamicBackingItemType.Content ? _content.SortOrder : 0; }
		}


		public string CreatorName
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.CreatorName : media.CreatorName; }
			get { return Type == DynamicBackingItemType.Content ? _content.CreatorName : null; }
		}

		public int WriterId
		{
			get { return Type == DynamicBackingItemType.Content ? _content.WriterId : 0; }
		}

		public int CreatorId
		{
			//get { if (IsNull()) return 0; return Type == DynamicBackingItemType.Content ? _content.CreatorID : media.CreatorID; }
			get { return Type == DynamicBackingItemType.Content ? _content.CreatorId : 0; }
		}

		public string Path
		{
			//get { if (IsNull()) return null; return Type == DynamicBackingItemType.Content ? _content.Path : media.Path; }
			get { return Type == DynamicBackingItemType.Content ? _content.Path : null; }
		}


	}
}