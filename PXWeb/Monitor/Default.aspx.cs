using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Web.Configuration;
using System.Xml;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Data.Common;
using PCAxis.Sql.DbClient; //For executing SQLs.
using PCAxis.Sql.DbConfig; // ReadSqlDbConfig;


using log4net; 
namespace PxWeb.Monitor
{
    public partial class Default : System.Web.UI.Page
    {
        const string CssClassOK = "applicationOK";
        const string CssClassError = "applicationERROR";
        //string connectionStatus, connectionMessage;
        public string connectionMessage;
        public string connectionString;
        const int minLuceneLimit = 2;
        string tableMetaQuery;
        string apiServerUrl;
        string databaseId;
        int maxIndexAge = 25; //max age in hour of indexes 
        int maxLastRun = 25; //max time in hour since indexing was run.
        string luceneNoServerMessage, luceneEnServerMessage = "Nok entries?";
        string triggerFilMessage, indexDateNoMessage, indexDateEnMessage, indexingLastRunMessage, indexStatusMessage;

        private static log4net.ILog _logger = LogManager.GetLogger(typeof(Default));

        //Trigger reading of settings file
        PXWeb.Settings s = PXWeb.Settings.Current;

        protected void Page_Load(object sender, EventArgs e)
        {
            SetMonitorTitle();
            RunIntegrationTests();

            if (s.Features.General.ApiEnabled)
            {
                RunApiTests();
                ShowApiTests();
            }
            else
            {
                HideApiTests();
            }                
        }
        protected void RunIntegrationTests()
        {
            DatabaseConnectionTest();
            IndexTests();
        }

        protected void DatabaseConnectionTest()
        {

            string[] ConnectionResult = checkConnection();
            connection.CssClass = ConnectionResult[0];
            connectionMessage = ConnectionResult[1];
            connectionString = ConnectionResult[2];
        }

        protected void IndexTests()
        {

            maxIndexAge = int.Parse(WebConfigurationManager.AppSettings["MonitorMaxIndexAge"]);
            maxLastRun = int.Parse(WebConfigurationManager.AppSettings["MonitorMaxLastRun"]);
            string tableDir = WebConfigurationManager.AppSettings["MonitorTableDirectoryLucene"];

            var triggerResult = CheckTriggerFile(tableDir + "database.config");
            triggerFil.CssClass = triggerResult[0];
            triggerFilMessage = triggerResult[1];


            var indexDirResult = CheckIndexDir(tableDir + @"_INDEX\no");
            indexDateNo.CssClass = indexDirResult[0];
            indexDateNoMessage = indexDirResult[1];

            indexDirResult = CheckIndexDir(tableDir + @"_INDEX\en");
            indexDateEn.CssClass = indexDirResult[0];
            indexDateEnMessage = indexDirResult[1];

            var IndexingLastRunResult = CheckIndexingLastRun(tableDir + "database.config");
            IndexingLastRun.CssClass = IndexingLastRunResult[0];
            indexingLastRunMessage = IndexingLastRunResult[1];

            var IndexStatusResult = CheckIndexStatus(tableDir + "database.config");
            IndexStatus.CssClass = IndexStatusResult[0];
            indexStatusMessage = IndexStatusResult[1];
        }

        protected void RunApiTests()
        {
            apiServerUrl = WebConfigurationManager.AppSettings["MonitorApiUrlServer"];

            string[] luceneHealthResult = CheckLuceneHealth(apiServerUrl, "no");
            luceneNoServer.CssClass = luceneHealthResult[0];
            luceneNoServerMessage = luceneHealthResult[1];

            luceneHealthResult = CheckLuceneHealth(apiServerUrl, "en");
            luceneEnServer.CssClass = luceneHealthResult[0];
            luceneEnServerMessage = luceneHealthResult[1];
        }

        protected void HideApiTests()
        {
            luceneNoServer.Visible = false;
            luceneEnServer.Visible = false;
        }

