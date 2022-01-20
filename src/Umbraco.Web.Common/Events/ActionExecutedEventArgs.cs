using System;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Web.Common.Events
{
    public class ActionExecutedEventArgs : EventArgs
    {
        public Controller Controller { get; set; }
        public object Model { get; set; }

        public ActionExecutedEventArgs(Controller controller, object model)
        {
            Controller = controller;
            Model = model;
        }
    }
}
