using System.Linq;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services.Implement;
using Umbraco.Examine;

namespace Umbraco.Web.PropertyEditors
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    internal class PropertyEditorsComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public void Initialize(IRuntimeState runtime, PropertyEditorCollection propertyEditors)
        {
            var fileUpload = propertyEditors.OfType<FileUploadPropertyEditor>().FirstOrDefault();
            if (fileUpload != null) Initialize(fileUpload);

            var imageCropper = propertyEditors.OfType<ImageCropperPropertyEditor>().FirstOrDefault();
            if (imageCropper != null) Initialize(imageCropper);

            // grid/examine moved to ExamineComponent
        }

        // as long as these methods are private+static they won't be executed by the boot loader

        private static void Initialize(FileUploadPropertyEditor fileUpload)
        {
            MediaService.Saving += fileUpload.MediaServiceSaving;
            MediaService.Created += fileUpload.MediaServiceCreated;
            ContentService.Copied += fileUpload.ContentServiceCopied;

            MediaService.Deleted += (sender, args)
                => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            ContentService.Deleted += (sender, args)
                => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MemberService.Deleted += (sender, args)
                => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
        }

        private static void Initialize(ImageCropperPropertyEditor imageCropper)
        {
            MediaService.Saving += imageCropper.MediaServiceSaving;
            MediaService.Created += imageCropper.MediaServiceCreated;
            ContentService.Copied += imageCropper.ContentServiceCopied;

            MediaService.Deleted += (sender, args)
                => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            ContentService.Deleted += (sender, args)
                => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MemberService.Deleted += (sender, args)
                => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
        }
    }
}
