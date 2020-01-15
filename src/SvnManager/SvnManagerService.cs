﻿using SvnManager.WebUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SvnManager
{
    public partial class SvnManagerService : ServiceBase
    {
        private const string Source = "SVNManagerService";

        Timer _backupTimer = new Timer(900_000); // check every 15 minutes (reduce time creep)
        BackgroundWorker _worker = new BackgroundWorker();
        NancyHostControl host;
        static ConfigMonitor monitor;

        public SvnManagerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!EventLog.SourceExists(Source))
                EventLog.CreateEventSource(Source, "Application");

            host = new NancyHostControl("", true);
            host.Start();

            _worker.DoWork += _worker_DoWork;
            _backupTimer.Elapsed += _backupTimer_Elapsed;
            _backupTimer.Start();

            monitor = new ConfigMonitor();
            monitor.Reloaded += Monitor_Reloaded;
        }

        private void Monitor_Reloaded(object sender, EventArgs e)
        {
            EventLog.WriteEntry(Source, "Configuration Reloaded", EventLogEntryType.Information);
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (SvnBackups.IsBackupTime())
            {
                SvnBackups.IsBackingUp = true;
                SvnBackups.RunBackups();
                SvnBackups.Upload();
                SvnBackups.DeleteLocal();
                SvnBackups.IsBackingUp = false;
            }
        }

        private void _backupTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (SvnBackups.IsBackingUp)
                return;

            _worker.RunWorkerAsync();
        }

        protected override void OnStop()
        {
            host.Stop();
        }
    }
}