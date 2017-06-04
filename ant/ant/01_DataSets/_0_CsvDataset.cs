/**********
[ANT] A machine learning plugin for Rhino\Grasshopper started by Mahmoud Abdelrahman [Mahmoud Ouf] under BSD License

Copyright (c) 2017, Mahmoud AbdelRahman <arch.mahmoud.ouf111@gmail.com>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 ************/

using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace ANT
{
    public class _0_CsvDataset : GH_Component
    {

        public static int nm = 0;
        bool fileUpdated = false;

        string workingDir = "C:\\ant\\_02_datasets\\csv\\";
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();

        //System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        FileSystemWatcher watcher = new FileSystemWatcher();

        IGH_DataAccess TempDa;
        
        


        /// <summary>
        /// Initializes a new instance of the _0_CsvDataset class.
        /// </summary>
        public _0_CsvDataset()
            : base("_0_CsvDataset", "CSV-ds",
                ".CSV data set file. ",
                "ANT", "1|Data Set")
        {
            workingDir += DateTime.Now.ToString("yyyyMMdd_Hmm") + "\\";
            if(!Directory.Exists(@workingDir))
            {
                nm = 0;
                Directory.CreateDirectory(@workingDir + "_" + nm.ToString());
                workingDir += "_" + nm.ToString() + "\\";
            }
            else
            {
                Directory.CreateDirectory(@workingDir + "_" + nm.ToString());
                workingDir += "_" + nm.ToString() + "\\";
                nm += 1;
            }
            this.Message = ".csv Data set";
            watcher.Changed += watcher_Changed;
            process2.Exited += process_Exited;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("CSV file", "csv file ", "csv file path", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log_", "Log_", "CSV file contents", GH_ParamAccess.item);
            pManager.AddTextParameter("Data_", "Data", "CSV file Data", GH_ParamAccess.item);
            pManager.AddTextParameter("Targets_", "targets", "CSV file targets", GH_ParamAccess.item);
            pManager.AddTextParameter("Features_", "features", "CSV file features", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string newStr = "";
            string workingDir2 = workingDir.Replace(@"\", @"\\");

            string filename = "";
            if (!DA.GetData(0, ref filename)) { return; }
            if (filename == "") { return; }
            if (!fileUpdated)
            {

                process2.StartInfo.FileName = "doAllWork.py";
                process2.StartInfo.WorkingDirectory = @workingDir;
                process2.EnableRaisingEvents = true;
                process2.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;


                /*openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "CSV File (*.csv)|*.csv";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;*/
                int n = 0;

                int f;
                string targets = "\n---------------------\n    TARGETS: \n        ";
                string DataLabel = "";
                string DataString = "[\n";
                string FeatureLabel = "";
                string FeaturesString = "";
                string data = "[";
                string target = "[";
                string features = "[";


                TempDa = DA;
                watcher.Path = @Path.GetDirectoryName(filename);
                watcher.Filter = "*.csv";
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.Attributes;
                /*watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);*/



                watcher.EnableRaisingEvents = true;
                using (TextFieldParser parser = new TextFieldParser(@filename))
                {

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        int h = 1;
                        //Processing row
                        string[] fields = parser.ReadFields();
                        f = fields.Length;
                        if (n == 0 & h < f)
                        {

                            FeatureLabel += "--------------------\n    FEATURES : " + (f - 1) + "\n";
                        }
                        foreach (string field in fields)
                        {
                            if (n == 0 & h < f)
                            {
                                if (h == 1)
                                { FeaturesString += "        [\"" + field + "\"" + ", "; }
                                else if (h < f - 1)
                                {
                                    FeaturesString += "\"" + field + "\"" + ", ";
                                    features += "\"" + field + "\", ";
                                }
                                else
                                {
                                    FeaturesString += "\"" + field + "\"" + "],";
                                    features += "\"" + field + "\"" + "]";
                                    DataLabel += "\n--------------------\n    DATA :\n";
                                }
                            }
                            if (n > 0 & h < f)
                            {
                                if (h == 1)
                                {
                                    DataString += String.Format("    {0, 4} - ", n - 1) + "[" + field + ", ";
                                    data += "[" + field + ", ";
                                }
                                else if (h == f - 1)
                                {
                                    DataString += field + "]\n";
                                    if (parser.LineNumber == -1)
                                        data += field + "]";
                                    else
                                        data += field + "], ";
                                }
                                else
                                {
                                    DataString += field + ", ";
                                    data += field + ", ";
                                }

                            }
                            if (n > 0 & h == f)
                            {
                                targets += "[" + field + "] ";
                                if (parser.LineNumber == -1)
                                    target += "[" + field + "]";
                                else
                                    target += "[" + field + "], ";
                            }

                            h++;
                        }
                        n++;
                    }
                }
                data += "]";
                target += "]";
                DataString += "]";
                newStr += FeatureLabel + FeaturesString + DataLabel + DataString;
                newStr += targets;
                PythonFile(data, target, features, workingDir2);

                process2.Start();
                DA.SetData(1, @workingDir+"data.txt");
                DA.SetData(2, @workingDir+"targets.txt");
                DA.SetData(0, newStr);
            }
        }


        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.ExpireSolution(true);
        }
        private void process_Exited(object sender, EventArgs e)
        {
            //processhasexit = true;
        }


        string PythonFile(string data, string target, string features, string workingDir2)
        {
            string thisFile = Resources.allResources.csvDataset;
            thisFile = thisFile.Replace("##directory##", workingDir2);
            thisFile = thisFile.Replace("##data##", data);
            thisFile = thisFile.Replace("##target##", target);
            thisFile = thisFile.Replace("##features##", features);
            System.IO.File.WriteAllText(@workingDir+"doAllWork.py", thisFile);

            return @workingDir + "doAllWork.py";
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return ANT.Resources.allResources.csv_icon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c3c61c28-e9e8-455f-9df5-7a4e9a218ac2}"); }
        }
    }
}