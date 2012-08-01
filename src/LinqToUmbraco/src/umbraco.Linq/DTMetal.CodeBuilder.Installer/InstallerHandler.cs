using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Globalization;

namespace Umbraco.Linq.DTMetal.CodeBuilder.Installer
{
    [RunInstaller(true)]
    public class InstallerHandler : System.Configuration.Install.Installer
    {
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var vsPath = GetVsPath();
            var targetPath = Context.Parameters["TargetDir"];

            if (vsPath != null && targetPath != null)
            {
                var progress = new InstallerProgress(InstallMode.Install, vsPath, targetPath);
                MessageBoxOptions options = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
                if (progress.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show("VS 2008 Setup Failed", "LINQ to Umbraco Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, options);
                    MessageBox.Show(progress.Error.ToString());
                }
            }
        }

        private static string GetVsPath()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false);
            if (key != null)
                return (string)key.GetValue("InstallDir");
            return null;
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            var vsPath = GetVsPath();
            if (vsPath != null)
            {
                var progress = new InstallerProgress(InstallMode.Uninstall, vsPath, null);
                progress.ShowDialog();
            }
        }
    }
}
