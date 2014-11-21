namespace umbraco.cms.presentation.Trees
{
	/// <summary>
	/// All Trees rely on the properties of an ITreeService interface. This has been created to avoid having trees
	/// dependant on the HttpContext
	/// </summary>
	public interface ITreeService
	{
		/// <summary>
		/// The NodeKey is a string representation of the nodeID. Generally this is used for tree's whos node's unique key value is a string in instead 
		/// of an integer such as folder names.
		/// </summary>
		string NodeKey { get; }
		int StartNodeID { get; }
		bool ShowContextMenu { get; }
		bool IsDialog { get; }
		TreeDialogModes DialogMode { get; }
		string FunctionToCall { get; }
	}
}
