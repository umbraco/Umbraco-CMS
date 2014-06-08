using System;

namespace Umbraco.Web.Mvc
{
    public class ActionExecutedEventArgs : EventArgs
    {
        public UmbracoController Controller { get; set; }
        public object Model { get; set; }
        
        public ActionExecutedEventArgs(UmbracoController controller, object model)
        {
            Controller = controller;
            Model = model;            
        }
    }
}