using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using umbraco.cms.businesslogic.media;
using Umbraco.Core;

namespace umbraco
{
	/// <summary>
	/// Extension methods for umbraco.cms.businesslogic.media.Media
	/// </summary>
	public static class MediaExtensions
	{
		/// <summary>
		/// Functionally similar to the XPath axis 'ancestor'
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetAncestorMedia(this Media media)
		{
			var ancestor = new Media(media.ParentId);

			while (ancestor != null && ancestor.Id != -1)
			{
				yield return ancestor;

				ancestor = new Media(ancestor.ParentId);
			}
		}

		/// <summary>
		/// Funtionally similar to the XPath axis 'ancestor-or-self'
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetAncestorOrSelfMedia(this Media media)
		{
			yield return media;

			foreach (var ancestor in media.GetAncestorMedia())
			{
				yield return ancestor;
			}
		}

		/// <summary>
		/// Gets all sibling Media
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetSiblingMedia(this Media media)
		{
			if (media.Parent != null)
			{
				var parentMedia = new Media(media.ParentId);

				foreach (var siblingMedia in parentMedia.GetChildMedia().Where(childMedia => childMedia.Id != media.Id))
				{
					yield return siblingMedia;
				}
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'descendant-or-self'
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetDescendantOrSelfMedia(this Media media)
		{
			yield return media;

			foreach (var descendant in media.GetDescendantMedia())
			{
				yield return descendant;
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'descendant'
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetDescendantMedia(this Media media)
		{
			foreach (var child in media.Children)
			{
				yield return child;

				foreach (var descendant in child.GetDescendantMedia())
				{
					yield return descendant;
				}
			}
		}

		/// <summary>
		/// Functionally similar to the XPath axis 'child'
		/// Performance optimised for just imediate children.
		/// </summary>
		/// <param name="media">The <c>umbraco.cms.businesslogic.media.Media</c> object.</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetChildMedia(this Media media)
		{
			foreach (var child in media.Children)
			{
				yield return child;
			}
		}

		/// <summary>
		/// Gets the child media that satisfy the <c>Func</c> condition.
		/// </summary>
		/// <param name="media">The <c>umbraco.cms.businesslogic.media.Media</c> object.</param>
		/// <param name="func">The func.</param>
		/// <returns>Media nodes as IEnumerable</returns>
		public static IEnumerable<Media> GetChildMedia(this Media media, Func<Media, bool> func)
		{
			foreach (var child in media.Children)
			{
				if (func(child))
				{
					yield return child;
				}
			}
		}

        /////// <summary>
        /////// Extension method on Meia obj to get it's depth
        /////// </summary>
        /////// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
        /////// <returns>an int representing the depth of the Media object in the tree</returns>
        ////[Obsolete("Use .Level instead")]
        ////public static int GetDepth(this Media media)
        ////{
        ////    return media.Path.Split(',').ToList().Count;
        ////}

		/// <summary>
		/// Tell me the level of this node (0 = root)
		/// updated from Depth and changed to start at 0
		/// to align with other 'Level' methods (eg xslt)
		/// </summary>
		/// <param name="media"></param>
		/// <returns></returns>
		public static int Level(this Media media)
		{
			return media.Path.Split(',').Length - 1;
		}

		/// <summary>
		/// Returns the url for a given crop name using the built in Image Cropper datatype
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <param name="propertyAlias">property alias</param>
		/// <param name="cropName">name of crop to get url for</param>
		/// <returns>emtpy string or url</returns>
		public static string GetImageCropperUrl(this Media media, string propertyAlias, string cropName)
		{
			string cropUrl = string.Empty;

			/*
			* Example xml : 
			* 
			* <crops date="28/11/2010 16:08:13">
			*   <crop name="Big" x="0" y="0" x2="1024" y2="768" url="/media/135/image_Big.jpg" />
			*   <crop name="Small" x="181" y="0" x2="608" y2="320" url="/media/135/image_Small.jpg" />
			* </crops>
			* 
			*/

			if (!string.IsNullOrEmpty(media.GetProperty<string>(propertyAlias)))
			{
				var xmlNode = media.getProperty(propertyAlias).ToXml(new XmlDocument());
				var cropNode = xmlNode.SelectSingleNode(string.Concat("descendant::crops/crop[@name='", cropName, "']"));

				if (cropNode != null)
				{
					cropUrl = cropNode.Attributes.GetNamedItem("url").InnerText;
				}
			}

			return cropUrl;
		}

		/// <summary>
		/// Gets the image URL.
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns></returns>
		public static string GetImageUrl(this Media media)
		{
			if (media.ContentType.Alias.Equals(Constants.Conventions.MediaTypes.Image))
			{
				var url = media.GetProperty<string>(Constants.Conventions.Media.File);
				if (!string.IsNullOrEmpty(url))
				{
					return url;
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// Gets the image thumbnail URL.
		/// </summary>
		/// <param name="media">an umbraco.cms.businesslogic.media.Media object</param>
		/// <returns></returns>
		public static string GetImageThumbnailUrl(this Media media)
		{
			if (media.ContentType.Alias.Equals(Constants.Conventions.MediaTypes.Image))
			{
				var url = media.GetProperty<string>(Constants.Conventions.Media.File);
				if (!string.IsNullOrEmpty(url))
				{
					var extension = media.GetProperty<string>(Constants.Conventions.Media.Extension);
                    return url.Replace(string.Concat(".", extension), "_thumb." + extension, StringComparison.InvariantCultureIgnoreCase);
				}
			}

			return string.Empty;
		}
	}
}