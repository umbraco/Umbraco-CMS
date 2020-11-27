using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.PropertyEditors
{
    public sealed class PropertyEditorsComponent : IComponent
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly List<Action> _terminate = new List<Action>();

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
        {
            foreach (var t in _terminate) t();
        }

        private void Initialize(FileUploadPropertyEditor fileUpload)
        {
            MediaService.Saving += fileUpload.MediaServiceSaving;
            _terminate.Add(() => MediaService.Saving -= fileUpload.MediaServiceSaving);
            ContentService.Copied += fileUpload.ContentServiceCopied;
            _terminate.Add(() => ContentService.Copied -= fileUpload.ContentServiceCopied);

            void mediaServiceDeleted(IMediaService sender, DeleteEventArgs<IMedia> args) => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MediaService.Deleted += mediaServiceDeleted;
            _terminate.Add(() => MediaService.Deleted -= mediaServiceDeleted);

            void contentServiceDeleted(IContentService sender, DeleteEventArgs<IContent> args) => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            ContentService.Deleted += contentServiceDeleted;
            _terminate.Add(() => ContentService.Deleted -= contentServiceDeleted);

            void memberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> args) => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MemberService.Deleted += memberServiceDeleted;
            _terminate.Add(() => MemberService.Deleted -= memberServiceDeleted);
        }

        private void Initialize(ImageCropperPropertyEditor imageCropper)
        {
            MediaService.Saving += imageCropper.MediaServiceSaving;
            _terminate.Add(() => MediaService.Saving -= imageCropper.MediaServiceSaving);
            ContentService.Copied += imageCropper.ContentServiceCopied;
            _terminate.Add(() => ContentService.Copied -= imageCropper.ContentServiceCopied);

            void mediaServiceDeleted(IMediaService sender, DeleteEventArgs<IMedia> args) => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MediaService.Deleted += mediaServiceDeleted;
            _terminate.Add(() => MediaService.Deleted -= mediaServiceDeleted);

            void contentServiceDeleted(IContentService sender, DeleteEventArgs<IContent> args) => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            ContentService.Deleted += contentServiceDeleted;
            _terminate.Add(() => ContentService.Deleted -= contentServiceDeleted);

            void memberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> args) => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MemberService.Deleted += memberServiceDeleted;
            _terminate.Add(() => MemberService.Deleted -= memberServiceDeleted);
        }
    }
}
