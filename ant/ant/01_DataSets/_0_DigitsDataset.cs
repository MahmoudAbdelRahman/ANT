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

namespace ANT._01_DataSets
{
    public class _0_DigitsDataset : GH_Component
    {
        private bool processhasexit = false;

        string workingDir = "C:\\ant\\_02_datasets\\DigitsDataset\\";


        /// <summary>
        /// Initializes a new instance of the _0_DigitsDataset class.
        /// </summary>
        public _0_DigitsDataset()
            : base("_0_DigitsDataset", "Digits",
                "Loads the Ditigts DataSEt",
                "ANT", "1|Data Set")
        {
            if (!Directory.Exists(@workingDir))
            {
                Directory.CreateDirectory(@workingDir);
            }
            this.Message = "The Digits Dataset";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Load Digits dataset ?   |", "LoadDigits  |", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("|    Log_ : ", "|   Log", "Result of loading DigitsDataset", GH_ParamAccess.item);
            pManager.AddTextParameter("|    Data |", "|   Data", "List of data to be trained .. ", GH_ParamAccess.item);
            pManager.AddTextParameter("|    Targets |", "|   Targets", "List of targets to be fitted .. ", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool load_iris = false;
            string workingDir2 = workingDir.Replace("\\", "\\\\");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "python.exe";
            process.StartInfo.Arguments = "working.py";
            process.StartInfo.WorkingDirectory = workingDir;
            process.EnableRaisingEvents = true;
            process.Exited += process_Exited;

            if (!DA.GetData(0, ref load_iris)) { return; }
            if (load_iris == false) { return; }

            string file = Resources.allResources.digitsDataset;
            file = file.Replace("##directory##", workingDir2);

            System.IO.File.WriteAllText(@workingDir + "working.py", file);

            process.Start();
            while (!processhasexit)
            {

            }
            try
            {
                DA.SetData(0, System.IO.File.ReadAllText(workingDir + "digitslog.txt") + "\n " + System.IO.File.ReadAllText(workingDir + "digitsdata_r.txt") + "\n " + System.IO.File.ReadAllText(workingDir + "digitstarget_r.txt"));
                DA.SetData(1, workingDir + "data.txt");
                DA.SetData(2, workingDir + "targets.txt");

            }
            catch (Exception e)
            {
                DA.SetData(0, e.Message);
            }
            processhasexit = false;

        }

        private void process_Exited(object sender, EventArgs e)
        {
            processhasexit = true;
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
                //return (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile("C:\\ant\\iris_icon.png");
                return ANT.Resources.allResources.digits;
            }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{3ab3d8f5-f524-46a7-84d9-018bba39e1ff}"); }
        }
    }
}