﻿using Umbraco.ModelsBuilder.Embedded.Configuration;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.ModelsBuilder.Embedded.BackOffice
{
    /// <summary>
    /// Used to validate the aliases for the content type when MB is enabled to ensure that
    /// no illegal aliases are used
    /// </summary>
    // ReSharper disable once UnusedMember.Global - This is typed scanned
    public class MediaTypeModelValidator : ContentTypeModelValidatorBase<MediaTypeSave, PropertyTypeBasic>
    {
        public MediaTypeModelValidator(IModelsBuilderConfig config) : base(config)
        {
        }
    }
}
