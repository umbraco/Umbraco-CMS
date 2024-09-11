using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface ITextBuilder
    {
        void Generate(StringBuilder sb, TypeModel typeModel);

        void Generate(StringBuilder sb, IEnumerable<TypeModel> typeModels);
    }
}
