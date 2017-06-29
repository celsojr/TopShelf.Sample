using NLog;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Timers;
using System.Security.Permissions;

namespace TopshelfFileWatcher
{
    using static Enum;
    using static Path;
    using static Helper;
    using static LogManager;
    using static NotifyFilters;
    using static SecurityAction;
    using static ConfigureService;

    internal class FileWatcherService
    {
        private bool isModified;
        private readonly Timer timer;

        private static FileSystemWatcher watcher;
        private static Logger logger = GetCurrentClassLogger();

        public FileWatcherService()
        {
            var i = GetInterval(Interval);
            timer = new Timer(i) { AutoReset = true };
            timer.Elapsed += (sender, e) =>
            {
                if (isModified)
                {
                    UploadFile();
                }
            };
        }

        [PermissionSet(Assert, Name = "FullTrust")]
        public bool Start()
        {
            timer.Start();

            logger.Info($"Watching: {LocalPath + File}");

            watcher = new FileSystemWatcher
            {
                Path = LocalPath,
                Filter = File,
                NotifyFilter = 0,
                EnableRaisingEvents = true
            };

            GetValues(typeof(NotifyFilters)).Cast<NotifyFilters>()
                .ForEach(filter =>
                {
                    if (filter != DirectoryName)
                    {
                        watcher.NotifyFilter |= filter;
                    }
                });

            logger.Info($"The applied filters are: {watcher.NotifyFilter}");

            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;

            watcher.Error += Watcher_Error;

            return true;
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();

            logger.Error(ex.Message);

            if (ex.InnerException != null)
            {
                logger.Error(ex.InnerException);
            }
        }

        public bool Stop()
        {
            timer.Stop();

            watcher.Dispose();

            return true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!isModified) // Prevent double trigger
            {
                logger.Info($"File {e.FullPath} {e.ChangeType}.");
                isModified = true;
            }
        }

        private void UploadFile()
        {
            logger.Info("Starting upload process...");

            try
            {
                // The file that must be updated should already be in the destination folder of the server.
                // Otherwise, an error will occur.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Combine(ServerPath, File));

                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Proxy = null;

                request.Credentials = new NetworkCredential(UserName, Password);

                using (FileStream fs = new FileStream(Combine(LocalPath, File), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Stream requestStream = request.GetRequestStream())
                using (StreamReader sourceStream = new StreamReader(fs))
                {
                    byte[] buffer = new byte[4096];
                    int count;
                    while ((count = fs.Read(buffer, 0, buffer.Length)) > 0)
                        requestStream.Write(buffer, 0, count);
                }

                logger.Info("Uploaded successfully!");

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                isModified = false;
            }
        }

    }
}
