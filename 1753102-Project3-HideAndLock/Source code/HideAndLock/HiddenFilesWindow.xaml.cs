using Microsoft.Win32;
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
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace HideAndLock
{
    /// <summary>
    /// Interaction logic for HiddenFilesWindow.xaml
    /// </summary>
    public partial class HiddenFilesWindow : Window
    {
        public HiddenFilesWindow()
        {
            InitializeComponent();

        }

        public string HiddenFilesType { get; set; }
        public MyDescription Description { get; set; }

        public ImageName ImageList { get; set; } = MainWindow.ImageList;
        private void HiddenFileWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = Description;
            ButtonPanel.DataContext = ImageList;
            FileListView.ItemsSource = Description.FileList;
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == true)
                {
                    // traverse all files
                    int count = 0;
                    foreach (string filename in dialog.FileNames)
                    {
                        try
                        {
                            // add to list
                            HALBus.AddFileToList(Description, filename);
                            count++;
                            
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    MessageBox.Show($"Added {count} file(s)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

  
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileListView.SelectedItems.Count > 0)
            {
                foreach (var item in FileListView.SelectedItems)
                {
                    // internal:
                    // get file description
                    FileDescription file = item as FileDescription;

                    try
                    {
                        HALBus.OpenFile(file);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void UnhideFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (FileListView.SelectedItems.Count > 0)
            {
                List<FileDescription> removedFiles = new List<FileDescription>();

                foreach (var item in FileListView.SelectedItems)
                {
                    // internal:
                    // get file description
                    FileDescription file = item as FileDescription;

                    // unhide
                    HALBus.UnhideFile(file);

                    // add to removed list
                    removedFiles.Add(file);
                }

                foreach (var file in removedFiles)
                {
                    // display: update listview
                    Description.FileList.Remove(file);
                    
                }
                Description.CountFiles.Value = Description.FileList.Count;

                // show result
                MessageBox.Show($"Unhided {removedFiles.Count} file(s)");
            }
        }

        private void FileListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // internal:
            // get file description
            ListViewItem item = sender as ListViewItem;
            FileDescription file = FileListView.ItemContainerGenerator.ItemFromContainer(item) as FileDescription;

            try
            {
                HALBus.OpenFile(file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
