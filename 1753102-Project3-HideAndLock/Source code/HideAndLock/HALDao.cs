using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideAndLock
{
    class HALDao
    {
        public static string dbName = AppDomain.CurrentDomain.BaseDirectory + "\\Database.txt";
        public static void LoadFromDatabase(BindingList<MyDescription> list)
        {
            StreamReader file = null;
            try
            {
                // open file database
                string line = "";
                file = new StreamReader(dbName);

                // read all lines
                while ((line = file.ReadLine()) != null)
                {
                    // Decode
                    line = HALBus.Decode(line);

                    // split
                    string[] tokens = line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in list)
                    {
                        // check tag
                        if (item.Tag == tokens[3])
                        {
                            if (item.Tag != "App" && item.Tag != "HideAndLock")
                            {
                                // file not app
                                FileDescription fd = new FileDescription()
                                {
                                    FullName = tokens[0],
                                    NewFullName = tokens[1],
                                    OutputFolder = tokens[2],

                                };
                                fd.UpdateProperties();

                                // add to list files
                                item.FileList.Add(fd);
                            }
                            else
                            {
                                // app
                                AppDescription ad = new AppDescription()
                                {
                                    FullName = tokens[0],
                                    NewFullName = tokens[1],
                                    OutputFolder = tokens[2],
                                    Password = tokens[4],
                                };
                                ad.UpdateProperties();

                                // add to list files
                                item.AppList.Add(ad);
                            }
                            
                        }
                    }
                }

                foreach (var item in list)
                {
                    item.CountFiles.Value = item.FileList.Count;
                }
            }
            finally
            {
                // close file
                if (file != null)
                    file.Close();
            }
        }

        public static void SaveToDatabase(BindingList<MyDescription> list, bool isAppend)
        {
            using (StreamWriter file = new StreamWriter(dbName, isAppend))
            {
                if (!isAppend)
                {
                    // write header
                    string header = "FullName,NewFullName,OutputFolder,Tag";

                    // Encode
                    header = HALBus.Encode(header);
                    file.WriteLine(header);
                }

                // write body
                foreach (var item in list)
                {
                    // traverse all files
                    foreach (var tuple in item.FileList)
                    {
                        string line = tuple.FullName + "," 
                            + tuple.NewFullName + "," 
                            + tuple.OutputFolder + "," 
                            + item.Tag;

                        // Encode
                        line = HALBus.Encode(line);
                        file.WriteLine(line);
                    }

                    foreach (var tuple in item.AppList)
                    {
                        string line = tuple.FullName + ","
                            + tuple.NewFullName + ","
                            + tuple.OutputFolder + ","
                            + item.Tag + ","
                            + tuple.Password;

                        // Encode
                        line = HALBus.Encode(line);
                        file.WriteLine(line);
                    }
                }
                
            }
        }
    }
}
