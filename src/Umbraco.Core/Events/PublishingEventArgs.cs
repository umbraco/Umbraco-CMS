namespace Umbraco.Core.Events
{
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
}