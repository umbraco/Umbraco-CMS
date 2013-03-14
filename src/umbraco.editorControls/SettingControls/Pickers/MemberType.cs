using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core;

namespace umbraco.editorControls.SettingControls.Pickers
{
    public class MemberType : BasePicker
    {
        public MemberType()
        {
            ObjectGuid = new Guid(Constants.ObjectTypes.MemberType);
        }
    }
}