using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace FAIRSCMFEditor.View.Common
{
    public class TimeInput : TextBox
    {

        public TimeInput()
        {
            this.Loaded += TimeInput_Loaded;
        }

        private void TimeInput_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            string len = this.Tag.ToString();
            this.MaxLength = len.Length;
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

            if (this.Text.Length == 2)
                this.Text = this.Text + ":";

            if (this.Text.Length == 5)
                this.Text = this.Text + ":";

            this.SelectionStart = this.Text.Length;
            this.SelectionLength = 0;
        }
    }
}