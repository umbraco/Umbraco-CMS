namespace Umbraco.Core.Persistence.Migrations.Model
{
    public class IndexColumnDefinition
    {
        public virtual string Name { get; set; }
        public virtual Direction Direction { get; set; }
    }

    public enum Direction
    {
        Ascending = 0,
        Descending = 1
    }
}