//-----------------------------------------------------------------------
// <copyright file="ControlSystem.cs" company="Crestron">
//     Copyright (c) Crestron Electronics. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;                       // For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronIO;            // For Directory
using Crestron.SimplSharpPro;                    // For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;     // For Threading
using Crestron.SimplSharpPro.DeviceSupport;      // For Generic Device Support
using Crestron.SimplSharpPro.Diagnostics;        // For System Monitor Access

namespace Ex_DynamicRegistration
{
    /// <summary>
    /// ControlSystem class that inherits from CrestronControlSystem
    /// </summary>
    public class ControlSystem : CrestronControlSystem
    {
        /// <summary>
        /// Used for logging information to error log
        /// </summary>
        private const string LogHeader = "[CS] ";

        /// <summary>
        /// Can be used to identify individual program.
        /// Mostly usefull on an appliance with multiple programs running
        /// See below how we use it
        /// </summary>
        private uint appId;

        /// <summary>
        /// Used to read/write config files
        /// </summary>
        private Configuration.ConfigManager config;

        /// <summary>
        /// Used to manage all the different subsystems
        /// </summary>
        private SystemManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlSystem" /> class.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                // Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(this.ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(this.ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(this.ControlSystem_ControllerEthernetEventHandler);

                // Potential way to make your program more dynamic
                // Not being used in either Lab1 or Lab2
                this.appId = InitialParametersClass.ApplicationNumber;
            }
            catch (Exception e)
            {
                ErrorLog.Error(string.Format(LogHeader + "Error in the constructor: {0}", e.Message));
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            Task.Run(() => this.SystemSetup());
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// which Ethernet adapter this event belongs to.
        /// </param>
        public void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {
                // Determine the event type Link Up or Link Down
                case eEthernetEventType.LinkDown:
                    // Next need to determine which adapter the event is for. 
                    // LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                    }

                    break;
                case eEthernetEventType.LinkUp:
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                    }

                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType">Stop, resume or pause</param>
        public void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case eProgramStatusEventType.Paused:
                    // ErrorLog.Notice(string.Format("Program Paused"));
                    break;
                case eProgramStatusEventType.Resumed:
                    // ErrorLog.Notice(string.Format("Program Resumed"));
                    break;
                case eProgramStatusEventType.Stopping:
                    // ErrorLog.Notice(string.Format("Program Stopping"));
                    break;
            }
        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType">Inserted, Removed, Rebooting</param>
        public void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case eSystemEventType.DiskInserted:
                    // Removable media was detected on the system
                    break;
                case eSystemEventType.DiskRemoved:
                    // Removable media was detached from the system
                    break;
                case eSystemEventType.Rebooting:
                    // The system is rebooting. 
                    // Very limited time to preform clean up and save any settings to disk.
                    break;
            }
        }

        /// <summary>
        /// Thread to create all the necessary logic and devices
        /// </summary>
        /// <returns>unused object</returns>
        private object SystemSetup()
        {
            this.config = new Configuration.ConfigManager();

            // Potential check you can do to make your program more dynamic
            // Any "box" processor is an appliance.
            // VC-4 is a server
            // We're not using this for either Lab1 or Lab2
            if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
            {
            }
            else if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Server)
            {
            }

            if (this.config.ReadConfig(System.IO.Path.Combine(Directory.GetApplicationRootDirectory(), "User", "config.json")))
            {
                this.manager = new SystemManager(this.config.RoomConfig, this);
            }
            else
            {
                ErrorLog.Error(LogHeader + "Unable to read config!");
            }

            return null;
        }
    }
}
