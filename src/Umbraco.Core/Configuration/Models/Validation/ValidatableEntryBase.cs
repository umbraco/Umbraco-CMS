using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.Configuration.Models.Validation
{
    public abstract class ValidatableEntryBase
    {
        internal virtual bool IsValid()
        {
            var ctx = new ValidationContext(this);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(this, ctx, results, true);
        }
    }
}
