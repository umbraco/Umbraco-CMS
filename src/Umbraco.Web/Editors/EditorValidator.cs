using System.Linq;
using System.Web.Http.ModelBinding;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Provides a method to validate an object using <see cref="IEditorValidator"/> validation.
    /// </summary>
    internal static class EditorValidator
    {
        /// <summary>
        /// Validates an object.
        /// </summary>
        public static void Validate(ModelStateDictionary modelState, object model)
        {
            var modelType = model.GetType();

            var validationResults = Current.EditorValidators // TODO: inject
                .Where(x => x.ModelType == modelType)
                .SelectMany(x => x.Validate(model))
                .Where(x => !string.IsNullOrWhiteSpace(x.ErrorMessage) && x.MemberNames.Any());

            foreach (var r in validationResults)
            foreach (var m in r.MemberNames)
                modelState.AddModelError(m, r.ErrorMessage);
        }
    }
}
