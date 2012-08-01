using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using Microsoft.Win32;
using System.Linq;

namespace Umbraco.Linq.DTMetal.CodeBuilder.Installer
{
    public partial class InstallerProgress : Form
    {
        private const string generatorKey = "{52B316AA-1997-4c81-9969-95404C09EEB4}";
        private const string dtml = ".dtml";
        private const string clas = "LINQtoUmbraco.DTML.1.0";
        private const string desc = "LINQ to Umbraco Entity Mapping";

        private InstallMode _mode;
        private string _vsPath;
        private string _targetPath;

        public InstallerProgress(InstallMode mode, string vsPath, string targetPath)
        {
            InitializeComponent();

            _mode = mode;
            _vsPath = vsPath;
            _targetPath = targetPath;
        }

        #region Events
        private void installWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            switch (_mode)
            {
                case InstallMode.Install:
                    InstallVsTemplates();
                    SetupRegistry();
                    InstallDtmlXsd();
                    ConfigVs();
                    break;
                case InstallMode.Uninstall:
                    UninstallVsTemplates();
                    TearDownRegistry();
                    UninstallDtmlXsd();
                    ConfigVs();
                    break;
                default:
                    break;
            }
        }

        private void installWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Error = e.Error;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }
        public Exception Error { get; set; }

        private void InstallerProgress_Load(object sender, EventArgs e)
        {
            installWorker.RunWorkerAsync();
        }
        #endregion

        #region Uninstall
        private void UninstallDtmlXsd()
        {
            try
            {
                File.Delete(GetDtmlXsdPath());
            }
            catch { }
        }

        private void TearDownRegistry()
        {
            if(Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0\CLSID\" + generatorKey) != null)
            {
                Registry.LocalMachine.DeleteSubKey(@"Software\Microsoft\VisualStudio\9.0\CLSID\" + generatorKey);
            }

            if (Registry.ClassesRoot.OpenSubKey(dtml) != null)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(dtml);
            }

            if (Registry.ClassesRoot.OpenSubKey(clas) != null)
            {
                Registry.ClassesRoot.DeleteSubKeyTree(clas);
            }
        }

        private void UninstallVsTemplates()
        {
            string csTgt = Path.Combine(_vsPath, @"ItemTemplates\CSharp\1033\LINQtoUmbracoCS.zip");
            string vbTgt = Path.Combine(_vsPath, @"ItemTemplates\VisualBasic\1033\LINQtoUmbracoVB.zip");

            try
            {
                File.Delete(csTgt);
                File.Delete(vbTgt);
            }
            catch { }
        }
        #endregion

        #region Install
        private void InstallDtmlXsd()
        {
            //string assemblyName = GetCodeBuilderAssemblyName();
            //Assembly assembly = Assembly.Load(assemblyName);

            //it seems that it runs this before the GAC is deployed, so the above code deosn't work
            //I have to embed the XSD in this assembly instead
            var assembly = Assembly.GetExecutingAssembly();

            // Load the XSD for DocTypeML.
            string xsd;
            using (Stream s = ClassGenerator.GetXsd())
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    xsd = sr.ReadToEnd();
                }
            }

            // Write the XSD to the Visual Studio folder with XML schemas.
            using (StreamWriter sw = File.CreateText(GetDtmlXsdPath()))
            {
                sw.Write(xsd);
            }
        }

        private void SetupRegistry()
        {
            #region LINQtoUmbracoGenerator Key
            RegistryKey genKey = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\VisualStudio\9.0\CLSID\" + generatorKey);
            genKey.SetValue("Assembly", GetCodeBuilderAssemblyName()); //full name of the assembly which the generator is in
            genKey.SetValue("Class", "Umbraco.Linq.DTMetal.CodeBuilder.LINQtoUmbracoGenerator"); //generator class
            genKey.SetValue("InprocServer32", Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"system32\mscoree.dll"));
            genKey.SetValue("ThreadingModel", "Both");
            genKey.Close();
            #endregion

            #region HKEY_CLASSES_ROOT\.dtml
            {
                RegistryKey dtmlKey = Registry.ClassesRoot.CreateSubKey(dtml, RegistryKeyPermissionCheck.ReadWriteSubTree);
                dtmlKey.SetValue(null, clas);
                dtmlKey.Close();
            }
            #endregion

            #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0
            {
                RegistryKey kClas = Registry.ClassesRoot.CreateSubKey(clas, RegistryKeyPermissionCheck.ReadWriteSubTree);
                kClas.SetValue(null, desc);
                kClas.SetValue("AlwaysShowExt", "1");

                #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\DefaultIcon
                {
                    RegistryKey kIcon = kClas.CreateSubKey("DefaultIcon");
                    kIcon.SetValue(null, Path.Combine(_targetPath, @"DTMetal.exe") + ",0");
                    kIcon.Close();
                }
                #endregion

                #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\shell
                {
                    RegistryKey kShel = kClas.CreateSubKey("shell");

                    #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\shell\Open
                    {
                        RegistryKey kOpen = kShel.CreateSubKey("Open");

                        #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\shell\Open\Command
                        {
                            RegistryKey kComm = kOpen.CreateSubKey("Command");
                            kComm.SetValue(null, "\"" + Path.Combine(_vsPath, "devenv.exe") + "\" /dde \"%1\"");
                            kComm.Close();
                        }
                        #endregion

                        #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\shell\Open\ddeexec
                        {
                            RegistryKey kDdex = kOpen.CreateSubKey("ddeexec");
                            kDdex.SetValue(null, "Open(\"%1\")");

                            #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\shell\Open\ddeexec\Application
                            {
                                RegistryKey kAppn = kDdex.CreateSubKey("Application");
                                kAppn.SetValue(null, "VisualStudio.9.0");
                                kAppn.Close();
                            }
                            #endregion

                            #region HKEY_CLASSES_ROOT\LINQtoUmbraco.DTML.1.0\shell\Open\ddeexec\Topic
                            {
                                RegistryKey kTopc = kDdex.CreateSubKey("Topic");
                                kTopc.SetValue(null, "system");
                                kTopc.Close();
                            }
                            #endregion

                            kDdex.Close();
                        }
                        #endregion

                        kOpen.Close();
                    }
                    #endregion

                    kShel.Close();
                }
                #endregion

                kClas.Close();
            }
            #endregion
        }

        private void InstallVsTemplates()
        {
            string csSrc = Path.Combine(_targetPath, @"Item packages\LINQtoUmbracoCS.zip");
            string vbSrc = Path.Combine(_targetPath, @"Item packages\LINQtoUmbracoVB.zip");
            string csTgt = Path.Combine(_vsPath, @"ItemTemplates\CSharp\1033\LINQtoUmbracoCS.zip");
            string vbTgt = Path.Combine(_vsPath, @"ItemTemplates\VisualBasic\1033\LINQtoUmbracoVB.zip");

            if (!File.Exists(csTgt))
                File.Copy(csSrc, csTgt);
            if (!File.Exists(vbTgt))
                File.Copy(vbSrc, vbTgt);
        }
        #endregion

        private static string GetCodeBuilderAssemblyName()
        {
            //get the assembly version info
            AssemblyName current = Assembly.GetExecutingAssembly().GetName();

            StringBuilder sb = new StringBuilder();
            foreach (byte b in current.GetPublicKeyToken())
                sb.AppendFormat("{0:x2}", b);

            string publicKeyToken = sb.ToString();
            string version = current.Version.ToString();

            return String.Format(CultureInfo.InvariantCulture, "Umbraco.Linq.DTMetal.CodeBuilder, Version={0}, Culture=neutral, PublicKeyToken={1}", version, publicKeyToken);
        }

        private string GetDtmlXsdPath()
        {
            string path = _vsPath; //%ProgramFiles%\Microsoft Visual Studio 9.0\Common7\IDE\
            path = path.TrimEnd('\\'); //%ProgramFiles%\Microsoft Visual Studio 9.0\Common7\IDE
            path = path.Substring(0, path.LastIndexOf('\\')); //%ProgramFiles%\Microsoft Visual Studio 9.0\Common7
            path = path.Substring(0, path.LastIndexOf('\\')); //%ProgramFiles%\Microsoft Visual Studio 9.0
            path = Path.Combine(path, "Xml"); //%ProgramFiles%\Microsoft Visual Studio 9.0\Xml
            path = Path.Combine(path, "Schemas"); //%ProgramFiles%\Microsoft Visual Studio 9.0\Xml\Schemas
            path = Path.Combine(path, "DocTypeML.xsd"); //%ProgramFiles%\Microsoft Visual Studio 9.0\Xml\Schemas\DocTypeML.xsd
            return path;
        }

        private void ConfigVs()
        {
            string devenv = Path.Combine(_vsPath, "devenv.exe");
            Process p = Process.Start(devenv, "/InstallVSTemplates");
            p.WaitForExit();
        }
    }

    public enum InstallMode
    {
        Install,
        Uninstall
    }
}
