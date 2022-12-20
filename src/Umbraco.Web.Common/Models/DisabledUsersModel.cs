using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Web.Common.Models;

[DataContract]
public class DisabledUsersModel : INotificationModel
{
    public List<BackOfficeNotification> Notifications { get; } = new();

    [DataMember(Name = "disabledUserIds")]
    public IEnumerable<int> DisabledUserIds { get; set; } = Enumerable.Empty<int>();
}
