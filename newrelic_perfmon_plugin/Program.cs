﻿using System;
using NewRelic.Platform.Sdk;
using NewRelic.Platform.Sdk.Utils;
using Topshelf;
using Topshelf.Runtime;
using System.Threading;

namespace newrelic_perfmon_plugin
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<PluginService>(sc =>
                {
                    sc.ConstructUsing(() => new PluginService());

                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });
                x.SetServiceName("newrelic_perfmon_plugin");
                x.SetDisplayName("NewRelic Windows Perfmon Plugin");
                x.SetDescription("Sends Perfmon Metrics to NewRelic Platform");
                x.StartAutomatically();
                x.RunAsPrompt();
            });
        }
    }

    class PluginService
    {
        Runner _runner;
        public Thread thread { get; set; }

        private static Logger logger = Logger.GetLogger("newrelic_perfmon_plugin");

        public PluginService()
        {
            _runner = new Runner();
            _runner.Add(new PerfmonAgentFactory());
        }

        public void Start()
        {
            logger.Info("Starting service.");
            thread = new Thread(new ThreadStart(_runner.SetupAndRun));
            try
            {
                thread.Start();
            }
            catch (Exception e)
            {
                logger.Error("Exception occurred, unable to continue. {0}\r\n{1}", e.Message, e.StackTrace);
            }
        }

        public void Stop()
        {
            logger.Info("Stopping service.");
            System.Threading.Thread.Sleep(5000);
            
            if (thread.IsAlive)
            {
                _runner = null;
                thread.Abort();
            }            
        }
    }
}