        protected void SetMonitorTitle()
        {
            string title = WebConfigurationManager.AppSettings["MonitorTitle"];
            monitorTitle.Text = title;
        }

        protected void ShowApiTests()
        {
            luceneNoServer.Visible = true;
            luceneEnServer.Visible = true;
        }
     
        public string[] checkConnection()
        {
            string[] myReturn = new string[3];
            var monitorSQId = "20000006";
            try
            {
               
                DbConnectionStringBuilder aux = new DbConnectionStringBuilder();
                aux.ConnectionString = WebConfigurationManager.AppSettings["SavedQueryConnectionString"];
                aux.Remove("Password");
                string conString = aux.ToString();

               myReturn[2] = String.Format("Tester SQ:{0} mot connection {1}", monitorSQId, conString);

                var sq = PCAxis.Query.SavedQueryManager.Current.Load(monitorSQId);
                if (sq == null)
                {
                    throw new Exception("Cant load sq");
                }
            }
            catch (Exception ex)
            {
                myReturn[0] = CssClassError;
                myReturn[1] = "Connection failed";
                _logger.Error("Exception i monitor",ex);
                return myReturn;
            }
            myReturn[0] = CssClassOK;
            myReturn[1] = "Connection succeed";
            return myReturn;
        }

        public static string AssemblyVersion
        {
            get
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var displayVersion = version.Major + "." + version.Minor + "." + version.Build;

                if (version.Revision > 0)
                {
                    displayVersion += "." + version.Revision;
                }

                return displayVersion;
            }
        }

        public static string DotNetVersion
        {
            get { return Assembly.GetExecutingAssembly().ImageRuntimeVersion; }
        }

        private string[] CheckIndexDir(string indexDir)
        {
            string[] myOut = new string[2] { CssClassError, "Bad error!" };
            try
            {
                System.IO.DirectoryInfo triggerFileInfo = new DirectoryInfo(indexDir);
                DateTime lastWrite = triggerFileInfo.LastWriteTime;
                if (lastWrite.AddHours(maxIndexAge).CompareTo(DateTime.Now) > 0)
                {
                    myOut[0] = CssClassOK;
                    myOut[1] = "Directory " + indexDir + " ble skrevet til: " + lastWrite;
                }
                else
                {
                    throw new Exception("Ble siste skrevet til: " + lastWrite);
                }
            }
            catch (Exception e)
            {
                myOut[0] = CssClassError;
                myOut[1] = "Oppslag mot " + indexDir + " ga feilmelding: " + e.Message;
            }
            return myOut;
        }

        /// <summary>
        /// Checks that the triggerFile has Length > 0
        /// Returns string[] where index 0 is CssClass for ok/Error and
        /// index 1 is a message.
        /// </summary>
        /// <param name="triggerFile">The file to check</param>
        /// <returns></returns>
        private string[] CheckTriggerFile(string triggerFile)
        {
            string[] myOut = new string[2] { CssClassError, "Bad error!" };
            try
            {
                System.IO.FileInfo triggerFileInfo = new FileInfo(triggerFile);
                if (triggerFileInfo.Length > 0)
                {
                    myOut[0] = CssClassOK;
                    myOut[1] = "Fila " + triggerFile + " har length: " + triggerFileInfo.Length;
                }
                else
                {
                    throw new Exception("Fila er tom");
                }
            }
            catch (Exception e)
            {
                myOut[0] = CssClassError;
                myOut[1] = "Oppslag mot " + triggerFile + " ga feilmelding: " + e.Message;
            }
            return myOut;
        }

