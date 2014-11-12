using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Newtonsoft.Json.Linq;

namespace FailedPrintJobDeleter
{
    public static class Config
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The path to the configuration file.
        /// </summary>
        public static string ConfigPath
        {
            get
            {
                return Path.Combine(Util.ProgramDirectory, "Config.json");
            }
        }

        /// <summary>
        /// Update period, in minutes.
        /// </summary>
        public static double UpdatePeriodInMinutes { get; set; }

        /// <summary>
        /// The printer devices.
        /// </summary>
        public static List<IPrinterDevice> PrinterDevices { get; set; }

        static Config()
        {
            UpdatePeriodInMinutes = 5.0;
            PrinterDevices = new List<IPrinterDevice>();
        }

        /// <summary>
        /// Loads the configuration from the JSON file.
        /// </summary>
        public static void Load()
        {
            string inputString;

            if (!File.Exists(ConfigPath))
            {
                // oh well
                Logger.Info("Config file does not exist. Not loading.");
                return;
            }

            Logger.Info("Loading config file...");

            using (var inReader = new StreamReader(new FileStream(ConfigPath, FileMode.Open, FileAccess.Read), Encoding.UTF8))
            {
                inputString = inReader.ReadToEnd();
            }

            var config = JObject.Parse(inputString);
            if (config["UpdatePeriodInMinutes"] != null)
            {
                UpdatePeriodInMinutes = (double)config["UpdatePeriodInMinutes"];
            }
            if (config["PrinterDevices"] != null)
            {
                var printerDeviceNodes = config["PrinterDevices"] as JArray;
                if (printerDeviceNodes == null)
                {
                    Logger.Error("PrinterDevices are not an array.");
                }
                else
                {
                    foreach (var printerDeviceNode in printerDeviceNodes)
                    {
                        var printerDevice = printerDeviceNode as JObject;
                        if (printerDevice == null)
                        {
                            Logger.Error("PrinterDevices element is not an object.");
                            continue;
                        }

                        var parameters = new Dictionary<string, string>();

                        foreach (var pair in printerDevice)
                        {
                            parameters[pair.Key] = pair.Value.ToString();
                        }

                        var assemblyName = parameters["Assembly"];
                        var className = parameters["Class"];

                        // instantiate the object
                        var assembly = Assembly.Load(assemblyName);
                        if (assembly == null)
                        {
                            Logger.ErrorFormat(
                                "assembly {1} (for class {0}) could not be loaded",
                                className,
                                assemblyName
                            );
                            continue;
                        }

                        var cls = assembly.GetType(className);
                        if (cls == null)
                        {
                            Logger.ErrorFormat(
                                "class {0} not found in assembly {1}",
                                className,
                                assemblyName
                            );
                            continue;
                        }

                        if (!typeof(IPrinterDevice).IsAssignableFrom(cls))
                        {
                            Logger.ErrorFormat(
                                "{0} (assembly {1}) does not implement IPrinterDevice",
                                className,
                                assemblyName
                            );
                            continue;
                        }

                        var constructor = cls.GetConstructor(new[] { typeof(Dictionary<string, string>) });
                        if (constructor == null)
                        {
                            Logger.ErrorFormat(
                                "{0} (assembly {1}) does not contain a constructor taking Dictionary<string, string>",
                                className,
                                assemblyName
                            );
                            continue;
                        }

                        var device = (IPrinterDevice)constructor.Invoke(new object[] { parameters });
                        PrinterDevices.Add(device);
                    }
                }
            }
        }
    }
}
