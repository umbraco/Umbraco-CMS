using System.Runtime.InteropServices;
using Umbraco.Cms.Core.Diagnostics;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreMarchal : IMarchal
{
    // This thing is not available in net standard, but exists in both .Net 4 and .Net Core 3
    public IntPtr GetExceptionPointers() => Marshal.GetExceptionPointers();
}
