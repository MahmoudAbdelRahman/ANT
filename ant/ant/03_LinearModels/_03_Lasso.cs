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
    public class _03_Lasso : GH_Component
    {
        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();
        /// <summary>
        /// Initializes a new instance of the _03_Lasso class.
        /// </summary>
        public _03_Lasso()
            : base("_03_Lasso", "Lasso",
                "The Lasso is a linear model that estimates sparse coefficients. \n"+
                "It is useful in some contexts due to its tendency to prefer solutions with fewer parameter values, effectively reducing the number of variables upon which the given solution is dependent.\n"+
                "For this reason, the Lasso and its variants are fundamental to the field of compressed sensing. ",
                "ANT", "3|Linear Model")
        {
            this.Message = "Lasso ";
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

            /*
             Parameters: 
             * alpha : float, optional
             *      Constant that multiplies the L1 term. Defaults to 1.0. alpha = 0 is equivalent to an ordinary least square, solved by the LinearRegression object. For numerical reasons, using alpha = 0 is with the Lasso object is not advised and you should prefer the LinearRegression object. 
             * fit_intercept : boolean
             *      whether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered).  
             * normalize : boolean, optional, default False
             *      If True, the regressors X will be normalized before regression. 
             * copy_X : boolean, optional, default True
             *      If True, X will be copied; else, it may be overwritten. 
             * precompute : True | False | ‘auto’ | array-like
             *      Whether to use a precomputed Gram matrix to speed up calculations. If set to 'auto' let us decide. The Gram matrix can also be passed as argument. For sparse input this option is always True to preserve sparsity. WARNING : The 'auto' option is deprecated and will be removed in 0.18.   
             * max_iter : int, optional
             *      The maximum number of iterations      
             * tol : float, optional
             *      The tolerance for the optimization: if the updates are smaller than tol, the optimization code checks the dual gap for optimality and continues until it is smaller than tol.      
             * warm_start : bool, optional
             *      When set to True, reuse the solution of the previous call to fit as initialization, otherwise, just erase the previous solution.     
             * positive : bool, optional
             *      When set to True, forces the coefficients to be positive.     
             * selection : str, default ‘cyclic’
             *      If set to ‘random’, a random coefficient is updated every iteration rather than looping over features sequentially by default. This (setting to ‘random’) often leads to significantly faster convergence especially when tol is higher than 1e-4.      
             * random_state : int, RandomState instance, or None (default)
             *      The seed of the pseudo random number generator that selects a random feature to update. Useful only when selection is set to ‘random’.
             */

            pManager.AddNumberParameter("_alpha_ \t    | "          , "_alpha_ \t    | "            , "float, optional\nConstant that multiplies the L1 term. Defaults to 1.0. alpha = 0 is equivalent to an ordinary least square, solved by the LinearRegression object. For numerical reasons, using alpha = 0 is with the Lasso object is not advised and you should prefer the LinearRegression object. ", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("_fit_intercept_ \t    | " , "_fit_intercept_ \t    | "    , "boolean\nwhether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered).", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("_normalize_ \t    | "     , "_normalize_ \t    | "        , "boolean, optional, default False\nIf True, the regressors X will be normalized before regression. ", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("_copy_X_ \t    | "        , "_copy_X_ \t    | "           , "boolean, optional, default True\nIf True, X will be copied; else, it may be overwritten. ", GH_ParamAccess.item, false);
            pManager.AddTextParameter("_precompute_ \t    | "       , "_precompute_ \t    | "       , "True | False | ‘auto’ | array-like\nWhether to use a precomputed Gram matrix to speed up calculations. If set to 'auto' let us decide. The Gram matrix can also be passed as argument. For sparse input this option is always True to preserve sparsity.\nWARNING : The 'auto' option is deprecated and will be removed in 0.18.", GH_ParamAccess.item, "False");
            pManager.AddIntegerParameter("_max_iter_ \t    | "      , "_max_iter_ \t    | "         , "int, optional, default value: 1000 \nThe maximum number of iterations", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("_tol_ \t    | "            , "_tol_ \t    | "              , "float, optional, default value: 0.0001\nhe tolerance for the optimization: if the updates are smaller than tol, the optimization code checks the dual gap for optimality and continues until it is smaller than tol.", GH_ParamAccess.item, 0.0001);
            pManager.AddBooleanParameter("_warm_start_ \t    | "       , "_warm_start_ \t    | "       , "bool, optional\nWhen set to True, reuse the solution of the previous call to fit as initialization, otherwise, just erase the previous solution.", GH_ParamAccess.item,false);
            pManager.AddBooleanParameter("positive \t    | "        , "positive \t    | "           , "bool, optional, default: False\nWhen set to True, forces the coefficients to be positive.", GH_ParamAccess.item, false);
            pManager.AddTextParameter("selection \t    | "          , "selection \t    | "          , "str, default ‘cyclic’\nIf set to ‘random’, a random coefficient is updated every iteration rather than looping over features sequentially by default.\nThis (setting to ‘random’) often leads to significantly faster convergence especially when tol is higher than 1e-4.", GH_ParamAccess.item, "cyclic");
            pManager.AddTextParameter("_random_state_ \t    | "     , "_random_state_ \t    | "     , "int, RandomState instance, or None (default)\nThe seed of the pseudo random number generator that selects a random feature to update. Useful only when selection is set to ‘random’.", GH_ParamAccess.item, "None");


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            /* Attributes: 
            * 
            * coef_ : array, shape = (n_features,) | (n_targets, n_features)
            *      parameter vector (w in the cost function formula)     
            * sparse_coef_ : scipy.sparse matrix, shape = (n_features, 1) | (n_targets, n_features)
            *      sparse_coef_ is a readonly property derived from coef_     
            * intercept_ : float | array, shape = (n_targets,)
            *      independent term in decision function.     
            * n_iter_ : int | array-like, shape (n_targets,)
            *      number of iterations run by the coordinate descent solver to reach the specified tolerance.*/

            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);
            pManager.AddTextParameter("score", "score", "score", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    coef_", " |    coef_", "array, shape = (n_features,) | (n_targets, n_features)\nparameter vector (w in the cost function formula)", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    sparse_coef_", " |    sparse_coef_", "scipy.sparse matrix, shape = (n_features, 1) | (n_targets, n_features)\nsparse_coef_ is a readonly property derived from coef_ ", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    intercept_", " |    intercept_", "float | array, shape = (n_targets,)\nindependent term in decision function.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    n_iter_", " |    n_iter_", "int | array-like, shape (n_targets,)\nnumber of iterations run by the coordinate descent solver to reach the specified tolerance", GH_ParamAccess.item);


        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_03_LASSO\\\\";

            string dataFile = null;
            string targetFile = null;
            string resultFile = "result.txt";
            string workingDir = null;

            string Logs = "";

            bool fit = false;

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
            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", Resources.allResources._03_LASSO,dataInput);

            // 05 Start running Python file. and wait until it is closed i.e. raising the process_Exited event. 
            process2.Start();
            while (!processhasexit)
            {
               
            }
            try
            {
                doc.Load(workingDir + "res.xml");
                //TODO : add all the output variables here
                //result 1 | score 2 | coef_ 3 | sparse_coef_ 4 | intercept_ 5 | n_iter_ 6 
                XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");

                XmlNode coef_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                XmlNode sparse_coef_node = doc.DocumentElement.SelectSingleNode("/result/sparse_coef");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");
                //XmlNode n_inter_node = doc.DocumentElement.SelectSingleNode("/result/n_inter");

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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                //return ANT.Resources.allResources._03_lasso_icon;
                return ANT.Resources.allResources._03_icon_lasso;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5ef220a2-a970-4dd2-b137-882c8664f168}"); }
        }
    }
}