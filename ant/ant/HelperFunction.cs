using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ANT
{
    class HelperFunction
    {
        /// <summary>
        /// This function writes down python file and iniate all variables.
        /// </summary>
        /// <param name="defaultDir">The default directory i.e. C:\ant\_02_datasets</param>
        /// <param name="dataFile">data file that contains all the training instances note that this file is a python dumps file</param>
        /// <param name="targetFile">target file that contains all the targets of the data note that this file is a python dumps file</param>
        /// <param name="workingDir">the directory at which calculations are performed.</param>
        /// <param name="resultFile">file in which all results are saved</param>
        /// <param name="predictData">data to be fitted</param>
        /// <param name="DumpToggle"></param>
        /// <param name="logFile">logfile to report errors... </param>
        /// <param name="resource">the main resource file that contains all the python syntax but variables are ommitted </param>
        /// <param name="inputdata">all the optional data for the component ordered as mentioned at scikit-learn module.</param>
        /// <returns>the full path to the python file to be run ... </returns>
        public string PythonFile(string defaultDir, string dataFile, string targetFile, string workingDir, string resultFile, string predictData, string DumpToggle, string logFile,string resource, List<string> inputdata)
        {
            /*  ##workingDir##  ---
                ##logFile##     
                ##resFile##
                ##dataFile##
                ##targetFile##
                ##predictData##
             * ##resFile##
             */

            dataFile = dataFile.Replace("\\", "\\\\");
            targetFile = targetFile.Replace("\\", "\\\\");
            workingDir = workingDir.Replace("\\", "\\\\");
            logFile = logFile.Replace("\\", "\\\\");

            string thisFile = resource; // System.IO.File.ReadAllText(defaultDir + "working");

            if (workingDir == null)
            {
                thisFile = thisFile.Replace("##workingDir##", defaultDir);
            }
            else
            {
                    int i = 0;
                    foreach (var x in inputdata)
                    {
                        thisFile = thisFile.Replace("##data"+i.ToString()+"##", x.ToString());
                        
                        i++;
                    }

                thisFile = thisFile.Replace("##workingDir##", workingDir);
                thisFile = thisFile.Replace("##dataFile##", dataFile);
                thisFile = thisFile.Replace("##targetFile##", targetFile);
                thisFile = thisFile.Replace("##DumpToggle##", DumpToggle);
                thisFile = thisFile.Replace("##predictData##", predictData);
                thisFile = thisFile.Replace("##logFile##", logFile);
                thisFile = thisFile.Replace("##resFile##", resultFile);

            }
            if (!System.IO.Directory.Exists(workingDir))
            {
                System.IO.Directory.CreateDirectory(workingDir);
            }
            tryexcept(workingDir + "doAllWork.py", predictData);
            System.IO.File.AppendAllText(workingDir + "doAllWork.py", thisFile,Encoding.UTF8);

            return workingDir + "doAllWork.py";
        }
        /// <summary>
        /// This method initates the predictdata to python 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public void tryexcept(string fileName, string data)
        {
            System.IO.File.WriteAllText(fileName, @"# -*- coding: utf-8 -*-\n");
            System.IO.File.WriteAllText(fileName, "error = \"\"\n");
            System.IO.File.AppendAllText(fileName, "try:\n");
            System.IO.File.AppendAllText(fileName, "    exec('PredictData=" + data + "')\n");
            System.IO.File.AppendAllText(fileName, "except Exception as e:\n");
            System.IO.File.AppendAllText(fileName, "    error += str(e)\n");

        }

        public string dataInput2Python(string workingDir, List<double> predicTData)
        {

            string workingDir2 = workingDir;
            workingDir = workingDir.Replace("\\", "\\\\");

            string newString = "[";
            for (int i = 0; i < predicTData.Count; i++)
            {
                if (i == predicTData.Count - 1)
                    newString += predicTData[i].ToString();
                else
                    newString += predicTData[i].ToString() + ", ";
            }
            newString += "]";

            return newString;
        }

        public string[] SetDefulatValues(Object[,] vars)
        {
            string[] temp = new string[vars.Length];
            for (int i = 0; i < vars.Length; i++)
            {
                if ((string)vars[i, 1] == "b")
                {
                    temp[i] = (Convert.ToBoolean(vars[i, 2]) == true ? "True" : "False");
                }
                else
                {
                    temp[i] = (vars[i, 2].ToString());
                }
            }
            return temp;
        }

    }

}
