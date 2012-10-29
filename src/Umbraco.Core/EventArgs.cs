using Umbraco.Core.Models;

namespace Umbraco.Core
{
    //Publishing Events
    public class PublishEventArgs : System.ComponentModel.CancelEventArgs { }
    public class UnPublishEventArgs : System.ComponentModel.CancelEventArgs { }
    public class SendToPublishEventArgs : System.ComponentModel.CancelEventArgs { }

    //Moving Content Events
    public class MoveEventArgs : System.ComponentModel.CancelEventArgs { }
    public class MoveToTrashEventArgs : System.ComponentModel.CancelEventArgs { }

    //Copying Content Events
    public class CopyEventArgs : System.ComponentModel.CancelEventArgs
    {
        public int CopyTo { get; set; }
        public IContent NewContent { get; set; }
    }
    
    //Rollback Content Event
    public class RollbackEventArgs : System.ComponentModel.CancelEventArgs { }

    //Content Cache Event args
    public class ContentCacheEventArgs : System.ComponentModel.CancelEventArgs { }
    public class RefreshContentEventArgs : System.ComponentModel.CancelEventArgs { }

    //Generel eventArgs
    public class DeleteEventArgs : System.ComponentModel.CancelEventArgs { }
    public class SaveEventArgs : System.ComponentModel.CancelEventArgs { }
    public class NewEventArgs : System.ComponentModel.CancelEventArgs { }
}