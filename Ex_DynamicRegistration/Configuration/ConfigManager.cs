//-----------------------------------------------------------------------
// <copyright file="ConfigManager.cs" company="Crestron">
//     Copyright (c) Crestron Electronics. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace Ex_DynamicRegistration.Configuration
{
    /// <summary>
    /// Reads/Writes data from config.json
    /// </summary>
    public class ConfigManager
    {
        /// <summary>
        /// Configuration object for this system
        /// </summary>
        public ConfigData.Configuration RoomConfig;

        /// <summary>
        /// Used for logging information to error log
        /// </summary>
        private const string LogHeader = "[Configuration] ";

        /// <summary>
        /// Locking object for config
        /// </summary>
        private static CCriticalSection configLock = new CCriticalSection();

        /// <summary>
        /// Was the read succesfull
        /// </summary>
        private bool readSuccess;

        /// <summary>
        /// Initializes a new instance of the ConfigManager class
        /// </summary>
        public ConfigManager()
        {
        }

        /// <summary>
        /// Reads a JSON formatted configuration from disc
        /// </summary>
        /// <param name="configFile">Location and name of the config file</param>
        /// <returns>True or False depending on read success</returns>
        public bool ReadConfig(string configFile)
        {
            // string for file contents
            string configData = string.Empty;

            ErrorLog.Notice(LogHeader + "Started loading config file: {0}", configFile);
            if (string.IsNullOrEmpty(configFile))
            {
                this.readSuccess = false;
                ErrorLog.Error(LogHeader + "No File?!?");
            }

            if (!File.Exists(configFile))
            {
                this.readSuccess = false;
                ErrorLog.Error(LogHeader + "Config file doesn't exist");
            }
            else if (File.Exists(configFile))
            {
                configLock.Enter();

                // Open, read and close the file
                using (StreamReader file = new StreamReader(configFile))
                {
                    configData = file.ReadToEnd();
                    file.Close();
                }

                try
                {
                    // Try to deserialize into a Room object. If this fails, the JSON file is probably malformed
                    this.RoomConfig = JsonConvert.DeserializeObject<ConfigData.Configuration>(configData);
                    ErrorLog.Notice(LogHeader + "Config file loaded!");
                    this.readSuccess = true;
                }
                catch (Exception e)
                {
                    this.readSuccess = false;
                    ErrorLog.Error(LogHeader + "Exception in reading config file: {0}", e.Message);
                }
                finally
                {
                    configLock.Leave();
                }
            }

            return this.readSuccess;
        }
    }
}