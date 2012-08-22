namespace System.Linq.Dynamic
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.ParseException")]
	public sealed class ParseException : Exception
	{
		private readonly Umbraco.Core.Dynamics.ParseException _inner;

		public ParseException(string message, int position)
			: base(message)
		{
			_inner = new Umbraco.Core.Dynamics.ParseException(message, position);
		}

		public int Position
		{
			get { return _inner.Position; }
		}

		public override string ToString()
		{
			return _inner.ToString();
		}
	}
}