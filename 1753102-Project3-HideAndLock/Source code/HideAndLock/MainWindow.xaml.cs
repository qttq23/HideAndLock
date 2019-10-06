using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace HideAndLock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        static public BindingList<MyDescription> FileDescriptionList;
        static public BindingList<MyDescription> AppDescriptionList;
        static public ImageName ImageList { get; set; } = new ImageName();
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            

            // set application icon
            //AppIconImage.Source = new BitmapImage(new Uri(ImageList.GalleryIcon[0]));
            this.DataContext = ImageList;

            FileDescriptionList = new BindingList<MyDescription>
            {
                new MyDescription(){Description = new MyString(){ Value="Image"}, ColorName="LightBlue",
                    ImageName = ImageList.ImageIcon[0],
                    Tag = "Image",
                },

                new MyDescription(){Description = new MyString(){ Value="Audio"}, ColorName="LightBlue"/*"LightPink"*/,
                    ImageName = ImageList.AudioIcon[0],
                    Tag = "Audio",
                },

                new MyDescription(){Description = new MyString(){ Value="Video"}, ColorName="LightBlue"/*"LightSeaGreen"*/,
                    ImageName =ImageList.VideoIcon[0],
                    Tag = "Video",
                },

                new MyDescription(){Description = new MyString(){ Value="Other files"}, ColorName="LightBlue"/*"CadetBlue"*/,
                    ImageName =ImageList.MultifileIcon[0],
                    Tag = "Others",
                },
            };

            AppDescriptionList = new BindingList<MyDescription>
            {
                new MyDescription(){Description = new MyString(){ Value="Application"}, ColorName="LightBlue",
                    ImageName =ImageList.ExeIcon[0],
                    Tag = "App",
                },

                new MyDescription(){Description = new MyString(){ Value="HideAndLock"}, ColorName="LightBlue",
                    ImageName =ImageList.GalleryIcon[0],
                    Tag = "HideAndLock",
                    
                },
            };

            var app = new AppDescription()
            {
                FullName = AppDomain.CurrentDomain.BaseDirectory + "\\HideAndLock.exe",
                NewFullName = AppDomain.CurrentDomain.BaseDirectory + "\\HideAndLock.exe",
                OutputFolder = AppDomain.CurrentDomain.BaseDirectory,
                Password = "0",
                Name = "HideAndLock",
            };

            AppDescriptionList[1].AppList.Add(app);


            try
            {
                // update list files, list apps from database
                HALBus.UpdateListFiles(FileDescriptionList);
                HALBus.UpdateListApps(AppDescriptionList);
                
   
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // attempt to ask password
            // child window
            var child = new PasswordWindow();
            child.Password = AppDescriptionList[1].AppList[AppDescriptionList[1].AppList.Count - 1].Password;
            child.AppName = AppDescriptionList[1].AppList[AppDescriptionList[1].AppList.Count - 1].Name;

            if (child.ShowDialog() != true)
            {
                this.Close();

            }


            // binding
            HideListView.ItemsSource = FileDescriptionList;
            LockListView.ItemsSource = AppDescriptionList[0].AppList;

            //create thread for locking apps
            appLockThread = new Thread(CheckAppLockProcess);
            appLockThread.Start();



            
        }


        // thread
        Thread appLockThread;
        bool isRunLock = true;
        List<Process> keepTrackingList = new List<Process>();
        List<Thread> keepTrackingThreadList = new List<Thread>();
        private void CheckAppLockProcess()
        {
            //MessageBox.Show("CheckAppLockProcess");

            while (isRunLock)
            {
                // traverse all processes exists
                Process[] processlist = Process.GetProcesses();

                foreach (Process process in processlist)
                {
                    // traverse all app in lock list
                    foreach (var app in AppDescriptionList[0].AppList)
                    {
                        if (process.ProcessName == app.Name)
                        {
                            bool isLock = true;
                            foreach (var kepttrack in keepTrackingList)
                            {
                                if (process.ProcessName == kepttrack.ProcessName)
                                {
                                    // this means the suspect process is already kept track of
                                    // no need to lock again
                                    isLock = false;
                                    break;
                                }
                            }

                            if (isLock)
                            {
                                //MessageBox.Show("isLock");

                                var target = process;

                                // add to keep track of list
                                keepTrackingList.Add(target);
                                target.EnableRaisingEvents = true;
                                target.Exited += new EventHandler(ProcessExited);

                                // put to thread lock
                                LockProcess lockProcess = new LockProcess() {
                                    Process = target,
                                    Password = app.Password,
                                };

                                Thread thread = new Thread(AppLockProcess);
                                thread.SetApartmentState(ApartmentState.STA);
                                thread.Start(lockProcess);
                                keepTrackingThreadList.Add(thread);


                            }

                            break;
                        }
                    }
                }

                
    
                    

            }
        }

        private void AppLockProcess(Object lockProcess)
        {
            //MessageBox.Show("AppLockProcess");

            Process target = (lockProcess as LockProcess).Process;
            string password = (lockProcess as LockProcess).Password;

            // lock by: suspend and acquire password


            // suspend app
            target.Suspend();

            // attempt to ask password
            // child window
            var child = new PasswordWindow();
            child.Password = password;
            child.AppName = target.ProcessName;

            if (child.ShowDialog() == true)
            {
                // right password
                // resume app
                target.Resume();

            }
            else
            {
                // false password
                // terminate app
                //process.Terminate();
                target.Kill();
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            
            Process process = sender as Process;
            keepTrackingList.Remove(process);

            MessageBox.Show(process.ProcessName);
        }

       


        private void HideListViewItemButton_Click(object sender, RoutedEventArgs e)
        {
            
            Button button = sender as Button;
            foreach (var item in FileDescriptionList)
            {
                if ((string)button.Tag == item.Tag)
                {
                    HiddenFilesWindow child = new HiddenFilesWindow();
                    child.Description = item;

                    child.ShowDialog();
                }
                
            }

        }

        

        private void AddGeneralButton_MouseEnter(object sender, MouseEventArgs e)
        {
            TabItem tab = MainTabControl.SelectedItem as TabItem;
            StackPanel panel = tab.Header as StackPanel;
            Label label = panel.Children[1] as Label;

            if ((string)label.Content == "Hide")
            {
                AddMenuListView.ItemsSource = FileDescriptionList;
                AddMenuListView.Visibility = Visibility.Visible;
            }

            else if ((string)label.Content == "Lock")
            {
                BindingList<MyDescription> list = new BindingList<MyDescription>();
                list.Add(AppDescriptionList[0]);
                AddMenuListView.ItemsSource = list;
                AddMenuListView.Visibility = Visibility.Visible;
            }

        }

        private void AddMenuPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            AddMenuListView.Visibility = Visibility.Hidden;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // save to database
            HALBus.SaveListFiles(FileDescriptionList, false); // override
            HALBus.SaveListApps(AppDescriptionList, true); // append

            // exit thread
            isRunLock = false;

            // clear temp files
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\HiddenFiles\\Temp");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            // determine which kind of file
            MyDescription description = null;
            bool isApp = false;
            string filter = "";
            foreach (var item in AppDescriptionList)
            {
                if (item.Tag == ((Button)sender).Tag as string)
                {
                    isApp = true;
                    description = item;
                    filter = "Application files (*.exe)|*.exe|All files (*.*)|*.*";
                    break;
                }
            }
            foreach (var item in FileDescriptionList)
            {
                if (item.Tag == ((Button)sender).Tag as string)
                {
                    description = item;
                    break;
                }
            }

            // dialog to choose file
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = filter;

            if (dialog.ShowDialog() == true)
            {
                // traverse all files
                int count = 0;
                foreach (string filename in dialog.FileNames)
                {
                    if (!isApp)
                    {
                        // add file
                        try{
                            // add to list
                            HALBus.AddFileToList(description, filename);
                            count++;
                            
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    else if (Path.GetExtension(filename) == ".exe")
                    {
                        try{
                            // add application
                            HALBus.AddAppToList(description, filename);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                MessageBox.Show($"Added {count} file(s)");
            }
        }

        private void ChangePasswordMenu_Click(object sender, RoutedEventArgs e)
        {
            // determine app
            var app = LockListView.SelectedItem as AppDescription;
            if (app != null)
            {
                // show dialog to get new password
                var child = new ChangePasswordWindow();
                child.Description = app.Name;
                if (child.ShowDialog() == true)
                {
                    app.Password = child.NewPassword;

                    MessageBox.Show("Change password successfully!");
                }
            }
        }

        private void RemoveMenu_Click(object sender, RoutedEventArgs e)
        {
            var app = LockListView.SelectedItem as AppDescription;
            if (app != null)
            {
                AppDescriptionList[0].AppList.Remove(app);
            }
        }

        private void TitleBarGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized) // In maximum window state case, window will return normal state and continue moving follow cursor
                {
                    this.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.Top = 3;// 3 or any where you want to set window location affter return from maximum state
                }
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        int countMaximize = 0;
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            countMaximize++;
            if (countMaximize >= 2)
                countMaximize = 0;

            if (countMaximize == 1)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void OnTabSelected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                {

                    TabItem tab = sender as TabItem;
                    StackPanel panel = tab.Header as StackPanel;
                    Label label = panel.Children[1] as Label;

                    if ((string)label.Content == "Options")
                    {
                        AddGeneralButton.Visibility = Visibility.Hidden;
                    }

                    else
                    {
                        AddGeneralButton.Visibility = Visibility.Visible;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OptionChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = OptionChangePasswordBox.Password;
            AppDescriptionList[1].AppList[AppDescriptionList[1].AppList.Count - 1].Password = newPassword;
            OptionChangePasswordResultTextBlock.Text = "Changed";
        }
    }
}
