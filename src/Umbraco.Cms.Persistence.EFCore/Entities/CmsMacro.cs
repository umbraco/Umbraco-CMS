using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Persistence.EFCore.Entities
{
    public partial class CmsMacro
    {
        public CmsMacro()
        {
            CmsMacroProperties = new HashSet<CmsMacroProperty>();
        }

        public int Id { get; set; }
        public Guid UniqueId { get; set; }
        public bool? MacroUseInEditor { get; set; }
        public int MacroRefreshRate { get; set; }
        public string MacroAlias { get; set; } = null!;
        public string? MacroName { get; set; }
        public bool? MacroCacheByPage { get; set; }
        public bool? MacroCachePersonalized { get; set; }
        public bool? MacroDontRender { get; set; }
        public string MacroSource { get; set; } = null!;
        public int MacroType { get; set; }

        public virtual ICollection<CmsMacroProperty> CmsMacroProperties { get; set; }
    }
}
