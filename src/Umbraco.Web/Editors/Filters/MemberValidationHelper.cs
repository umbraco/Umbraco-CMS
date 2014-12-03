using System.Linq;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Custom validation helper so that we can exclude the Member.StandardPropertyTypeStubs from being validating for existence
    /// </summary>
    internal class MemberValidationHelper : ContentValidationHelper<IMember, MemberSave>
    {
        protected override bool ValidateProperties(ContentItemBasic<ContentPropertyBasic, IMember> postedItem, HttpActionContext actionContext)
        {
            var propertiesToValidate = postedItem.Properties.ToList();
            var defaultProps = Constants.Conventions.Member.GetStandardPropertyTypeStubs();
            var exclude = defaultProps.Select(x => x.Value.Alias).ToArray();
            foreach (var remove in exclude)
            {
                propertiesToValidate.RemoveAll(property => property.Alias == remove);
            }

            return ValidateProperties(propertiesToValidate.ToArray(), postedItem.PersistedContent.Properties.ToArray(), actionContext);
        }
    }
}