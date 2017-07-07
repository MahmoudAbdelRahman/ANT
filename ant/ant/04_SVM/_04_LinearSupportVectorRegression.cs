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
using System.Diagnostics;
using ANT.Resources;

namespace ANT._04_SVM
{
    public class _04_LinearSupportVectorRegression : GH_Component
    {
        string VersionDate = "12-6-2017" + "\n";
        string Version = "0.1alpha" + "\n";
        string CompName = "_04_LinearSupportVectorRegression" + "\n";

        // this handles exiting pyton file event. 
        private bool processhasexit = false;

        // This Process starts runnign python file. 
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();

        // XML result file initiating 
        XmlDocument doc = new XmlDocument();


        //Input variables and documentations
        Object[,] Invars;
        string[] Indocs;

        // Output variables and documentation
        string[] Outvars;
        string[] OutDocs;


        /// <summary>
        /// Initializes a new instance of the _04_LinearSupportVectorRegression class.
        /// </summary>
        public _04_LinearSupportVectorRegression()
            : base("_04_LinearSupportVectorRegression", "LinearSVR",
                "",
                "ANT", "4|SVM")
        {
            this.Message = CompName + "V " + Version + VersionDate;
            this.Description = "";

            //initiating the process which runs the Python file. 
            process2.StartInfo.FileName = "python.exe";
            process2.StartInfo.Arguments = "doAllWork.py";
            process2.EnableRaisingEvents = true;
            process2.StartInfo.CreateNoWindow = true;
            process2.StartInfo.UseShellExecute = true;
            process2.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process2.Exited += process_Exited;

        }


        public override Grasshopper.Kernel.GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {


            Indocs = new string[]{
                "C : float, optional (default=1.0)\nPenalty parameter C of the error term. The penalty is a squared l2 penalty. The bigger this parameter, the less regularization is used.",
				"loss : string, ‘epsilon_insensitive’ or ‘squared_epsilon_insensitive’ (default=’epsilon_insensitive’)\nSpecifies the loss function. ‘l1’ is the epsilon-insensitive loss (standard SVR) while ‘l2’ is the squared epsilon-insensitive loss.",
				"epsilon : float, optional (default=0.1)\nEpsilon parameter in the epsilon-insensitive loss function. Note that the value of this parameter depends on the scale of the target variable y. If unsure, set epsilon=0.",
				"dual : bool, (default=True)\nSelect the algorithm to either solve the dual or primal optimization problem. Prefer dual=False when n_samples > n_features.",
				"tol : float, optional (default=1e-4)\nTolerance for stopping criteria.",
				"fit_intercept : boolean, optional (default=True)\nWhether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered).",
				"intercept_scaling : float, optional (default=1)\nWhen self.fit_intercept is True, instance vector x becomes [x, self.intercept_scaling], i.e. a “synthetic” feature with constant value equals to intercept_scaling is appended to the instance vector. The intercept becomes intercept_scaling * synthetic feature weight Note! the synthetic feature weight is subject to l1/l2 regularization as all other features. To lessen the effect of regularization on synthetic feature weight (and therefore on the intercept) intercept_scaling has to be increased.",
				"verbose : int, (default=0)\nEnable verbose output. Note that this setting takes advantage of a per-process runtime setting in liblinear that, if enabled, may not work properly in a multithreaded context.",
				"random_state : int seed, RandomState instance, or None (default=None)\nThe seed of the pseudo random number generator to use when shuffling the data.",
				"max_iter : int, (default=1000)The maximum number of iterations to be run."
            };

            // Input Variables : {"variableName", "VaribaleType 's', 'b','f', 'i'", defaultValue}
            Invars = new Object[,] {{"C","f",1.0},
				{"loss","s","epsilon_insensitive"},
				{"epsilon","f",0.1},
				{"dual","b",true},
				{"tol","f",1e-4},
				{"fit_intercept","b",true},
				{"intercept_scaling","f",1.0},
				{"verbose","i",0},
				{"random_state","s","None"},
				{"max_iter","i",1000}};

            initInputParams(Indocs, Invars, pManager);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            Outvars = new string[] {"coef_",
				"intercept_"};
            OutDocs = new string[] {"coef_ : array, shape = [n_features] if n_classes == 2 else [n_classes, n_features]\nWeights assigned to the features (coefficients in the primal problem). This is only available in the case of linear kernel.\ncoef_ is a readonly property derived from raw_coef_ that follows the internal memory layout of liblinear.",
				"intercept_ : array, shape = [1] if n_classes == 2 else [n_classes]\nConstants in decision function."};
            initOutputParams(OutDocs, Outvars, pManager);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_04_LinearSupportVectorRegression\\\\";

            string dataFile = null;
            string targetFile = null;
            string resultFile = "result.txt";
            string workingDir = null;
            bool fit = false;
            string Logs = "";

            //Optional Data:

            double C = 1.0;
            string loss = "epsilon_insensitive";
            double epsilon = 0.1;
            bool dual = true;
            double tol = 1e-4;
            bool fit_intercept = true;
            double intercept_scaling = 1.0;
            int verbose = 0;
            string random_state = "None";
            int max_iter = 1000;


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
            try
            {
                DA.GetData(5, ref C);
                DA.GetData(6, ref loss);
                DA.GetData(7, ref epsilon);
                DA.GetData(8, ref dual);
                DA.GetData(9, ref tol);
                DA.GetData(10, ref fit_intercept);
                DA.GetData(11, ref intercept_scaling);
                DA.GetData(12, ref verbose);
                DA.GetData(13, ref random_state);
                DA.GetData(14, ref max_iter);

            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Logs += line.ToString() + "\n";
            }

            // 01 Specify the working directory at which pythonFile exists, note that this Python file should run separately using Proccess2 (see 5). 
            process2.StartInfo.WorkingDirectory = workingDir;
            try
            {
                // 02 Initiate helperFunctions
                HelperFunction helpFunctions = new HelperFunction();

                // Put inputData in one list of strings. 
                List<string> dataInput = new List<string>(new string[] {
                C.ToString(),
				@"'"+loss+@"'",
				epsilon.ToString(),
				dual==true?"True":"False",
				tol.ToString(),
				fit_intercept==true?"True":"False",
				intercept_scaling.ToString(),
				verbose.ToString(),
				@"'"+random_state+@"'",
				max_iter.ToString()
            });

                // 03 Convert data from grasshopper syntax to python NumPy like array. 
                string newString = helpFunctions.dataInput2Python(workingDir, predicTData);

                // 04 Write the Python file in the working directory 
                helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._04_LinearSupportVectorRegressionPy, dataInput);


            }
            catch (Exception e)
            {
                this.Message = e.ToString();
            }

