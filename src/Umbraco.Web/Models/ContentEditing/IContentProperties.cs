using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    ///// <summary>
    ///// An interface that needs to be implemented for the ContentItemValidationHelper to be used
    ///// </summary>
    ///// <typeparam name="TPersisted"></typeparam>
    ///// <remarks>
    ///// We want to share the validation and model binding logic with content, media and members but because of variants content
    ///// is now quite different than the others so this allows us to continue sharing the logic between these models.
    ///// </remarks>
    //internal interface IContentItemValidationHelperModel<TPersisted>
    //    where TPersisted : IContentBase
    //{
    //    TPersisted GetPersisted();
    //    ContentItemDto<TPersisted> GetDto();
    //    IContentProperties<ContentPropertyBasic> GetProperties();
    //}

    public interface IContentProperties<T>
        where T : ContentPropertyBasic
    {
        IEnumerable<T> Properties { get; set; }
    }
}
