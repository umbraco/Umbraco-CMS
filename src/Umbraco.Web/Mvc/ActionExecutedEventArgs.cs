using System;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// Migrated already to .Net Core
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
