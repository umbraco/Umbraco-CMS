namespace umbraco.controls.Tree
{
	/// <summary>
	/// Simple data object to hold information about a node
	/// </summary>
	public class NodeInfo
	{
		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value>The id.</value>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the path.
		/// </summary>
		/// <value>The path.</value>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the path as names.
		/// </summary>
		/// <value>The path as names.</value>
		public string PathAsNames { get; set; }

		/// <summary>
		/// Gets or sets the type of the node.
		/// </summary>
		/// <value>The type of the node.</value>
		public string NodeType { get; set; }
	}
}