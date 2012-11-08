namespace System.Linq.Dynamic
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.DynamicProperty")]
	public class DynamicProperty
	{
		private readonly Umbraco.Core.Dynamics.DynamicProperty _inner;

		public DynamicProperty(string name, Type type)
		{
			_inner = new Umbraco.Core.Dynamics.DynamicProperty(name, type);
		}

		public string Name
		{
			get { return _inner.Name; }
		}

		public Type Type
		{
			get { return _inner.Type; }
		}
	}
}