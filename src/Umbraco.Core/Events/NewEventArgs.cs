namespace Umbraco.Core.Events
{
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