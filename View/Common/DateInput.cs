using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace FAIRSCMFEditor.View.Common
{
    public class DateInput : TextBox
    {
        public DateInput()
        {
            this.Loaded += DateInput_Loaded;
        }

        private void DateInput_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            string len = this.Tag.ToString();
            this.MaxLength = len.Length;
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

            if (this.Text.Length == 4)
                this.Text = this.Text + "-";
            if (this.Text.Length == 7)
                this.Text = this.Text + "-";
            this.SelectionStart = this.Text.Length;
            this.SelectionLength = 0;
        }

         
    }
}
