using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.editorControls.macrocontainer
{
    public static class MacroContainerEvent
    {
        public delegate void ExecuteHandler();
        public static event ExecuteHandler Execute;

        public static void Add()
        {
            if (Execute != null)
                Execute();
        }

        public static void Delete()
        {
            if (Execute != null)
                Execute();
        }
    }
}
