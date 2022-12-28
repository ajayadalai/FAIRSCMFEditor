using CMFFileEditor.ViewModel;
using FAIRSCMFEditor.Domain;
using FAIRSCMFEditor.View.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
    /// Interaction logic for SourceLab.xaml
    /// </summary>
    public partial class SourceLab : UserControl
    {
        SourceLabViewModel vm = null;
        public SourceLab()
        {
            InitializeComponent();
            vm = ViewModelMain.Instance.CurrentView as SourceLabViewModel;
            vm.OnSaveCompleted += Vm_OnSaveCompleted;
            if (App.CMFEditorVesion == "9")
            {
                SetMaxLength();

            }
        }

        private void SetMaxLength()
        {
            txtValue1.MaxLength = 9;
            txtValue2.MaxLength = 9;
            txtValue3.MaxLength = 9;
            txtValue4.MaxLength = 9;
            txtValue5.MaxLength = 9;
            txtValue6.MaxLength = 9;

        }

        private void Vm_OnSaveCompleted(Domain.EventArguements<Domain.IViewEventArguments> arguments)
        {
            if (!arguments.IsMessage)
            {
                // Message.Display("Saved Successfully!!", string.Format("{0} saved successfully!!", ViewModelMain.Instance.Header));
                Message.Display("Saved Successfully", System.Environment.NewLine + string.Format("{0} saved successfully.", ViewModelMain.Instance.Header));
            }
            else
            {
                if ((arguments.EventData as ArguementMessage).Type == MessageType.Message)
                    Message.Display("Error", System.Environment.NewLine + arguments.EventData.Content);
            }
        }
    }
}
