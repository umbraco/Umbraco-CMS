using System.Collections.Generic;

namespace Umbraco.Web.Editors
{
    public interface IOperathorTerm
    {
        string Name { get; set; }

        Operathor Operathor { get; set; }

        IEnumerable<string> AppliesTo { get; set; }
        
    }
}