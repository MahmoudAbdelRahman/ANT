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
using System.Xml;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ANT._03_LinearModels
{
    public class _03_LinearRegression : GH_Component
    {

        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();

        /// <summary>
        /// Initializes a new instance of the _03_LinearRegression class.
        /// </summary>
        public _03_LinearRegression()
            : base("_03_LinearRegression", "LinReg",
                "Linear regression learning algorithm. ",
                "ANT", "3|Linear Model")
        {
            this.Message = "Linear Regression\n Ordinary Least Squares";
            this.Description = "LinearRegression fits a linear model with coefficients w = (w_1, ..., w_p) to minimize the residual sum of squares between the observed responses in the dataset, and the responses predicted by the linear approximation";

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
            pManager.AddTextParameter("_Data \t    | ",         "_D", "Input data for training .. ", GH_ParamAccess.item);                          //0
            pManager.AddTextParameter("_Target \t    | ",       "_T", "Input Targets for Fitting .. ", GH_ParamAccess.item);                        //1
            pManager.AddTextParameter("_WorkingDir \t    | ", "Dir", " Woring directory ... ", GH_ParamAccess.item);                                //2
            pManager.AddGenericParameter("_Predict \t    | ",   "_P", "Insert the new features of the current test  .. ", GH_ParamAccess.list);     //3
            pManager.AddBooleanParameter("_FIT? \t    | ", "_F", "Start fitting data ? ", GH_ParamAccess.item, false);                              //4

            pManager.AddTextParameter("-----", "-----", "------", GH_ParamAccess.item, "--------");
            //optional inputs:
            /*
             *Parameters: 
             *      fit_intercept : boolean, optional
             *          whether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered).
             * 
             *      normalize : boolean, optional, default False
             *          If True, the regressors X will be normalized before regression.
             *          
             *      copy_X : boolean, optional, default True
             *          If True, X will be copied; else, it may be overwritten.
             *      
             *      n_jobs : int, optional, default 1
             *          The number of jobs to use for the computation. If -1 all CPUs are used. This will only provide speedup for n_targets > 1 and sufficient large problems.
             *          
             */

            pManager.AddBooleanParameter("_fit_intercept_\t    | ", "fit_intercept \t    | ", "boolean, optional\nwhether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered).", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("_normalize_\t    | ", "normalize \t    | ", "boolean, optional, default False\n If True, the regressors X will be normalized before regression.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("_copy_X_\t    | ", "copy_X \t    | ", "boolean, optional, default True\nIf True, X will be copied; else, it may be overwritten.", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("_n_jobs_\t    | ", "n_jobs \t    | ", "int, optional, default 1\nThe number of jobs to use for the computation. If -1 all CPUs are used. This will only provide speedup for n_targets > 1 and sufficient large problems.", GH_ParamAccess.item, 1);


            

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);

            pManager.AddTextParameter("score", "score", "result score", GH_ParamAccess.item);
            /*
             * Attributes: 
             *      coef_ : array, shape (n_features, ) or (n_targets, n_features)
             *          Estimated coefficients for the linear regression problem. If multiple targets are passed during the fit (y 2D), this is a 2D array of shape (n_targets, n_features), while if only one target is passed, this is a 1D array of length n_features.
             *          
             *      intercept_ : array
             *          Independent term in the linear model.
             *      
             *      Score_ :
             *          Returns the coefficient of determination R^2 of the prediction.
             *          The coefficient R^2 is defined as (1 - u/v), where u is the regression sum of squares ((y_true - y_pred) ** 2).sum() and v is the residual sum of squares ((y_true - y_true.mean()) ** 2).sum(). Best possible score is 1.0, lower values are worse.
             * 
             */

            pManager.AddTextParameter(" |    coef_",        " |    coef_",      "array, shape (n_features, ) or (n_targets, n_features)\nEstimated coefficients for the linear regression problem. If multiple targets are passed during the fit (y 2D), this is a 2D array of shape (n_targets, n_features), while if only one target is passed, this is a 1D array of length n_features.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    intercept_",   " |    intercept_", "array\nIndependent term in the linear model.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\init\\\\LinearRegression\\\\";

            string dataFile = null;
            string targetFile = null;
            string resultFile = "result.txt";
            string logFile = "logFile.txt";
            string workingDir = null;
            bool fit = false;

            string Logs = ""; 


            List<double> predicTData = new List<double>();


            //string mainDir = "C:\\ant\\init\\";
            //string workingDir = mainDir + "LinearRegression\\";
            //DA.SetData(0, workingDir);


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
            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", Resources.allResources._03_LinearRegression, dataInput);

            // 05 Start running Python file. and wait until it is closed i.e. raising the process_Exited event. 
            process2.Start();

            while (!processhasexit)
            {
                
            }
            try
            {
                doc.Load(workingDir+"res.xml");
                //TODO : add all the output variables here
                //result 0 | coeff 3 | intercept 4 | score 5
                XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                XmlNode coeff_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");
                XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");

                string res = res_node.InnerText;
                string coeff = coeff_node.InnerText;
                string intercept = intercept_node.InnerText;
                string score = score_node.InnerText;


                //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(3, coeff);
                DA.SetData(4, intercept);


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
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return ANT.Resources.allResources.linearreg;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{26a56a84-b922-4fef-b389-887838e32476}"); }
        }
    }
}