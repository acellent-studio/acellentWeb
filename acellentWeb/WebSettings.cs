using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace acellentWeb
{
    /// <summary>
    /// Get or set web cofigurations. 
    /// The configuration file should be in JSON format.
    /// </summary>
    public class WebSettings
    {
        /// <summary>
        /// Get or set the domain name or IP address of a URL. 
        /// </summary>
        [JsonProperty]
        public UrlConfig UrlSetting { get; set; }
        /// <summary>
        /// Get or set the physical directories and virtual pathes of the web server.
        /// </summary>
        [JsonProperty]
        public List<FolderConfig> FolderSetting { get; set; }
        /// <summary>
        /// Get or set customed WebApi settings. 
        /// </summary>
        [JsonProperty]
        public List<WebApiConfig> WebApiSetting { get; set; }
        /// <summary>
        /// Get or set attributes regarding AngularJS usage. 
        /// </summary>
        [JsonProperty]
        public Angular AngularJS { get; set; }
    }

    /// <summary>
    /// Get or set web domain(IP address) and port information.
    /// </summary>
    public class UrlConfig
    {
        string _fqdn = "";
        string _domain = "+";
        string _port = "80";

        /// <summary>Get or set the domain name or IP address of a URL. Default value is "+", 
        /// which means listening to all available IPs of the machine running this program.</summary>
        public string Domain
        {
            get
            {
                //IPHostEntry localHostEntry = Dns.GetHostEntry("");
                //_domain = (_domain != "+") ? _domain : localHostEntry.AddressList.FirstOrDefault(current => current.AddressFamily == AddressFamily.InterNetwork).ToString();
                return _domain;
            }
            set { _domain = value.Trim(); }
        }
        /// <summary>Get or set the connection port of a URL. Default value is 80.</summary>
        public string Port
        {
            get { return _port; }
            set { _port = value.Trim(); }
        }
        /// <summary>Return the Fully Qualified Domain Name (FQDN) of the full URL. Default will be "http://+:80".</summary>
        public string GetFQDN
        {
            get
            {
                _fqdn = (Domain.Trim().IndexOf("http") == 0) ? Domain : "http://" + Domain.Trim().Replace("+", "localhost");
                return _fqdn + ":" + Port.Trim();
            }
        }
    }

    /// <summary>
    /// Get or set a web's folder type, physical installation path, virtual path information.
    /// </summary>
    public class FolderConfig
    {
        string _dir = "";
        string _dirType = "root";
        string _virtual = "/";
        /// <summary>Get or set the folder type. Only three types are allowed: "root", "allow_list" and "file_only".
        /// Default value is "root".</summary>
        public string FolderType
        {
            get { return _dirType; }
            set { _dirType = value.Trim(); }
        }
        /// <summary>Get or set the virtual path name for the web. Default value is "/". 
        /// Ex: if "root", the recommand value is "/"; if "allow_list", the recommand value is "/public"; 
        /// if "file_only", the recommand value is "/files".</summary>
        public string VirtualPath
        {
            get { return _virtual; }
            set { _virtual = value.Trim(); }
        }
        /// <summary>Get or set the physical directory of a virtual path.</summary>
        public string PhysicalDir
        {
            get { return _dir; }
            set
            {
                if (value.Trim().ToLower() == "default")
                {
                    _dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    switch (_dirType)
                    {
                        case "allow_list":
                            _dir += @"\webroot\public";
                            break;
                        case "file_only":
                            _dir += @"\webroot\files";
                            break;
                        case "root":
                        default:
                            _dir += @"\webroot";
                            break;
                    }
                }
                else
                {
                    _dir = value.Trim();
                }
            }
        }
    }

    /// <summary>
    /// Folder type options. Only three types are allowed: "root", "allow_list" and "file_only".
    /// </summary>
    public enum FolderOptions
    {
        /// <summary>root: the entry path of a web.</summary>
        root,
        /// <summary>allow_list: A browsable folder of a web.</summary>
        allow_list,
        /// <summary>file_only: Users can only access files or webpages inside this folder by static link.</summary>
        file_only
    }

    /// <summary>
    /// Get or set settings of external web api DLL. 
    /// </summary>
    public class WebApiConfig
    {
        string _dll = "antom.WebApi.dll";
        string _dllPath = "default";
        string _routeName = "DefaultApi";
        string _routePath = @"api/{controller}/{id}";
        string _useJSON = "Y";

        /// <summary>
        /// Get or set the dll file name of the customed(external) webapi dll. 
        /// Default is "antom.WebApi.dll" located in the installation folder.
        /// </summary>
        public string DLLName
        {
            get { return _dll; }
            set { _dll = value.Trim(); }
        }
        /// <summary>
        /// Get or set the path of the customed(external) webapi dll. 
        /// Default is the AcellentWeb.exe installation directory.
        /// </summary>
        public string DLLPath
        {
            get { return _dllPath; }
            set
            {
                _dllPath = (value.Trim().ToLower() == "default") ?
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : value.Trim();
            }
        }
        /// <summary>
        /// Get or set the route name of the customed webapi. Default is "DefaultApi".
        /// </summary>
        public string RouteName
        {
            get { return _routeName; }
            set { _routeName = value.Trim(); }
        }
        /// <summary>
        /// Get or set the route template of the customed webapi. Default is "api/{controller}/{id}".
        /// </summary>
        public string RouteTemplate
        {
            get { return _routePath; }
            set { _routePath = value.Trim(); }
        }
        /// <summary>
        /// Get or set the controller default values of the customed webapi. 
        /// </summary>
        public Dictionary<string, object> ApiDefaults { get; set; }

        /// <summary>
        /// Get or set whether to use JSON as the default web api data format. Default is "Y".
        /// </summary>
        public string UseJSON
        {
            get { return _useJSON; }
            set { _useJSON = value.Trim(); }
        }
    }

    /// <summary>
    /// Get or set attributes regarding AngularJS usage. 
    /// </summary>
    public class Angular
    {
        /// <summary>
        /// Field: Get or set whether use AngularJS or not.
        /// </summary>
        public string enable { get; set; }
        /// <summary>
        /// Field: Get or set whether use AngularJS html5Mode or not.
        /// </summary>
        public string html5Mode { get; set; }
        /// <summary>
        /// Field: Get or set the root path of AngularJS web.
        /// </summary>
        public string rootPath { get; set; }
        /// <summary>
        /// Field: Get or set the entry path of AngularJS web.
        /// </summary>
        public string entryPath { get; set; }
    }

    /// <summary>
    /// 客製化 Log 類別。
    /// </summary>
    public class webLog
    {
        /// <summary>
        /// 欄位：設定(set)或讀取(get) Log 內容。
        /// </summary>
        public static string LogMessage { get; set; }
    }
}
