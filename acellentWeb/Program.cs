using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace acellentWeb
{
    static class Program
    {
        /// <summary>
        /// Main entry point of AcellentWeb. 
        /// </summary>
        static void Main(string[] args)
        {
#if DEBUG
            var svc = new WebService();
            string cfg = "webconfig.json";
            Console.WriteLine("Starting web service...");
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
            svc.InternalStart(cfg);

            Console.WriteLine("Press any key to stop web service...");
            Console.ReadLine();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WebService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
