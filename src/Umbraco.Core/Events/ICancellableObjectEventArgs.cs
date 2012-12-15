namespace Umbraco.Core.Events
{
	/// <summary>
	/// Interface for EventArgs clases to implement when they support cancelling operations
	/// </summary>
	public interface ICancellableObjectEventArgs
	{
		/// <summary>
		/// If this instance supports cancellation, this gets/sets the cancel value
		/// </summary>
		bool Cancel { get; set; }

		/// <summary>
		/// Flag to determine if this instance will support being cancellable
		/// </summary>
		bool CanCancel { get; set; }
	}
}