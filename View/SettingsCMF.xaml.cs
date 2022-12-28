using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FAIRSCMFEditor.View
{
    /// <summary>
    /// Interaction logic for SettingsCMF.xaml
    /// </summary>
    public partial class SettingsCMF : UserControl
    {
        public SettingsCMF()
        {
            InitializeComponent();

            if (App.CMFEditorVesion == "10")
            {
                gridVesion13.Visibility = Visibility.Visible;
                gridVesion9.Visibility = Visibility.Collapsed;
                gridVesion16.Visibility = Visibility.Collapsed;
            }
            else if(App.CMFEditorVesion == "9")
            {
                gridVesion13.Visibility = Visibility.Collapsed;
                gridVesion9.Visibility = Visibility.Visible;
                gridVesion16.Visibility = Visibility.Collapsed;
            }
            else 
            {
                gridVesion13.Visibility = Visibility.Collapsed;
                gridVesion9.Visibility = Visibility.Collapsed;
                gridVesion16.Visibility = Visibility.Visible;
            }
        }
        
    }
}
