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

namespace umbraco.Linq.DTMetal.CodeBuilder.Installer
{
    public partial class InstallerProgress : Form
    {
        private const string generatorKey = "{52B316AA-1997-4c81-9969-95404C09EEB4}";
        private const string dtml = ".dtml";
        private const string clas = "LINQtoumbraco.DTML.1.0";
        private const string desc = "LINQ to umbraco Entity Mapping";

        private InstallMode _mode;
        private string _vs9Path;
        private string _vs10Path;
        private string _targetPath;

        internal InstallerProgress(InstallMode mode, string vs9Path, string vs10Path, string targetPath)
        {
            InitializeComponent();

            _mode = mode;
            _vs9Path = vs9Path;
            _vs10Path = vs10Path;
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
                File.Delete(GetDtmlXsdPathVs9());
            }
            catch { }

            try
            {
                File.Delete(GetDtmlXsdPathVs10());
            }
            catch { }
        }

        private void TearDownRegistry()
        {
            if(Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0\CLSID\" + generatorKey) != null)
            {
                Registry.LocalMachine.DeleteSubKey(@"Software\Microsoft\VisualStudio\9.0\CLSID\" + generatorKey);
            }

            if (Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\10.0\CLSID\" + generatorKey) != null)
            {
                Registry.LocalMachine.DeleteSubKey(@"Software\Microsoft\VisualStudio\10.0\CLSID\" + generatorKey);
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
            if (!string.IsNullOrEmpty(_vs9Path))
            {
                string csTgt = Path.Combine(_vs9Path, @"ItemTemplates\CSharp\1033\LINQtoumbracoCS.zip");
                string vbTgt = Path.Combine(_vs9Path, @"ItemTemplates\VisualBasic\1033\LINQtoumbracoVB.zip");

                try
                {
                    File.Delete(csTgt);
                    File.Delete(vbTgt);
                }
                catch { } 
            }

            if (!string.IsNullOrEmpty(_vs10Path))
            {
                string csTgt = Path.Combine(_vs10Path, @"ItemTemplates\CSharp\1033\LINQtoumbracoCS.zip");
                string vbTgt = Path.Combine(_vs10Path, @"ItemTemplates\VisualBasic\1033\LINQtoumbracoVB.zip");

                try
                {
                    File.Delete(csTgt);
                    File.Delete(vbTgt);
                }
                catch { }
            }
        }
        #endregion

        #region Install
        private void InstallDtmlXsd()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Load the XSD for DocTypeML.
            string xsd;
            using (Stream s = assembly.GetManifestResourceStream(assembly.GetName().Name + ".DocTypeML.xsd"))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    xsd = sr.ReadToEnd();
                }
            }

            // Write the XSD to the Visual Studio folder with XML schemas.
            if (!string.IsNullOrEmpty(_vs9Path))
            {
                using (StreamWriter sw = File.CreateText(GetDtmlXsdPathVs9()))
                {
                    sw.Write(xsd);
                } 
            }

            if (!string.IsNullOrEmpty(_vs10Path))
            {
                using (StreamWriter sw = File.CreateText(GetDtmlXsdPathVs10()))
                {
                    sw.Write(xsd);
                } 
            }
        }

        private void SetupRegistry()
        {
            #region LINQtoumbracoGenerator Key
            if (!string.IsNullOrEmpty(_vs9Path))
            {
                RegistryKey genKey = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\VisualStudio\9.0\CLSID\" + generatorKey);
                genKey.SetValue("Assembly", Assembly.GetExecutingAssembly().FullName); //full name of the assembly which the generator is in
                genKey.SetValue("Class", "umbraco.Linq.DTMetal.CodeBuilder.LINQtoumbracoGenerator"); //generator class
                genKey.SetValue("InprocServer32", Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"system32\mscoree.dll"));
                genKey.SetValue("ThreadingModel", "Both");
                genKey.Close(); 
            }

            if (!string.IsNullOrEmpty(_vs10Path))
            {
                RegistryKey genKey = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\VisualStudio\10.0\CLSID\" + generatorKey);
                genKey.SetValue("Assembly", Assembly.GetExecutingAssembly().FullName); //full name of the assembly which the generator is in
                genKey.SetValue("Class", "umbraco.Linq.DTMetal.CodeBuilder.LINQtoumbracoGenerator"); //generator class
                genKey.SetValue("InprocServer32", Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"system32\mscoree.dll"));
                genKey.SetValue("ThreadingModel", "Both");
                genKey.Close();
            }
            #endregion

            #region HKEY_CLASSES_ROOT\.dtml
            {
                RegistryKey dtmlKey = Registry.ClassesRoot.CreateSubKey(dtml, RegistryKeyPermissionCheck.ReadWriteSubTree);
                dtmlKey.SetValue(null, clas);
                dtmlKey.Close();
            }
            #endregion

            #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0
            {
                RegistryKey kClas = Registry.ClassesRoot.CreateSubKey(clas, RegistryKeyPermissionCheck.ReadWriteSubTree);
                kClas.SetValue(null, desc);
                kClas.SetValue("AlwaysShowExt", "1");

                #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\DefaultIcon
                {
                    RegistryKey kIcon = kClas.CreateSubKey("DefaultIcon");
                    kIcon.SetValue(null, Path.Combine(_targetPath, @"DTMetal.exe") + ",0");
                    kIcon.Close();
                }
                #endregion

                #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\shell
                {
                    RegistryKey kShel = kClas.CreateSubKey("shell");

                    #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\shell\Open
                    {
                        RegistryKey kOpen = kShel.CreateSubKey("Open");

                        #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\shell\Open\Command
                        {
                            RegistryKey kComm = kOpen.CreateSubKey("Command");
                            kComm.SetValue(null, "\"" + Path.Combine(_vs9Path, "devenv.exe") + "\" /dde \"%1\"");
                            kComm.Close();
                        }
                        #endregion

                        #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\shell\Open\ddeexec
                        {
                            RegistryKey kDdex = kOpen.CreateSubKey("ddeexec");
                            kDdex.SetValue(null, "Open(\"%1\")");

                            #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\shell\Open\ddeexec\Application
                            {
                                RegistryKey kAppn = kDdex.CreateSubKey("Application");
                                kAppn.SetValue(null, "VisualStudio.9.0");
                                kAppn.Close();
                            }
                            #endregion

                            #region HKEY_CLASSES_ROOT\LINQtoumbraco.DTML.1.0\shell\Open\ddeexec\Topic
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
            if (!string.IsNullOrEmpty(_vs9Path))
            {
                string csSrc = Path.Combine(_targetPath, @"Item packages\LINQtoumbracoCS.zip");
                string vbSrc = Path.Combine(_targetPath, @"Item packages\LINQtoumbracoVB.zip");
                string csTgt = Path.Combine(_vs9Path, @"ItemTemplates\CSharp\1033\LINQtoumbracoCS.zip");
                string vbTgt = Path.Combine(_vs9Path, @"ItemTemplates\VisualBasic\1033\LINQtoumbracoVB.zip");

                if (!File.Exists(csTgt))
                    File.Copy(csSrc, csTgt);
                if (!File.Exists(vbTgt))
                    File.Copy(vbSrc, vbTgt); 
            }

            if (!string.IsNullOrEmpty(_vs10Path))
            {
                string csSrc = Path.Combine(_targetPath, @"Item packages\LINQtoumbracoCS.zip");
                string vbSrc = Path.Combine(_targetPath, @"Item packages\LINQtoumbracoVB.zip");
                string csTgt = Path.Combine(_vs10Path, @"ItemTemplates\CSharp\1033\LINQtoumbracoCS.zip");
                string vbTgt = Path.Combine(_vs10Path, @"ItemTemplates\VisualBasic\1033\LINQtoumbracoVB.zip");

                if (!File.Exists(csTgt))
                    File.Copy(csSrc, csTgt);
                if (!File.Exists(vbTgt))
                    File.Copy(vbSrc, vbTgt);
            }
        }
        #endregion

        private string GetDtmlXsdPathVs9()
        {
            string path = _vs9Path; //%ProgramFiles%\Microsoft Visual Studio 9.0\Common7\IDE\
            path = path.TrimEnd('\\'); //%ProgramFiles%\Microsoft Visual Studio 9.0\Common7\IDE
            path = path.Substring(0, path.LastIndexOf('\\')); //%ProgramFiles%\Microsoft Visual Studio 9.0\Common7
            path = path.Substring(0, path.LastIndexOf('\\')); //%ProgramFiles%\Microsoft Visual Studio 9.0
            path = Path.Combine(path, "Xml"); //%ProgramFiles%\Microsoft Visual Studio 9.0\Xml
            path = Path.Combine(path, "Schemas"); //%ProgramFiles%\Microsoft Visual Studio 9.0\Xml\Schemas
            path = Path.Combine(path, "DocTypeML.xsd"); //%ProgramFiles%\Microsoft Visual Studio 9.0\Xml\Schemas\DocTypeML.xsd
            return path;
        }

        private string GetDtmlXsdPathVs10()
        {
            string path = _vs10Path; //%ProgramFiles%\Microsoft Visual Studio 10.0\Common7\IDE\
            path = path.TrimEnd('\\'); //%ProgramFiles%\Microsoft Visual Studio 10.0\Common7\IDE
            path = path.Substring(0, path.LastIndexOf('\\')); //%ProgramFiles%\Microsoft Visual Studio 10.0\Common7
            path = path.Substring(0, path.LastIndexOf('\\')); //%ProgramFiles%\Microsoft Visual Studio 10.0
            path = Path.Combine(path, "Xml"); //%ProgramFiles%\Microsoft Visual Studio 10.0\Xml
            path = Path.Combine(path, "Schemas"); //%ProgramFiles%\Microsoft Visual Studio 10.0\Xml\Schemas
            path = Path.Combine(path, "DocTypeML.xsd"); //%ProgramFiles%\Microsoft Visual Studio 10.0\Xml\Schemas\DocTypeML.xsd
            return path;
        }

        private void ConfigVs()
        {
            if (!string.IsNullOrEmpty(_vs9Path))
            {
                string devenv = Path.Combine(_vs9Path, "devenv.exe");
                Process p = Process.Start(devenv, "/InstallVSTemplates");
                p.WaitForExit(); 
            }

            if (!string.IsNullOrEmpty(_vs10Path))
            {
                string devenv = Path.Combine(_vs10Path, "devenv.exe");
                Process p = Process.Start(devenv, "/InstallVSTemplates");
                p.WaitForExit();
            }
        }
    }

    internal enum InstallMode
    {
        Install,
        Uninstall
    }
}
