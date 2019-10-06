using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HideAndLock
{
    /// <summary>
    /// Interaction logic for PasswordWindow.xaml
    /// </summary>
    public partial class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        public string Password { get; set; } = "";
        public string AppName { get; set; } = "";
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ResultTextBlock.Text = "";
            AllowUIToUpdate();
            Thread.Sleep(300);

            string input = PasswordBox.Password;
            if (input == Password)
            {
                
                ResultTextBlock.Text = "Success!";
                AllowUIToUpdate();
                Thread.Sleep(500);
                
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                ResultTextBlock.Text = "Wrong password. Please try again!";
            }
        }

        private void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
            //EDIT:
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                          new Action(delegate { }));
        }

        private void PasswordWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = AppName + " - Password";
            PasswordBox.Focus();

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
