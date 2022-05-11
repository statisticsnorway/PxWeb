using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
    class ApiSSBLogger : IApilogger
    {
        private static log4net.ILog _logger_db = log4net.LogManager.GetLogger("ApiSSBLoggerDB");
        private static log4net.ILog _logger_file = log4net.LogManager.GetLogger("ApiSSBLoggerFile");
        public void LoggStatistics(string url, string caller, string type, string lang, string db, string tableid, string format, int matrixsize, string cache)
        {

            if (type == "data")
            {
                string cacheNumeric = cache == "true" ? "1" : "0";
                log4net.LogicalThreadContext.Properties["system"] = System.Configuration.ConfigurationManager.AppSettings["system-api"];
                log4net.LogicalThreadContext.Properties["type"] = type;
                log4net.LogicalThreadContext.Properties["lang"] = lang;
                log4net.LogicalThreadContext.Properties["database"] = db;
                log4net.LogicalThreadContext.Properties["tableid"] = tableid;
                log4net.LogicalThreadContext.Properties["format"] = FiletypeConverter(format);
                log4net.LogicalThreadContext.Properties["matrixsize_string"] = matrixsize.ToString();
                log4net.LogicalThreadContext.Properties["cached"] = cacheNumeric;
                _logger_db.Info(String.Format("url='{0}', type='{7}', caller='{3}', cached='{6}', format='{1}', matrix-size='{2}', db='{4}',tableid='{5}'", url, format, matrixsize, caller, db, tableid, cache, type));
                _logger_file.Info(String.Format("url='{0}', type='{7}', caller='{3}', cached='{6}', format='{1}', matrix-size='{2}', db='{4}',tableid='{5}'", url, format, matrixsize, caller, db, tableid, cache, type));
            }
            else
            {
                //Logs usage to file
                _logger_file.Info(String.Format("url='{0}', type='{7}', caller='{3}', cached='{6}', format='{1}', matrix-size='{2}', db='{4}',tableid='{5}'", url, format, matrixsize, caller, db, tableid, cache, type));
            }
            
        }

        private string FiletypeConverter(string format)
        {
            switch (format)
            {
                case "px":
                    return "p05";
                case "json":
                    return "p06";
                case "json-stat":
                    return "p07";
                case "json-stat2":
                    return "p08";
                case "csv":
                    return "p24";
                case "csv2":
                    return "p41";
                case "csv3":
                    return "p42";
                case "xlsx":
                    return "p11";
            }
        return "p99";
    }
    }
}
