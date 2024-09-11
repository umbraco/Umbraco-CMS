using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilderActions
    {
        void WriteHeader(StringBuilder sb);
        void WriteAssemblyAttributesMarker(StringBuilder sb);
        void WriteUsing(StringBuilder sb, IEnumerable<string> typeUsing);

        void WriteNamespace(StringBuilder sb, string modelNamespace);
        void WriteContentType(StringBuilder sb, TypeModel type, bool lineBreak);

        void WriteContentTypeProperties(StringBuilder sb, TypeModel type);
        void WriteCloseClass(StringBuilder sb);
    }
}
