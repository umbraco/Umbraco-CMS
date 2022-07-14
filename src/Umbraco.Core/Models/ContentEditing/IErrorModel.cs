namespace Umbraco.Cms.Core.Models.ContentEditing;

public interface IErrorModel
{
    /// <summary>
    ///     This is used for validation of a content item.
    /// </summary>
    /// <remarks>
    ///     A content item can be invalid but still be saved. This occurs when there's property validation errors, we will
    ///     still save the item but it cannot be published. So we need a way of returning validation errors as well as the
    ///     updated model.
    /// </remarks>
    IDictionary<string, object> Errors { get; set; }
}
