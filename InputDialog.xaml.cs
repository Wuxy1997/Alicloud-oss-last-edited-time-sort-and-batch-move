using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Input dialog window
    /// </summary>
    public partial class InputDialog : Window
    {
        public string Title { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

        public InputDialog(string title, string question, string defaultAnswer = "")
        {
            InitializeComponent();
            Title = title;
            Question = question;
            Answer = defaultAnswer;
            DataContext = this;
            txtAnswer.Text = defaultAnswer;
            txtAnswer.Focus();
            txtAnswer.SelectAll();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Answer = txtAnswer.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
