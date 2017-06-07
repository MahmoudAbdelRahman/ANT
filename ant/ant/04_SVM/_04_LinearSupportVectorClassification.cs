using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Xml;
using System.Diagnostics;
using ANT.Resources;

namespace ANT._04_SVM
{
    public class _04_LinearSupportVectorClassification : GH_Component
    {
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
        /// Initializes a new instance of the _04_LinearSupportVectorClassification class.
        /// </summary>
        public _04_LinearSupportVectorClassification()
            : base("_04_LinearSupportVectorClassification", "",
                "",
                "ANT", "4|SVM")
        {
            this.Message = "";
            this.Description = "Similar to SVC but uses a parameter to control the number of support vectors.\nThe implementation is based on libsvm.";

            //initiating the process which runs the Python file. 
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

            Indocs = new string[]{
                "C : float, optional (default=1.0)\nPenalty parameter C of the error term."
                ,"loss : string, ‘hinge’ or ‘squared_hinge’ (default=’squared_hinge’)\nSpecifies the loss function. ‘hinge’ is the standard SVM loss (used e.g. by the SVC class) while ‘squared_hinge’ is the square of the hinge loss."
                ,"penalty : string, ‘l1’ or ‘l2’ (default=’l2’)\nSpecifies the norm used in the penalization. The ‘l2’ penalty is the standard used in SVC. The ‘l1’ leads to coef_ vectors that are sparse."
                ,"dual : bool, (default=True)\nSelect the algorithm to either solve the dual or primal optimization problem. Prefer dual=False when n_samples > n_features."
                ,"tol : float, optional (default=1e-4)\nTolerance for stopping criteria."
                ,"multi_class: string, ‘ovr’ or ‘crammer_singer’ (default=’ovr’) :\nDetermines the multi-class strategy if y contains more than two classes. ovr trains n_classes one-vs-rest classifiers, while crammer_singer optimizes a joint objective over all classes. While crammer_singer is interesting from an theoretical perspective as it is consistent it is seldom used in practice and rarely leads to better accuracy and is more expensive to compute. If crammer_singer is chosen, the options loss, penalty and dual will be ignored."
                ,"fit_intercept : boolean, optional (default=True)\nWhether to calculate the intercept for this model. If set to false, no intercept will be used in calculations (e.g. data is expected to be already centered)."
                ,"intercept_scaling : float, optional (default=1)\nWhen self.fit_intercept is True, instance vector x becomes [x, self.intercept_scaling], i.e. a “synthetic” feature with constant value equals to intercept_scaling is appended to the instance vector. The intercept becomes intercept_scaling * synthetic feature weight Note! the synthetic feature weight is subject to l1/l2 regularization as all other features. To lessen the effect of regularization on synthetic feature weight (and therefore on the intercept) intercept_scaling has to be increased"
                ,"class_weight : {dict, ‘auto’}, optional\nSet the parameter C of class i to class_weight[i]*C for SVC. If not given, all classes are supposed to have weight one. The ‘auto’ mode uses the values of y to automatically adjust weights inversely proportional to class frequencies."
                ,"verbose : int, (default=0)\nEnable verbose output. Note that this setting takes advantage of a per-process runtime setting in liblinear that, if enabled, may not work properly in a multithreaded context."
                ,"random_state : int seed, RandomState instance, or None (default=None)\nThe seed of the pseudo random number generator to use when shuffling the data."
                ,"max_iter : int, (default=1000)\nThe maximum number of iterations to be run."
            };

            Invars = new Object[,] {
                {"C", "f",1.0 },
                {"loss", "s" ,"squared_hinge" },
                {"penalty","s", "l1" },
                {"dual", "b", true},
                {"tol", "f", 0.00001},
                {"multi_class", "s" , "ovr" },
                {"fit_intercept", "b", true},
                {"intercept_scaling","f", 1.0},
                {"class_weight","s", "auto"},
                {"verbose", "i", 0},
                {"random_state","s", "None"},
                {"max_iter", "i", 1000}};

            initInputParams(Indocs, Invars, pManager);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            Outvars = new string[] { "coef_", "intercept_" };
             OutDocs = new string[] {
                 "coef_ : array, shape = [n_features] if n_classes == 2 else [n_classes, n_features]\nWeights assigned to the features (coefficients in the primal problem). This is only available in the case of linear kernel.\ncoef_ is a readonly property derived from raw_coef_ that follows the internal memory layout of liblinear.",
                 "intercept_ : array, shape = [1] if n_classes == 2 else [n_classes]\nConstants in decision function."};
             initOutputParams(OutDocs, Outvars, pManager);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\_04_SVM\\\\";

            string dataFile = null;
            string targetFile = null;
            string resultFile = "result.txt";
            string workingDir = null;
            bool fit = false;
            string Logs = "";

            //Optional Data:

            double C= 1.0;
            string loss= "squared_hinge";
            string penalty= "l1";
            bool dual = true;
            double tol= 0.00001;
            string multi_class= "ovr" ;
            bool fit_intercept= true;
            double intercept_scaling= 1.0;
            string class_weight= "auto";
            int verbose= 0;
            string random_state="None";
            int max_iter= 1000;

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
                DA.GetData(5, ref C);                   //0
                DA.GetData(6, ref loss);                //1
                DA.GetData(7, ref penalty);             //2
                DA.GetData(8, ref dual);                //3
                DA.GetData(9, ref tol);                 //4
                DA.GetData(10, ref multi_class);        //5
                DA.GetData(11, ref fit_intercept);      //6
                DA.GetData(12, ref intercept_scaling);  //7
                DA.GetData(13, ref class_weight);       //8
                DA.GetData(14, ref verbose);            //9
                DA.GetData(15, ref random_state);       //10
                DA.GetData(16, ref max_iter);           //11
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
                @"'"+penalty+@"'",
                dual==true?"True":"False",
                tol.ToString(),
                @"'"+multi_class+@"'",
                fit_intercept==true?"True":"False",
                intercept_scaling.ToString(),
                @"'"+class_weight+@"'",
                verbose.ToString(),
                @"'"+random_state+@"'",
                max_iter.ToString(),
            });

                // 03 Convert data from grasshopper syntax to python NumPy like array. 
                string newString = helpFunctions.dataInput2Python(workingDir, predicTData);

                // 04 Write the Python file in the working directory 
                helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._04_LinearSupportVectorClassifier, dataInput);


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
                XmlNode coeff_node = doc.DocumentElement.SelectSingleNode("/result/coef");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");


                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string coeff = coeff_node.InnerText;
                string intercept = intercept_node.InnerText;

                //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(7, coeff);
                DA.SetData(8, intercept);

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
                return allResources.LinearSVC;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{13b99642-de82-4e5f-bd67-46590bb63d1f}"); }
        }
    }
}