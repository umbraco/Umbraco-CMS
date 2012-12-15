namespace Umbraco.Core.Events
{
	public class DeleteEventArgs : System.ComponentModel.CancelEventArgs
	{
		/// <summary>
		/// Gets or Sets the Id of the object being deleted.
		/// </summary>
		public int Id { get; set; }
	}
}