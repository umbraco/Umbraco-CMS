namespace Umbraco.Tests.Integration.Testing
{
    public interface ITestDatabase
    {
        string ConnectionString { get; }
        int AttachEmpty();
        int AttachSchema();
        void Detach(int id);
    }
}
