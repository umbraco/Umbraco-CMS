namespace Umbraco.Core.Events
{

	//Publishing Events
    public class PublishingEventArgs : System.ComponentModel.CancelEventArgs 
	{
	    public PublishingEventArgs()
	    {
		    IsAllRepublished = false;
	    }

	    public PublishingEventArgs(bool isAllPublished)
	    {
		    IsAllRepublished = isAllPublished;
	    }

        public bool IsAllRepublished { get; private set; }
    }

    public class SendToPublishEventArgs : System.ComponentModel.CancelEventArgs { }

    //Moving object Events
    public class MoveEventArgs : System.ComponentModel.CancelEventArgs
    {
        /// <summary>
        /// Gets or Sets the Id of the objects new parent.
        /// </summary>
        public int ParentId { get; set; }
    }

    //Copying object Events
    public class CopyEventArgs : System.ComponentModel.CancelEventArgs
    {
        /// <summary>
        /// Gets or Sets the Id of the objects new parent.
        /// </summary>
        public int ParentId { get; set; }
    }
    
    //Rollback Content Event
    public class RollbackEventArgs : System.ComponentModel.CancelEventArgs { }

    //Content Cache Event args
    public class ContentCacheEventArgs : System.ComponentModel.CancelEventArgs { }
    public class RefreshContentEventArgs : System.ComponentModel.CancelEventArgs { }

    //Generel eventArgs
    public class DeleteEventArgs : System.ComponentModel.CancelEventArgs
    {
        /// <summary>
        /// Gets or Sets the Id of the object being deleted.
        /// </summary>
        public int Id { get; set; }
    }
	public class SaveEventArgs : System.ComponentModel.CancelEventArgs { }
    public class NewEventArgs : System.ComponentModel.CancelEventArgs
    {
        /// <summary>
        /// Gets or Sets the Alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or Sets the Id of the parent.
        /// </summary>
        public int ParentId { get; set; }
    }
}