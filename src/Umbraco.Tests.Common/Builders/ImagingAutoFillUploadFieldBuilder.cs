using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class ImagingAutoFillUploadFieldBuilder : ImagingAutoFillUploadFieldBuilder<object>
    {
        public ImagingAutoFillUploadFieldBuilder() : base(null)
        {
        }
    }

    public class ImagingAutoFillUploadFieldBuilder<TParent>
       : ChildBuilderBase<TParent, ImagingAutoFillUploadField>
    {
        private string _alias;
        private string _widthFieldAlias;
        private string _heightFieldAlias;
        private string _lengthFieldAlias;
        private string _extensionFieldAlias;

        public ImagingAutoFillUploadFieldBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public ImagingAutoFillUploadFieldBuilder<TParent> WithAlias(string alias)
        {
            _alias = alias;
            return this;
        }

        public ImagingAutoFillUploadFieldBuilder<TParent> WithWidthFieldAlias(string widthFieldAlias)
        {
            _widthFieldAlias = widthFieldAlias;
            return this;
        }

        public ImagingAutoFillUploadFieldBuilder<TParent> WithHeightFieldAlias(string heightFieldAlias)
        {
            _heightFieldAlias = heightFieldAlias;
            return this;
        }

        public ImagingAutoFillUploadFieldBuilder<TParent> WithLengthFieldAlias(string lengthFieldAlias)
        {
            _lengthFieldAlias = lengthFieldAlias;
            return this;
        }

        public ImagingAutoFillUploadFieldBuilder<TParent> WithExtensionFieldAlias(string extensionFieldAlias)
        {
            _extensionFieldAlias = extensionFieldAlias;
            return this;
        }

        public override ImagingAutoFillUploadField Build()
        {
            var alias = _alias ?? "testAlias";
            var widthFieldAlias = _widthFieldAlias ?? "testWidthFieldAlias";
            var heightFieldAlias = _heightFieldAlias ?? "testHeightFieldAlias";
            var lengthFieldAlias = _lengthFieldAlias ?? "testLengthFieldAlias";
            var extensionFieldAlias = _extensionFieldAlias ?? "testExtensionFieldAlias";

            return new ImagingAutoFillUploadField
            {
                Alias = alias,
                WidthFieldAlias = widthFieldAlias,
                HeightFieldAlias = heightFieldAlias,
                LengthFieldAlias = lengthFieldAlias,
                ExtensionFieldAlias = extensionFieldAlias,
            };
        }
    }
}
