namespace Umbraco.Cms.Core.Events;

public interface IEventMessagesAccessor
{
    EventMessages? EventMessages { get; set; }
}
