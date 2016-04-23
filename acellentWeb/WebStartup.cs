using Microsoft.Owin;
using Owin;
using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using System.Web.Http.Dispatcher;
using System.Threading.Tasks;
using Microsoft.Owin.Extensions;
using System.Web;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

[assembly: OwinStartup(typeof(acellentWeb.WebStartup))]

namespace acellentWeb
{

    using AppFunc = Func<
        IDictionary<string, object>, // Environment
        Task>; // Done

    /// <summary>
    /// The startup class of OWIN.
    /// </summary>
    public class WebStartup
    {
        /// <summary>
        /// The configuration of OWIN self-hosted web startup method. 
        /// By default it's using configuration read from the config file: webconfig.json, 
        /// but user could specify a customed config file though. 
        /// </summary>
        /// <param name="app">Owin.IAppBuilder interface</param>
        public void Configuration(IAppBuilder app)
        {
            if (WebService.WebConfig.FolderSetting?.Count > 0)
            {
                foreach (var dirOption in WebService.WebConfig.FolderSetting)
                {
                    switch (dirOption.FolderType)
                    {
                        case "allow_list":
                            // Turns on static files, directory browsing, and default files.
                            app.UseFileServer(new FileServerOptions()
                            {
                                RequestPath = new PathString(dirOption.VirtualPath),
                                FileSystem = new PhysicalFileSystem(@dirOption.PhysicalDir),
                                EnableDirectoryBrowsing = true,
                            });
                            break;
                        case "file_only":
                            // If needs a staticfile folder, unmark the following code
                            // Only serve files requested by name.
                            app.UseStaticFiles(new StaticFileOptions()
                            {
                                RequestPath = new PathString(dirOption.VirtualPath),
                                FileSystem = new PhysicalFileSystem(dirOption.PhysicalDir)
                            });
                            break;
                        case "root":
                        default:
                            // Setting OWIN based web root directory
                            if (dirOption.VirtualPath.Trim() == "/")
                            {
                                app.UseFileServer(new FileServerOptions()
                                {
                                    RequestPath = PathString.Empty,
                                    FileSystem = new PhysicalFileSystem(@dirOption.PhysicalDir),
                                });
                            }
                            else
                            {
                                app.UseFileServer(new FileServerOptions()
                                {
                                    RequestPath = new PathString(dirOption.VirtualPath),
                                    FileSystem = new PhysicalFileSystem(@dirOption.PhysicalDir),
                                });
                            }
                            break;
                    }
                }
            }


            // Setting up a default .Net WebApi route
            if (WebService.WebConfig.WebApiSetting?.Count > 0)
            {
                foreach (var apiInfo in WebService.WebConfig.WebApiSetting)
                {
                    using (HttpConfiguration config = new HttpConfiguration())
                    {
                        // Using controller settings defined in an external dll
                        config.Services.Replace(typeof(IHttpControllerSelector), new antomController(config, apiInfo.DLLName, @apiInfo.DLLPath));
                        config.Routes.MapHttpRoute(
                            name: apiInfo.RouteName.Trim(),
                            routeTemplate: apiInfo.RouteTemplate.Trim(),
                            defaults: apiInfo.ApiDefaults
                            );
                        // Check whether JSON format is being used or not?
                        if (apiInfo.UseJSON.Trim().ToUpper() == "Y")
                        {
                            config.Formatters.Clear();
                            config.Formatters.Add(new JsonMediaTypeFormatter());
                            config.Formatters.JsonFormatter.SerializerSettings =
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            };
                        }
                        app.UseWebApi(config);
                    };
                }

            }


            // The UseAngularServer extention for OWIN
            if (WebService.WebConfig.AngularJS?.enable.Trim().ToUpper() == "Y")
            {
                //app.UseAngularServer("/", "/index.html");
                // If the html5Mode of AngularJS is enabled, then use the AngularJS middleware.
                if (WebService.WebConfig.AngularJS.html5Mode.Trim().ToUpper() == "Y")
                {
                    app.UseAngularServer(WebService.WebConfig.AngularJS.rootPath.Trim(), WebService.WebConfig.AngularJS.entryPath.Trim());
                }
            }

