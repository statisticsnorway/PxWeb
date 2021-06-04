﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norway.LogVisitorStatistics
{
    public static class SavedQueryHelper
    {
         static IQuerylogger _logger;
         static log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(SavedQueryHelper));



        public static void LoggStatistics(string context, string lang, string db, string tableid, string actionType, string actionName, int numberOfCells, int nunmerOfContents, bool cached)
        {

            if (_logger == null)
            {
                CreateLogger();
        }
            if (!(_logger == null))
            {
                try
                {
                    _logger.LoggStatistics(context, lang, db, tableid, actionType, actionName, numberOfCells, nunmerOfContents, cached);
                }
                catch (Exception ex)
                {
                    _log4net.ErrorFormat("Failed to log statistics for database {0} : {1}", db, ex.Message);
                }
            }

        }

        static IQuerylogger Logger
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
    
        public static void CreateLogger()
        {
            string loggerTypeStr = "";
            System.Type loggerType = null;

            if (!String.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["visitorStatisticsQueryLogger"]))
            {
                loggerTypeStr = System.Configuration.ConfigurationManager.AppSettings["visitorStatisticsQueryLogger"];
                try
                {
                    loggerType = System.Type.GetType(loggerTypeStr);
                   _logger = (IQuerylogger)Activator.CreateInstance(loggerType);
                    _log4net.Info("Visitor statistics logger of type  '" + loggerType.ToString() + "' was created successfully");
                }

                catch (Exception ex)
                {
                    _log4net.Info("Unabled to create visitor statistics logger of type  '" + loggerTypeStr + "' " + ex.Message);
                }
            }
            else
            {
                _logger = new QueryDefaultLogger();
                _log4net.Info("Visitor statistics logger of type 'ApiDefaultLogger' was created successfully");
            }

            }
            


        }
    }
