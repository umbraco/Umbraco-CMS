using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.ModelBinding;

namespace Umbraco.Core.Models.Validation
{
    /// <summary>
    /// A custom validation attribute which adds additional metadata to the property to indicate that 
    /// the value is required to be persisted.
    /// </summary>
    /// <remarks>
    /// In Umbraco, we persist content even if it is invalid, however there are some properties that are absolutely required
    /// in order to be persisted such as the Name of the content item. This attribute is re-usable to check for these types of
    /// properties over any sort of model.
    /// </remarks>
    public class RequiredForPersistenceAttribute : RequiredAttribute
    {
    }
}
