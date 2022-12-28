using CMFFileEditor.ViewModel;
using FAIRSCMFEditor.View.Common;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Interaction logic for CMFFilesForRunID.xaml
    /// </summary>
    public partial class CMFFilesForRunID : UserControl
    {

        List<CMFRunID> runIDList = null;
        DateTime minDate = DateTime.MinValue;
        DateTime maxDate = DateTime.MaxValue;
       // public PagingElements pagingCtrl_;

        public CMFFilesForRunID()
        {
            try
            {
                InitializeComponent();
                dateRange_.ForwardSelectionChanged += DateRange_ForwardSelectionChanged;
                PrepareDataSource();
                SetMinMaxDate();
                 pagingCtrl_.PageSize = SetGridPageSize(ConfigurationManager.AppSettings["AdvanceSearchPageSize"]);
               // pagingCtrl_.PageSize = SetGridPageSize("15");
                pagingCtrl_.CurrentPageIndex = 0;

                if (App.CMFEditorVesion == "16")
                {
                    stkPanelCMFFileSearch.Visibility = Visibility.Visible;
                    grdSearch.RowDefinitions[4].Height = new GridLength(40);
                    grdRunIds.RowDefinitions[0].Height = new GridLength(200);
                }
                else
                {
                    stkPanelCMFFileSearch.Visibility = Visibility.Collapsed;
                    grdSearch.RowDefinitions[4].Height = new GridLength(0);
                    grdRunIds.RowDefinitions[0].Height = new GridLength(160);
                }

            }
            catch (Exception ex)
            {

                
            }

        }

        private void SetMinMaxDate()
        {
            try
            {
                minDate = runIDList.Min(x => Convert.ToDateTime(x.CreatedDate));
                maxDate = runIDList.Max(x => Convert.ToDateTime(x.CreatedDate));
                dateRange_.InitStartEnd(minDate, maxDate);
            }
            catch (Exception ex)
            {

            }
        }

        private void DateRange_ForwardSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                pagingCtrl_.CurrentPageIndex = 0;
                BindRunIDs();
                
            }
            catch(Exception ex)
            {

            }
        }
        private void PrepareDataSource()
        {

            try
            {
                runIDList = new List<CMFRunID>();
                string runIDPath = GetRunIdPath();
                string datFilePath = "";
                Dictionary<string, string> dicFileList = new Dictionary<string, string>();

                foreach (var item in Directory.GetDirectories(runIDPath))
                {
                    string folderPath = item;
                    if (Directory.Exists(folderPath))
                    {
                        datFilePath = folderPath + "\\downloaded.dat";
                        if (File.Exists(datFilePath))
                        {
                            foreach (var line in File.ReadAllLines(datFilePath))
                            {
                                string[] arr = line.Split('\t');
                                if (arr.Count() >= 2)
                                {
                                    string[] dateSplit = arr[1].Split(' ');
                                    string fileName = "";
                                    if (dateSplit != null && dateSplit.Count() >= 1)
                                    {

                                        fileName = Convert.ToString(arr[0]) + "_Run_" + Convert.ToDateTime(arr[1]).ToString("MMddyy_HHmmss");

                                    }
                                    string fileValue = folderPath + "\\" + fileName + "_decrypted.zip";

                                    if (File.Exists(fileValue))
                                    {

                                        CMFRunID runID = new CMFRunID { FileName = fileName, FilePath = fileValue, CreatedDate = Convert.ToString(Convert.ToDateTime(arr[1])), InstrumentName = arr[0],CMFFiles= BindCMFFiles(fileValue) };
                                        runIDList.Add(runID);
                                    }
                                }
                            }
                        }

                    }
                }
                //_runIdList.ItemsSource = runIDList;
            }
            catch (Exception ex)
            {

            }
        }

        private string GetRunIdPath()
        {
            //By default it is checking with the guid extension which is the latest implementation
            string retVal = System.Configuration.ConfigurationManager.AppSettings["RunIDPath"];
            if(!Directory.Exists(retVal))
            {
                //if the folder doesn't exist then it will look for older implentation of FAIRS where the s2 folder is not locked
                //so it will remove the guid extension
                retVal = retVal.Split('.')[0];
            }
            return retVal;
        }

        private List<string> BindCMFFiles(string RunIdPath)
        {
            List<string> filesList = new List<string>();
            try
            {              
                if (File.Exists(RunIdPath))
                {
                    using (ZipFile zip = new ZipFile(RunIdPath))
                    {

                        foreach (ZipEntry zipEntry in zip.Entries.Where(x => x.FileName.Contains(".xml")))
                        {
                            if (!zipEntry.FileName.ToLower().Contains("rfidmismatch"))
                            {
                               
                                    filesList.Add(zipEntry.FileName);
                                                              
                            }

                        }
                    }
                }            
            }
            catch (Exception ex)
            {

            }

            return filesList;
        }

        void PageLoaded(object sender, RoutedEventArgs e)
        {

            if (null != pagingCtrl_)
            {
                pagingCtrl_.PageChanged += () =>
                {
                    BindRunIDs();
                };
            }

            // BindRunIDs();
        }

        private int SetGridPageSize(string configPageSize)
        {
            var retVal = 10; //ConfigurationConstant.DefaultPageSize;
            if (int.TryParse(configPageSize, out retVal))
            {
                return retVal;
            }
            return retVal;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 450)
            {
                var no_of_items_canbeshown = Convert.ToInt32((e.NewSize.Height - 475) / 22);
                var newPagesize = Convert.ToInt32(ConfigurationManager.AppSettings["AMFFlileRunIDPageSize"]) + no_of_items_canbeshown;
                newPagesize = newPagesize > 11 ? 11 : newPagesize;
                if (pagingCtrl_.PageSize != newPagesize)
                {
                    pagingCtrl_.CurrentPageIndex = 0;
                    pagingCtrl_.PageSize = newPagesize;
                    BindRunIDs();
                }
            }
            else
            {
                var defaultPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["AMFFlileRunIDPageSize"]);
                if (pagingCtrl_.PageSize != defaultPageSize)
                {
                    pagingCtrl_.CurrentPageIndex = 0;
                    pagingCtrl_.PageSize = defaultPageSize;
                    BindRunIDs();
                }
            }
        }

        private void pagingCtrl__PageChanged()
        {

        }

        private void SetPagingControl(int totalCount)
        {
            pagingCtrl_.TotalItems = totalCount;
            pagingCtrl_.currentPage.Text = (pagingCtrl_.CurrentPage).ToString();
            pagingCtrl_.totalPage.Text = pagingCtrl_.TotalPages.ToString();
            pagingCtrl_.ConfigureButtons();
        }

        private void btnSelectRunID_Click(object sender, RoutedEventArgs e)
        {
            var row = _runIdList.SelectedItem as CMFRunID;
            if(row!=null)
            {
                List<string> CMFFileList = null;
                if (!string.IsNullOrEmpty(SearchText_CMFFile))
                {
                    CMFFileList = BindCMFFiles(row.FilePath).Where(x => x.ToLower().Contains(SearchText_CMFFile.ToLower())).ToList();
                }
                else
                {
                    CMFFileList = BindCMFFiles(row.FilePath);
                }
               

                if (App.CMFEditorVesion == "16")
                {
                    if (CMFFileList.Count > 1)
                    {
                        ViewModelMain.Instance.CurrentView = new CMFXMLFilesViewModel(row.FilePath, row.InstrumentName, row.FileName, SearchText_CMFFile, CMFFileList);
                    }
                    else if (CMFFileList.Count == 1)
                    {                           
                            ViewModelMain.Instance.CurrentView = new CMFXmlFileEditorRevision16ViewModel(row.FilePath, row.InstrumentName, CMFFileList, true);
                    }
                }
                else
                {
                    ViewModelMain.Instance.CurrentView = new CMFXMLFilesViewModel(row.FilePath, row.InstrumentName, row.FileName, SearchText_CMFFile, CMFFileList);
                }

                             
                /*if (App.CMFEditorVesion == "13")
                {
                    ViewModelMain.Instance.CurrentView = new CMFXMLFilesViewModel(row.FilePath, row.InstrumentName, row.FileName); 
                }
                else if(App.CMFEditorVesion == "9")
                {
                    ViewModelMain.Instance.CurrentView = new CMFXmlFileEditorViewModel(row.FilePath, row.InstrumentName, row.FileName);
                }
                else if (App.CMFEditorVesion == "16")
                {
                    ViewModelMain.Instance.CurrentView = new CMFXMLFilesViewModel(row.FilePath, row.InstrumentName, row.FileName);
                }*/
            }
        }

        private void _runIdList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = _runIdList.SelectedItem as CMFRunID;
            borderSelectRunID.IsEnabled = row != null;
            btnSelectRunID.IsEnabled = row != null;
        }

        private void searchKeywords__OnSearch(string obj)
        {
            try
            {
                BindRunIDs();
            }
            catch (Exception ex) { }
        }

        private void searchKeywords__OnClear()
        {
            try
            {
                if (null != _runIdList)
                {
                    BindRunIDs();
                    pagingCtrl_.CurrentPageIndex = 0;
                }
            }
            catch (Exception ex)
            {
            }
        }


        private void CMFFilesSearchKeywords__OnSearch(string obj)
        {
            try
            {
                BindRunIDs();
            }
            catch (Exception ex) { }
        }

        private void CMFFilesSearchKeywords__OnClear()
        {
            try
            {
                if (null != _runIdList)
                {
                    BindRunIDs();
                    pagingCtrl_.CurrentPageIndex = 0;
                }
            }
            catch (Exception ex)
            {
            }
        }



        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            pagingCtrl_.CurrentPageIndex = 0;
            PrepareDataSource();
            SetMinMaxDate();
            BindRunIDs();         
        }

        private void BindRunIDs()
        {
            try
            {
                int skipCount = pagingCtrl_.PageSize * pagingCtrl_.CurrentPageIndex;
                int takeCount = pagingCtrl_.PageSize;
                List<CMFRunID> filterList = null;
                if(string.IsNullOrEmpty(SearchText))
                {
                    filterList = runIDList.Where(i => Convert.ToDateTime(i.CreatedDate) >= MinimumDate && Convert.ToDateTime(i.CreatedDate) <= MaximumDate).ToList();
                }
                else
                {
                    filterList = runIDList.Where(i => string.Format("{0}{1}", i.FileName.ToLower(), i.InstrumentName.ToLower()).Contains(SearchText.ToLower()) 
                    && Convert.ToDateTime(i.CreatedDate) >= MinimumDate && Convert.ToDateTime(i.CreatedDate) <= MaximumDate).ToList();
                }
                
                if (!string.IsNullOrEmpty(SearchText_CMFFile))
                {
                    filterList = filterList.Where(i => i.CMFFiles.Any(x => x.ToLower().Contains(SearchText_CMFFile.ToLower()))).ToList();                
                }


                    SetPagingControl(filterList.Count);
                 _runIdList.ItemsSource = filterList.Skip(skipCount).Take(takeCount); 
                //if (filterList!=null && filterList.Count > 0)
                //{
                //    _runIdList.ItemsSource = filterList.Skip(skipCount).Take(takeCount); 
                //}
                //else
                //{
                    
                //}
            }
            catch (Exception ex)
            {

            }
        }

        private string  SearchText
        {
            get
            {
                return null == searchKeywords_ || searchKeywords_.SearchInstructions != searchKeywords_.SearchText ? searchKeywords_.SearchText : string.Empty;
            }
        }

        private string SearchText_CMFFile
        {
            get
            {
                return null == searchKeywordsCMFFile_ || searchKeywordsCMFFile_.SearchInstructions != searchKeywordsCMFFile_.SearchText ? searchKeywordsCMFFile_.SearchText : string.Empty;
            }
        }

        private DateTime MinimumDate
        {
            get
            {
                return null != dateRange_ ? dateRange_.GetCurrStart() : minDate;
            }
        }

        private DateTime MaximumDate
        {
            get
            {
                return null != dateRange_ ? dateRange_.GetCurrEnd() : maxDate;
            }
        }

       
    }

    public class CMFRunID
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public string CreatedDate { get; set; }

        public string InstrumentName { get; set; }

        public List<string> CMFFiles{ get; set; }
    }
}
