namespace umbraco.interfaces
{
	public interface ITag
	{
		int Id { get; }
		string TagCaption { get; }
		string Group { get; }
	}
}