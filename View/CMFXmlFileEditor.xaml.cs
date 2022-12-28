using CMFFileEditor.ViewModel;
using FAIRSCMFEditor.Domain;
using FAIRSCMFEditor.View.Common;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
//using FAIRSCMFEditor.Helper;

namespace FAIRSCMFEditor.View
{
    /// <summary>
    /// Interaction logic for CMFXmlFileEditor.xaml
    /// </summary>
    public partial class CMFXmlFileEditor : UserControl
    {
        CMFXmlFileEditorViewModel vm = ViewModelMain.Instance.CurrentView as CMFXmlFileEditorViewModel;
        string RunIdPath;
        string xmlFileName;
        string InstrumentName = string.Empty;
        XmlNamespaceManager nameSpaceManager = null;
        string namespaceName = "ns";
        XmlDocument xdoc = new XmlDocument();
        List<string> CMFFileList = null;
        string SearchCMFFileNameText = string.Empty;
        public CMFXmlFileEditor()
        {
            InitializeComponent();
            RunIdPath = vm.RunIdPath;
            xmlFileName = null != vm.SelectedCMFFiles ? vm.SelectedCMFFiles.FirstOrDefault():string.Empty;
            vm.OnCancelEvent += OnCancelClick;
            InstrumentName = vm.InstrumentName;

            SearchCMFFileNameText = vm.SearchCMFFileNameText;
            CMFFileList = vm.CMFFileList;

            BindData();
            if (App.CMFEditorVesion == "9")
            {
                gridEditor.RowDefinitions[3].Height = new GridLength(0);
                gridEditor.RowDefinitions[6].Height = new GridLength(0);
            }
            BindComboBox();

        }

