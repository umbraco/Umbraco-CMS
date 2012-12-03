using System;

namespace umbraco.cms.businesslogic.macro
{
	[Serializable]
	public class MacroPropertyModel
	{
		public string Key { get; set; }
		public string Value { get; set; }
		public string Type { get; set; }
		public string CLRType { get; set; }
		public MacroPropertyModel()
		{

		}

		public MacroPropertyModel(string key, string value)
		{
			Key = key;
			Value = value;
		}

		public MacroPropertyModel(string key, string value, string type, string clrType)
		{
			Key = key;
			Value = value;
			Type = type;
			CLRType = clrType;
		}
	}
}