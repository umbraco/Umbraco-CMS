using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Plugins;

namespace Umbraco.Web.Editors
{
    internal interface IEditorValidator : IDiscoverable
    {
        Type ModelType { get; }
        IEnumerable<ValidationResult> Validate(object model);
    }
}