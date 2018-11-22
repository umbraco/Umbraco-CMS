using System;

namespace Umbraco.Core.Dynamics
{
	internal class DynamicProperty
	{
		readonly string _name;
		readonly Type _type;

		public DynamicProperty(string name, Type type)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (type == null) throw new ArgumentNullException("type");
			this._name = name;
			this._type = type;
		}

		public string Name
		{
			get { return _name; }
		}

		public Type Type
		{
			get { return _type; }
		}
	}
}