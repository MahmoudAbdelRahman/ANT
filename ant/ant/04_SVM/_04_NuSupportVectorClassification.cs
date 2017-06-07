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
    public class _04_NuSupportVectorClassification : GH_Component
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
        /// Initializes a new instance of the _04_NuSupportVectorClassification class.
        /// </summary>
        public _04_NuSupportVectorClassification()
            : base("_04_NuSupportVectorClassification", "nuSVC",
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
                "nu : float, optional (default=0.5)\nAn upper bound on the fraction of training errors and a lower bound of the fraction of support vectors. Should be in the interval (0, 1].",
                "kernel : string, optional (default=’rbf’) \nSpecifies the kernel type to be used in the algorithm. It must be one of ‘linear’, ‘poly’, ‘rbf’, ‘sigmoid’, ‘precomputed’ or a callable. If none is given, ‘rbf’ will be used. If a callable is given it is used to precompute the kernel matrix.",
                "degree : int, optional (default=3)\nDegree of kernel function is significant only in poly, rbf, sigmoid.",
                "gamma : float, optional (default=0.0)\nKernel coefficient for rbf and poly, if gamma is 0.0 then 1/n_features will be taken.",
                "coef0 : float, optional (default=0.0)\nIndependent term in kernel function. It is only significant in poly/sigmoid.",
                "probability: boolean, optional (default=False) :\nWhether to enable probability estimates. This must be enabled prior to calling fit, and will slow down that method.",
                "shrinking: boolean, optional (default=True) :\nWhether to use the shrinking heuristic.",
                "tol : float, optional (default=1e-3)\nTolerance for stopping criterion.",
                "cache_size : float, optional\nSpecify the size of the kernel cache (in MB).",
                "verbose : bool, default: False\nEnable verbose output. Note that this setting takes advantage of a per-process runtime setting in libsvm that, if enabled, may not work properly in a multithreaded context.",
                "max_iter : int, optional (default=-1)\nHard limit on iterations within solver, or -1 for no limit.",
                "random_state : int seed, RandomState instance, or -1 = None (default)\nThe seed of the pseudo random number generator to use when shuffling the data for probability estimation."
            };

            Invars = new Object[,] {
                {"nu"       , "f"      , 0.5},
                {"kernel"   , "s"  , "rbf"},
                {"degree"   , "i"  , 3},
                {"gamma"    , "f"   , 0.0},
                {"coef0"    , "f"   , 0.0},
                {"probability", "b", false},
                {"shrinking", "b", true},
                {"tol"      , "f"     , 0.0001},
                {"cache_size", "f", 200},
                {"verbose"  , "b" , false},
                {"max_iter" , "i", -1},
                {"random_state", "s", "None"}
            };

            initInputParams(Indocs, Invars, pManager);

        }


        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            Outvars = new string[] {"support_", "suuport_vectors", "n_supports_", "dual_coef_", "coef_", "intercept"};
            OutDocs = new string[] {
                "array-like, shape = [n_SV]\nIndices of support vectors",
                "support_vectors_ : array-like, shape = [n_SV, n_features]\nSupport vectors.",
                "n_support_ : array-like, dtype=int32, shape = [n_class]\nNumber of support vector for each class.",
                "dual_coef_ : array, shape = [n_class-1, n_SV]\nCoefficients of the support vector in the decision function. For multiclass, coefficient for all 1-vs-1 classifiers. The layout of the coefficients in the multiclass case is somewhat non-trivial. See the section about multi-class classification in the SVM section of the User Guide for details.",
                "coef_ : array, shape = [n_class-1, n_features]\nWeights assigned to the features (coefficients in the primal problem). This is only available in the case of linear kernel.coef_ is readonly property derived from dual_coef_ and support_vectors_.",
                "intercept_ : array, shape = [n_class * (n_class-1) / 2]\nConstants in decision function."
            };
            try
            {
                initOutputParams(OutDocs, Outvars, pManager);
            }catch (Exception e)
                {
                    this.Message = e.ToString();
                }
                
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

                double nu= 0.5;
                string kernel = "rbf";
                int degree = 3;
                double gamma = 0.0;
                double coef0=0.0;
                bool probability = false;
                bool shrinking= true;
                double tol = 0.0001;
                double cache_size = 200;
                bool verbose = false;
                int max_iter = -1;
                string random_state= "None";



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
                DA.GetData(5, ref nu);              //0
                DA.GetData(6, ref kernel);          //1
                DA.GetData(7, ref degree);          //2
                DA.GetData(8, ref gamma);           //3
                DA.GetData(9, ref coef0);           //4
                DA.GetData(10, ref probability);    //5
                DA.GetData(11, ref shrinking);      //6
                DA.GetData(12, ref tol);            //7
                DA.GetData(13, ref cache_size);     //8
                DA.GetData(14, ref verbose);        //9
                DA.GetData(15, ref max_iter);       //10
                DA.GetData(16, ref random_state);   //11
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
                nu.ToString(),
                @"'"+kernel+@"'",
                degree.ToString(),
                gamma.ToString(),
                coef0.ToString(),
                probability==true?"True":"False",
                shrinking==true?"True":"False",
                tol.ToString(),
                cache_size.ToString(),
                verbose==true?"True":"False",
                max_iter.ToString(),
                "'"+random_state+"'",
            });

                // 03 Convert data from grasshopper syntax to python NumPy like array. 
                string newString = helpFunctions.dataInput2Python(workingDir, predicTData);

                // 04 Write the Python file in the working directory 
                helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources._04_NuSupportVectorClassification, dataInput);

                
            }catch (Exception e)
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
                XmlNode support_node = doc.DocumentElement.SelectSingleNode("/result/support");
                XmlNode support_vectors_node = doc.DocumentElement.SelectSingleNode("/result/support_vectors");
                XmlNode n_support_node = doc.DocumentElement.SelectSingleNode("/result/n_support");
                XmlNode dual_coef_node = doc.DocumentElement.SelectSingleNode("/result/dual_coef");
                XmlNode coeff_node = doc.DocumentElement.SelectSingleNode("/result/coef");
                XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");


                string res = res_node.InnerText;
                string score = score_node.InnerText;
                string support = support_node.InnerText;
                string support_vectors = support_vectors_node.InnerText;
                string n_support = n_support_node.InnerText;
                string dual_coef = dual_coef_node.InnerText;
                string coeff = coeff_node.InnerText;
                string intercept = intercept_node.InnerText;



                //string res = System.IO.File.ReadAllText(workingDir + "result.txt");
                res = res.Replace("[", "").Replace("]", "");
                DA.SetData(1, res);
                DA.SetData(2, score);
                DA.SetData(3, support);
                DA.SetData(4, support_vectors);
                DA.SetData(5, n_support);
                DA.SetData(6, dual_coef);
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

            for (int i = 0; i<vars.Length; i++)
            {
                pManager.AddTextParameter(" |    "+(string)vars[i], (string)vars[i], (string)docs[i], GH_ParamAccess.item);
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
                return allResources._04_NuSVC;
            }
        }



        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c5671663-637e-45e4-b120-a82ff1c11188}"); }
        }
    }
}