        private void OnCancelClick(object args)
        {
            ViewModelMain.Instance.CurrentView = new CMFXMLFilesViewModel(RunIdPath, InstrumentName, SearchCMFFileNameText, CMFFileList);
            ViewModelMain.Instance.SubTitle = string.Empty;
            //if (App.CMFEditorVesion == "13")
            //{
            //    ViewModelMain.Instance.CurrentView = new CMFXMLFilesViewModel(RunIdPath, InstrumentName);
            //}
            //else
            //{
            //    ViewModelMain.Instance.CurrentView = new CMFFilesForRunIDViewModel();
            //}
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileGeneratorFactory fileGenerator = GetFileGenerator();
                string message;
                var isSucceed = fileGenerator.GetFileGenerator().GenerateFile(out message);
               //var isSucceed = fileGenerator.GetFileGenerator()
                if(isSucceed)
                {
                    if(!string.IsNullOrEmpty(message))
                    {
                        Message.Display("Warning!!", message);
                    }
                    Message.Display("Saved Successfully", System.Environment.NewLine+ "CMF File Saved Successfully.");
                    //Message.Display("Saved Successfully", System.Environment.NewLine + string.Format("{0} saved successfully.", ViewModelMain.Instance.Header));
                    ViewModelMain.Instance.CurrentView = new CMFFilesForRunIDViewModel();
                }
                else
                {
                    Message.Display("Error!! ", message);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private FileGeneratorFactory GetFileGenerator()
        {
            FileGeneratorFactory fileGenerator = null;
            switch (App.CMFEditorVesion)
            {
                case "10": //"13":
                    fileGenerator = new Revision13Factory(RunIdPath, vm.SelectedCMFFiles.ToList() , InstrumentName, GetSourceLab(), GetDestinationORI(), GetSubmitBy(), GetSpecimentCategory(), GetReadingByUser(), cmbSSId.Text, cmbSpecimenPartial.Text.ToLower(), xdoc);
                    break;
                case "9":
                    fileGenerator = new Revision9Factory(RunIdPath,vm.SelectedCMFFiles.ToList(), InstrumentName, GetSourceLab(), GetDestinationORI(), GetSpecimentCategory(), GetReadingByUser(), vm.GetMessageIDForRevision9());
                    break;
                default:
                    break;
            }
            return fileGenerator;
        }

        #region Private helper methods
        private void BindDataForRunID()
        {
            using (ZipFile zip = new ZipFile(RunIdPath))
            {
                ZipEntry zipEntry = zip.Entries.Where(x => x.FileName.Contains(".xml")).FirstOrDefault();
                BindValuesFromZip(zipEntry);
            }
        }

        private void BindComboBox()
        {
            Dictionary<ConfigType, List<string>> _configValues = vm.ConfigValues();
            BindComboBoxData(_configValues[ConfigType.DestinatinORI], cmbDestinationORI);
            BindComboBoxData(_configValues[ConfigType.SourceLab], cmbSourceLab);
            BindComboBoxData(_configValues[ConfigType.SubmitByUser], cmbSubmitByUserID);
            BindComboBoxData(_configValues[ConfigType.SpecimenType], cmbSpecimenCat);
            BindComboBoxData(_configValues[ConfigType.ReadingByUser], cmbReadingBy);
        }

        private void BindComboBoxData(List<string> _list, ComboBox cmb)
        {

            if (_list != null && _list.Count > 0)
            {
                _list.Insert(0, "Select");
            }
            else
            {
                _list = new List<string>();
                _list.Add("Select");
            }
            cmb.ItemsSource = _list.Where(x=> !string.IsNullOrEmpty(x)) ;
            cmb.SelectedValue = "Select";
        }

        private void BindData()
        {
            try
            {
                using (ZipFile zip = new ZipFile(RunIdPath))
                {
                    foreach (ZipEntry entry in zip.Entries.Where(x => x.FileName.Equals(xmlFileName)))
                    {
                        BindValuesFromZip(entry);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        private void BindValuesFromZip(ZipEntry entry)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    entry.Extract(ms);
                    ms.Position = 0;
                    var sr = new StreamReader(ms);
                    var myStr = sr.ReadToEnd();


                    xdoc.LoadXml(myStr);

                    createNameSpaceManager(xdoc);

                    LoadValues();
                }
            }
            catch (Exception ex)
            {

            }
        }


        private void createNameSpaceManager(XmlDocument xmlDoc)
        {
            if (xmlDoc.ChildNodes[1].Attributes != null)
            {
                var xmlns = xmlDoc.ChildNodes[1].Attributes["xmlns"];

                if (xmlns != null)
                {
                    nameSpaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                    nameSpaceManager.AddNamespace(namespaceName, xmlns.Value);
                    nameSpaceManager.AddNamespace(namespaceName, "urn:CODISImportFile-schema");
                }
            }
        }

        private void LoadValues()
        {
            try
            {
                txtDestinationORI.Text = NodeResults("//ns:CODISImportFile/ns:DESTINATIONORI");
                txtSourceLab.Text = NodeResults("//ns:CODISImportFile/ns:SOURCELAB");
                txtSubmitByUserID.Text = NodeResults("//ns:CODISImportFile/ns:SUBMITBYUSERID");
                txtSpecimenCat.Text = NodeResults("//ns:CODISImportFile/ns:SPECIMEN/ns:SPECIMENCATEGORY");
                txtReadingBy.Text = NodeResults("//ns:CODISImportFile/ns:SPECIMEN/ns:LOCUS/ns:READINGBY");
            }
            catch (Exception ex)
            {

            }
        }

        private string NodeResults(string ChildNodePath)
        {
            string nodeValue = string.Empty;
            return nodeValue = xdoc.SelectSingleNode(ChildNodePath, nameSpaceManager).InnerText;
        }

        private string GetSourceLab()
        {
            if (cmbSourceLab.SelectedIndex > 0)
            {
                return cmbSourceLab.SelectedValue.ToString();
            }
            else
            {
                return txtSourceLab.Text;
            }
        }

        private string GetDestinationORI()
        {
            if (cmbDestinationORI.SelectedIndex > 0)
            {
                return cmbDestinationORI.SelectedValue.ToString();
            }
            else
            {
                return txtDestinationORI.Text;
            }
        }

        private string GetSubmitBy()
        {
            if (cmbSubmitByUserID.SelectedIndex > 0)
            {
                return cmbSubmitByUserID.SelectedValue.ToString();
            }
            else
            {
                return txtSubmitByUserID.Text;
            }
        }

        private string GetSpecimentCategory()
        {
            if (cmbSpecimenCat.SelectedIndex > 0)
            {
                return cmbSpecimenCat.SelectedValue.ToString();
            }
            else
            {
                return txtDestinationORI.Text;
            }
        }

        private string GetReadingByUser()
        {
            if (cmbReadingBy.SelectedIndex > 0)
            {
                return cmbReadingBy.SelectedValue.ToString();
            }
            else
            {
                return txtReadingBy.Text;
            }
        }
        #endregion
    }
}
