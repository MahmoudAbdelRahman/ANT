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
using ANT.Resources;

namespace ANT._03_LinearModels
{
    public class _03_BayesianRegression : GH_Component
    {
        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();
        /// <summary>
        /// Initializes a new instance of the _03_BayesianRegression class.
        /// </summary>
        public _03_BayesianRegression()
            : base("_03_BayesianRegression", "BayesianRegression",
                "Description",
                "ANT", "3|Linear Model")
        {
            this.Description = "Bayesian regression techniques can be used to include regularization parameters in the estimation procedure: the regularization parameter is not set in a hard sense but tuned to the data at hand.";
            this.Message = "Bayesian Regression";

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

            /*
             PARAMETERS: 
                 * n_iter : int, optional
                 *      Maximum number of iterations. Default is 300.
                 * tol : float, optional
                 *      Stop the algorithm if w has converged. Default is 1.e-3.
                 * alpha_1 : float, optional
                 *      Hyper-parameter : shape parameter for the Gamma distribution prior over the alpha parameter. Default is 1.e-6
                 * alpha_2 : float, optional
                 *      Hyper-parameter : inverse scale parameter (rate parameter) for the Gamma distribution prior over the alpha parameter. Default is 1.e-6.
                 * lambda_1 : float, optional
                 *      Hyper-parameter : shape parameter for the Gamma distribution prior over the lambda parameter. Default is 1.e-6.
                 * lambda_2 : float, optional
                 *      Hyper-parameter : inverse scale parameter (rate parameter) for the Gamma distribution prior over the lambda parameter. Default is 1.e-6
                 * compute_score : boolean, optional
                 *      If True, compute the objective function at each step of the model. Default is False
                 * fit_intercept : boolean, optional
                 *      whether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered). Default is True.
                 * normalize : boolean, optional, default False
                 *      If True, the regressors X will be normalized before regression.
                 * copy_X : boolean, optional, default True
                 *      If True, X will be copied; else, it may be overwritten.
                 * verbose : boolean, optional, default False
                 *      Verbose mode when fitting the model. 
             */
            pManager.AddIntegerParameter("n_iter \t    | ", "n_iter", "int, optional\nMaximum number of iterations. Default is 300.", GH_ParamAccess.item, 300);
            pManager.AddNumberParameter("tol \t    | ", "tol", "float, optional\nStop the algorithm if w has converged. Default is 1.e-3.", GH_ParamAccess.item, 0.001);
            pManager.AddNumberParameter("alpha_1 \t    | ", "alpha_1", "float, optional\nHyper-parameter : shape parameter for the Gamma distribution prior over the alpha parameter. Default is 1.e-6", GH_ParamAccess.item, 1e-6);
            pManager.AddNumberParameter("alpha_2 \t    | ", "alpha_2", "float, optional\nHyper-parameter : inverse scale parameter (rate parameter) for the Gamma distribution prior over the alpha parameter. Default is 1.e-6.", GH_ParamAccess.item, 1e-6);
            pManager.AddNumberParameter("lambda_1 \t    | ", "lambda_1", "float, optional\nHyper-parameter : shape parameter for the Gamma distribution prior over the lambda parameter. Default is 1.e-6.", GH_ParamAccess.item, 1e-6);
            pManager.AddNumberParameter("lambda_2 \t    | ", "lambda_2", "float, optional\nHyper-parameter : inverse scale parameter (rate parameter) for the Gamma distribution prior over the lambda parameter. Default is 1.e-6", GH_ParamAccess.item, 1e-6);
            pManager.AddBooleanParameter("compute_score \t    | ", "compute_score", "boolean, optional\nIf True, compute the objective function at each step of the model. Default is False", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("fit_intercept \t    | ", "fit_intercept", "boolean, optional\nwhether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered). Default is True.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("normalize \t    | ", "normalize", "boolean, optional, default False\nIf True, the regressors X will be normalized before regression.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("copy_X \t    | ", "copy_X", "boolean, optional, default True\nIf True, X will be copied; else, it may be overwritten.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("verbose \t    | ", "verbose", "boolean, optional, default False\nVerbose mode when fitting the model", GH_ParamAccess.item, false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            /*
             * ATTRIBUTES 
             * 
             * coef_ : array, shape = (n_features)
             *      Coefficients of the regression model (mean of distribution)
             * alpha_ : float
             *      estimated precision of the noise.
             * lambda_ : array, shape = (n_features)
             *      estimated precisions of the weights.
             * scores_ : float
             *      if computed, value of the objective function (to be maximized)*/

            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);
            pManager.AddTextParameter("score", "score", "score", GH_ParamAccess.item);

            pManager.AddTextParameter(" |    coef_", " |    coef_", "Coefficients of the regression model (mean of distribution)", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    alpha_", " |    alpha_", "estimated precision of the noise.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    lambda_", " |    lambda_", "estimated precisions of the weights.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_03_BayesianRegression\\\\";

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
            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._03_BayesianRegression, dataInput);


            // 05 Start running Python file. and wait until it is closed i.e. raising the process_Exited event.
            process2.Start();

            while (!processhasexit)
            {

            }
            try
            {
                doc.Load(workingDir + "res.xml");
                //TODO : add all the output variables here
                //result 1 | score 2 | coef_ 3 | alpha_ 4 | lambda_ 5  
                XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");

                XmlNode coef_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                XmlNode alpha_node = doc.DocumentElement.SelectSingleNode("/result/alpha");
                XmlNode lambda_node = doc.DocumentElement.SelectSingleNode("/result/lambda_");

                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string coef = coef_node.InnerText;
                string alpha = alpha_node.InnerText;
                string lambda_ = lambda_node.InnerText;

                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(3, coef);
                DA.SetData(4, alpha);
                DA.SetData(5, lambda_);


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
                return ANT.Resources.allResources._03_icon_Bayes;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c2c9edb1-c172-4888-a007-17df51cdbd48}"); }
        }
    }
}