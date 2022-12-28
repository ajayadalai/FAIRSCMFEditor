using CMFFileEditor.ViewModel;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for CMFXmlFiles.xaml
    /// </summary>
    public partial class CMFXmlFiles : UserControl
    {
        CMFXMLFilesViewModel vm = ViewModelMain.Instance.CurrentView as CMFXMLFilesViewModel;
        string RunIdPath = string.Empty;
        string InstrumentName = string.Empty;
        string SearchCMFFileNameText = string.Empty;
        List<string> filesList = null;
        List<string> SelectedFileList = new List<string>();
        List<string> CMFFileList = null;
        public CMFXmlFiles()
        {
            InitializeComponent();

            if (App.CMFEditorVesion == "9")
            {
                xmlFileList.SelectionMode = SelectionMode.Multiple;
            }
            else if (App.CMFEditorVesion == "10")
            {
                xmlFileList.SelectionMode = SelectionMode.Multiple;
            }
            else
            {
                xmlFileList.SelectionMode = SelectionMode.Single;
            }

            RunIdPath = vm.FilePath;
            InstrumentName = vm.InstrumentName;
            SearchCMFFileNameText = vm.SearchCMFFileNameText;
            CMFFileList = vm.CMFFileList;
            BindList();
        }

        //private void BindList()
        //{
        //    try
        //    {
        //        filesList = new List<string>();

        //        if (File.Exists(RunIdPath))
        //        {
        //            using (ZipFile zip = new ZipFile(RunIdPath))
        //            {

        //                foreach (ZipEntry zipEntry in zip.Entries.Where(x => x.FileName.Contains(".xml")))
        //                {
        //                    if (!zipEntry.FileName.ToLower().Contains("rfidmismatch"))
        //                    {
        //                        if (!string.IsNullOrEmpty(SearchCMFFileNameText))
        //                        {
        //                            if (zipEntry.FileName.ToLower().Contains(SearchCMFFileNameText.ToLower()))
        //                            {
        //                                filesList.Add(zipEntry.FileName);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            filesList.Add(zipEntry.FileName);
        //                        }

        //                    }

        //                }
        //            }

        //            if (filesList != null && filesList.Count > 0)
        //        {
        //            xmlFileList.Visibility = Visibility.Visible;
        //            borderNoRecord.Visibility = Visibility.Collapsed;
        //            xmlFileList.ItemsSource = filesList;
        //        }
        //        else
        //        {
        //            xmlFileList.Visibility = Visibility.Collapsed;
        //            borderNoRecord.Visibility = Visibility.Visible;
        //        }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        private void BindList()
        {
            try
            {
                filesList = new List<string>();

                foreach (string fileName in CMFFileList)
                {
                    if (!string.IsNullOrEmpty(SearchCMFFileNameText))
                    {
                        if (fileName.ToLower().Contains(SearchCMFFileNameText.ToLower()))
                        {
                            filesList.Add(fileName);
                        }
                    }
                    else
                    {
                        filesList.Add(fileName);
                    }
                }
                
                if (filesList != null && filesList.Count > 0)
                    {
                        xmlFileList.Visibility = Visibility.Visible;
                        borderNoRecord.Visibility = Visibility.Collapsed;
                        xmlFileList.ItemsSource = filesList;
                    }
                    else
                    {
                        xmlFileList.Visibility = Visibility.Collapsed;
                        borderNoRecord.Visibility = Visibility.Visible;
                    }
            }
            catch (Exception ex)
            {

            }
        }

        private void pagingCtrl__PageChanged()
        {

        }

        private void btnSelectCMFFile_Click(object sender, RoutedEventArgs e)
        {
            var row = xmlFileList.SelectedItem;
            if (row != null)
            {
                if (App.CMFEditorVesion == "9" || App.CMFEditorVesion == "10")
                {
                    ViewModelMain.Instance.CurrentView = new CMFXmlFileEditorViewModel(RunIdPath, InstrumentName, xmlFileList.SelectedItems.Cast<string>(), SearchCMFFileNameText, CMFFileList);
                }
                else if (App.CMFEditorVesion == "16")
                {
                    SelectedFileList = xmlFileList.SelectedItems.Cast<string>().ToList();
                    ViewModelMain.Instance.CurrentView = new CMFXmlFileEditorRevision16ViewModel(RunIdPath, InstrumentName, SelectedFileList, SearchCMFFileNameText, CMFFileList);
                }
            }
        }

        private void xmlFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = xmlFileList.SelectedItem;

            borderSelectCMFFile.IsEnabled = row != null;
            btnSelectCMFFile.IsEnabled = row != null;
        }
    }
}
