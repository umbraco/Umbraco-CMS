using System.Collections.Generic;
using umbraco.cms.businesslogic.media;

namespace umbraco
{
	public static partial class uQuery
	{
		/// <summary>
		/// Get a collection of media items from an XPath expression (note XML source is currently a flat structure)
		/// </summary>
		/// <param name="xPath">XPath expression</param>
		/// <returns>collection or empty collection</returns>
		public static IEnumerable<Media> GetMediaByXPath(string xPath)
		{
			var media = new List<Media>();
			var xmlDocument = uQuery.GetPublishedXml(UmbracoObjectType.Media);
			var xPathNavigator = xmlDocument.CreateNavigator();
			var xPathNodeIterator = xPathNavigator.Select(xPath);

			while (xPathNodeIterator.MoveNext())
			{
				var mediaItem = uQuery.GetMedia(xPathNodeIterator.Current.Evaluate("string(@id)").ToString());
				if (mediaItem != null)
				{
					media.Add(mediaItem);
				}
			}

			return media;
		}

		/// <summary>
		/// Get collection of media objects from the supplied CSV of IDs
		/// </summary>
		/// <param name="csv">string csv of IDs</param>
		/// <returns>collection or emtpy collection</returns>
		public static IEnumerable<Media> GetMediaByCsv(string csv)
		{
			var media = new List<Media>();
			var ids = uQuery.GetCsvIds(csv);

			if (ids != null)
			{
				foreach (string id in ids)
				{
					var mediaItem = uQuery.GetMedia(id);
					if (mediaItem != null)
					{
						media.Add(mediaItem);
					}
				}
			}

			return media;
		}

		/// <summary>
		/// Gets the media by XML.
		/// </summary>
		/// <param name="xml">The XML.</param>
		/// <returns></returns>
		public static IEnumerable<Media> GetMediaByXml(string xml)
		{
			var media = new List<Media>();
			var ids = uQuery.GetXmlIds(xml);

			if (ids != null)
			{
				foreach (int id in ids)
				{
					var mediaItem = uQuery.GetMedia(id);
					if (mediaItem != null)
					{
						media.Add(mediaItem);
					}
				}
			}

			return media;
		}

		/// <summary>
		/// Get Media by name
		/// </summary>
		/// <param name="name">name of node to look for</param>
		/// <returns>list of nodes, or empty list</returns>
		public static IEnumerable<Media> GetMediaByName(string name)
		{
			return uQuery.GetMediaByXPath(string.Concat("descendant::*[@nodeName='", name, "']"));
		}

		/// <summary>
		/// Get Media by media type alias
		/// </summary>
		/// <param name="mediaTypeAlias">The media type alias</param>
		/// <returns>list of media, or empty list</returns>
		public static IEnumerable<Media> GetMediaByType(string mediaTypeAlias)
		{
			// Both XML schema versions have this attribute
			return uQuery.GetMediaByXPath(string.Concat("descendant::*[@nodeTypeAlias='", mediaTypeAlias, "']"));
		}

		// public static Media GetCurrentMedia() { }

		/// <summary>
		/// Get media item from an ID
		/// </summary>
		/// <param name="mediaId">string ID of media item to get</param>
		/// <returns>media or null</returns>
		public static Media GetMedia(string mediaId)
		{
			int id;
			Media mediaItem = null;

			if (int.TryParse(mediaId, out id))
			{
				mediaItem = uQuery.GetMedia(id);
			}

			return mediaItem;
		}

		/// <summary>
		/// Get media item from an ID
		/// </summary>
		/// <param name="id">ID of media item to get</param>
		/// <returns>Media or null</returns>
		public static Media GetMedia(int id)
		{
			// suppress error if media with supplied id doesn't exist
			try
			{
				return new Media(id);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Extension method on Media collection to return key value pairs of: media.Id / media.Text
		/// </summary>
		/// <param name="media">generic list of Media objects</param>
		/// <returns>a collection of mediaIDs and their text fields</returns>
		public static Dictionary<int, string> ToNameIds(this IEnumerable<Media> media)
		{
			var dictionary = new Dictionary<int, string>();

			foreach (var mediaItem in media)
			{
				if (mediaItem != null && mediaItem.Id != -1)
				{
					dictionary.Add(mediaItem.Id, mediaItem.Text);
				}
			}

			return dictionary;
		}
	}
}