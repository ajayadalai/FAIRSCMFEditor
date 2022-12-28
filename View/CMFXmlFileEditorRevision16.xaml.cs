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
using CMFFileEditor.ViewModel;
using FAIRSCMFEditor.Domain;
//using FAIRSCMFEditor.Helper;
using FAIRSCMFEditor.View.Common;

namespace FAIRSCMFEditor.View
{
    /// <summary>
    /// Interaction logic for CMFXmlFileEditorRevision16.xaml
    /// </summary>
    public partial class CMFXmlFileEditorRevision16 : UserControl
    {
        CMFXmlFileEditorRevision16ViewModel vm = ViewModelMain.Instance.CurrentView as CMFXmlFileEditorRevision16ViewModel;
        string RunIdPath;
        List<string> selectedFiles = new List<string>();
        string InstrumentName = string.Empty;
        public CMFXmlFileEditorRevision16()
        {
            InitializeComponent();

            RunIdPath = vm.RunIdPath;
            selectedFiles = vm.SelectedCMFFiles;
            InstrumentName = vm.InstrumentName;
         
            BindComboBox();
            cmbDestinationORI.Text =  string.IsNullOrEmpty(ViewModelMain.Instance.DestinationORI) ? "Select" : ViewModelMain.Instance.DestinationORI;
            cmbSourceORI.Text = string.IsNullOrEmpty(ViewModelMain.Instance.SourceLab) ? "Select" : ViewModelMain.Instance.SourceLab;
            cmbSpecimenCategory.Text = string.IsNullOrEmpty(ViewModelMain.Instance.SpecimenCategory) ? "Select" : ViewModelMain.Instance.SpecimenCategory;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsValid())
                {
                    FileGeneratorFactory fileGenerator = new Revision16Factory(prepareDataForRevision16()); //GetFileGenerator();
                    string message;
                    var isSucceed = fileGenerator.GetFileGenerator().GenerateFile(out message);
                    if (isSucceed)
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            Message.Display("Warning!!", message);
                        }
                        Message.Display("Saved Successfully", System.Environment.NewLine + string.Format("{0} saved successfully.", "Rapid Import Format CMF file"));
                        //Message.Display("Saved Successfully!! ", "CMF File Saved Successfully");
                        ViewModelMain.Instance.DestinationORI = cmbDestinationORI.Text;
                        ViewModelMain.Instance.SourceLab = cmbSourceORI.Text;
                        ViewModelMain.Instance.SpecimenCategory = cmbSpecimenCategory.Text;
                        ViewModelMain.Instance.CurrentView = new CMFFilesForRunIDViewModel();
                    }
                    else
                    {
                        Message.Display("Error!! ", message);
                    } 
                }
            }
            catch (Exception ex)
            {

            }
        }

        private bool IsValid()
        {
            if (cmbSourceORI.SelectedIndex == 0)
            {
                //tbError.Text = "Invalid Selection for Source ORI";
                //tbError.Visibility = Visibility.Visible;

                Message.Display("Error", "Please select Source ORI.");
                return false;
            }
            if (cmbDestinationORI.SelectedIndex == 0)
            {
                //tbError.Text = "Invalid Selection for Destination ORI";
                //tbError.Visibility = Visibility.Visible;
                Message.Display("Error", "Please select Destination ORI.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUniqueId.Text))
            {
                Message.Display("Error", "Please enter Livescan Unique Identifier.");

                return false;
            }
            DateTime date = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(txtArrestingDate.Text) && !DateTime.TryParse(txtArrestingDate.Text, out date))
            {
                Message.Display("Error", "Please enter valid Arrest Submission Date.");

                return false;
            }

            if(date > DateTime.Now)
            {
                Message.Display("Error", "Arrest submission date cannot be a future date.");

                return false;
            }
            TimeSpan time = TimeSpan.MinValue;
            if (!string.IsNullOrWhiteSpace(txtArrestingTime.Text))
            {
                if(string.IsNullOrEmpty(txtArrestingDate.Text))
                {
                    Message.Display("Error", "Please enter Arrest Submission Date.");
                    return false;
                }
                else if(!TimeSpan.TryParse(txtArrestingTime.Text, out time))
                {
                    Message.Display("Error", "Please enter valid Arrest Submission Time.");
                    return false;
                }
                if(date.AddSeconds(time.TotalSeconds) > DateTime.Now)
                {
                    Message.Display("Error", "Arrest Submission Time cannot be a future time.");
                    return false;
                }
            }
            if (string.IsNullOrEmpty(txtFingerprintDate.Text))
            {
                Message.Display("Error", "Please enter Fingerprint Capture Date.");

                return false;
            }

            if (string.IsNullOrEmpty(txtFingerprintTime.Text))
            {
                Message.Display("Error", "Please enter Fingerprint Capture Time.");

                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtFingerprintDate.Text) && !DateTime.TryParse(txtFingerprintDate.Text, out date))
            {
                Message.Display("Error", "Please enter valid Fingerprint Capture Date.");

                return false;
            }
            if (date > DateTime.Now)
            {
                Message.Display("Error", "Fingerprint Capture Date cannot be a future date.");

                return false;
            }
            if (!string.IsNullOrWhiteSpace(txtFingerprintTime.Text) && !TimeSpan.TryParse(txtFingerprintTime.Text, out time))
            {
                Message.Display("Error", "Please enter valid Fingerprint Capture Time.");

                return false;
            }
            if (date.AddSeconds(time.TotalSeconds) > DateTime.Now)
            {
                Message.Display("Error", "Fingerprint Capture Time cannot be a future time.");

                return false;
            }
            if (cmbSpecimenCategory.SelectedIndex == 0)
            {
                //tbError.Text = "Invalid Selection for Destination ORI";
                //tbError.Visibility = Visibility.Visible;
                Message.Display("Error", "Please select Specimen Category.");
                return false;
            }
            if (string.IsNullOrEmpty(txtSID.Text) && string.IsNullOrEmpty(txtUCN.Text))
            {
                //tbError.Text = "Invalid Selection for Destination ORI";
                //tbError.Visibility = Visibility.Visible;
                Message.Display("Error", "Please enter either SID or UCN. Both fields cannot be blank.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtArrestDesc.Text))
            {
                Message.Display("Error", "Please enter Arrest Offence Description.");

                return false;
            }




            return true;

        }
        private void BindComboBox()
        {
            Dictionary<ConfigType, List<string>> _configValues = vm.ConfigValues();
            BindComboBoxData(_configValues[ConfigType.DestinatinORI], cmbDestinationORI);
            BindComboBoxData(_configValues[ConfigType.SourceLab], cmbSourceORI);
            var specimenCategoryList = GetSpecimentCategoryList(_configValues[ConfigType.SpecimenType]);
            BindComboBoxData(specimenCategoryList, cmbSpecimenCategory);
            
        }

        private List<string> GetSpecimentCategoryList(List<string> list)
        {
            AddFixedCategories(ref list);
            return list;
        }

        private void BindComboBoxData(List<string> _list, ComboBox cmb)
        {
            if (_list == null)
                _list = new List<string>();
            _list.Insert(0, "Select");
            cmb.ItemsSource = _list;
            cmb.SelectedValue = "Select";
        }

        private void AddFixedCategories(ref List<string> list)
        {
            if (list == null)
                list = new List<string>();
            if (list.FindIndex(i => i.ToLower() == "legal") < 0)
            {
                list.Insert(0, "Legal");
            }
            if (list.FindIndex(i => i.ToLower() == "juvenile") < 0)
            {
                list.Insert(0, "Juvenile");
            }
            if (list.FindIndex(i => i.ToLower() == "detainee") < 0)
            {
                list.Insert(0, "Detainee");
            }
            if (list.FindIndex(i => i.ToLower() == "convicted offender") < 0)
            {
                list.Insert(0, "Convicted Offender");
            }
            if (list.FindIndex(i=>i.ToLower()=="arrestee") < 0)
            {
                list.Insert(0, "Arrestee");
            }
        }

        private FileGeneratorFactory GetFileGenerator()
        {
            FileGeneratorFactory fileGenerator = null;

            fileGenerator = new Revision16Factory(prepareDataForRevision16());
            return fileGenerator;
        }

        private Revision16Values prepareDataForRevision16()
        {
            Revision16Values revision16Values = new Revision16Values();

            revision16Values.RunIdPath = RunIdPath;
            revision16Values.SelectedXMLPath = selectedFiles;
            revision16Values.InstrumentName = InstrumentName;
            revision16Values.DestinationORI = GetDestinationORI();
            revision16Values.SourceORI = GetSourceLab();
            revision16Values.CODISSpecimenCategory = GetSpecimentCategory();
            revision16Values.SID = txtSID.Text;
            revision16Values.UCN = txtUCN.Text;
            revision16Values.LivescanUniqueEventIdentifier = txtUniqueId.Text;
            revision16Values.BookingAgencyConfigurableIdentifier = txtBookingAgencyConfigId.Text;
            revision16Values.ArrestingAgencyConfigurableIdentifier = txtArrestingAgencyConfigId.Text;
            //revision16Values.ArrestSubmissionDate = string.Format("{0}T{1}", txtArrestingDate.SelectedDate.Value.ToString("yyyy-MM-dd"), txtArrestingTime.SelectedDate.Value.ToString("HH:mm:ss"));
            revision16Values.ArrestSubmissionDate = GetArrestSubmissionDate();
            revision16Values.FingerprintCaptureDateTime = GetFingerprintCaptureTime();
            revision16Values.ArrestOffenseDescription = txtArrestDesc.Text;
            revision16Values.SpecimenComment = txtSpecimenComment.Text;
            revision16Values.MessageId = vm.GetMessageIDForRevision16();
            return revision16Values;
        }

        private string GetArrestSubmissionDate()
        {
            var retVal = string.Empty;
            retVal = string.IsNullOrEmpty(txtArrestingDate.Text) && string.IsNullOrEmpty(txtArrestingTime.Text) ? string.Empty :
                       !string.IsNullOrEmpty(txtArrestingTime.Text) ? string.Format("{0}T{1}", txtArrestingDate.Text, txtArrestingTime.Text) :
                       string.Format("{0}T{1}", txtArrestingDate.Text, "00:00:00");
            return retVal;
        }

        private string GetFingerprintCaptureTime()
        {
            var retVal = string.Empty;
            retVal = string.IsNullOrEmpty(txtFingerprintDate.Text) && string.IsNullOrEmpty(txtFingerprintTime.Text) ? string.Empty :
                       !string.IsNullOrEmpty(txtFingerprintTime.Text) ? string.Format("{0}T{1}", txtFingerprintDate.Text, txtFingerprintTime.Text) :
                       string.Format("{0}T{1}", txtFingerprintDate.Text, "00:00:00");
            return retVal;
        }

        private string GetDestinationORI()
        {
                return cmbDestinationORI.SelectedValue.ToString();            
        }

        private string GetSourceLab()
        {
                return cmbSourceORI.SelectedValue.ToString();           
        }

        private string GetSpecimentCategory()
        {
            return cmbSpecimenCategory.SelectedValue.ToString();            
        }

        
    }
}
