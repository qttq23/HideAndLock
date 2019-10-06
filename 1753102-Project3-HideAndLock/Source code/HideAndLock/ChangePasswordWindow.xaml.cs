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
using System.Windows.Shapes;

namespace HideAndLock
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        public string Description { get; set; }
        public string NewPassword { get; set; }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            NewPassword = PasswordBox.Password;

            this.DialogResult = true;
            this.Close();
        }

        private void ChangePasswordWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = Description + " - Change password";
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OKButton_Click(sender, null);
            }
        }
    }
}