        private string[] CheckLuceneHealth(string apiUrl, string lang)
        {
            databaseId = WebConfigurationManager.AppSettings["MonitorDatabaseId"];
            string[] myOut = new string[3] { CssClassError, "Bad error!", "0" };
            string seachUrl = apiUrl + "/" + lang + "/" + databaseId + "/?query=*&filter=*";
            try
            {
                var client = new WebClient();
                client.Headers["Content_type"] = "application/json;charset=UTF-8";
                client.Encoding = System.Text.Encoding.UTF8;
                string json = client.DownloadString(seachUrl);
                int antall = json.Split('{').Length - 1;
                myOut[2] = antall.ToString();
                if (antall < minLuceneLimit)
                {
                    throw new Exception("Fant " + antall + ", skal være minst " + minLuceneLimit + ".");
                }
                myOut[1] = seachUrl + "Fant " + antall + ".";
                myOut[0] = CssClassOK;
            }
            catch (Exception e)
            {
                myOut[0] = CssClassError;
                myOut[1] = seachUrl + " ga feilmelding: " + e.Message;
            }
            return myOut;
        }

        private string[] CheckIndexingLastRun(string triggerFile)
        {
            string[] myOut = new string[3] { CssClassError, "Bad error!", "0" };


            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.Load(triggerFile);
            }
            catch(DirectoryNotFoundException e )
            {
                _logger.Error("checkIndexStatus: Could not get search index status. " + e.Message);
                myOut[1] = "Oppslag mot " + triggerFile + " ga feilmelding: " + e.Message;
                return myOut;
            }

            XmlNode node = xdoc.SelectSingleNode("/settings/searchIndex/status");

            if (node == null)
            {
                _logger.Error("checkIndexStatus: Could not get search index status");
            }
            node = xdoc.SelectSingleNode("/settings/searchIndex/indexUpdated");
            if (node == null)
            {
                _logger.Error("checkIndexStatus: Could not get search index updated");
                throw new ApplicationException();
            }
            else
            {
                DateTime lastUpdated;
                if (DateTime.TryParseExact(node.InnerText.ToString(), "yyyyMMdd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out lastUpdated))
                {
                    if (lastUpdated.AddHours(maxLastRun).CompareTo(DateTime.Now) > 0)
                    {
                        myOut[0] = CssClassOK;
                        myOut[1] = "Indekseringsjobb ble sist kjørt:" + lastUpdated.ToString();
                    }
                    else
                    {
                        myOut[0] = CssClassError;
                        myOut[1] = "Indekseringsjobb ble sist kjørt:" + lastUpdated.ToString() + " for gammel";
                    }

                }

            }
            return myOut;
        }

        private string[] CheckIndexStatus(string triggerFile)
        {
            string[] myOut = new string[3] { CssClassError, "Bad error!", "0" };

            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.Load(triggerFile);
            }
            catch (DirectoryNotFoundException e)
            {
                _logger.Error("checkIndexStatus: Could not get search index status. " + e.Message);
                myOut[1] = "Oppslag mot " + triggerFile + " ga feilmelding: " + e.Message;
                return myOut;
            }

            XmlNode node = xdoc.SelectSingleNode("/settings/searchIndex/status");

            if (node == null)
            {
                _logger.Error("checkIndexStatus: Could not get search index status");
            }
            else
            {
                if (node.InnerText.ToString() == "Failed")
                {
                    myOut[0] = CssClassError;
                }
                else
                {
                    myOut[0] = CssClassOK;
                }
                myOut[1] = "Indekseringsstatus: " + node.InnerText.ToString();
            }
            return myOut;
        }

        public string LuceneEnServerMessage
        {
            get { return WebUtility.HtmlEncode(luceneEnServerMessage); }
        }
        public string LuceneNoServerMessage
        {
            get { return WebUtility.HtmlEncode(luceneNoServerMessage); }
        }

        public string TriggerFilMessage
        {
            get { return WebUtility.HtmlEncode(triggerFilMessage); }
        }

        public string IndexDateNoMessage
        {
            get { return WebUtility.HtmlEncode(indexDateNoMessage); }
        }

        public string IndexDateEnMessage
        {
            get { return WebUtility.HtmlEncode(indexDateEnMessage); }
        }

        public string IndexingLastRunMessage
        {
            get { return WebUtility.HtmlEncode(indexingLastRunMessage); }
        }

        public string IndexStatusMessage
        {
            get { return WebUtility.HtmlEncode(indexStatusMessage); }
        }


    }
}