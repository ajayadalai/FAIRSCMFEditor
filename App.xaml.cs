using CMFFileEditor.ViewModel;
using FAIRSCMFEditor.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace FAIRSCMFEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static string cmfEditorVersion;

        public static string CMFEditorVesion
        {
            get
            {
                if (cmfEditorVersion == null)
                {
                    cmfEditorVersion = Convert.ToString(System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(System.Configuration.ConfigurationManager.AppSettings["CMFEditorVersion"])));
                }
                return cmfEditorVersion;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                CheckAdminAndInitiate();
               
            }
            catch (Exception ex) { }
        }

        private void CheckAdminAndInitiate()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,
                // so instead we launch the app as administrator in a new process.
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    MessageBox.Show("Sorry, but I don't seem to be able to start " +
                       "this program with administrator rights!");
                }

                // Shut down the current process
                Application.Current.Shutdown();
            }
            else
            {
                ConfigConstants.DBPath = System.Configuration.ConfigurationSettings.AppSettings["SQLiteDBPath"];
                var AppVersionViewModel = new AppVersionViewModel();
                var curVersion = AppVersionViewModel.GetCurrentVersion();
                if (string.IsNullOrEmpty(curVersion))
                {
                    AppVersionViewModel.CreateAppVersionTable(CMFEditorVesion);
                }
                else if (CMFEditorVesion != curVersion)
                {
                    AppVersionViewModel.DropTables();
                    AppVersionViewModel.CreateAppVersionTable(CMFEditorVesion);
                }
            }
        }
    }
}
