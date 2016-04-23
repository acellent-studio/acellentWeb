using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace acellentWeb
{
    /// <summary>
    /// A windows service inherites from System.ServiceProcess.ServiceBase. 
    /// </summary>
    public partial class WebService : ServiceBase
    {
        /// <summary>
        /// Store web settings read from configuration file. 
        /// By default, the configuration file is "webconfig.json".
        /// </summary>
        public static WebSettings WebConfig;

        /// <summary>
        /// The constructor of AcellentWeb windows service. 
        /// It loads web settings from the configuration file and tries to start the service accordingly.
        /// The default config file is 'webconfig.json', it should be located in the same directory 
        /// as AcellentWeb.exe does and should be a JSON file. 
        /// The starting result will be recorded in event log: AcellentWeb.
        /// </summary>
        public WebService()
        {
            InitializeComponent();
            webLog.LogMessage = Properties.Resources.ServiceStarting + "\n";
        }

        /// <summary>
        /// Read configuration settings from the config file and then start the WebService. 
        /// </summary>
        /// <param name="args">Only one argument is allowed: the config file name. 
        /// If no argument inserts, default config will be used.</param>
        /// <remarks>
        /// The default config file is 'webconfig.json', it should be located in the same directory 
        /// as AcellentWeb.exe does and should be a JSON file. 
        /// The starting result will be recorded in event log: AcellentWeb.
        /// </remarks>
        protected override void OnStart(string[] args)
        {
            string cfg = "webconfig.json";
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToString() == "--config" || args[i].ToString() == "-c")
                    {
                        cfg = args[i + 1];
                    }
                }
            }
            InternalStart(cfg);
        }

        internal void InternalStart(string configFile = "webconfig.json")
        {
            // Check if a webconfig.json exists first and then retrieve setting values
            try
            {
                using (StreamReader readConfig = new StreamReader(
                    Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    configFile), Encoding.UTF8))
                {
                    string cfgContent = readConfig.ReadToEnd();
                    WebConfig = JsonConvert.DeserializeObject<WebSettings>(cfgContent);
                }
                webLog.LogMessage += Properties.Resources.FileReadOK + configFile + "\n";
            }
            catch (Exception ex)
            {
                webLog.LogMessage = Properties.Resources.FileReadFail + "\nReading " + configFile + " file failed, using default values instead.\n";
                webLog.LogMessage += "Error message:\n" + ex.ToString();
            }

            webLog.LogMessage += "\nService Installed in: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\n\n";
            webLog.LogMessage += "Configuration settings:\n";
            webLog.LogMessage += "\tConfig File: \"" + configFile + "\n";
            webLog.LogMessage += "\t\tWeb FQDN: \"" + WebConfig.UrlSetting.GetFQDN + "\n";
            webLog.LogMessage += "\t\tWeb Directory: \n";
            // Reading web folder settings
            if (WebConfig.FolderSetting?.Count > 0)
            {
                int i = 0;
                foreach (var dirInfo in WebConfig.FolderSetting)
                {
                    webLog.LogMessage += "\t\t\tFolder(" + (++i).ToString() + ") Type: " + dirInfo.FolderType + "\n";
                    webLog.LogMessage += "\t\t\tVirtual Path: \"" + dirInfo.VirtualPath + "\"\n";
                    webLog.LogMessage += "\t\t\tPhysical Path: \"" + dirInfo.PhysicalDir + "\"\n\n";
                }
            }
            else
            {
                webLog.LogMessage += "\t\t\tNo folder setting is found!\n\n";
            }

            // Reading WebApi settings
            webLog.LogMessage += "\t\tUser defined WebApi Setting: \n";
            if (WebConfig.WebApiSetting?.Count > 0)
            {
                int i = 0;
                foreach (var apiInfo in WebConfig.WebApiSetting)
                {
                    webLog.LogMessage += "\t\t\tDLL (" + (++i).ToString() + "): " + apiInfo.DLLName + "\n";
                    webLog.LogMessage += (@apiInfo.DLLPath.Trim().ToLower() == "default") ?
                        ("\t\t\tDLL Path: \"" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\"\n") :
                        ("\t\t\tDLL Path: \"" + apiInfo.DLLPath + "\"\n");
                    webLog.LogMessage += "\t\t\tRoute Name: \"" + apiInfo.RouteName + "\"\n";
                    webLog.LogMessage += "\t\t\tRoute Template: \"" + @apiInfo.RouteTemplate + "\"\n";
                    if (apiInfo.ApiDefaults?.Count > 0)
                    {
                        webLog.LogMessage += "\t\t\tControl Defaults: \n";
                        foreach (var dfs in apiInfo.ApiDefaults)
                        {
                            webLog.LogMessage += "\t\t\t    " + dfs.Key.ToString() + " = " + dfs.Value.ToString() + ",\n";
                        }
                    }
                    webLog.LogMessage += "\t\t\tUse JSON: \"" + apiInfo.UseJSON + "\"\n\n";
                }
            }
            else
            {
                webLog.LogMessage += "\t\tNo Customed WebApi setting is found!\n\n";
            }

            // Reading AngularJS settings
            webLog.LogMessage += "\t\tAngularJS Setting: \n";
            if (WebConfig.AngularJS?.enable.Trim().ToUpper() == "Y")
            {
                webLog.LogMessage += "\t\t\thtml5Mode: \"" + WebConfig.AngularJS.html5Mode + "\"\n";
                webLog.LogMessage += "\t\t\trootPath: \"" + WebConfig.AngularJS.rootPath + "\"\n";
                webLog.LogMessage += "\t\t\tentryPath: \"" + WebConfig.AngularJS.entryPath + "\"\n\n";
            }
            else
            {
                webLog.LogMessage += "\t\tNo AngularJS setting is found.\n\n";
            }

            try
            {
                // Trying to start OWIN services
                WebApp.Start<WebStartup>(WebConfig.UrlSetting.GetFQDN);
                webLog.LogMessage += Properties.Resources.ServiceStartOK + "\n";
                webLog.LogMessage += "The web server is listening at " + WebConfig.UrlSetting.GetFQDN;
            }
            catch (Exception ex)
            {
                // OWIN services started failed.
                webLog.LogMessage += Properties.Resources.ServiceStartOK + "\n\n";
                webLog.LogMessage += "Error message: \n" + ex.ToString();
            }
#if DEBUG
            Console.WriteLine(webLog.LogMessage);
#else
            eventLog1.WriteEntry(webLog.LogMessage);
#endif
        }

        /// <summary>
        /// Stop AcellentWeb Service, and record the result in event log AcellentWeb.
        /// </summary>
        protected override void OnStop()
        {
            InternalStop();
        }

        internal void InternalStop()
        {
            eventLog1.WriteEntry(Properties.Resources.ServiceStopOK + " on " + DateTime.Now.ToString());
        }
    }
}
