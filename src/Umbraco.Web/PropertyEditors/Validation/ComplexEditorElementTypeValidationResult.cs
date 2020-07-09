using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Web.PropertyEditors.Validation
{
    /// <summary>
    /// A collection of <see cref="ComplexEditorPropertyTypeValidationResult"/> for an element type within complex editor represented by an Element Type
    /// </summary>
    public class ComplexEditorElementTypeValidationResult : ValidationResult
    {
        public ComplexEditorElementTypeValidationResult(string elementTypeAlias, Guid blockId)
            : base(string.Empty)
        {
            ElementTypeAlias = elementTypeAlias;
            BlockId = blockId;
        }

        public IList<ComplexEditorPropertyTypeValidationResult> ValidationResults { get; } = new List<ComplexEditorPropertyTypeValidationResult>();

        // TODO: We don't use this anywhere, though it's nice for debugging
        public string ElementTypeAlias { get; }
        public Guid BlockId { get; }
    }
}
