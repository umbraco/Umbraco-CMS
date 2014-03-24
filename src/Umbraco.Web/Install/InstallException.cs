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
        public string View { get; private set; }
        public object ViewModel { get; private set; }

        public override string Message
        {
            get { return _message; }
        }

        public InstallException(string message, string view, object viewModel)
        {
            _message = message;
            View = view;
            ViewModel = viewModel;
        }

        public InstallException(string message, object viewModel)
        {
            _message = message;
            View = "error";
            ViewModel = viewModel;
        }

        public InstallException(string message)
        {
            _message = message;
            View = "error";
            ViewModel = null;
        }
    }
}
