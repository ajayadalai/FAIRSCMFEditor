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
    /// Interaction logic for SpecimenCategory.xaml
    /// </summary>
    public partial class SpecimenCategory : UserControl
    {
        SpecimenCategoryViewModel vm = null;
        public SpecimenCategory()
        {
            InitializeComponent();
            vm = ViewModelMain.Instance.CurrentView as SpecimenCategoryViewModel;
            vm.OnSaveCompleted += Vm_OnSaveCompleted;
            if (App.CMFEditorVesion == "9")
            {
                SetMaxLength();

            }
        }

        private void SetMaxLength()
        {
            txtCategory1.MaxLength = 21;
            txtCategory2.MaxLength = 21;
            txtCategory3.MaxLength = 21;
            txtCategory4.MaxLength = 21;
            txtCategory5.MaxLength = 21;
            txtCategory6.MaxLength = 21;
        }

        private void Vm_OnSaveCompleted(Domain.EventArguements<Domain.IViewEventArguments> arguments)
        {
            if (!arguments.IsMessage)
            {
                //  Message.Display("Saved Successfully!!", string.Format("{0} saved successfully!!", ViewModelMain.Instance.Header));
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


