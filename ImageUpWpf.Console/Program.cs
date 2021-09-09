using ImageUpWpf.Core;
using ImageUpWpf.Core.App;
using ImageUpWpf.Core.Plugin;
using ImageUpWpf.Core.Upload;
using NLog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ImageUpWpf.Console
{
    class Program
    {
        private static Logger logger;
        private static IuAppContext context;

        static void Main(string[] args)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Trace("App startup");
            context = CreateContext();
            logger.Trace("Context created");

            try
            {
                var task = Task.Run(async () =>
                {
                    System.Console.WriteLine(
                       TyporaFormatter.Format(
                           await new UploadTask
                           {
                               UploadMode = ChainUploadMode.Parallel,
                               NamingTemplate = context.AppConfig.NamingTemplate
                           }
                           .AddChainUploaders(context.ChainUploaders)
                           .AddFiles(args, AddFileCallback)
                           .Run()
                       )
                   );
                });
                task.Wait();
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
                throw e;
            }

        }

        private static IuAppContext CreateContext()
        {
            var context = new IuAppContext();
            context.OnPluginLoadError += Context_OnPluginLoadError;
            context.Init();
            return context;
        }

        private static void Context_OnPluginLoadError(PluginLoadErrorType errorType, string message)
        {
            if(errorType == PluginLoadErrorType.NotFound)
            {
                System.Console.WriteLine("Warning: " + message);
            }
        }

        private static void AddFileCallback(object sender, TaskGroupCreatingResult e)
        {
            var r = !e.HasError ? "SUCC" : "FAIL";
            var err = !e.HasError ? "" : $", Error Message: {e.Message}";
            logger.Info($"{r}: {e.FileName}{err}");
        }
    }
}
