using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Represents a repository responsible for persisting and managing webhook request entities in the database.
/// </summary>
public class WebhookRequestRepository : IWebhookRequestRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookRequestRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">An <see cref="IScopeAccessor"/> used to manage the database scope for repository operations.</param>
    public WebhookRequestRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database
    {
        get
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new NotSupportedException("Need to be executed in a scope");
            }

            return _scopeAccessor.AmbientScope.Database;
        }
    }

    /// <summary>
    /// Creates a new <see cref="WebhookRequest"/> asynchronously.
    /// </summary>
    /// <param name="webhookRequest">The webhook request to create.</param>
    /// <returns>The created <see cref="WebhookRequest"/> with its new identifier.</returns>
    public async Task<WebhookRequest> CreateAsync(WebhookRequest webhookRequest)
    {
        WebhookRequestDto dto = WebhookRequestFactory.CreateDto(webhookRequest);
        var result = await Database.InsertAsync(dto);
        var id = Convert.ToInt32(result);
        webhookRequest.Id = id;
        return webhookRequest;
    }

    /// <summary>
    /// Asynchronously deletes the specified <see cref="WebhookRequest"/> from the database.
    /// </summary>
    /// <param name="webhookRequest">The <see cref="WebhookRequest"/> instance to delete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
    public async Task DeleteAsync(WebhookRequest webhookRequest)
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Delete<WebhookRequestDto>()
            .Where<WebhookRequestDto>(x => x.Id == webhookRequest.Id);

        await Database.ExecuteAsync(sql);
    }

    /// <summary>
    /// Asynchronously retrieves all webhook requests from the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IEnumerable{WebhookRequest}"/> with all webhook requests.</returns>
    public async Task<IEnumerable<WebhookRequest>> GetAllAsync()
    {
        Sql<ISqlContext>? sql = Database.SqlContext.Sql()
            .Select<WebhookRequestDto>()
            .From<WebhookRequestDto>();

        List<WebhookRequestDto> webhookDtos = await Database.FetchAsync<WebhookRequestDto>(sql);

        return webhookDtos.Select(WebhookRequestFactory.CreateModel);
    }

    /// <summary>
    /// Updates an existing <see cref="WebhookRequest"/> asynchronously in the database.
    /// </summary>
    /// <param name="webhookRequest">The <see cref="WebhookRequest"/> to update.</param>
    /// <returns>A task representing the asynchronous operation, containing the updated <see cref="WebhookRequest"/>.</returns>
    public async Task<WebhookRequest> UpdateAsync(WebhookRequest webhookRequest)
    {
        WebhookRequestDto dto = WebhookRequestFactory.CreateDto(webhookRequest);
        await Database.UpdateAsync(dto);
        return webhookRequest;
    }
}
