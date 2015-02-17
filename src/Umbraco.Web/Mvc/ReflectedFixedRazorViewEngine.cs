using System;
using System.Reflection;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// This is here to support compatibility with both MVC4 and MVC5
    /// </summary>
    public abstract class ReflectedFixedRazorViewEngine : IViewEngine
    {
        protected ReflectedFixedRazorViewEngine()
        {
            if (MvcVersionCheck.MvcVersion >= System.Version.Parse("5.0.0"))
            {
                _wrappedEngine = new RazorViewEngine();
            }
            else
            {
                var assembly = Assembly.Load("Microsoft.Web.Mvc.FixedDisplayModes");
                var engineType = assembly.GetType("Microsoft.Web.Mvc.FixedRazorViewEngine");
                _wrappedEngine = (IViewEngine)Activator.CreateInstance(engineType);
            }
        }

        public string[] ViewLocationFormats
        {
            get { return _viewLocationFormats; }
            set
            {
                _wrappedEngine.GetType().GetProperty("ViewLocationFormats").SetValue(_wrappedEngine, value);
                _viewLocationFormats = value;
            }
        }

        public string[] PartialViewLocationFormats
        {
            get { return _partialViewLocationFormats; }
            set
            {
                _wrappedEngine.GetType().GetProperty("PartialViewLocationFormats").SetValue(_wrappedEngine, value);
                _partialViewLocationFormats = value;
            }
        }

        public string[] AreaViewLocationFormats
        {
            get { return _areaViewLocationFormats; }
            set
            {
                _wrappedEngine.GetType().GetProperty("AreaViewLocationFormats").SetValue(_wrappedEngine, value);
                _areaViewLocationFormats = value;
            }
        }

        public string[] AreaMasterLocationFormats
        {
            get { return _areaMasterLocationFormats; }
            set
            {
                _wrappedEngine.GetType().GetProperty("AreaMasterLocationFormats").SetValue(_wrappedEngine, value);
                _areaMasterLocationFormats = value;
            }
        }

        public string[] AreaPartialViewLocationFormats
        {
            get { return _areaPartialViewLocationFormats; }
            set
            {
                _wrappedEngine.GetType().GetProperty("AreaPartialViewLocationFormats").SetValue(_wrappedEngine, value);
                _areaPartialViewLocationFormats = value;
            }
        }

        private readonly IViewEngine _wrappedEngine;
        private string[] _areaViewLocationFormats;
        private string[] _areaMasterLocationFormats;
        private string[] _areaPartialViewLocationFormats;
        private string[] _viewLocationFormats;
        private string[] _partialViewLocationFormats;

        public virtual ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return _wrappedEngine.FindPartialView(controllerContext, partialViewName, useCache);
        }

        public virtual ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return _wrappedEngine.FindView(controllerContext, viewName, masterName, useCache);
        }
        
        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            _wrappedEngine.ReleaseView(controllerContext, view);
        }
    }
}