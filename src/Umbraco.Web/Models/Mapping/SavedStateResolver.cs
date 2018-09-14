using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Returns the <see cref="ContentSavedState"/> for an <see cref="IContentBase"/> item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SavedStateResolver<T> : IValueResolver<IContentBase, IContentProperties<T>, ContentSavedState>
        where T : ContentPropertyBasic
    {
        public ContentSavedState Resolve(IContentBase source, IContentProperties<T> destination, ContentSavedState destMember, ResolutionContext context)
        {
            return source.Id == 0 ? ContentSavedState.NotCreated : ContentSavedState.Draft;
        }
    }
}
