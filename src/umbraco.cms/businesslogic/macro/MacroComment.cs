using System;

namespace umbraco.cms.businesslogic.macro
{
	/// <summary>
	/// Not implemented
	/// </summary>

	[AttributeUsage(AttributeTargets.Class |
		 AttributeTargets.Constructor |
		 AttributeTargets.Field |
		 AttributeTargets.Method |
		 AttributeTargets.Property,
		 AllowMultiple = true)]
	public class MacroComment : System.Attribute
	{
		private string _comment;

		/// <summary>
		/// Not implemented
		/// </summary>
		public string Comment 
		{
			set {_comment = value;}
			get {return _comment;}
		}
		/// <summary>
		/// Not implemented
		/// </summary>
		/// <param name="Comment">Not implemented</param>
		public MacroComment(string Comment)
		{
			_comment = Comment;
		}
	}
}
