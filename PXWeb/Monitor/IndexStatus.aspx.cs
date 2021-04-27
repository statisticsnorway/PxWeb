using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using System.Web.Configuration;
using System.Globalization;


namespace PXWeb.Monitor
{
    public partial class IndexStatus : System.Web.UI.Page
    {
        private static string configPath;
        private static string tableDir;
        private static log4net.ILog _logger;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (VerifyConfigFile())
            {
                checkIndexStatus();
            }
        }

        private static bool VerifyConfigFile()
        {
            tableDir = WebConfigurationManager.AppSettings["tableDirectoryLucene"];
            if (File.Exists(Path.Combine(tableDir, "database.config")))
            {
                configPath = Path.Combine(tableDir, "database.config");
                return true;
            }
            else
            {
                _logger.Error("Non-existent database.config file");
                return false;
            }
        }
        private void checkIndexStatus()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(configPath);
            XmlNode node = xdoc.SelectSingleNode("/settings/searchIndex/status");

            if (node == null)
            {
                _logger.Error("checkIndexStatus: Could not get search index status");

            }
            txtIndexStatus.Text = node.InnerText;
            if (node.InnerText == "Indexed")
            {
                node = xdoc.SelectSingleNode("/settings/searchIndex/indexUpdated");

                if (node == null)
                {
                    _logger.Error("checkIndexStatus: Could not get search index updated");
                    throw new ApplicationException();
                }
                else
                {
                    txtIndexStatus.CssClass = "indexOK";
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime lastUpdated;
                    if (DateTime.TryParseExact(node.InnerText.ToString(), "yyyyMMdd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out lastUpdated))
                    {
                        txtLastIndexed.Text = lastUpdated.ToString();
                    }
                    else
                    {
                        txtLastIndexed.Text = node.InnerText.ToString();
                    }
                    txtLastIndexed.CssClass = "indexOK";                   
                }
            }
            else
            {
                txtIndexStatus.CssClass = "indexError";
                txtLastIndexed.Text = "----------";
                txtLastIndexed.CssClass = "indexError";
            }
        }
    }
}
