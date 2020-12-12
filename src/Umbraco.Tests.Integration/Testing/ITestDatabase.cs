namespace Umbraco.Tests.Integration.Testing
{
    public interface ITestDatabase
    {
        TestDbMeta AttachEmpty();
        TestDbMeta AttachSchema();
        void Detach(TestDbMeta id);
    }
}
