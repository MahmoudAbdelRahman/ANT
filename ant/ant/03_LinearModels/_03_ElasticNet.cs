/*
    [ANT] A machine learning plugin for Rhino\Grasshopper 
    Started by Mahmoud Abdelrahman [Mahmoud Ouf] under BSD License

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
 */

using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Xml;
using ANT.Resources;

namespace ANT._03_LinearModels
{
    public class _03_ElasticNet : GH_Component
    {

        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();

        /// <summary>
        /// Initializes a new instance of the _03_ElasticNet class.
        /// </summary>
        public _03_ElasticNet()
            : base("_03_ElasticNet", "ElasticNet",
                "",
                "ANT", "3|Linear Model")
        {
            this.Description = "ElasticNet is a linear regression model trained with L1 and L2 prior as regularizer.\n\nThis combination allows for learning a sparse model where few of the weights are non-zero like Lasso, while still maintaining the regularization properties of Ridge. We control the convex combination of L1 and L2 using the l1_ratio parameter.\n\nElastic-net is useful when there are multiple features which are correlated with one another. Lasso is likely to pick one of these at random, while elastic-net is likely to pick both.\n\nA practical advantage of trading-off between Lasso and Ridge is it allows Elastic-Net to inherit some of Ridge’s stability under rotation.";
            this.Message = "Elastic Net";

            process2.StartInfo.FileName = "doAllWork.py";
            process2.EnableRaisingEvents = true;
            process2.StartInfo.CreateNoWindow = true;
            process2.StartInfo.UseShellExecute = true;
            process2.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process2.Exited += process_Exited;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddTextParameter("_Data \t    | ", "_D", "Input instances for training ..\nndarray or scipy.sparse matrix, (n_samples, n_features) Data", GH_ParamAccess.item);                                      //0
            pManager.AddTextParameter("_Target \t    | ", "_T", "Input Targets for Fitting .. ", GH_ParamAccess.item);                                  //1
            pManager.AddTextParameter("_workingFolder   | ", "Fldr", "Please specify the folder that you are working on .. ", GH_ParamAccess.item);     //2
            pManager.AddGenericParameter("_Predict \t    | ", "_P", "Insert the new features of the current test  .. ", GH_ParamAccess.list);           //3
            pManager.AddBooleanParameter("_FIT? \t    | ", "_F", "Start fitting data ? ", GH_ParamAccess.item, false);                                  //4

            //Seperator
            pManager.AddTextParameter("-----", "-----", "------", GH_ParamAccess.item, "--------");
            

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);
            pManager.AddTextParameter("score", "score", "score", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    coef_", " |    coef_", "array, shape = (n_features,) | (n_targets, n_features)\nparameter vector (w in the cost function formula)", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    sparse_coef_", " |    sparse_coef_", "scipy.sparse matrix, shape = (n_features, 1) | (n_targets, n_features)\nsparse_coef_ is a readonly property derived from coef_ ", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    intercept_", " |    intercept_", "float | array, shape = (n_targets,)\nindependent term in decision function.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_03_ElasticNet\\\\";

            string dataFile = null;
            string targetFile = null;
            string resultFile = "result.txt";
            string workingDir = null;
            bool fit = false;


            string Logs = "";


            List<double> predicTData = new List<double>();

            if (!DA.GetData(0, ref dataFile)) { return; }
            if (dataFile == null) { return; }
            if (!DA.GetData(1, ref targetFile)) { return; }
            if (targetFile == null) { return; }
            if (!DA.GetData(2, ref workingDir)) { return; }
            if (workingDir == null) { return; }
            if (!DA.GetDataList(3, predicTData)) { return; }
            if (predicTData == null) { return; }
            if (!DA.GetData(4, ref fit)) { return; }
            if (fit == false) { return; }

            long ticks0 = DateTime.Now.Ticks;

            //TODO: FILL IN THE DATA INPUT PROPERLY 

            List<string> dataInput = new List<string>(new string[] { "dd", "dd" });

            // 01 Specify the working directory at which pythonFile exists, note that this Python file should run separately using Proccess2 (see 5). 
            process2.StartInfo.WorkingDirectory = workingDir;

            // 02 Initiate helperFunctions
            HelperFunction helpFunctions = new HelperFunction();

            // 03 Convert data from grasshopper syntax to python NumPy like array. 
            string newString = helpFunctions.dataInput2Python(workingDir, predicTData);

            // 04 Write the Python file in the working directory 
            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._03_ElasticNet, dataInput);

            // 05 Start running Python file. and wait until it is closed i.e. raising the process_Exited event.
            process2.Start();

            while (!processhasexit)
            {

            }
            try
            {
                doc.Load(workingDir + "res.xml");
                //TODO : add all the output variables here
                //result 1 | score 2 | coef_ 3 | sparse_coef_ 4 | intercept_ 5  
                XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");

                XmlNode coef_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                XmlNode sparse_coef_node = doc.DocumentElement.SelectSingleNode("/result/sparse_coef");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");

                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string coef = coef_node.InnerText;
                string sparse_coef = sparse_coef_node.InnerText;
                string intercept = intercept_node.InnerText;

                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(3, coef);
                DA.SetData(4, sparse_coef);
                DA.SetData(5, intercept);


                long ticks1 = DateTime.Now.Ticks;
                long timeElaspsed = (ticks1 - ticks0) / 10000;
                Logs += "Success !! in : " + timeElaspsed + " Milli Seconds\n ";

            }
            catch (Exception e)
            {
                Logs += "001 " + e + "\n";
            }

            DA.SetData(0, Logs);
            processhasexit = false;

        }

        private void process_Exited(object sender, EventArgs e)
        {
            processhasexit = true;
        }
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return ANT.Resources.allResources._03_ElasticNet1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{f9a0c2e4-a559-4b16-a066-849ee4dc7f44}"); }
        }
    }
}