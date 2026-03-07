using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Api.Management.Filters;

/// <summary>
/// When applied to a controller, this ensures that any event messages created during a request (e.g. by notification
/// handlers) are appended to the response in a custom header.
/// </summary>
/// <remarks>
/// GET operations are explicitly ignored by this, as they are not expected to generate event messages.
/// </remarks>
public sealed class AppendEventMessagesAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppendEventMessagesAttribute"/> class, which is used to append event messages to the HTTP response.
    /// </summary>
    public AppendEventMessagesAttribute()
        : base(typeof(AppendEventMessagesFilter))
    {
    }

    private sealed class AppendEventMessagesFilter : IActionFilter
    {
        private readonly IEventMessagesFactory _eventMessagesFactory;
        private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppendEventMessagesFilter"/> class.
    /// </summary>
    /// <param name="eventMessagesFactory">Factory for creating event messages.</param>
    /// <param name="jsonSerializer">Serializer used for JSON serialization.</param>
        public AppendEventMessagesFilter(IEventMessagesFactory eventMessagesFactory, IJsonSerializer jsonSerializer)
        {
            _eventMessagesFactory = eventMessagesFactory;
            _jsonSerializer = jsonSerializer;
        }

    /// <summary>
    /// Executes after an action method has completed, and if applicable, appends event messages as a notification header to the HTTP response.
    /// This is typically used to communicate event-related notifications to the client for non-GET requests.
    /// If there are no event messages or the request is a GET, no header is added.
    /// </summary>
    /// <param name="context">The context for the executed action, providing access to the HTTP request and response.</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Response is null)
            {
                return;
            }

            if (context.HttpContext.Request.Method == HttpMethod.Get.Method)
            {
                return;
            }

            EventMessage[]? eventMessages = _eventMessagesFactory.GetOrDefault()?.GetAll()?.ToArray();
            if (eventMessages is null || eventMessages.Any() is false)
            {
                return;
            }

            var headerContent = _jsonSerializer.Serialize(eventMessages.Select(message =>
                new NotificationHeaderModel
                {
                    Message = message.Message, Category = message.Category, Type = message.MessageType
                }));

            context.HttpContext.Response.Headers[Constants.Headers.Notifications] = headerContent;
        }

    /// <summary>
    /// Method invoked before the action executes, intended to append event messages to the context.
    /// </summary>
    /// <param name="context">The context for the action that is executing.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

    }
}
