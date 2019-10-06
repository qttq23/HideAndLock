using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace HideAndLock
{
    // integer with INotifyPropertyChanged
    public class MyInt : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseEventHandler(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                RaiseEventHandler("Value");
            }
        }

        private int value;
    }

    // string with INotifyPropertyChanged
    public class MyString : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseEventHandler(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                RaiseEventHandler("Value");
            }
        }

        private string value;
    }

    public class MyDescription
    {
        public MyString Description { get; set; }

        public string ColorName { get; set; }

        public string ImageName { get; set; }

        public string Tag { get; set; }

        public BindingList<FileDescription> FileList { get; set; } = new BindingList<FileDescription>();

        public MyInt CountFiles { get; set; } = new MyInt() { Value = 0 };


        public BindingList<AppDescription> AppList { get; set; } = new BindingList<AppDescription>();

        public MyInt CountApps { get; set; } = new MyInt() { Value = 0 };

    }

    public class FileDescription
    {
        // importants properties
        public string FullName { get; set; }

        public string NewFullName { get; set; }

        public string OutputFolder { get; set; }


        // belows can be refered from aboves
        public string Name { get; set; }

        public BitmapImage Thumbnail { get; set; }

        public string TempName { get; set; }


        // method
        virtual public void UpdateProperties()
        {
            // name
            Name = Path.GetFileName(FullName);

            // thumbnail
            // copy to temp folder, decrypt
            TempName = AppDomain.CurrentDomain.BaseDirectory + "HiddenFiles\\Temp\\" + Guid.NewGuid().ToString();
            HALBus.CopyFile(NewFullName, TempName);
            string outputName = HALBus.DecryptAndMove(
                TempName, TempName + Path.GetExtension(FullName));

            if (HALBus.CheckFileImage(outputName) == true || HALBus.CheckFileVideo(outputName) == true)
            {
                //Thumbnail = HALBus.GetThumbnail(outputName);  // some error occured, fix later
                Thumbnail = HALBus.GetIcon(outputName);
            }
            else
            {
                Thumbnail = HALBus.GetIcon(outputName);

            }
        }
    }

    public class AppDescription : FileDescription
    {
        public string Password { get; set; } = "0";
        override public void UpdateProperties()
        {
            // name
            Name = Path.GetFileName(FullName);
            Name = Name.Replace(".exe", "");

            // thumbnail
            string outputName = FullName;

            if (HALBus.CheckFileImage(outputName) == true || HALBus.CheckFileVideo(outputName) == true)
            {
                Thumbnail = HALBus.GetThumbnail(outputName);
            }
            else
            {
                Thumbnail = HALBus.GetIcon(outputName);

            }
        }
    }

    public class LockProcess
    {
        public Process Process { get; set; }

        public string Password { get; set; }
    }
}
