using System;
using System.IO;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using ColossalFramework.IO;
using ColossalFramework;
using ColossalFramework.PlatformServices;
using System.Collections.Generic;





namespace XCatcher
{
    // Make the class public so it can be loaded by Cities: Skylines
    public class XCatcher : IUserMod
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
            _logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order\\Cities_Skylines\\Addons\\Mods\\XCatcher\\Logs");
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }
        }

        public static UILabel CreateTitle(UIComponent parent, string name, string text)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.autoSize = false;
            label.height = 20f;
            label.verticalAlignment = UIVerticalAlignment.Middle;
            label.relativePosition = new Vector3(0f, 0f);

            return label;
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

                    // Specify the path of the EXCEPTLOG.txt file
                    string EXCEPTLOGFilePath = Path.Combine(_logsDirectory, "EXCEPTLOG.txt");

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

                    // Write the error message and the full stack trace to the EXCEPTLOG.txt file
                    using (StreamWriter writer = new StreamWriter(EXCEPTLOGFilePath, true))
                    {
                        writer.WriteLine("Date and Time: " + DateTime.Now);
                        writer.WriteLine(_errorMessage);
                        writer.WriteLine(stackTrace);
                        writer.WriteLine("Source of Exception: " + className);
                        writer.WriteLine("----------------------");
                    }

                    // Recommend sending the log file to a developer or support center
                    using (StreamWriter writer = new StreamWriter(EXCEPTLOGFilePath, true))
                    {
                        writer.WriteLine("Recommendation: Please send this log file to a Nyoko, a developer or support center for further investigation.");
                        writer.WriteLine("----------------------");
                    }

                    // Log the error message and stack trace to the output log file
                    string outputLogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam\\steamapps\\common\\Cities_Skylines\\Cities_Data\\output_log.txt");
                    using (StreamWriter writer = new StreamWriter(outputLogFilePath, true))
                    {
                        writer.WriteLine("Date and Time: " + DateTime.Now);
                        writer.WriteLine(_errorMessage);
                        writer.WriteLine(stackTrace);
                        writer.WriteLine("------Exception caught by XCatcher------");
                    }

                    // Copy the EXCEPTLOG.txt file to the specified directory
                    string destinationDirectory = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2952968348\XCatcher\XCATCHER";
                    string destinationFilePath = Path.Combine(destinationDirectory, "EXCEPTLOG.txt");

                    try
                    {
                        // Create the destination directory if it doesn't exist
                        Directory.CreateDirectory(destinationDirectory);

                        // Copy the EXCEPTLOG.txt file to the destination directory
                        File.Copy(EXCEPTLOGFilePath, destinationFilePath, true);

                        Debug.Log("EXCEPTLOG.txt copied to XCATCHER directory.");
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "Failed to copy EXCEPTLOG.txt to XCATCHER directory.\n\n" + ex.Message;
                        Debug.LogError(errorMessage);
                        ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                        panel.SetMessage("XCatcher Exception", errorMessage, false);
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
                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("XCatcher caught an error", _errorMessage, true);

                // Reset the flag
                _isErrorCaught = false;
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("XCatcher Options");

            // Add a button to trigger NullReferenceException
            UIButton nreButton = group.AddButton("Trigger NullReferenceException (Experimental)", TriggerNullReferenceException) as UIButton;
            nreButton.tooltip = "This button will trigger a NullReferenceException (NRE) which can help test if the mod can properly handle such an error.";

            group.AddSpace(10);

            UIButton logsFolderButton = group.AddButton("Launch XCatcher", OpenXCatcherExe) as UIButton;
            logsFolderButton.tooltip = "Launch XCatcher executable.";

            group.AddSpace(20);

            UIButton outputFolderButton = group.AddButton("Open output_log.txt Folder", OpenOutputFolder) as UIButton;
            outputFolderButton.tooltip = "Click this button to open the folder where output_log.txt is located.";


            group.AddSpace(20);

            group.AddButton("Retrieve and Save Mod List", () =>
            {
                // Get the path to the Cities: Skylines workshop folder
                string workshopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "workshop", "content", "255710");

                // Get a list of all the subdirectories (each subdirectory is an ID of a mod)
                string[] modIds = Directory.GetDirectories(workshopPath);

                // Create a dictionary to hold the names of the mods with their corresponding ID
                Dictionary<string, string> modNames = new Dictionary<string, string>();

                // Loop through the mod IDs and get the name of each mod by its .dll file name
                foreach (string id in modIds)
                {
                    // Get the path to the mod's .dll file
                    string[] dllFiles = Directory.GetFiles(id, "*.dll");
                    if (dllFiles.Length > 0)
                    {
                        string dllFileName = Path.GetFileName(dllFiles[0]);
                        string modName = Path.GetFileNameWithoutExtension(dllFileName);
                        modNames.Add(modName, Path.GetFileName(id));
                    }
                }

                // Create a string containing the list of mod names with their corresponding ID
                string modList = "";
                foreach (var mod in modNames)
                {
                    modList += $"{mod.Key} ({mod.Value}){Environment.NewLine}";
                }

                // Write the mod list to a file
                string filePath = Path.Combine(Application.dataPath, "modlist.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, modList);
            });

            group.AddSpace(20);
        }
            

private void OpenOutputFolder()
                {
                    // Open the logs directory in file explorer
                    string logsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "Cities_Skylines", "Cities_Data");
                    try
                    {
                        System.Diagnostics.Process.Start(logsDirectory);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to open output folder: " + ex.Message);
                    }
                }

        private void OpenXCatcherExe()
        {
            string xcatcherPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2952968348\XCatcher\xcatcher.exe";
            try
            {
                System.Diagnostics.Process.Start(xcatcherPath);
            }
            catch (Exception ex)
            {
                string message = "Failed to open XCatcher.\n\n" + ex.Message + "\n" + ex.StackTrace;
                Debug.LogError("Failed to open XCatcher: " + ex.Message);
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("XCatcher Exception", message, false);
            }
        }


        private void TriggerNullReferenceException()
        {
            try
            {
                // Trigger a null reference exception
                string str = null;
                str.ToLower();
                Debug.Log("NRC example exception triggered.");
            }
            catch (NullReferenceException ex)
            {
                string message = "Generated by XCatcher\n\n" + ex.Message + "\n" + ex.StackTrace;
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Example Null Reference Exception", message, false);
            }
        }
        





        public string Name => "XCatcher";

        public string Description => "Catches and logs NullReferenceExceptions in Cities: Skylines.";
    }
}




