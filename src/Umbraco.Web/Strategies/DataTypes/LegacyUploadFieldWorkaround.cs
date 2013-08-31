using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.interfaces;
using Umbraco.Core;

namespace Umbraco.Web.Strategies.DataTypes
{
	/// <summary>
	/// Before Save Content/Media subscriber that checks for Upload fields and updates related fields accordingly.
	/// </summary>
	/// <remarks>
	/// This is an intermediate fix for the legacy DataTypeUploadField and the FileHandlerData, so that properties
	/// are saved correctly when using the Upload field on a (legacy) Document or Media class.
	/// </remarks>
	public class LegacyUploadFieldWorkaround : IApplicationStartupHandler
	{
		public LegacyUploadFieldWorkaround()
		{
			global::umbraco.cms.businesslogic.media.Media.BeforeSave += MediaBeforeSave;
			global::umbraco.cms.businesslogic.web.Document.BeforeSave += DocumentBeforeSave;
		}

		void DocumentBeforeSave(global::umbraco.cms.businesslogic.web.Document sender, global::umbraco.cms.businesslogic.SaveEventArgs e)
		{
			if (LegacyUmbracoSettings.ImageAutoFillImageProperties != null)
			{
				var property = sender.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == new Guid(Constants.PropertyEditors.UploadField));
				if (property == null)
					return;


				FillProperties(sender, property);
			}
		}

		void MediaBeforeSave(global::umbraco.cms.businesslogic.media.Media sender, global::umbraco.cms.businesslogic.SaveEventArgs e)
		{
			if (LegacyUmbracoSettings.ImageAutoFillImageProperties != null)
			{
				var property = sender.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == new Guid(Constants.PropertyEditors.UploadField));
				if (property == null)
					return;


				FillProperties(sender, property);
			}
		}

        private void FillProperties(global::umbraco.cms.businesslogic.Content content, global::umbraco.cms.businesslogic.property.Property property)
		{
			XmlNode uploadFieldConfigNode = global::umbraco.UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(string.Format("uploadField [@alias = \"{0}\"]", property.PropertyType.Alias));

			if (uploadFieldConfigNode != null)
			{
				var fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
                //Ensure that the Property has a Value before continuing
                if(property.Value == null)
                    return;

				var path = fileSystem.GetRelativePath(property.Value.ToString());

				if (string.IsNullOrWhiteSpace(path) == false && fileSystem.FileExists(path))
				{
					long size;
					using (var fileStream = fileSystem.OpenFile(path))
					{
						size = fileStream.Length;
					}

					var extension = fileSystem.GetExtension(path) != null
						? fileSystem.GetExtension(path).Substring(1).ToLowerInvariant()
						: "";

					var isImageType = ("," + LegacyUmbracoSettings.ImageFileTypes + ",").Contains(string.Format(",{0},", extension));
					var dimensions = isImageType ? GetDimensions(path, fileSystem) : null;

					// only add dimensions to web images
					UpdateProperty(uploadFieldConfigNode, content, "widthFieldAlias", isImageType ? dimensions.Item1.ToString(CultureInfo.InvariantCulture) : string.Empty);
					UpdateProperty(uploadFieldConfigNode, content, "heightFieldAlias", isImageType ? dimensions.Item2.ToString(CultureInfo.InvariantCulture) : string.Empty);

					UpdateProperty(uploadFieldConfigNode, content, "lengthFieldAlias", size == default(long) ? string.Empty : size.ToString(CultureInfo.InvariantCulture));
					UpdateProperty(uploadFieldConfigNode, content, "extensionFieldAlias", string.IsNullOrEmpty(extension) ? string.Empty : extension);
				}
			}
		}

        private void UpdateProperty(XmlNode uploadFieldConfigNode, global::umbraco.cms.businesslogic.Content content, string propertyAlias, object propertyValue)
		{
			XmlNode propertyNode = uploadFieldConfigNode.SelectSingleNode(propertyAlias);
			if (propertyNode != null && !String.IsNullOrEmpty(propertyNode.FirstChild.Value))
			{
                if (content.GenericProperties.Any(x => x.PropertyType.Alias == propertyNode.FirstChild.Value) && content.getProperty(propertyNode.FirstChild.Value) != null)
				{
				    content.getProperty(propertyNode.FirstChild.Value).Value = propertyValue;
				}
			}
		}

		private Tuple<int, int> GetDimensions(string path, IFileSystem fs)
		{

			int fileWidth;
			int fileHeight;
			using (var stream = fs.OpenFile(path))
			{
				using (var image = Image.FromStream(stream))
				{
					fileWidth = image.Width;
					fileHeight = image.Height;
				}
			}

			return new Tuple<int, int>(fileWidth, fileHeight);
		}
	}
}