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
	public class LegacyUploadFieldWorkaround : ApplicationEventHandler
	{
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            global::umbraco.cms.businesslogic.media.Media.BeforeSave += MediaBeforeSave;
            global::umbraco.cms.businesslogic.web.Document.BeforeSave += DocumentBeforeSave;
        }
        
		void DocumentBeforeSave(global::umbraco.cms.businesslogic.web.Document sender, global::umbraco.cms.businesslogic.SaveEventArgs e)
		{
            if (UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.Any())
			{
				var property = sender.GenericProperties.FirstOrDefault(x =>x.PropertyType.DataTypeDefinition.DataType!=null && x.PropertyType.DataTypeDefinition.DataType.Id == new Guid(Constants.PropertyEditors.UploadField));
				if (property == null)
					return;


				FillProperties(sender, property);
			}
		}

		void MediaBeforeSave(global::umbraco.cms.businesslogic.media.Media sender, global::umbraco.cms.businesslogic.SaveEventArgs e)
		{
            if (UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties.Any())
			{
				var property = sender.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == new Guid(Constants.PropertyEditors.UploadField));
				if (property == null)
					return;


				FillProperties(sender, property);
			}
		}

        private void FillProperties(global::umbraco.cms.businesslogic.Content content, global::umbraco.cms.businesslogic.property.Property property)
		{			
            var uploadFieldConfigNode = UmbracoConfig.For.UmbracoSettings().Content.ImageAutoFillProperties
                                                            .FirstOrDefault(x => x.Alias == property.PropertyType.Alias);

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

				    var isImageType = UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.InvariantContains(extension);
                        
					var dimensions = isImageType ? GetDimensions(path, fileSystem) : null;

					
				    if (isImageType)
				    {
                        // only add dimensions to web images
				        content.getProperty(uploadFieldConfigNode.WidthFieldAlias).Value = dimensions.Item1.ToString(CultureInfo.InvariantCulture);
				        content.getProperty(uploadFieldConfigNode.HeightFieldAlias).Value = dimensions.Item2.ToString(CultureInfo.InvariantCulture);
				    }

				    content.getProperty(uploadFieldConfigNode.LengthFieldAlias).Value = size == default(long) ? string.Empty : size.ToString(CultureInfo.InvariantCulture);
                    content.getProperty(uploadFieldConfigNode.ExtensionFieldAlias).Value = string.IsNullOrEmpty(extension) ? string.Empty : extension;

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