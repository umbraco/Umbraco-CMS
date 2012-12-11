using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
	
	//Publishing Events
    public class PublishingEventArgs : System.ComponentModel.CancelEventArgs { }
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
	public class SaveEventArgs : System.ComponentModel.CancelEventArgs
    {
		/// <summary>
		/// public constructor
		/// </summary>
		public SaveEventArgs()
		{

		}

		/// <summary>
		/// internal constructor used for unit testing
		/// </summary>
		/// <param name="unitOfWork"></param>
		internal SaveEventArgs(IUnitOfWork unitOfWork)
		{
			UnitOfWork = unitOfWork;
		}

		/// <summary>
		/// Used for unit testing
		/// </summary>
		internal IUnitOfWork UnitOfWork { get; private set; }
    }
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