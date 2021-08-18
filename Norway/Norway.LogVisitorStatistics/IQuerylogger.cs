using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
     interface IQuerylogger
    {

        /// Log visitor statistics event to media (db, file...)
        /// </summary>
        /// <param name="type">data/meta/error</param>
        /// <param name="lang">Language</param>
        /// <param name="database">Database id</param>      
        /// <param name="tableid">Table Id</param>
        /// <param name="format">Format (JSON, PX...)</param>
        /// <param name="matrixsize">size of datapart returned</param>
        /// <param name="cache>true/false</param>
        /// <remarks></remarks>
        void LoggStatistics(string context, string lang, string db, string tableid, string actionType, string actionName, int numberOfCells, int nunmerOfContents, bool cached);
    }
}
