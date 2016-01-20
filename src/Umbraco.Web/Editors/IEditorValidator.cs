using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.Editors
{
    internal interface IEditorValidator
    {
        Type ModelType { get; }
        IEnumerable<ValidationResult> Validate(object model);
    }
}