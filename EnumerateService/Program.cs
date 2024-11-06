using System.Configuration.Install;
using System.Reflection;
using System;
using System.ServiceProcess;
using System.Threading;

namespace EnumerateService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            if (args.Length > 0) 
            {
                string arg = args[0];
                switch (arg)
                {
                    case "-install":
                        // Below does not work. Use SC CREATE xxxx
                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        break;
 
                    case "-uninstall":
                        // Below does not work. Use SC DELETE xxxx
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;

                    case "-console":
                        Service1 s = new Service1();
                        s.Start();

                        while (true)
                        {
                            Thread.Sleep(10); // 10 ms
                        }
                        break;

                    default:
                        Console.WriteLine("Unrecognized input");
                        return 1;
                }
            }

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
            return 0;
        }
    }
}
