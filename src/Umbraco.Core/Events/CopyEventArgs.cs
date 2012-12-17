namespace Umbraco.Core.Events
{
	public class CopyEventArgs : System.ComponentModel.CancelEventArgs
	{
		/// <summary>
		/// Gets or Sets the Id of the objects new parent.
		/// </summary>
		public int ParentId { get; set; }
	}
}