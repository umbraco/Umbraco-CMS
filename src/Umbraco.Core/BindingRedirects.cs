using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using Umbraco.Core;

[assembly: PreApplicationStartMethod(typeof(BindingRedirects), "Initialize")]

namespace Umbraco.Core
{
    /// <summary>
    /// Manages any assembly binding redirects that cannot be done via config
    /// </summary>
    public class BindingRedirects
    {
        public static void Initialize()
        {
            // this only gets called when an assembly can't be resolved
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static readonly Regex Log4NetAssemblyPattern = new Regex("log4net, Version=([\\d\\.]+?), Culture=neutral, PublicKeyToken=null", RegexOptions.Compiled);
        private const string Log4NetReplacement = "log4net, Version=1.2.15.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a";
        
        /// <summary>
        /// This is used to do an assembly binding redirect of log4net if it does not exist in the web.config
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (Log4NetAssemblyPattern.IsMatch(args.Name) && args.Name != Log4NetReplacement)
            {
                return Assembly.Load(Log4NetAssemblyPattern.Replace(args.Name, Log4NetReplacement));
            }

            return null;
        }
    }
}