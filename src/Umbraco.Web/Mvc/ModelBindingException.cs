using System;

namespace Umbraco.Web.Mvc
{
    public class ModelBindingException : Exception
    {
        public ModelBindingException()
        { }

        public ModelBindingException(string message)
            : base(message)
        { }
    }
}
