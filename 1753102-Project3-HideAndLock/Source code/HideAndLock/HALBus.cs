using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace HideAndLock
{
    class HALBus
    {

        public static void EncryptFile(string filename)
        {
            // Encrypt

            // read all bytes of a file

            byte[] bytes = File.ReadAllBytes(filename);
            //Console.WriteLine($"Encrypt, Filename: {filename}");
            //Console.WriteLine($"bytes read: {bytes.Length}");

            // convert name of file to bytes for later retreiving
            // add to the begin of file to make file unreadable
            byte[] extra = Encoding.ASCII.GetBytes("1753102\n");
            byte[] newBytes = extra.Concat(bytes).ToArray();

            // save to another folder
            //string newFilename = AppDomain.CurrentDomain.BaseDirectory + @"\Images\dencrypt\google.dat";
            File.WriteAllBytes(filename, newBytes);
            //Console.WriteLine($"Encrypt successfully! {newFilename}");
        }

        public static void DecryptFile(string filename)
        {
            // Decrypt

            // read all bytes of previous file
            byte[] byteOrigin = null;
            byte[] bytes = File.ReadAllBytes(filename);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 10)     // new line
                {
                    // retrieve original bytes of file
                    int length = bytes.Length - (i + 1);
                    byteOrigin = new byte[length];
                    Array.Copy(bytes, i + 1, byteOrigin, 0, length);
                    break;
                }
            }
            //Console.WriteLine($"Decpypt, new Filename: {newFilename}");
            //Console.WriteLine($"Decpypt, bytes extracted: {newBytes.Length}");

            // save
            //filename = Encoding.ASCII.GetString(byteName);
            //Console.WriteLine($"'{stringName}' {stringName.Length}");

            //newFilename = AppDomain.CurrentDomain.BaseDirectory + @"Images\dencrypt\" + filename;
            File.WriteAllBytes(filename, byteOrigin);
            //Console.WriteLine($"Decrypt successfully! Please check folder 'dencrypt'");
        }

        public static string Encode(string input, int password = 1)
        {
            string result = "";

            foreach (char c in input)
            {
                char newChar = (char)((int)c + password);
                result += newChar;
            }
            return result;
        }

        public static string Decode(string input, int password = 1)
        {
            string result = "";

            foreach (char c in input)
            {
                char newChar = (char)((int)c - password);
                result += newChar;
            }
            return result;
        }

        public static void MoveFile(string source, string des)
        {
            File.Move(source, des);
        }

        public static void CopyFile(string source, string des)
        {
            File.Copy(source, des);
        }

        // encrypt and move to another folder, return full new filename
        public static string EncryptAndMove(string filename)
        {
            EncryptFile(filename);
            string newFilename = AppDomain.CurrentDomain.BaseDirectory + "HiddenFiles\\" + Guid.NewGuid().ToString();
            MoveFile(filename, newFilename);

            return newFilename;

        }



        // decrypt and move to output folder, return full new filename
        public static string DecryptAndMove(string filename, string output)
        {
            DecryptFile(filename);
            MoveFile(filename, output);
            return output;
        }


        // get thumbnail image from a file: image, video
        // return a bitmap
        public static BitmapImage GetThumbnail(string filename)
        {
            ShellFile shellFile = ShellFile.FromFilePath(filename);
            Bitmap bitmap = shellFile.Thumbnail.ExtraLargeBitmap;   // used for extract thumbnail from file


            BitmapImage bitmapImage = null;
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }


            return bitmapImage;
        }


        // get icon image from a file: music, exe, other files
        // return a bitmap
        public static BitmapImage GetIcon(string filename)
        {
            Icon icon = Icon.ExtractAssociatedIcon(filename);
            Bitmap bitmap = icon.ToBitmap(); // use for extract icon from file.exe


            BitmapImage bitmapImage = null;
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }


            return bitmapImage;
        }

        public static bool CheckFileImage(string filename)
        {
            bool result = false;
            var list = new List<string>();
            list.Add(".jpg");
            list.Add(".png");
            list.Add(".bmp");
            list.Add(".gif");
            list.Add(".jpeg");
            list.Add(".tiff");

            foreach (var extenstion in list)
            {
                if (Path.GetExtension(filename).ToLower() == extenstion.ToLower())
                {

                    result = true;
                    break;
                }
            }
            return result;
        }
        public static bool CheckFileVideo(string filename)
        {
            bool result = false;
            var list = new List<string>()
            {
                ".mp4", ".m4a", ".m4v", ".mov",
                ".3gp", ".3gp2", ".3g2", ".3gpp", ".3gpp2",
                ".wmv", ".wma",
                ".webm", ".flv",".avi"

            };

            foreach (var extenstion in list)
            {
                if (Path.GetExtension(filename).ToLower() == extenstion.ToLower())
                {

                    result = true;
                    break;
                }
            }
            return result;
        }



        public static void UpdateListFiles(BindingList<MyDescription> list)
        {
            HALDao.LoadFromDatabase(list);
        }

        public static void UpdateListApps(BindingList<MyDescription> list)
        {
            HALDao.LoadFromDatabase(list);
        }

        public static void SaveListFiles(BindingList<MyDescription> list, bool isAppend)
        {
            HALDao.SaveToDatabase(list, isAppend);
        }

        public static void SaveListApps(BindingList<MyDescription> list, bool isAppend)
        {
            HALDao.SaveToDatabase(list, isAppend);
        }

        public static void AddFileToList(MyDescription description, string filename)
        {
   

            // internal: encrypt and move
            string newFilename = HALBus.EncryptAndMove(filename);


            // create file description
            FileDescription fd = new FileDescription()
            {
                FullName = filename,
                NewFullName = newFilename,
                OutputFolder = Path.GetDirectoryName(filename),

            };
            fd.UpdateProperties();

 
            // add to list
            description.FileList.Add(fd);
            description.CountFiles.Value = description.FileList.Count;
    
        }

        public static void AddAppToList(MyDescription description, string filename)
        {

            // internal:

            // create file description
            AppDescription fd = new AppDescription()
            {
                FullName = filename,
                NewFullName = filename,
                OutputFolder = Path.GetDirectoryName(filename),

            };
            fd.UpdateProperties();

            // add to list
            description.AppList.Add(fd);
            description.CountApps.Value = description.AppList.Count;
        }

        public static void OpenFile(FileDescription file)
        {
            // copy to temp folder, decrypt
            file.TempName = AppDomain.CurrentDomain.BaseDirectory + "HiddenFiles\\Temp\\" + Guid.NewGuid().ToString();
            HALBus.CopyFile(file.NewFullName, file.TempName);
            string outputName = HALBus.DecryptAndMove(
                file.TempName, file.TempName + Path.GetExtension(file.FullName));

            // open by calling window default player
            Process.Start(outputName);
        }

        public static void UnhideFile(FileDescription file)
        {
            // filename = guid
            // output = output folder + name
            string filename = file.NewFullName;
            string output = file.OutputFolder + "\\" + file.Name;

            // call decrypt and move
            string outputFilename = HALBus.DecryptAndMove(filename, output);
    
        }
    }
}
