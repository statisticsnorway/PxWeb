using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
    class QuerySSBLogger : IQuerylogger
    {
        private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(QuerySSBLogger));
        public void LoggStatistics(string context, string lang, string db, string tableid, string actionType,string actionName, int numberOfCells,int nunmerOfContents, bool cached)
        {
            string cacheNumeric = cached  ? "1" : "0";
            string system = System.Configuration.ConfigurationManager.AppSettings["system"];
            log4net.LogicalThreadContext.Properties["System"] = system;
            log4net.LogicalThreadContext.Properties["Context"] = context;
            log4net.LogicalThreadContext.Properties["Lang"] = lang;
            log4net.LogicalThreadContext.Properties["Database"] = db;
            log4net.LogicalThreadContext.Properties["TableId"] = tableid;
            log4net.LogicalThreadContext.Properties["ActionType"] = actionType;
            log4net.LogicalThreadContext.Properties["ActionName"] = FiletypeConverter(actionName);
            log4net.LogicalThreadContext.Properties["NumberOfCells"] = numberOfCells;
            log4net.LogicalThreadContext.Properties["NumberOfContents"] = nunmerOfContents;
            log4net.LogicalThreadContext.Properties["Cached"] = cacheNumeric;

            //Logs usage

            _logger.Info(String.Format("system={0}, type=data, context={2}, Lang={3}, Database={4}, Tableid{5}, db={4},tableid={5},ActionType={6},ActionName={7},NumberOfCells={8},NumberOfContents={9},Cached={10}", system , context, actionType, lang, db, tableid, actionType, actionName, numberOfCells, nunmerOfContents, cached));
        }

        private string FiletypeConverter(string format)
        {
            switch (format)
            {
                case "table":
                    return "p01";
                case "FileTypePX":
                    return "p05";
                case "FileTypeExcelX":
                    return "p11";
                case "FileTypeExcelXDoubleColumn":
                    return "p14";
                case "FileTypeJson":
                    return "p06";
                case "FileTypeJsonStat":
                    return "p07";
                case "FileTypeJsonStat2":
                    return "p08";
                case "FileTypeRelational":
                    return "p30";
                case "FileTypeHtml":
                    return "p31";
                case "FileTypeCsvWithoutHeadingAndTabulator":
                    return "p22";
                case "FileTypeCsvWithHeadingAndSemiColon":
                    return "p27";
            }
        return "p99";
    }
    }
}
