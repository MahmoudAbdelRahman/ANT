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
*/



using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Resources;

namespace ANT
{
    /// <summary>
    /// 
    /// </summary>
    public class antComponent : GH_Component
    {
        bool processhasexit = false;
        List<string> folders = new List<string>(new string[] { "init", "_02_datasets", "_03_LinearModel", "_04_SVM", "_05_SGD", "_06_NearestNeighbors"});
        string MainDirectory = "C:\\";
        string MainFolder = "ant\\";
        public antComponent()
            : base("ANT", "ANT-ML",
                "Start The ant Colony ... ",
                "ANT", "0_ant")
        {
            if(!System.IO.Directory.Exists(MainDirectory+MainFolder))
            {
                System.IO.Directory.CreateDirectory(MainDirectory+MainFolder);
                System.IO.Directory.CreateDirectory(MainDirectory+MainFolder);
            }
            for (int i = 0; i < folders.Count; i++)
            {
                if (!System.IO.Directory.Exists(MainDirectory + MainFolder + folders[i] + "\\"))
                {
                    System.IO.Directory.CreateDirectory(MainDirectory + MainFolder + folders[i] + "\\");
                }
            }
            if(!System.IO.File.Exists(MainDirectory+MainFolder+"init\\_00_init.bat"))
            {
                string batfile = Resources.allResources._00_init;
                System.IO.File.WriteAllText(MainDirectory + MainFolder + "init\\_00_init.bat", batfile);
            }

            if (!System.IO.File.Exists(MainDirectory + MainFolder + "init\\init.py"))
            {
                string pythonFile = Resources.allResources.init;
                System.IO.File.WriteAllText(MainDirectory + MainFolder + "init\\init.py", pythonFile);
            }
            if (!System.IO.File.Exists(MainDirectory + MainFolder + "init\\errorlog.txt"))
            {
                System.IO.File.CreateText(MainDirectory + MainFolder + "init\\errorlog.txt");
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter(" Init    |", "Start ANT", "strtAnt", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("|    Log", "logFile", "log file of initializing all required libraries... ", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool data = false;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "init.py";
            process.StartInfo.WorkingDirectory = "C:\\ant\\init\\";
            process.EnableRaisingEvents = true;
            process.Exited += process_Exited;

            if(!DA.GetData(0, ref data))
            {
                return;
            }
            if(data == false)
            {
                return;
            }
            process.Start();
            while(!processhasexit)
            {
                
            }
            try
            {
                DA.SetData(0, System.IO.File.ReadAllText(@"C:\ant\init\errorlog.txt"));
            }
            catch (Exception e)
            {
                DA.SetData(0, e.Message);
            }
            processhasexit = false;

        }
        
        void process_Exited(object sender, EventArgs e)
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
                return ANT.Resources.allResources.icon2;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{8feb36d4-8bbd-407c-937d-5fc8124d68ad}"); }
        }

        
    }
}
