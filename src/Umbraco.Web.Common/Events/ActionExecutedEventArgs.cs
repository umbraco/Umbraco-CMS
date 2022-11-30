using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.Events;

public class ActionExecutedEventArgs : EventArgs
{
    public ActionExecutedEventArgs(Controller controller, object model)
    {
        Controller = controller;
        Model = model;
    }

    public Controller Controller { get; set; }

    public object Model { get; set; }
}
