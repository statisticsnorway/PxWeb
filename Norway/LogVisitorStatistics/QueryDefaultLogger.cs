using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
    class QueryDefaultLogger: IQuerylogger
    {
        private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(QueryDefaultLogger));
        public void LoggStatistics(string context, string lang, string db, string tableid, string actionType, string actionName, int numberOfCells, int nunmerOfContents, bool cached)
        {
            string cacheNumeric = cached  ? "1" : "0";
            string system = System.Configuration.ConfigurationManager.AppSettings["system"];

            _logger.Info(String.Format("system={0}, type=data, context={2}, Lang={3}, Database={4}, Tableid{5}, db={4},tableid={5},ActionType={6},ActionName={7},NumberOfCells={8},NumberOfContents={9},Cached={10}", system, context, actionType, lang, db, tableid, actionType, actionName, numberOfCells, nunmerOfContents, cached));
        }
    }
}
