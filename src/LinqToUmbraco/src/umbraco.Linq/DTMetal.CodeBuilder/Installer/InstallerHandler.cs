using System.ComponentModel;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Globalization;

namespace umbraco.Linq.DTMetal.CodeBuilder.Installer
{
    [RunInstaller(true)]
    public class InstallerHandler : System.Configuration.Install.Installer
    {
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var vsPath9 = GetVsPath9();
            var vsPath10 = GetVsPath10();
            var targetPath = Context.Parameters["TargetDir"];

            if ((vsPath9 != null || vsPath10 != null) && targetPath != null)
            {
                var progress = new InstallerProgress(InstallMode.Install, vsPath9, vsPath10, targetPath);
                MessageBoxOptions options = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0;
                if (progress.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show(Strings.InstallFailureMessage, "LINQ to umbraco Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, options);
                    MessageBox.Show(progress.Error.ToString());
                }
            }
        }

        private static string GetVsPath9()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\9.0", false);
            if (key != null)
                return (string)key.GetValue("InstallDir");
            return null;
        }

        private static string GetVsPath10()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\VisualStudio\10.0", false);
            if (key != null)
                return (string)key.GetValue("InstallDir");
            return null;
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            var vsPath9 = GetVsPath9();
            var vsPath10 = GetVsPath10();
            if (vsPath9 != null || vsPath10 != null)
            {
                var progress = new InstallerProgress(InstallMode.Uninstall, vsPath9, vsPath10, null);
                progress.ShowDialog();
            }
        }
    }
}
