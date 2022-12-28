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
    /// Interaction logic for ReadingByUser.xaml
    /// </summary>
    public partial class ReadingByUser : UserControl
    {
        ReadingByUserViewModel vm = null;
        Button btnRemoveUser = new Button();
        public ReadingByUser()
        {
            InitializeComponent();
            if (App.CMFEditorVesion == "10")
            {
                grdAddUserID.Visibility = Visibility.Visible;
            }
            else
            {
                grdAddUserID.Visibility = Visibility.Collapsed;
            }
            vm = ViewModelMain.Instance.CurrentView as ReadingByUserViewModel;
            vm.OnAddDynamicUsers += Vm_OnAddDynamicUsers;
            vm.OnSaveCompleted += Vm_OnSaveCompleted;
            vm.OnSaveWithDynamicUsers += Vm_OnSaveWithDynamicUsers;
            vm.BindData();
            if (App.CMFEditorVesion == "9")
            {
                SetMaxLength();
            }
            btnRemoveUser.Click += BtnRemoveUser_Click;
        }

        private void Vm_OnSaveWithDynamicUsers()
        {
            var users = new List<string>();

            foreach (Control ele in grdUser.Children)
            {
                if (ele.GetType() == typeof(TextBox))
                {
                    string val = ((TextBox)ele).Text.ToString();
                    users.Add(val);
                }
            }

            vm.ValidateAndSave(users);
        }

        private void Vm_OnAddDynamicUsers(List<string> users)
        {
            //Binding logic
            vm.NoOfUsers = users.Count;

            for (int i= 6;i <= users.Count-1;i++)
            {
                RowDefinition gridRow1 = new RowDefinition();
                gridRow1.Height = new GridLength(40);
                grdUser.RowDefinitions.Add(gridRow1);

                //Label Placement
                Label lblBoxUser = new Label();
                lblBoxUser.Name = "lblUser" + (grdUser.RowDefinitions.Count - 1).ToString();
                lblBoxUser.Style = (Style)Application.Current.Resources["AuthenticationLabel"];
                Thickness margin = lblBoxUser.Margin;
                margin.Left = 31;
                lblBoxUser.Margin = margin;
                lblBoxUser.FontSize = 11;
                Grid.SetRow(lblBoxUser, grdUser.RowDefinitions.Count - 1);
                Grid.SetColumn(lblBoxUser, 0);
                lblBoxUser.Content = "User " + (grdUser.RowDefinitions.Count - 1).ToString();
                grdUser.Children.Add(lblBoxUser);

                //Textbox Placement
                TextBox txtBoxUser = new TextBox();
                txtBoxUser.Style = (Style)Application.Current.Resources["TextBoxSubHeaderStyle"];
                txtBoxUser.Width = 250;
                txtBoxUser.Height = 30;
                txtBoxUser.FontSize = 11;
                txtBoxUser.Name = "txtUser" + (grdUser.RowDefinitions.Count - 1).ToString();
                txtBoxUser.Text = users[i];
                Grid.SetRow(txtBoxUser, grdUser.RowDefinitions.Count - 1);
                Grid.SetColumn(txtBoxUser, 1);
                grdUser.Children.Add(txtBoxUser);
                Grid.SetColumnSpan(txtBoxUser, 3);
            }
            CreateRemoveButton();
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


        private void SetMaxLength()
        {
            txtUser1.MaxLength = 8;
            txtUser2.MaxLength = 8;
            txtUser3.MaxLength = 8;
            txtUser4.MaxLength = 8;
            txtUser5.MaxLength = 8;
            txtUser6.MaxLength = 8;

        }
        
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            vm.NoOfUsers++;
            RowDefinition gridRow1 = new RowDefinition();
            gridRow1.Height = new GridLength(40);
            grdUser.RowDefinitions.Add(gridRow1);

            //Label Placement
            Label lblBoxUser = new Label();
            lblBoxUser.Name = "lblUser" + (grdUser.RowDefinitions.Count - 1).ToString();
            lblBoxUser.Style = (Style)Application.Current.Resources["AuthenticationLabel"];
            Thickness margin = lblBoxUser.Margin;
            margin.Left = 31;
            lblBoxUser.Margin = margin;
            lblBoxUser.FontSize = 11;
            Grid.SetRow(lblBoxUser, grdUser.RowDefinitions.Count - 1);
            Grid.SetColumn(lblBoxUser, 0);
            lblBoxUser.Content = "User " + (grdUser.RowDefinitions.Count - 1).ToString();
            grdUser.Children.Add(lblBoxUser);


            //Textbox Placement
            TextBox txtBoxUser = new TextBox();
            txtBoxUser.MaxLength = 20;
            txtBoxUser.Name = "txtUser" + (grdUser.RowDefinitions.Count - 1).ToString();
            txtBoxUser.Style = (Style)Application.Current.Resources["TextBoxSubHeaderStyle"];
            txtBoxUser.Width = 250;
            txtBoxUser.Height = 30;
            txtBoxUser.FontSize = 11;
            txtBoxUser.Name = "txtUser" + (grdUser.RowDefinitions.Count - 1).ToString();
            Grid.SetRow(txtBoxUser, grdUser.RowDefinitions.Count - 1);
            Grid.SetColumn(txtBoxUser, 1);
            grdUser.Children.Add(txtBoxUser);
            Grid.SetColumnSpan(txtBoxUser, 3);

            //Textbox Binding
            Binding myBinding = new Binding();
            myBinding.Source = vm;
            myBinding.Path = new PropertyPath(lblBoxUser.Content.ToString());
            myBinding.Mode = BindingMode.TwoWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            myBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(txtBoxUser, TextBox.TextProperty, myBinding);

            var yourButton = grdUser.Children.OfType<Button>().FirstOrDefault(x => x.Name == "btnRemoveUser");
            if (yourButton == null)
            {
                CreateRemoveButton();
            }
            else
            {
                grdUser.Children.Remove(btnRemoveUser);
                CreateRemoveButton();

            }
            btnRemoveUser.Visibility = Visibility.Visible;
        }

        private void CreateRemoveButton()
        {

            BitmapImage btm = new BitmapImage(new Uri("Images/Cancel_64x.png", UriKind.Relative));
            Image img = new Image();
            img.Source = btm;
            img.Stretch = Stretch.Fill;
            btnRemoveUser.Content = img;
            btnRemoveUser.Height = 20;
            btnRemoveUser.Width = 20;
            
            // path.Width = 20; path.Height = 20;
            btnRemoveUser.Name = "btnRemoveUser";
            //btnRemoveUser.Content = path;
            btnRemoveUser.Style = (Style)this.Resources["MyButton"];
            //btnRemoveUser.Width = 20; btnRemoveUser.Height = 20;
            btnRemoveUser.Foreground = new SolidColorBrush(Colors.Red);
            btnRemoveUser.Background = new SolidColorBrush(Colors.Transparent);
            btnRemoveUser.HorizontalAlignment = HorizontalAlignment.Left;
            //btnRemoveUser.VerticalContentAlignment = VerticalAlignment.Top;
            btnRemoveUser.BorderBrush = new SolidColorBrush(Colors.Transparent);
            btnRemoveUser.ToolTip = "Remove this User ID";
            Grid.SetRow(btnRemoveUser, grdUser.RowDefinitions.Count - 1);
            Grid.SetColumn(btnRemoveUser, 3);
           
            grdUser.Children.Add(btnRemoveUser);
        }

        private void BtnRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            vm.NoOfUsers--;
            if (Grid.GetRow(btnRemoveUser) > 7)
            { }
            else
                btnRemoveUser.Visibility = Visibility.Collapsed;

            RowDefinition myRow = grdUser.RowDefinitions[Grid.GetRow(btnRemoveUser)];
            //grdUser.Children.RemoveAt(Grid.GetRow(btnRemoveUser));

            grdUser.Children.Remove(FindChild<TextBox>(grdUser, "txtUser" + Grid.GetRow(btnRemoveUser)));
            grdUser.Children.Remove(FindChild<Label>(grdUser, "lblUser" + Grid.GetRow(btnRemoveUser)));

            grdUser.RowDefinitions.Remove(myRow);

            Grid.SetRow(btnRemoveUser, grdUser.RowDefinitions.Count - 1);
            Grid.SetColumn(btnRemoveUser, 3);
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        //F1 M 26.9166,22.1667L 37.9999,33.25L 49.0832,22.1668L 53.8332,26.9168L 42.7499,38L 53.8332,49.0834L 49.0833,53.8334L 37.9999,42.75L 26.9166,53.8334L 22.1666,49.0833L 33.25,38L 22.1667,26.9167L 26.9166,22.1667 Z 
        //<Path Stroke = "Red" StrokeThickness="2">
        //                <Path.Data>
        //                    <PathGeometry>
        //                        <PathGeometry.Figures>
        //                            <PathFigure StartPoint = "0,0" >
        //                                < LineSegment Point="25,25"/>
        //                            </PathFigure>
        //                            <PathFigure StartPoint = "0,25" >
        //                                < LineSegment Point="25,0"/>
        //                            </PathFigure>
        //                        </PathGeometry.Figures>
        //                    </PathGeometry>
        //                </Path.Data>
        //            </Path>
    }
}