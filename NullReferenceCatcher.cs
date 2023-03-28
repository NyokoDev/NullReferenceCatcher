using System;
using System.IO;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using ColossalFramework.IO;



namespace NullReferenceCatcher
{
    // Make the class public so it can be loaded by Cities: Skylines
    public class NullReferenceCatcher : IUserMod
    {
        private bool _isErrorCaught;
        private string _errorMessage;
        private bool _showOptions;
        private string _logsDirectory;

        public void OnEnabled()
        {
            // Subscribe to the game's log message received event
            Application.logMessageReceived += OnLogMessageReceived;

            // Create the logs directory if it doesn't exist
            _logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order\\Cities_Skylines\\Addons\\Mods\\NullReferenceCatcher\\Logs");
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }
        }

        public void OnDisabled()
        {
            // Unsubscribe from the game's log message received event
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            // Check if the log message is an error or exception
            if (type == LogType.Error || type == LogType.Exception)
            {
                // Check if the error message contains "Object reference not set to an instance of an object"
                if (logString.Contains("Object reference not set to an instance of an object"))
                {
                    // Set the error message
                    _errorMessage = "NullReferenceException caught: " + logString;

                    // Log the error message to the game's output log
                    Debug.LogError(_errorMessage);

                    // Specify the path of the NRClogs.txt file
                    string nrcLogsFilePath = Path.Combine(_logsDirectory, "NRClogs.txt");

                    // Get the class name of the source of the exception
                    string className = "";
                    foreach (string line in stackTrace.Split('\n'))
                    {
                        if (line.Contains("at "))
                        {
                            string[] parts = line.Split('.');
                            className = parts[parts.Length - 2];
                            break;
                        }
                    }

                    // Write the error message and the full stack trace to the NRClogs.txt file
                    using (StreamWriter writer = new StreamWriter(nrcLogsFilePath, true))
                    {
                        writer.WriteLine("Date and Time: " + DateTime.Now);
                        writer.WriteLine(_errorMessage);
                        writer.WriteLine(stackTrace);
                        writer.WriteLine("Source of Exception: " + className);
                        writer.WriteLine("----------------------");
                    }

                    // Recommend sending the log file to a developer or support center
                    using (StreamWriter writer = new StreamWriter(nrcLogsFilePath, true))
                    {
                        writer.WriteLine("Recommendation: Please send this log file to a developer or support center for further investigation.");
                        writer.WriteLine("----------------------");
                    }


                    // Log the error message and stack trace to the output log file
                    string outputLogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam\\steamapps\\common\\Cities_Skylines\\Cities_Data\\output_log.txt");
                    using (StreamWriter writer = new StreamWriter(outputLogFilePath, true))
                    {
                        writer.WriteLine("Date and Time: " + DateTime.Now);
                        writer.WriteLine(_errorMessage);
                        writer.WriteLine(stackTrace);
                        writer.WriteLine("------Exception caught by NullReferenceCatcher------");
                    }

                    // Display the error message to the user
                    _isErrorCaught = true;
                }
            }
        }

        public void OnGUI()
        {
            if (_isErrorCaught)
            {
                // Display the error message to the user
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("NullReferenceCatcher caught an error", _errorMessage, true);

                // Reset the flag
                _isErrorCaught = false;
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("NullReferenceCatcher Options");

            // Add a button to trigger NullReferenceException
            group.AddButton("Trigger NullReferenceException (Experimental)", TriggerNullReferenceException);
            group.AddSpace(10);

            // Add a button to open the NRClogs.txt location folder
            group.AddButton("Open NRC Report Folder", OpenLogsFolder);
            group.AddSpace(20);

            group.AddButton("Open output_log.txt Folder", OpenOutputFolder);
            group.AddSpace(20);

            // Add a help button
            group.AddButton("Help", () =>
            {
                Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=2952968348");
            });
            group.AddSpace(20);

            // Add a button to generate NRC report
            group.AddButton("Generate NRC Report (WIP - Experimental)", GenerateNRCReport);
            group.AddSpace(20);

            
        }

        private static void GenerateNRCReport()
        {
            // Retrieve past errors and exceptions from the entire Cities Skylines simulation
            string report = "";
            var errorLogPath = Path.Combine(@"C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines\Cities_Data", "output_log.txt");

            try
            {
                if (File.Exists(errorLogPath))
                {
                    report = File.ReadAllText(errorLogPath);
                }
                else
                {
                    throw new Exception("output_log.txt not found");
                }
            }
            catch (Exception e)
            {
                // Log the exception message to NRClogs.txt
                string logPath = Path.Combine(DataLocation.localApplicationData, "Colossal Order", "Cities_Skylines", "Addons", "Mods", "NullReferenceCatcher", "Logs", "NRClogs.txt");
                File.AppendAllText(logPath, "Exception occurred while trying to read output_log.txt: " + e.Message + "\n");
            }


            // Save the report to a file
            string path = Path.Combine(DataLocation.localApplicationData, "Colossal Order", "Cities_Skylines", "Addons", "Mods", "NullReferenceCatcher", "Logs", "NRC Report.txt");
            File.WriteAllText(path, report);

            // Show a message box to inform the user that the report has been generated
            string message = "NRC Report generated successfully. Please find the report at:\n" + path;
            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("NRC Report", message, true);

        }

        private void OpenOutputFolder()
        {
            // Open the logs directory in file explorer
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam\\steamapps\\common\\Cities_Skylines\\Cities_Data");
            System.Diagnostics.Process.Start(logsDirectory);
        }


        private void OpenLogsFolder()
        {
            // Open the logs directory in file explorer
            string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order\\Cities_Skylines\\Addons\\Mods\\NullReferenceCatcher\\Logs");
            System.Diagnostics.Process.Start(logsDirectory);
        }





        private void TriggerNullReferenceException()
        {
            try
            {
                // Trigger a null reference exception
                string str = null;
                str.ToLower();

                Debug.Log("Example exception operation failed, generated by NRC (NullReferenceCatcher)");
            }
            catch (Exception ex)
            {
                Debug.Log("Example exception generated by NRC (NullReferenceCatcher): " + ex.ToString());
            }
        }


        public string Name
        {
            get { return "NullReferenceCatcher"; }
        }

        public string Description
        {
            get { return "Catches and logs NullReferenceExceptions in Cities: Skylines."; }
        }

    }
}