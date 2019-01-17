using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Trees
{
    public class TreeCollection : BuilderCollectionBase<Tree>
    {
        public TreeCollection(IEnumerable<Tree> items)
            : base(items)
        { }
    }
}
