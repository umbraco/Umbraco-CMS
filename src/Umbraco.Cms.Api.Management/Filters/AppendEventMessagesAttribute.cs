using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
    public AppendEventMessagesAttribute()
        : base(typeof(AppendEventMessagesFilter))
    {
    }

    private class AppendEventMessagesFilter : IActionFilter
    {
        private readonly IEventMessagesFactory _eventMessagesFactory;
        private readonly IJsonSerializer _jsonSerializer;

        public AppendEventMessagesFilter(IEventMessagesFactory eventMessagesFactory, IJsonSerializer jsonSerializer)
        {
            _eventMessagesFactory = eventMessagesFactory;
            _jsonSerializer = jsonSerializer;
        }

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
                new Notification
                {
                    Message = message.Message, Category = message.Category, Type = message.MessageType
                }));

            context.HttpContext.Response.Headers[Constants.Headers.Notifications] = headerContent;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        /// <summary>
        /// This is the format when communicating notification messages to the API consumers.
        /// </summary>
        /// <remarks>
        /// The class is made public on purpose, to make it clear that changing it might constitute a breaking change towards API consumers.
        /// </remarks>
        public sealed class Notification
        {
            public required string Message { get; init; }

            public required string Category { get; init; }

            public required EventMessageType Type { get; init; }
        }
    }
}
