using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using log4net;
using log4net.Config;

namespace GlimpseSample.WebApp
{
    public class Global : HttpApplication
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Global));

        public Global()
        {
            Error += OnError;
            InitLog4Net();
        }

        private void OnError(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            string errorToken = Guid.NewGuid().ToString("N");
            log.Error(string.Format("Encountered an unhandled application error. Associated error token is: {0}.", errorToken), exception);
        }

        private static void InitLog4Net()
        {
            string log4NetConfigFilePath = WebConfigurationManager.AppSettings[Constants.AppSettings.LOG4_NET_CONFIG_FILE_PATH];
            string log4NetMappedConfigFilePath = HostingEnvironment.MapPath(log4NetConfigFilePath);
            Debug.Assert(log4NetMappedConfigFilePath != null, "log4NetMappedConfigFilePath != null");
            var log4NetFileInfo = new FileInfo(log4NetMappedConfigFilePath);
            XmlConfigurator.ConfigureAndWatch(log4NetFileInfo);
        }
    }
}