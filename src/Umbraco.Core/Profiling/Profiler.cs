using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Profiling;

namespace Umbraco.Core.Profiling
{
    public class Profiler
    {
        private static Profiler _instance;

        private Profiler()
        {
        }

        public string Render()
        {
            return MiniProfiler.RenderIncludes().ToString();
        }

        public IDisposable Step(string name)
        {
            return MiniProfiler.Current.Step(name);
        }

        public void Start()
        {
            MiniProfiler.Start();
        }

        public void Stop()
        {
            MiniProfiler.Stop();
        }



        public static Profiler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Profiler();
                }
                
                return _instance;
            }
        }
    }
}
