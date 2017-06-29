using Topshelf;
using System.Configuration;

namespace TopshelfFileWatcher
{
    using static HostFactory;
    using static ConfigurationManager;

    internal class ConfigureService
    {
        internal void Configure()
        {
            Run(configure =>
            {
                configure.Service<FileWatcherService>(service =>
                {
                    service.ConstructUsing(s => new FileWatcherService());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });

                configure.UseNLog();
                configure.RunAsLocalSystem();
                configure.SetServiceName(AppSettings["ServiceName"]);
                configure.SetDisplayName(AppSettings["DisplayName"]);
                configure.SetDescription(AppSettings["Description"]);

            });
        }

        internal static string File => AppSettings["fileToWatch"];
        internal static string UserName => AppSettings["userName"];
        internal static string Password => AppSettings["password"];
        internal static string LocalPath => AppSettings["watchPath"];
        internal static string ServerPath => AppSettings["serverPath"];
        internal static string Interval => AppSettings["interval-minutes"];
    }
}