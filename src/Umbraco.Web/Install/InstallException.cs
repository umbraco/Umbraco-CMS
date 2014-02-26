using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Install
{
    /// <summary>
    /// Used for steps to be able to return a json structure back to the UI
    /// </summary>
    internal class InstallException : Exception
    {
        private readonly string _message;
        public object Result { get; set; }

        public override string Message
        {
            get { return _message; }
        }

        public InstallException(string message, object result)
        {
            _message = message;
            Result = result;
        }
    }
}