            // This line is used for URL rewrite
            //app.Use(typeof(UrlRewritter));
            // Making URL rewrite working
            //app.UseStageMarker(PipelineStage.MapHandler);
        }
    }

    #region AngularJS html5Mode Middleware
    /// <summary>
    /// Extension class for OWIN self-hosted web to work with AngularJS, 
    /// especially when the html5Mode is enabled in AngularJS. 
    /// </summary>
    public static class AngularServerExtension
    {
        /// <summary>
        /// Extension class for OWIN self-hosted web to work with AngularJS, 
        /// especially when the html5Mode is enabled in AngularJS. 
        /// </summary>
        /// <param name="builder">Owin.IAppBuilder interface</param>
        /// <param name="rootPath">The root path of the web using AngularJS. Default value is "/".</param>
        /// <param name="entryPath">The entry/redirect page. Default value is "/index.html".</param>
        /// <returns>Returns a public static type of OWIN.IAppBuilder.</returns>
        public static IAppBuilder UseAngularServer(this IAppBuilder builder, string rootPath = "/", string entryPath = "/index.html")
        {
            var options = new AngularServerOptions()
            {
                FileServerOptions = new FileServerOptions()
                {
                    EnableDirectoryBrowsing = false,
                    FileSystem = new PhysicalFileSystem(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rootPath))
                },
                EntryPath = new PathString(entryPath)
            };

            builder.UseDefaultFiles(options.FileServerOptions.DefaultFilesOptions);

            return builder.Use(new Func<AppFunc, AppFunc>(next => new AngularServerMiddleware(next, options).Invoke));
        }
    }

    /// <summary>
    /// AngularJS Server Options of the OWIN self-hosted web.
    /// </summary>
    public class AngularServerOptions
    {
        /// <summary>
        /// Get or set Microsoft.Owin.StaticFiles.FileServerOptions。
        /// </summary>
        public FileServerOptions FileServerOptions { get; set; }

        /// <summary>
        /// Get or set Microsoft.Owin.PathString。
        /// </summary>
        public PathString EntryPath { get; set; }

        /// <summary>
        /// Returns the AngularServerOptions.EntryPath.
        /// </summary>
        public bool Html5Mode
        {
            get
            {
                return EntryPath.HasValue;
            }
        }

        /// <summary>
        /// Constructor of Angular Server Options.
        /// </summary>
        public AngularServerOptions()
        {
            FileServerOptions = new FileServerOptions();
            EntryPath = PathString.Empty;
        }
    }

    /// <summary>
    /// An OWIN Middleware extension to use with AngularJS.
    /// </summary>
    public class AngularServerMiddleware
    {
        private readonly AngularServerOptions _options;
        private readonly AppFunc _next;
        private readonly StaticFileMiddleware _innerMiddleware;

        /// <summary>
        /// Constructor of AngularServerMiddleware. There are two parameters: "next" and "options".
        /// </summary>
        /// <param name="next">An AppFunc delegation.</param>
        /// <param name="options">An AngularServerOptions</param>
        public AngularServerMiddleware(AppFunc next, AngularServerOptions options)
        {
            _next = next;
            _options = options;

            _innerMiddleware = new StaticFileMiddleware(next, options.FileServerOptions.StaticFileOptions);
        }

        /// <summary>
        /// Running AngularServerMiddleware asynchronously.
        /// </summary>
        /// <param name="arg">A System.Collections.Generic.IDictionary argument.</param>
        /// <returns>Returns asynchronous execution result. If no webpage found, the server yields a 404 error, 
        /// and then redirect the web routing back to the EntryPath.</returns>
        public async Task Invoke(IDictionary<string, object> arg)
        {
            await _innerMiddleware.Invoke(arg);
            // route to root path if the status code is 404
            // and need support angular html5mode
            if ((int)arg["owin.ResponseStatusCode"] == 404 && _options.Html5Mode)
            {
                arg["owin.RequestPath"] = _options.EntryPath.Value;
                await _innerMiddleware.Invoke(arg);
            }
        }
    }
    #endregion

}
