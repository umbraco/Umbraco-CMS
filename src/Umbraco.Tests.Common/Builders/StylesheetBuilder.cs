using Umbraco.Core.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class StylesheetBuilder
        : BuilderBase<Stylesheet>
    {
        private string _path;
        private string _content;

        public StylesheetBuilder WithPath(string path)
        {
            _path = path;
            return this;
        }

        public StylesheetBuilder WithContent(string content)
        {
            _content = content;
            return this;
        }

        public override Stylesheet Build()
        {
            var path = _path ?? string.Empty;
            var content = _content ?? string.Empty;

            Reset();
            return new Stylesheet(path)
            {
                Content = content,
            };
        }

        protected override void Reset()
        {
            _path = null;
            _content = null;
        }
    }
}
