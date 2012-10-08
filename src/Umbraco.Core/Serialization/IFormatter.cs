namespace Umbraco.Core.Serialization
{
    public interface IFormatter
    {
        string Intent { get; }

        ISerializer Serializer { get; }
    }
}