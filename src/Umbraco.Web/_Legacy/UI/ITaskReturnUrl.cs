using System;

namespace Umbraco.Web._Legacy.UI
{
    [Obsolete("ITask is used for legacy webforms back office editors, change to using the v7 angular approach")]
    public interface ITaskReturnUrl : ITask
    {
        string ReturnUrl {get;}
    }
}
