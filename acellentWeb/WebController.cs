using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace acellentWeb
{
    /// <summary>
    /// 使用外部 DLL 檔所定義的 controller，取代定義於 Antom Web Service 內部的 controller
    /// </summary>
    public class antomController : DefaultHttpControllerSelector
    {
        private readonly HttpConfiguration _configuration;
        private string _dllName = "antom.WebApi.dll";
        private string _dllPath = @Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Constructor of Antom WebApi controller
        /// </summary>
        /// <param name="configuration">A type of System.Web.Http.HttpConfiguration configuration</param>
        /// <param name="dllName">A dll file name (without path).</param>
        /// <param name="dllPath">The path for the customed WebApi dll.</param>
        public antomController(HttpConfiguration configuration, string dllName, string dllPath)
            : base(configuration)
        {
            _configuration = configuration;
            _dllName = dllName;
            _dllPath = dllPath;
        }

        /// <summary>
        /// Override the SelectController method of System.Web.Http.HttpControllerDescriptor
        /// </summary>
        /// <param name="request">A type of System.Net.Http.HttpRequestMessage eb request.</param>
        /// <returns></returns>
        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var assembly = Assembly.LoadFile(Path.Combine(@_dllPath, @_dllName));
            var types = assembly.GetTypes(); //GetExportedTypes doesn't work with dynamic assemblies
            var matchedTypes = types.Where(i => typeof(IHttpController).IsAssignableFrom(i)).ToList();

            var controllerName = base.GetControllerName(request);
            var matchedController =
                matchedTypes.FirstOrDefault(i => i.Name.ToLower() == controllerName.ToLower() + "controller");

            return new HttpControllerDescriptor(_configuration, controllerName, matchedController);
        }
    }
}
