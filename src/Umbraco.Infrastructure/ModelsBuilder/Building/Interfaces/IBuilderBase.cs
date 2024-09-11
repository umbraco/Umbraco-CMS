using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building.Interfaces
{
    public interface IBuilderBase
    {

        IEnumerable<TypeModel> GetModelsToGenerate();
        string GetModelsNamespace();

        bool IsAmbiguousSymbol(string symbol, string match);
        string GetModelsBaseClassName(TypeModel type);

        void Prepare(IEnumerable<TypeModel> types);

        IList<string> Using { get; set; }
    }
}
