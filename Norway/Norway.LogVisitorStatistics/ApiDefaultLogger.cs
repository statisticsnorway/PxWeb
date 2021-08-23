using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
    class ApiDefaultLogger: IApilogger
    {
        private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ApiDefaultLogger));
        public void LoggStatistics(string url, string caller, string type, string lang, string db, string tableid, string format, int matrixsize, string cache)
        {
            //Logs usage
            _logger.Info(String.Format("url={0}, type={2}, caller={4}, cached={7}, format={1}, matrix-size={3}, db={5},tableid={6}", url, format,type, matrixsize, caller, db, tableid,cache));
        }
    }
}
