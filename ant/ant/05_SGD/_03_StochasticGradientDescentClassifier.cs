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

namespace ANT._03_LinearModels
{

    public class _05_StochasticGradientDescentClassifier : GH_Component
    {
        private bool processhasexit = false;
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();
        XmlDocument doc = new XmlDocument();
        /// <summary>
        /// Initializes a new instance of the _03_StochasticGradientDescentClassifier class.
        /// </summary>
        public _05_StochasticGradientDescentClassifier()
            : base("_05_StochasticGradientDescentClassifier", "SGDClassifier",
                "",
                "ANT", "5|SGD")
        {
            this.Message = "Stochastic Gradient descent Classifier";


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


            //   loss
            string[] docs = new string[]{
                "str, ‘hinge’, ‘log’, ‘modified_huber’, ‘squared_hinge’, ‘perceptron’, or a regression loss: ‘squared_loss’, ‘huber’, ‘epsilon_insensitive’, or ‘squared_epsilon_insensitive’\n The loss function to be used. Defaults to ‘hinge’, which gives a linear SVM. The ‘log’ loss gives logistic regression, a probabilistic classifier. ‘modified_huber’ is another smooth loss that brings tolerance to outliers as well as probability estimates. ‘squared_hinge’ is like hinge but is quadratically penalized. ‘perceptron’ is the linear loss used by the perceptron algorithm. The other losses are designed for regression but can be useful in classification as well; see SGDRegressor for a description.",
                "str, ‘none’, ‘l2’, ‘l1’, or ‘elasticnet’\nThe penalty (aka regularization term) to be used. Defaults to ‘l2’ which is the standard regularizer for linear SVM models. ‘l1’ and ‘elasticnet’ might bring sparsity to the model (feature selection) not achievable with ‘l2’.",
                "float\nconstant that multiplies the regularization term. Defaults to 0.0001",
                "float\nThe Elastic Net mixing parameter, with 0 <= l1_ratio <= 1. l1_ratio=0 corresponds to L2 penalty, l1_ratio=1 to L1. Defaults to 0.15.",
                "bool\nWhether the intercept should be estimated or not. If False, the data is assumed to be already centered. Defaults to True.",
                "int, optional\nThe number of passes over the training data (aka epochs). The number of iterations is set to 1 if using partial_fit. Defaults to 5.",
                "bool, optional\nWhether or not the training data should be shuffled after each epoch. Defaults to True.",
                "int seed, RandomState instance, or None (default)\nThe seed of the pseudo random number generator to use when shuffling the data.",
                "integer, optional\nThe verbosity level",
                "float\nEpsilon in the epsilon-insensitive loss functions; only if loss is ‘huber’, ‘epsilon_insensitive’, or ‘squared_epsilon_insensitive’. For ‘huber’, determines the threshold at which it becomes less important to get the prediction exactly right. For epsilon-insensitive, any differences between the current prediction and the correct label are ignored if they are less than this threshold.",
                "integer, optional\nThe number of CPUs to use to do the OVA (One Versus All, for multi-class problems) computation. -1 means ‘all CPUs’. Defaults to 1.",
                "string, optional\nThe learning rate schedule: constant: eta = eta0 optimal: eta = 1.0 / (t + t0) [default] invscaling: eta = eta0 / pow(t, power_t) where t0 is chosen by a heuristic proposed by Leon Bottou.",
                "double\nThe initial learning rate for the ‘constant’ or ‘invscaling’ schedules. The default value is 0.0 as eta0 is not used by the default schedule ‘optimal’.",
                "double\nThe exponent for inverse scaling learning rate [default 0.5].",
                "dict, {class_label: weight} or “auto” or None, optional\nPreset for the class_weight fit parameter.\nWeights associated with classes. If not given, all classes are supposed to have weight one.\nThe “auto” mode uses the values of y to automatically adjust weights inversely proportional to class frequencies.",
                "bool, optional\nWhen set to True, reuse the solution of the previous call to fit as initialization, otherwise, just erase the previous solution.",
                "bool or int, optional\nWhen set to True, computes the averaged SGD weights and stores the result in the coef_ attribute. If set to an int greater than 1, averaging will begin once the total number of samples seen reaches average. So average=10 will begin averaging after seeing 10 samples."
            };

            /*loss
            penalty
            alpha
            l1_ratio
            fit_intercept
            n_iter
            shuffle
            random_state
            verbose
            epsilon
            n_jobs
            learning_rate
            eta0
            power_t
            class_weight
            warm_start
            average*/
             

            pManager.AddTextParameter("loss \t    | ", "loss"                   , docs[0], GH_ParamAccess.item, "hinge");
            pManager.AddTextParameter("penalty \t    | ", "penalty"             , docs[1], GH_ParamAccess.item, "l2");
            pManager.AddNumberParameter("alpha \t    | ", "alpha"               , docs[2], GH_ParamAccess.item, 0.0001);
            pManager.AddNumberParameter("l1_ratio \t    | ", "l1_ratio"         , docs[3], GH_ParamAccess.item, 0.15);
            pManager.AddBooleanParameter("fit_intercept \t    | ", "fit_intercept", docs[4], GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("n_iter \t    | ", "n_iter"            , docs[5], GH_ParamAccess.item, 5);
            pManager.AddBooleanParameter("shuffle \t    | ", "shuffle"          , docs[6], GH_ParamAccess.item, true);
            pManager.AddTextParameter("random_state \t    | ", "random_state", docs[7], GH_ParamAccess.item, "None");
            pManager.AddIntegerParameter("verbose \t    | ", "verbose"          , docs[8], GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("epsilon \t    | ", "epsilon"           , docs[9], GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("n_jobs \t    | ", "n_jobs"            , docs[10], GH_ParamAccess.item, 1);
            pManager.AddTextParameter("learning_rate \t    | ", "learning_rate" , docs[11], GH_ParamAccess.item, "optimal");
            pManager.AddNumberParameter("eta0 \t    | ", "eta0"                 , docs[12], GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("power_t \t    | ", "power_t"           , docs[13], GH_ParamAccess.item, 0.5);
            pManager.AddTextParameter("class_weight \t    | ", "class_weight"   , docs[14], GH_ParamAccess.item, "None");
            pManager.AddBooleanParameter("warm_start \t    | ", "warm_start"    , docs[15], GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("average \t    | ", "average"          , docs[16], GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
          
               //coef_ 
             string[] docs = new string[]{ "array, shape (1, n_features) if n_classes == 2 else (n_classes, n_features)\nWeights assigned to the features.",
               //intercept_ 
             " array, shape (1,) if n_classes == 2 else (n_classes,)\nConstants in decision function"};

            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);

            pManager.AddTextParameter("score", "score", "result score", GH_ParamAccess.item);

            pManager.AddTextParameter(" |    coef_", " |    coef_", docs[0], GH_ParamAccess.item);
            pManager.AddTextParameter(" |    intercept_", " |    intercept_", docs[1], GH_ParamAccess.item);

             
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


            string loss = "hinge";
            string penalty = "l2";
            double alpha = 0.0001;
            double l1_ratio = 0.015;
            bool fit_intercept = true;
            int n_iter = 5;
            bool shuffle = true;
            string random_state = "None";
            int verbose = 0;
            double epsilon = 0.1;
            int n_jobs = 1;
            string learning_rate = "optimal";
            double eta0 = 0.0;
            double power_t = 0.5;
            string class_weight = "None";
            bool warm_start = false;
            bool average = false;
            


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
                DA.GetData(5, ref loss);
                DA.GetData(6, ref penalty);
                DA.GetData(7, ref alpha);
                DA.GetData(8, ref l1_ratio);
                DA.GetData(9, ref fit_intercept);
                DA.GetData(10, ref n_iter);
                DA.GetData(11, ref shuffle);
                DA.GetData(12, ref random_state);
                DA.GetData(13, ref verbose);
                DA.GetData(14, ref epsilon);
                DA.GetData(15, ref n_jobs);
                DA.GetData(16, ref learning_rate);
                DA.GetData(17, ref eta0);
                DA.GetData(18, ref power_t);
                DA.GetData(19, ref class_weight);
                DA.GetData(20, ref warm_start);
                DA.GetData(21, ref average);
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
                @"'"+loss+@"'",
                @"'"+penalty+@"'",
                alpha.ToString(),
                l1_ratio.ToString(),
                fit_intercept==true?"True":"False",
                n_iter.ToString(),
                shuffle==true?"True":"False",
                "'"+random_state+"'",
                verbose.ToString(),
                epsilon.ToString(),
                n_jobs.ToString(),
                "'"+learning_rate+"'",
                eta0.ToString(),
                power_t.ToString(),
                "'"+class_weight+"'",
                warm_start==true?"True":"False",
                average == true?"True":"False"
            });

            // 01 Specify the working directory at which pythonFile exists, note that this Python file should run separately using Proccess2 (see 5). 
            process2.StartInfo.WorkingDirectory = workingDir;

            // 02 Initiate helperFunctions
            HelperFunction helpFunctions = new HelperFunction();

            // 03 Convert data from grasshopper syntax to python NumPy like array. 
            string newString = helpFunctions.dataInput2Python(workingDir, predicTData);

            // 04 Write the Python file in the working directory 
            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._05_SGDClassifier, dataInput);

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


                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string coeff = coeff_node.InnerText;
                string intercept = intercept_node.InnerText;



                //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(3, coeff);
                DA.SetData(4, intercept);


                long ticks1 = DateTime.Now.Ticks;
                double timeElaspsed = ((double)ticks1 - (double)ticks0) /10000000;
                Logs += "Success !! in : "+timeElaspsed+" Seconds\n ";

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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.allResources._03_icon_StochasticGradientDescentC1;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{a9507935-ff6b-4e80-bdfd-00bb2072e6ed}"); }
        }
    }
}