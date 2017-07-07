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
using System.Xml;
using ANT.Resources;

namespace ANT._03_LinearModels
{
    public class _03_LassoLars : GH_Component
    {

        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();

        /// <summary>
        /// Initializes a new instance of the _03_LassoLars class.
        /// </summary>
        public _03_LassoLars()
            : base("_03_LassoLars", "LassoLars",
                "",
                "ANT", "3|Linear Model")
        {
            this.Message = "LARS Lasso";
            this.Description = "LassoLars is a lasso model implemented using the LARS algorithm, and unlike the implementation based on coordinate_descent, this yields the exact solution, which is piecewise linear as a function of the norm of its coefficients.";

            process2.StartInfo.FileName = "python.exe";
            process2.StartInfo.Arguments = "doAllWork.py";
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

            pManager.AddTextParameter("score", "score", "result score", GH_ParamAccess.item);

            pManager.AddTextParameter(" |    alphas_", " |    alphas_", "array, shape (n_alphas + 1,) | list of n_targets such arrays\nMaximum of covariances (in absolute value) at each iteration. n_alphas is either n_nonzero_coefs or n_features, whichever is smaller.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    active_", " |    active_", "* active_ : list, length = n_alphas | list of n_targets such lists\nIndices of active variables at the end of the path.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    coef_path_", " |    coef_path_", "coef_path_ : array, shape (n_features, n_alphas + 1) | list of n_targets such arrays\nhe varying values of the coefficients along the path. It is not present if the fit_path parameter is False.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    coef_", " |    coef_", "coef_ : array, shape (n_features,) or (n_targets, n_features)\nParameter vector (w in the formulation formula).", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    intercept_", " |    intercept_", "intercept_ : float | array, shape (n_targets,)\nIndependent term in decision function.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    n_iter_", " |    n_iter_", "n_iter_ : array-like or int\nThe number of iterations taken by lars_path to find the grid of alphas for each target.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_03_LassoLars\\\\";

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
            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._03_LassoLars, dataInput);
            process2.Start();
            while (!processhasexit)
            {
                DA.SetData(0, "Please Wait .... ");
            }
            try
            {
                doc.Load(workingDir + "res.xml");


                //TODO : add all the output variables here
                //result 1 | score 2 | coef_ 3 | sparse_coef_ 4 | intercept_ 5

                XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");


                XmlNode alphas_node = doc.DocumentElement.SelectSingleNode("/result/alphas");
                XmlNode active_node = doc.DocumentElement.SelectSingleNode("/result/active");
                XmlNode coef_path_node = doc.DocumentElement.SelectSingleNode("/result/coef_path");
                XmlNode coeff_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");
                //XmlNode n_inter_node = doc.DocumentElement.SelectSingleNode("/result/n_inter");


                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string alphas = alphas_node.InnerText;
                string active = active_node.InnerText;
                string coef_path = coef_path_node.InnerText;
                string coeff = coeff_node.InnerText;
                string intercept = intercept_node.InnerText;



                //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(3, alphas);
                DA.SetData(4, active);
                DA.SetData(5, coef_path);
                DA.SetData(6, coeff);
                DA.SetData(7, intercept);


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
                return ANT.Resources.allResources._03_icon_LarsLasso;
            }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{435bf387-bd38-493e-9e21-f8c0f8bb1ad3}"); }
        }
    }
}