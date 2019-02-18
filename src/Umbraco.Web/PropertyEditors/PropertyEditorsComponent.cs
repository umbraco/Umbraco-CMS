using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.PropertyEditors
{
    internal sealed class PropertyEditorsComponent : IComponent
    {
        private readonly PropertyEditorCollection _propertyEditors;

        public PropertyEditorsComponent(PropertyEditorCollection propertyEditors)
        {
            _propertyEditors = propertyEditors;
        }

        public void Initialize()
        {
            var fileUpload = _propertyEditors.OfType<FileUploadPropertyEditor>().FirstOrDefault();
            if (fileUpload != null) Initialize(fileUpload);

            var imageCropper = _propertyEditors.OfType<ImageCropperPropertyEditor>().FirstOrDefault();
            if (imageCropper != null) Initialize(imageCropper);

            // grid/examine moved to ExamineComponent
        }

        public void Terminate()
        { }

        private static void Initialize(FileUploadPropertyEditor fileUpload)
        {
            MediaService.Saving += fileUpload.MediaServiceSaving;
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
