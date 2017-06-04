using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;

namespace ANT._01_DataSets
{
    public class _0_BostonDataset : GH_Component
    {
        bool fileExited = false;
        
        string workingDir = "C:\\ant\\_02_datasets\\BostonDataset\\";

        /// <summary>
        /// Initializes a new instance of the _0_BostonDataset class.
        /// </summary>
        public _0_BostonDataset()
            : base("_0_BostonDataset", "Boston",
                "Boston DAtaset used for regression",
                "ANT", "1|Data Set")
        {

            if (!Directory.Exists(@workingDir))
            {
                Directory.CreateDirectory(@workingDir);
                string file = Resources.allResources.boston_dataset;
                string workingDir2 = workingDir.Replace("\\", "\\\\");
                file = file.Replace("##directory##", workingDir2);
                File.WriteAllText(workingDir + "boston_dataset.py", file);
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Load Boston Dataset? ", "LoadBoston", "Would you like to load Boston Dataset ? ", GH_ParamAccess.item, false);
        }
        public override Grasshopper.Kernel.GH_Exposure Exposure
        {
            get { return GH_Exposure.obscure; }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Data Log", "Log", "data log of boston dataset loading ...", GH_ParamAccess.item);
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

            bool loadBoston = false;

            if (!DA.GetData(0, ref loadBoston)) { return; }
            if (!loadBoston) { return; }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "boston_dataset.py";
            process.StartInfo.WorkingDirectory = workingDir;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.EnableRaisingEvents = true;
            process.Exited += process_Exited;


            process.Start();
            while (!fileExited)
            {

            }
            try
            {
                DA.SetData(0, System.IO.File.ReadAllText(workingDir + "bostonDatasetLog.txt") + "\n " + System.IO.File.ReadAllText(workingDir + "bostondata_r.txt") + "\n " + System.IO.File.ReadAllText(workingDir + "bostontargets_r.txt"));
                DA.SetData(1, workingDir + "data.txt");
                DA.SetData(2, workingDir + "targets.txt");

            }
            catch (Exception e)
            {
                DA.SetData(0, e.Message);
            }
            fileExited = false;

        }

        void process_Exited(object sender, EventArgs e)
        {
            fileExited = true;
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
                return Resources.allResources.boston;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c4613893-de3f-4594-94eb-2361d894b431}"); }
        }
    }
}