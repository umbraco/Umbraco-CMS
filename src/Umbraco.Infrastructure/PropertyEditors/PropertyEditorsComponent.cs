// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;

namespace Umbraco.Cms.Core.PropertyEditors
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
            void memberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> args) => args.MediaFilesToDelete.AddRange(fileUpload.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MemberService.Deleted += memberServiceDeleted;
            _terminate.Add(() => MemberService.Deleted -= memberServiceDeleted);
        }

        private void Initialize(ImageCropperPropertyEditor imageCropper)
        {
            void memberServiceDeleted(IMemberService sender, DeleteEventArgs<IMember> args) => args.MediaFilesToDelete.AddRange(imageCropper.ServiceDeleted(args.DeletedEntities.Cast<ContentBase>()));
            MemberService.Deleted += memberServiceDeleted;
            _terminate.Add(() => MemberService.Deleted -= memberServiceDeleted);
        }
    }
}
