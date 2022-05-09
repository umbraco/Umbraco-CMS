namespace Umbraco.Cms.Core.Snippets
{
    public class Snippet : ISnippet
    {
        public string Name { get; }
        public string Content { get; }

        public Snippet(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
