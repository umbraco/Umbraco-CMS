namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class UmbracoLock
    {
        public int Id { get; set; }

        public int Value { get; set; } = 1;

        public string Name { get; set; } = null!;
    }
}
