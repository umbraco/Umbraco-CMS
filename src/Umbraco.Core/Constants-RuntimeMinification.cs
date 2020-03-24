using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        public static class RuntimeMinification
        {
            public static class CssBundles
            {
                public const string Default = "default-css";
                public const string Index = "index-css";
            }

            public static class JsBundles
            {
                public const string Default = "default-js";
            }
        }
    }
}
