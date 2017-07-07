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
using Microsoft.Office.Interop.Excel;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;


namespace ANT
{
    public class _0_ExcelDataSet : GH_Component
    {

        public static int nm = 0;
        string workingDir = "C:\\ant\\_02_datasets\\excel\\";

        Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
        Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
        Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
        Microsoft.Office.Interop.Excel.Range range;
        System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();


        /// <summary>
        /// Initializes a new instance of the _0_ExcelDataSet class.
        /// </summary>
        public _0_ExcelDataSet()
            : base("_0_ExcelDataSet", "Excel",
                "Microsoft Excel Data file",
                "ANT", "1|Data set")
        {
            this.Message = "Excel Data set component";


            workingDir += DateTime.Now.ToString("yyyyMMdd_Hmm") + "\\";
            if (!Directory.Exists(@workingDir))
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


        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("OpenFile?", "Open? ", "Openx Excel File ", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log_", "Log_", "Excel file contents", GH_ParamAccess.item);
            pManager.AddTextParameter("Data_", "Data", "Boston dataset Data", GH_ParamAccess.item);
            pManager.AddTextParameter("Targets_", "targets", "Boston datasets targets", GH_ParamAccess.item);
            pManager.AddTextParameter("Features_", "features", "Boston datasets features", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\init\\\\ExcelDataset\\\\";

            process2.StartInfo.FileName = "python.exe";
            process2.StartInfo.Arguments = "doAllWork.py";
            process2.StartInfo.WorkingDirectory = @workingDir;
            process2.EnableRaisingEvents = true;
            process2.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;


            string workingDir2 = workingDir.Replace("\\", "\\\\");
            bool openfile = false;
            if (!DA.GetData(0, ref openfile)) { return; }
            if (!openfile) { return; }

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Excel File (*.xlsx)|*.xlsx";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string data = "[";
                string target = "[";
                string features = "[";
                int rCnt;
                int cCnt;
                int rw = 0;
                int cl = 0;

                string thisfile = openFileDialog1.FileName;
                xlWorkBook = xlApp.Workbooks.Open(@thisfile, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
                xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                range = xlWorkSheet.UsedRange;
                rw = range.Rows.Count;
                cl = range.Columns.Count;

                string strText = "N. of Features : " + (cl - 1) + "\t N. of Samples: " + (rw - 1) + "\t \n    ------------------------------ \n--- FEATURES  : \n    ";
                //MessageBox.Show(rw.ToString() + " rows , " + cl.ToString() + " Cols");
                string newstr = "[";

                for (cCnt = 1; cCnt <= cl - 1; cCnt++)
                {
                    if (cCnt > 1)
                    {
                        newstr += ", ";
                        features += ", ";
                    }
                    newstr += "\"";
                    newstr += range.Cells[1, cCnt].Value2.ToString();
                    newstr += "\"";

                    features += "\"";
                    features += range.Cells[1, cCnt].Value2.ToString();
                    features += "\"";
                }
                features += "]";
                newstr += "]\n    ------------------------------ \n--- DATA  : \n    ";
                for (rCnt = 2; rCnt <= rw; rCnt++)
                {
                    if (rCnt > 2)
                        data += ",";
                    data += "[";
                    newstr += "\n        " + (rCnt - 1).ToString() + "- ";
                    newstr += "[";
                    for (cCnt = 1; cCnt <= cl - 1; cCnt++)
                    {
                        if (cCnt > 1)
                        {
                            data += ",";
                            newstr += ", ";
                        }
                        newstr += range.Cells[rCnt, cCnt].Value2.ToString();
                        data += range.Cells[rCnt, cCnt].Value2.ToString();
                    }
                    newstr += "]";
                    data += "]";
                }
                data += "]";

                newstr += "]\n------------------------------ \n--- TARGETS  : \n        ";
                for (rCnt = 2; rCnt <= rw; rCnt++)
                {
                    if (rCnt > 2)
                        target += ",";
                    newstr += "[";
                    newstr += range.Cells[rCnt, cl].Value2.ToString();
                    target += range.Cells[rCnt, cl].Value2.ToString();
                    newstr += "] ";
                }
                target += "]";
                PythonFile(@data, @target, @features, workingDir);
                xlWorkBook.Close(true, null, null);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlWorkSheet);
                Marshal.ReleaseComObject(xlWorkBook);
                Marshal.ReleaseComObject(xlApp);
                process2.Start();

                DA.SetData(0, strText + newstr);
                DA.SetData(1, @workingDir2+"data.txt");
                DA.SetData(2, @workingDir2+"targets.txt");
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                //return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile("C:\\ant\\exceldataset.png");
                return ANT.Resources.allResources.exceldataset;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{b8531de2-7e5d-4fdd-8266-51c04c18ac57}"); }
        }

        string PythonFile(string data, string target, string features, string workingDir2)
        {
            workingDir2 = workingDir2.Replace("\\", "\\\\");
            string thisFile = Resources.allResources.ExcelDataset1;
            thisFile = thisFile.Replace("##directory##", workingDir2);
            thisFile = thisFile.Replace("##data##", data);
            thisFile = thisFile.Replace("##target##", target);
            thisFile = thisFile.Replace("##features##", features);
            System.IO.File.WriteAllText(@workingDir2+"doAllWork.py", thisFile);

            return @"C:\ant\init\ExcelDataset\doAllWork.py";
        }

    }
}