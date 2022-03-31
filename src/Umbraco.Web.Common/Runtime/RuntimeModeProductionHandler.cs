using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.Common.Runtime
{
    internal class RuntimeModeProductionHandler :
        INotificationHandler<ContentTypeSavingNotification>,
        INotificationHandler<ContentTypeDeletingNotification>,
        INotificationHandler<MediaTypeSavingNotification>,
        INotificationHandler<MediaTypeDeletingNotification>,
        INotificationHandler<MemberTypeSavingNotification>,
        INotificationHandler<MemberTypeDeletingNotification>,
        INotificationHandler<DataTypeSavingNotification>,
        INotificationHandler<DataTypeDeletingNotification>,
        INotificationHandler<TemplateSavingNotification>,
        INotificationHandler<TemplateDeletingNotification>,
        INotificationHandler<PartialViewCreatingNotification>,
        INotificationHandler<PartialViewSavingNotification>,
        INotificationHandler<PartialViewDeletingNotification>
    {
        private readonly EventMessage _cancelEventMessage = new EventMessage(
            "Production mode is enabled",
            "This operation has been cancelled, because this change is not allowed while production mode is enabled.",
            EventMessageType.Error);

        public void Handle(ContentTypeSavingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(ContentTypeDeletingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(MediaTypeSavingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(MediaTypeDeletingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(MemberTypeSavingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(MemberTypeDeletingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(DataTypeSavingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(DataTypeDeletingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(TemplateSavingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(TemplateDeletingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(PartialViewCreatingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(PartialViewSavingNotification notification) => notification.CancelOperation(_cancelEventMessage);
        public void Handle(PartialViewDeletingNotification notification) => notification.CancelOperation(_cancelEventMessage);
    }
}
