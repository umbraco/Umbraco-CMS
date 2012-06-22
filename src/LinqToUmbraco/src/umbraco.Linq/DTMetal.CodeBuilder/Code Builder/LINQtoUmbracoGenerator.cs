
/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;
using System.Xml.Linq;

namespace umbraco.Linq.DTMetal.CodeBuilder
{
    /// <summary>
    /// This is the generator class. 
    /// When setting the 'Custom Tool' property of a C# or VB project item to "LINQtoumbracoGenerator", 
    /// the GenerateCode function will get called and will return the contents of the generated file 
    /// to the project system
    /// </summary>
    [ComVisible(true)]
    [Guid("52B316AA-1997-4c81-9969-95404C09EEB4")]
    [CodeGeneratorRegistration(typeof(LINQtoumbracoGenerator), "C# LINQ to umbraco Class Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [CodeGeneratorRegistration(typeof(LINQtoumbracoGenerator), "VB LINQ to umbraco Class Generator", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof(LINQtoumbracoGenerator))]
    public class LINQtoumbracoGenerator : BaseCodeGeneratorWithSite
    {
#pragma warning disable 0414
        //The name of this generator (use for 'Custom Tool' property of project item)
        internal static string name = "LINQtoumbracoGenerator";
#pragma warning restore 0414

        /// <summary>
        /// Function that builds the contents of the generated file based on the contents of the input file
        /// </summary>
        /// <param name="inputFileContent">Content of the input file</param>
        /// <returns>Generated file as a byte array</returns>
        protected override byte[] GenerateCode(string inputFileContent)
        {
            var args = new ClassGeneratorArgs()
            {
                Namespace = this.FileNameSpace,
                Dtml = XDocument.Parse(inputFileContent),
                Provider = this.GetCodeProvider()
            };

            var builder = new ClassGenerator(args);

            //this.GetVSProject().References.Add("umbraco.Linq.Core");
            //this.GetVSProject().References.Add("System.Core");
            //this.GetVSProject().References.Add("umbraco");

            builder.GenerateCode();
            return builder.SaveForVs();
        }

        protected override string GetDefaultExtension()
        {
            return ".designer" + base.GetDefaultExtension();
        }
    }
}