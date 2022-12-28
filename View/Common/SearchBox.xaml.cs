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

namespace FAIRSCMFEditor.View.Common
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        #region Events and delegates
        /// <summary>
        /// 
        /// </summary>
        public event SearchCompleteHandler SearchComplete;

        /// <summary>
        /// 
        /// </summary>
        public delegate void SearchCompleteHandler();
        #endregion
        private string searchInstructions_ = string.Empty;

        public string SearchInstructions
        {
            get
            {
                return searchInstructions_;
            }
            set
            {
                searchInstructions_ = value;
                if (searchBox_ != null)
                {
                    searchBox_.Text = searchInstructions_;
                }
            }

        }

        public string SearchText
        {
            get
            {
                var ret = searchBox_.Text.Trim();
                if (string.IsNullOrEmpty(ret))
                {
                    ret = string.Empty;
                }
                return ret;
            }
        }

        private Action<string> onSearch_;

        public event Action<string> OnSearch
        {
            add
            {
                onSearch_ += value;
            }

            remove
            {
                onSearch_ -= value;
            }
        }

        private Action onClear_;

        public event Action OnClear
        {
            add
            {
                onClear_ += value;
            }

            remove
            {
                onClear_ -= value;
            }
        }

        private bool isEmpty_ = true;
        private bool isSettingToEmpty_ = false;

        public SearchBox()
        {

            SearchInstructions = "Search by User ID or User Role";
            InitializeComponent();
            SetEmpty();
        }

        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(string), typeof(SearchBox), new PropertyMetadata(""));



        private void SetEmpty()
        {
            isSettingToEmpty_ = true;
            searchBox_.Foreground = Brushes.Gray;
            searchBox_.FontStyle = FontStyles.Italic;
            searchBox_.Text = SearchInstructions;
            searchBox_.CaretIndex = 0;
            isEmpty_ = true;
            isSettingToEmpty_ = false;
        }

        public void ShowSearch()
        {
            searchIcon_.Visibility = Visibility.Visible;
            clearIcon_.Visibility = Visibility.Hidden;
        }

        public void ShowClear()
        {
            searchIcon_.Visibility = Visibility.Hidden;
            clearIcon_.Visibility = Visibility.Visible;
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (isSettingToEmpty_)
            {
                return;
            }

            if (SearchInstructions.Trim().Length > 0 && searchBox_.Text.Contains(SearchInstructions) && searchBox_.Text.Length > SearchInstructions.Length)
            {
                isEmpty_ = false;
                searchBox_.Text = searchBox_.Text.Replace(SearchInstructions, "");
            }

            if (!isEmpty_ && searchBox_.Text.Length == 0)
            {
                SetEmpty();
            }

            if (searchBox_.Text.Length != 0)
                DoSearch();
            else
                DoClear();
            ShowSearch();
        }

        private void SearchPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (isSettingToEmpty_)
            {
                return;
            }

            if (isEmpty_ && !e.Text.StartsWith("\r"))
            {
                searchBox_.Foreground = Brushes.Black;
                searchBox_.FontStyle = FontStyles.Normal;
                isSettingToEmpty_ = true;
                searchBox_.Text = string.Empty;
                isSettingToEmpty_ = false;
                isEmpty_ = false;
            }
        }

        private void SearchPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isEmpty_)
            {
                searchBox_.Focus();
                searchBox_.CaretIndex = 0;
                e.Handled = true;
            }
        }

        private void SearchKeyDown(object sender, KeyEventArgs e)
        {
            if (isEmpty_ && (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Space))
            {
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                if (!isEmpty_)
                {
                    DoSearch();
                }
                else
                {
                    DoClear();
                }
            }
        }

        private void ClickSearch(object sender, MouseButtonEventArgs e)
        {
            DoSearch();
        }

        private void ClickClear(object sender, MouseButtonEventArgs e)
        {
            DoClear();
        }

        public void DoSearch()
        {
            if (!isEmpty_)
            {
                ShowClear();
                if (onSearch_ != null)
                {
                    onSearch_(SearchText);
                }
            }
            else
            {
                if (onClear_ != null)
                {
                    onClear_();
                }
            }
            NotifyCompleted();
        }

        public void DoClear()
        {
            SetEmpty();
            if (onClear_ != null)
            {
                onClear_();
            }
            ShowSearch();
            NotifyCompleted();
        }

        private void NotifyCompleted()
        {
            if (SearchComplete != null)
            {
                SearchComplete();
            }
        }
    }
}
