using System.Collections;

namespace umbraco.NodeFactory
{
	public class Nodes : CollectionBase
	{
		public virtual void Add(Node NewNode)
		{
			List.Add(NewNode);
		}

		public virtual Node this[int Index]
		{
			get { return (Node)List[Index]; }
		}
	}
}