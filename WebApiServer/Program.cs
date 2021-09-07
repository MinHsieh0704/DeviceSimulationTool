using Min_Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace WebApiServer
{
    public class IAccount
    {
        public string account { get; set; }
        public string password { get; set; }
    }

    class Program
    {
        public static IAccount basicAuth { get; set; } = null;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            try
            {
                if (args == null || args.Length == 0)
                {
                    throw new Exception("args can not null or empty");
                }

                int parentProcessId = Convert.ToInt32(args[0]);

                if (args.Length == 1)
                {
                    throw new Exception("port can not null or empty");
                }
                int port = Convert.ToInt32(args[1]);

                if (args.Length >= 4)
                {
                    basicAuth = new IAccount()
                    {
                        account = args[2],
                        password = args[3]
                    };
                }

                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration($"http://127.0.0.1:{port}");
                //config.Routes.MapHttpRoute(
                //    name: "Api",
                //    routeTemplate: "{controller}/{action}/{id}",
                //    defaults: new { id = RouteParameter.Optional }
                //);

                config.Routes.MapHttpRoute(
                    name: "Default",
                    routeTemplate: "{*url}",
                    defaults: new { controller = "Index", action = "Handle" }
                );

                var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
                config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

                using (Process parent = Process.GetProcessById(parentProcessId))
                {
                    using (HttpSelfHostServer httpServer = new HttpSelfHostServer(config))
                    {
                        httpServer.OpenAsync().Wait();

                        while (true)
                        {
                            if (parent.HasExited)
                            {
                                httpServer.CloseAsync().Wait();
                                break;
                            }

                            Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                Console.Error.WriteLine($"{ex.Message}");
            }
        }
    }
}
