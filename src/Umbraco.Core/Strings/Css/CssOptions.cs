using System;

namespace Umbraco.Core.Strings.Css
{
    [Flags]
    internal enum CssOptions
    {
        None = 0x00,
        PrettyPrint = 0x01,
        Overwrite = 0x02
    }

}