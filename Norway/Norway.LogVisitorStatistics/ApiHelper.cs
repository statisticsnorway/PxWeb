using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
    public static class ApiHelper
    {
        private static IApilogger _logger;
        private static log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(ApiHelper));



        public static void LoggStatistics(string url, string caller, string type, string lang, string db, string tableid, string format, int matrixsize, string cache)
        {

            if (_logger == null)
            {
                CreateLogger();
        }
            if (!(_logger == null))
            {
                try
                {
                    _logger.LoggStatistics(url, caller, type, lang, db, tableid, format, matrixsize, cache);
                }
                catch (Exception ex)
                {
                    _log4net.ErrorFormat("Failed to log statistics for database {0} : {1}", db, ex.Message);
                }
            }

        }

        public static IApilogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    CreateLogger();
                }
                return _logger;

            }
            set
            {
                _logger = value;
            }
        }
    
        private static void CreateLogger()
        {
            string loggerTypeStr = "";
            System.Type loggerType = null;

            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["visitorStatisticsApiLogger"]))
            {
                loggerTypeStr = System.Configuration.ConfigurationManager.AppSettings["visitorStatisticsApiLogger"];
                try
                {
                    loggerType = System.Type.GetType(loggerTypeStr);
                   _logger = (IApilogger)Activator.CreateInstance(loggerType);
                    _log4net.Info("Visitor statistics logger of type  '" + loggerType.ToString() + "' was created successfully");
                }

                catch (Exception ex)
                {
                    _log4net.Info("Unabled to create visitor statistics logger of type  '" + loggerTypeStr + "' " + ex.Message);
                }
            }
            else
            {
                _logger = new ApiDefaultLogger();
                _log4net.Info("Visitor statistics logger of type 'ApiDefaultLogger' was created successfully");
            }

            }
            


        }
    }

