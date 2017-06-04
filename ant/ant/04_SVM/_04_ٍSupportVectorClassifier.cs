using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using ANT.Resources;
using System.Xml;



namespace ANT._04_SVM
{
    public class _04_SupportVectorClassifier : GH_Component
    {
        System.Diagnostics.Process process2 = new System.Diagnostics.Process();

        bool processhasexit = false;
        XmlDocument doc = new XmlDocument();
        /// <summary>
        /// Initializes a new instance of the _04_SupportVectorClassifier class.
        /// </summary>
        public _04_SupportVectorClassifier()
            : base("_04_SupportVectorClassifier", "SVC",
                "Support vector machines (SVMs) are a set of supervised learning methods used for classification, regression and outliers detection.",
                "ANT", "4|SVM")
        {
            this.Message = "sklearn.svm: Support Vector Machines \n C-Support Vector Classification.";

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
            /*  ##workingDir##
                ##logFile##
                ##resFile##
                ##dataFile##
                ##targetFile##
                ##predictData##
             */
            pManager.AddTextParameter("_Data   | ", "Dt", "source Data to be trained", GH_ParamAccess.item);                                        //0
            pManager.AddTextParameter("_Target   | ", "Tgt", "The target of the data ... ", GH_ParamAccess.item);                                   //1
            pManager.AddTextParameter("_workingFolder   | ", "Fldr", "Please specify the folder that you are working on .. ", GH_ParamAccess.item); //2
            pManager.AddGenericParameter("Predict    |", "predict2", "none", GH_ParamAccess.list);                                                  //3
            pManager.AddBooleanParameter(" _Fit?   | ", "Fit", "Start fitting data ? ", GH_ParamAccess.item, false);                                //4

            pManager.AddNumberParameter("C   | ", "C", "float, optional (default=1.0) \n Penalty parameter C of the error term.", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("kernel   | ", "krnl", "string, optional (default=’rbf’) \npecifies the kernel type to be used in the algorithm. It must be one of ‘linear’, ‘poly’, ‘rbf’, ‘sigmoid’, ‘precomputed’ or a callable. If none is given, ‘rbf’ will be used. If a callable is given it is used to pre-compute the kernel matrix from data matrices; that matrix should be an array of shape (n_samples, n_samples).", GH_ParamAccess.item, "rbf");
            pManager.AddNumberParameter("degree   | ", "deg", " int, optional (default=3) \n Degree of the polynomial kernel function (‘poly’). Ignored by all other kernels.", GH_ParamAccess.item, 3);
            pManager.AddTextParameter("gamma   | ", "g", " float, optional (default=’auto’) \nKernel coefficient for ‘rbf’, ‘poly’ and ‘sigmoid’. If gamma is ‘auto’ then 1/n_features will be used instead.", GH_ParamAccess.item, "auto");
            pManager.AddNumberParameter("coef0   | ", "coef0", "float, optional (default=0.0) \n Independent term in kernel function. It is only significant in ‘poly’ and ‘sigmoid’.", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("probability   | ", "prob", "boolean, optional (default=False)\n Whether to enable probability estimates. This must be enabled prior to calling fit, and will slow down that method.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("shrinking   | ", "shrink", " boolean, optional (default=True)\nWhether to use the shrinking heuristic.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("tolerance   | ", "tol", "tol : float, optional (default=1e-3)\nTolerance for stopping criterion.", GH_ParamAccess.item, 0.001);
            pManager.AddNumberParameter("cache_size   | ", "cshsize", "float, optional\nSpecify the size of the kernel cache (in MB).", GH_ParamAccess.item, 20);
            pManager.AddBooleanParameter("verbose   | ", "verbos", "bool, default: False\nEnable verbose output. Note that this setting takes advantage of a per-process runtime setting in libsvm that, if enabled, may not work properly in a multithreaded context.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("max_iter   | ", "maxiter", "int, optional (default=-1)\nHard limit on iterations within solver, or -1 for no limit.", GH_ParamAccess.item, -1);
            pManager.AddTextParameter("decision_function_shape   | ", "desFuncShp", "‘ovo’, ‘ovr’ or None, default=None\nWhether to return a one-vs-rest (‘ovr’) decision function of shape (n_samples, n_classes) as all other classifiers, or the original one-vs-one (‘ovo’) decision function of libsvm which has shape (n_samples, n_classes * (n_classes - 1) / 2). The default of None will currently behave as ‘ovo’ for backward compatibility and raise a deprecation warning, but will change ‘ovr’ in 0.19.", GH_ParamAccess.item, "None");
            pManager.AddNumberParameter("random_state   | ", "randSt", "int seed, RandomState instance, or None (default)\nThe seed of the pseudo random number generator to use when shuffling the data for probability estimation", GH_ParamAccess.item, 0);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter(" |    Log", " |    Log", "data log", GH_ParamAccess.item);
            pManager.AddTextParameter(" |    Result", "  |    Result", "Fitting result", GH_ParamAccess.item);

            pManager.AddTextParameter("score", "score", "result score", GH_ParamAccess.item);

            pManager.AddTextParameter(" |   support_ ", "support_ ", "array-like, shape = [n_SV]\nIndices of support vectors.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |   support_vectors_", "support_vectors_", "array-like, shape = [n_SV, n_features]\nSupport vectors.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |   n_support_", "n_support_", "array-like, dtype=int32, shape = [n_class]\nNumber of support vectors for each class.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |   dual_coef_", "dual_coef_", "array, shape = [n_class-1, n_SV]\nCoefficients of the support vector in the decision function. For multiclass, coefficient for all 1-vs-1 classifiers. The layout of the coefficients in the multiclass case is somewhat non-trivial. See the section about multi-class classification in the SVM section of the User Guide for details.", GH_ParamAccess.item);
            pManager.AddTextParameter(" |   coef_", "coef_", "array, shape = [n_class-1, n_features]\nWeights assigned to the features (coefficients in the primal problem). This is only available in the case of a linear kernel.\n coef_ is a readonly property derived from dual_coef_ and support_vectors_", GH_ParamAccess.item);
            pManager.AddTextParameter(" |   intercept_", "intercept_", "array, shape = [n_class * (n_class-1) / 2]\nConstants in decision function.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            /*  ##workingDir##
                ##logFile##
                ##resFile##
                ##dataFile##
                ##targetFile##
                ##predictData##
             */


            string defaultDir = "C:\\\\ant\\\\_03_LinearModel\\\\SVM\\\\";

            bool fit = false;
            string workingDir = null;
            string resultFile = "result.txt";
            string dataFile = null;
            string targetFile = null;
            string Logs = ""; 

            List<double> predicTData = new List<double>();

            /*IGH_DataAccess df = null;
            df.GetData(0, ref logFile);
            */
            if (!DA.GetData(0, ref dataFile)) { return; }
            if (dataFile == null) { return; }
            if (!DA.GetData(1, ref targetFile)) { return; }
            if (targetFile == null) { return; }
            if (!DA.GetData(2, ref workingDir)) { return; }
            if (workingDir == null) { return; }
            if (!DA.GetDataList(3, predicTData)) { return; }
            if (!DA.GetData(4, ref fit)) { return; }
            if (!fit) { return; }


            long ticks0 = DateTime.Now.Ticks;

            process2.StartInfo.WorkingDirectory = workingDir;


            HelperFunction helpFunctions = new HelperFunction();

            string newString = helpFunctions.dataInput2Python(workingDir, predicTData);
            List<string> dataInput = new List<string>(new string[] {"dd", "dd"});

            helpFunctions.PythonFile(defaultDir, dataFile, targetFile, workingDir, resultFile, newString, "True", workingDir + "logFile.txt", allResources.supportVectorClassifier, dataInput);

            // start running the simulation ...

                process2.Start();

                while (!processhasexit)
                {
                    
                }
                try
                {
                    doc.Load(workingDir + "res.xml");
                    
                }
                catch (Exception e)
                {
                    Logs += "001 " + e +"\n";
                }
                try{

                    //TODO : add all the output variables here
                    XmlNode res_node = doc.DocumentElement.SelectSingleNode("/result/prediction");
                    XmlNode score_node = doc.DocumentElement.SelectSingleNode("/result/score");
                    
                    XmlNode support_node = doc.DocumentElement.SelectSingleNode("/result/support");
                    XmlNode support_vectors_node = doc.DocumentElement.SelectSingleNode("/result/support_vectors");
                    XmlNode n_support_node = doc.DocumentElement.SelectSingleNode("/result/n_support");
                    XmlNode dual_coef_node = doc.DocumentElement.SelectSingleNode("/result/dual_coef");
                    
                    XmlNode coeff_node = doc.DocumentElement.SelectSingleNode("/result/coeff");
                    XmlNode intercept_node = doc.DocumentElement.SelectSingleNode("/result/intercept");


                    string res = res_node.InnerText;
                    string score = score_node.InnerText;
                    /*
                    string support = support_node.InnerText;
                    string support_vectors = support_vectors_node.InnerText;
                    string n_support = n_support_node.InnerText;
                    string dual_coef = dual_coef_node.InnerText;
                   */
                    string coeff = coeff_node.InnerText;
                    string intercept = intercept_node.InnerText;

                    res = res.Replace("[", "").Replace("]", "");
                    DA.SetData(1, res);
                    DA.SetData(2, score);
                    /*
                    DA.SetData(3, support);
                    DA.SetData(4, support_vectors);
                    DA.SetData(5, n_support);
                    DA.SetData(6, dual_coef);
                    */
                    DA.SetData(7, coeff);
                    DA.SetData(8, intercept);

                    long ticks1 = DateTime.Now.Ticks;
                    long timeElaspsed = (ticks1 - ticks0) / 10000;
                    Logs += "Success !! in : " + timeElaspsed + " Milli Seconds\n ";

                }
                catch (Exception e)
                {
                    Logs += "0001000 " +  e + "\n";
                }
                DA.SetData(0, Logs);
                processhasexit = false;
                //int time = process2.ExitTime.Millisecond;
                //DA.SetData(0, newString);

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
                return Resources.allResources._03_SupportVectorClassify;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{6e9af7ab-d28d-4705-8df6-a1e0a6cef5b4}"); }
        }
    }
}