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
using System.Diagnostics;

namespace ANT._03_LinearModels
{
    public class _03_LogisticRegression : GH_Component
    {

        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();

        /// <summary>
        /// Initializes a new instance of the _03_LogisticRegression class.
        /// </summary>
        public _03_LogisticRegression()
            : base("_03_LogisticRegression", "LogisticRegression",
                "",
                "ANT", "3|Linear Model")
        {
            this.Message = "logistic Regression";
            this.Description = "Logistic regression, despite its name, is a linear model for classification rather than regression. Logistic regression is also known in the literature as logit regression, maximum-entropy classification (MaxEnt) or the log-linear classifier. In this model, the probabilities describing the possible outcomes of a single trial are modeled using a logistic function.\n\n"+
                "The implementation of logistic regression can fit a multiclass (one-vs-rest) logistic regression with optional L2 or L1 regularization";

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
            //pManager.AddTextParameter("-----", "-----", "------", GH_ParamAccess.item, "--------");

            /*
                penalty
                dual
                C
                fit_intercept
                intercept_scaling 
                class_weight 
                max_iter
                random_state 
                solver 
                tol
                multi_class
                verbose
             */
            
            pManager.AddTextParameter("penalty \t    | ", "penalty", "str, ‘l1’ or ‘l2’\nUsed to specify the norm used in the penalization. The newton-cg and lbfgs solvers support only l2 penalties.", GH_ParamAccess.item, "l2");
            pManager.AddBooleanParameter("dual \t    | ", "dual", "bool\nDual or primal formulation. Dual formulation is only implemented for l2 penalty with liblinear solver. Prefer dual=False when n_samples > n_features.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("C \t    | ", "C","float, optional (default=1.0)\nInverse of regularization strength; must be a positive float. Like in support vector machines, smaller values specify stronger regularization.", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("fit_intercept \t    | ", "fit_intercept", "bool, default: True\nSpecifies if a constant (a.k.a. bias or intercept) should be added the decision function.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("intercept_scaling \t    | ", "intercept_scaling", "float, default: 1\nUseful only if solver is liblinear. when self.fit_intercept is True, instance vector x becomes [x, self.intercept_scaling], i.e. a “synthetic” feature with constant value equals to intercept_scaling is appended to the instance vector. The intercept becomes intercept_scaling * synthetic feature weight Note! the synthetic feature weight is subject to l1/l2 regularization as all other features. To lessen the effect of regularization on synthetic feature weight (and therefore on the intercept) intercept_scaling has to be increased.", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("class_weight \t    | ", "class_weight", "{dict, ‘auto’}, optional\nOver-/undersamples the samples of each class according to the given weights. If not given, all classes are supposed to have weight one. The ‘auto’ mode selects weights inversely proportional to class frequencies in the training set.", GH_ParamAccess.item, "None");
            pManager.AddIntegerParameter("max_iter \t    | ", "max_iter", "int\nUseful only for the newton-cg and lbfgs solvers. Maximum number of iterations taken for the solvers to converge.", GH_ParamAccess.item, 100);
            pManager.AddTextParameter("random_state \t    | ", "random_state", "int seed, RandomState instance, or None (default)\nThe seed of the pseudo random number generator to use when shuffling the data.", GH_ParamAccess.item, "None");
            pManager.AddTextParameter("solver \t    | ", "solver", "{‘newton-cg’, ‘lbfgs’, ‘liblinear’}\nAlgorithm to use in the optimization problem.", GH_ParamAccess.item, "liblinear");
            pManager.AddNumberParameter("tol \t    | ", "tol", "float, optional\nTolerance for stopping criteria.", GH_ParamAccess.item, 0.001);
            pManager.AddTextParameter("multi_class \t    | ", "multi_class", "str, {‘ovr’, ‘multinomial’}\nMulticlass option can be either ‘ovr’ or ‘multinomial’. If the option chosen is ‘ovr’, then a binary problem is fit for each label. Else the loss minimised is the multinomial loss fit across the entire probability distribution. Works only for the ‘lbfgs’ solver.", GH_ParamAccess.item, "ovr");
            pManager.AddIntegerParameter("verbose \t    | ", "verbose", "int\nFor the liblinear and lbfgs solvers set verbose to any positive number for verbosity.", GH_ParamAccess.item,0);


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);

            pManager.AddTextParameter("score", "score", "result score", GH_ParamAccess.item);

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
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_03_LogisticRegression\\\\";

            string dataFile = null;
            string targetFile = null;
            string resultFile = "result.txt";
            string workingDir = null;
            bool fit = false;
            string Logs = ""; 

            //Optional Data: 
                string penalty = "";
                bool dual = false;
                double C = 1.0;                
                bool fit_intercept = true;
                double intercept_scaling = 1.0;
                string class_weight = "None";
                int max_iter = 100;
                string random_state = "None" ;
                string solver = "liblinear" ;
                double tol = 0.001;
                string multi_class = "ovr";
                int verbose = 0;


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

            // Optionaldata:| penalty   |dual       |C          |fit_intercept  |intercept_scaling  |class_weight   |max_iter   |random_state   |solver     |tol    |multi_class    |verbose
            // indeces:     |5          |6          |7          |8              |9                  |10             |11         |12             |13         |14     |15             |16
            try
            {
                DA.GetData(5, ref penalty);
                DA.GetData(6, ref dual);
                DA.GetData(7, ref C);
                DA.GetData(8, ref fit_intercept);
                DA.GetData(9, ref intercept_scaling);
                DA.GetData(10, ref class_weight);
                DA.GetData(11, ref max_iter);
                DA.GetData(12, ref random_state);
                DA.GetData(13, ref solver);
                DA.GetData(14, ref tol);
                DA.GetData(15, ref multi_class);
                DA.GetData(16, ref verbose);
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
                // Put inputData in one list of strings. 
            List<string> dataInput = new List<string>(new string[] {
                @"'"+penalty+@"'",
                dual==true?"True":"False",
                C.ToString(),
                fit_intercept==true?"True":"False",
                intercept_scaling.ToString(),
                class_weight,
                max_iter.ToString(),
                random_state,
                @"'"+solver+@"'",
                tol.ToString(),
                @"'"+multi_class+@"'",
                verbose.ToString()
            });

                // 01 Specify the working directory at which pythonFile exists, note that this Python file should run separately using Proccess2 (see 5). 
                process2.StartInfo.WorkingDirectory = workingDir;

                // 02 Initiate helperFunctions
                HelperFunction helpFunctions = new HelperFunction();

                // 03 Convert data from grasshopper syntax to python NumPy like array. 
                string newString = helpFunctions.dataInput2Python(workingDir, predicTData);

                // 04 Write the Python file in the working directory 
                helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._03_LogisticRegression, dataInput);

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
                    XmlNode coeff_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                    XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");
                    XmlNode n_inter_node = doc.DocumentElement.SelectSingleNode("/result/n_inter");

                    string res = res_node.InnerText;
                    string score = score_node.InnerText;
                    string coeff = coeff_node.InnerText;
                    string intercept = intercept_node.InnerText;
                    string n_iterations = n_inter_node.InnerText;


                    //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                    res = res.Replace("[", "").Replace("]", "");
                    DA.SetData(1, res);
                    DA.SetData(2, score);
                    DA.SetData(3, coeff);
                    DA.SetData(4, intercept);
                    DA.SetData(5, n_iterations);

                    long ticks1 = DateTime.Now.Ticks;
                    long timeElaspsed = (ticks1 - ticks0)/10000;
                    Logs += "Success !! in : " + timeElaspsed + " Milli Seconds\n ";

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
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return ANT.Resources.allResources._03_icon_LogisticRegression;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{53438f3b-2c31-4477-92e6-526a82c7dbd0}"); }
        }
    }
}