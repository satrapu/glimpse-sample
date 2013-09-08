using System;
using System.Web.UI;
using log4net;

namespace GlimpseSample.WebApp
{
    public class Log4Net : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Log4Net));

        protected void Page_Load(object sender, EventArgs e)
        {
            Exception sampleException = new Exception("Exception message", new Exception("Inner exception #1", new Exception("Inner exception #2")));

            log.Debug("Sample debug message");
            log.Info("Sample info message");
            log.Warn("Sample warn message", sampleException);
            log.Error("Sample error message", sampleException);
            log.Fatal("Sample fatal message", sampleException);
        }
    }
}