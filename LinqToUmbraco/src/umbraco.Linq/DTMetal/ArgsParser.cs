using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using umbraco.Linq.DTMetal.CodeBuilder;

namespace umbraco.Linq.DTMetal
{
    enum RunMode
    {
        Xml, Class, Help
    }

    internal sealed class ArgsParser
    {
        public ArgsParser(string[] args)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            Regex r = new Regex(@"[-/](?<option>\w+)(:(""(?<value>.*)""|(?<value>.*)))?", RegexOptions.CultureInvariant | RegexOptions.Compiled);
            foreach (string arg in args)
                foreach (Match m in r.Matches(arg))
                    res.Add(m.Groups["option"].Value.ToLowerInvariant(), m.Groups["value"].Value);

            if (res.ContainsKey("help"))
            {
                this.Mode = RunMode.Help;
                return;
            }

            if (!res.ContainsKey("mode"))
            {
                throw new ArgumentNullException("mode", Strings.GenerationModeMissing);
            }

            this.Mode = (RunMode)Enum.Parse(typeof(RunMode), res["mode"], true);


            switch (this.Mode)
            {
                case RunMode.Xml:
                    if (!res.ContainsKey("connectionstring"))
                    {
                        throw new ArgumentNullException("connectionString");
                    }
                    this.ConnectionString = res["connectionstring"];

                    if (!res.ContainsKey("output"))
                    {
                        res["output"] = Environment.CurrentDirectory;
                    }

                    if (!Directory.Exists(res["output"]))
                    {
                        throw new DirectoryNotFoundException(string.Format(Strings.InvalidFolder, res["output"]));
                    }

                    this.OutputFilePath = res["output"];
                    if (res.ContainsKey("datacontext"))
                    {
                        this.DataContextName = res["datacontext"];
                    }

                    if (res.ContainsKey("disablepluralization"))
                    {
                        this.DisablePluralization = true;
                    }
                    break;
                case RunMode.Class:
                    if (!res.ContainsKey("dtml"))
                    {
                        throw new ArgumentNullException("dtml");
                    }
                    if (!File.Exists(res["dtml"]))
                    {
                        throw new FileNotFoundException(string.Format(Strings.FileNotFound, res["dtml"]));
                    }
                    this.DtmlPath = res["dtml"];

                    if (!res.ContainsKey("language"))
                    {
                        this.Language = GenerationLanguage.CSharp;
                    }
                    else
                    {
                        this.Language = (GenerationLanguage)Enum.Parse(typeof(GenerationLanguage), res["language"], true); 
                    }

                    if (!res.ContainsKey("namespace") || string.IsNullOrEmpty(res["namespace"]))
                    {
                        this.Namespace = "umbraco";
                    }
                    else
                    {
                        this.Namespace = res["namespace"];
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public RunMode Mode { get; set; }

        //xml
        public string ConnectionString { get; set; }
        public string DataContextName { get; set; }
        public string OutputFilePath { get; set; }
        public bool DisablePluralization { get; set; }

        //code
        public string DtmlPath { get; set; }
        public GenerationLanguage Language { get; set; }
        public string Namespace { get; set; }
    }
}
