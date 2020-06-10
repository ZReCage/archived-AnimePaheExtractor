using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CrashReporterDotNET;

namespace AnimePaheExtractorWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ReportCrash _reportCrash;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if RELEASE
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
#endif
            _reportCrash = new ReportCrash("contact.zrec@gmail.com")
            {
                DeveloperMessage = "AnimePahe Extractor - Crash Report",
                IncludeScreenshot = false
            };

            _reportCrash.DoctorDumpSettings = new DoctorDumpSettings
            {
                ApplicationID = new Guid("87cfb35d-8dc8-483c-a0a0-3a5b26c7593e")
            };
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        {
            SendReport(unobservedTaskExceptionEventArgs.Exception);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            SendReport(dispatcherUnhandledExceptionEventArgs.Exception);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            SendReport((Exception)unhandledExceptionEventArgs.ExceptionObject);
        }

        public static void SendReport(Exception exception)
        {
            _reportCrash.Silent = false;
            _reportCrash.Send(exception);
        }

        public static void SendReportSilently(Exception exception)
        {
            _reportCrash.Silent = true;
            _reportCrash.Send(exception);
        }
    }
}