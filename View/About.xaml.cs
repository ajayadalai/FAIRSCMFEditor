using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FAIRSCMFEditor.View
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
            tbVersion.Text = string.Format("Version {0} CMF 3.3 Revision {1}", Convert.ToString(ConfigurationManager.AppSettings["Version"]), App.CMFEditorVesion);
            tbCopyRight.Text = string.Format("{1} {0} ANDE Corporation. All rights reserved.", DateTime.Now.Year, "\u00A9");
        }

        public static bool Display()
        {
            var abt = new About();
            bool? ret = abt.ShowDialog();
            return ret.HasValue ? ret.Value : false;
        }

        private void ok__Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
