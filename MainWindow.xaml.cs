using CMFFileEditor.ViewModel;
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

namespace FAIRSCMFEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = CMFFileEditor.ViewModel.ViewModelMain.Instance;
        }

        private void mainWindow__SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 480)
            {
                ApplicationValues.ViewHeight = contentViewer.ActualHeight;
                ViewModelMain.Instance.CurrentView.ViewHeight = contentViewer.ActualHeight;
            }
            else if (e.NewSize.Height == 480)
            {
                ApplicationValues.ViewHeight = contentViewer.ActualHeight;
                ViewModelMain.Instance.CurrentView.ViewHeight = contentViewer.ActualHeight;
            }
        }
    }
}
