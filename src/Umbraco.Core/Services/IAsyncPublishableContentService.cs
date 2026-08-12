using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Asynchronous counterpart of <see cref="IPublishableContentService{TContent}" />.
/// </summary>
/// <remarks>
///     This is the async-first contract used while the content, media, and member repositories are migrated to EF
///     Core. For now it is only implemented by <see cref="IContentService" />; the media and member services continue
///     to use the synchronous <see cref="IPublishableContentService{TContent}" /> until their repositories are
///     migrated. Starts empty (all members currently live on <see cref="IAsyncContentServiceBase{TContent}" />);
///     grows one member at a time as each <see cref="IPublishableContentService{TContent}" /> member gets its async
///     conversion.
/// </remarks>
/// <typeparam name="TContent">The type of content item managed by this service.</typeparam>
public interface IAsyncPublishableContentService<TContent> : IAsyncContentServiceBase<TContent>
    where TContent : class, IContentBase
{
}