            // 05 Start running Python file. and wait until it is closed i.e. raising the process_Exited event. 
            process2.Start();

            while (!processhasexit)
            {

            }
            try
            {
                doc.Load(workingDir + "res.xml");

                //TODO : add all the output variables here
                //AllData = {"prediction":result, "score":sroce, "support":support, "support_vectors":support_vectors, "n_support": n_support, "dual_coef": dual_coef, "coef": coeff, "intercept":intercept}

                XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");

                XmlNode coef_node = doc.DocumentElement.SelectSingleNode("/result/coef");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");



                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string coef = coef_node.InnerText;
                string intercept = intercept_node.InnerText;





                //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);

                DA.SetData(3, coef);
                DA.SetData(4, intercept);


                long ticks1 = DateTime.Now.Ticks;
                double timeElaspsed = ((double)ticks1 - (double)ticks0) / 10000000;
                Logs += "Success !! in : " + timeElaspsed + " Seconds\n ";


            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Logs += e.Message + line.ToString() + "\n";
            }
            DA.SetData(0, Logs);
            processhasexit = false;

        }

        private void process_Exited(object sender, EventArgs e)
        {
            processhasexit = true;
        }


        /// <summary>
        /// This function initiates input parametrs of the component. 
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="vars"></param>
        /// <param name="pManager"></param>
        private void initInputParams(string[] docs, Object[,] vars, GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddTextParameter("_Data \t    | ", "_D", "Input instances for training ..\nndarray or scipy.sparse matrix, (n_samples, n_features) Data", GH_ParamAccess.item);                                      //0
            pManager.AddTextParameter("_Target \t    | ", "_T", "Input Targets for Fitting .. ", GH_ParamAccess.item);                                  //1
            pManager.AddTextParameter("_workingFolder   | ", "Fldr", "Please specify the folder that you are working on .. ", GH_ParamAccess.item);     //2
            pManager.AddGenericParameter("_Predict \t    | ", "_P", "Insert the new features of the current test  .. ", GH_ParamAccess.list);           //3
            pManager.AddBooleanParameter("_FIT? \t    | ", "_F", "Start fitting data ? ", GH_ParamAccess.item, false);                                  //4


            for (int i = 0; i < docs.Length; i++)
            {
                if ((string)vars[i, 1] == "f")
                {
                    pManager.AddNumberParameter((string)vars[i, 0] + " \t    | ", (string)vars[i, 0], docs[i], GH_ParamAccess.item, Convert.ToDouble(vars[i, 2]));
                }
                else if ((string)vars[i, 1] == "s")
                {
                    pManager.AddTextParameter((string)vars[i, 0] + " \t    | ", (string)vars[i, 0], docs[i], GH_ParamAccess.item, vars[i, 2].ToString());
                }
                else if ((string)vars[i, 1] == "b")
                {
                    pManager.AddBooleanParameter((string)vars[i, 0] + " \t    | ", (string)vars[i, 0], docs[i], GH_ParamAccess.item, Convert.ToBoolean(vars[i, 2]));
                }
                else if ((string)vars[i, 1] == "i")
                {
                    pManager.AddIntegerParameter((string)vars[i, 0] + " \t    | ", (string)vars[i, 0], docs[i], GH_ParamAccess.item, Convert.ToInt32(vars[i, 2]));

                }
            }
        }


        private void initOutputParams(string[] docs, string[] vars, GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);

            pManager.AddTextParameter("score", "score", "result score", GH_ParamAccess.item);

            for (int i = 0; i < vars.Length; i++)
            {
                pManager.AddTextParameter(" |    " + (string)vars[i], (string)vars[i], (string)docs[i], GH_ParamAccess.item);
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return allResources._04_LinearSupportVectorRegression_icon;
            }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7ea20ce8-51ab-414b-966e-65431c536875}"); }
        }
    }
}