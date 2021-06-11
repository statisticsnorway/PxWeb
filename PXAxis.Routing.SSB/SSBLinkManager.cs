using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PXWeb;
using System.Web;
using PCAxis.Web.Core.Management;

namespace PXAxis.Routing.SSB
{
    /// <summary>
    /// Class for generating user friendly links (URLs) in PX-Web
    /// </summary>
    public class SSBLinkManager
    {
        private const string DEFAULT_lANGUAGE = "no";

        public static string GetVirtualPath()
        {
            string path = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;

            if (path.Length == 1 && path.StartsWith("/"))
            {
                path = path.TrimStart('/');
            }
            return path;
        }

        public static string CreateLink(string page, bool formatHtmlEntities, params PCAxis.Web.Core.Management.LinkManager.LinkItem[] links)
        {
            IPxUrl url = RouteInstance.PxUrlProvider.Create(links);

            if (page != null)
            {
                if (page.Contains("Default.aspx"))
                {
                    url.Table = null;
                    url.View = null;
                    url.Layout = null;
                }
                else if (page.Contains("Menu.aspx"))
                {
                    url.Table = null;
                    url.View = null;
                    url.Layout = null;
                }
                else if (page.Contains("Search.aspx"))
                {
                    url.Table = null;
                    url.View = null;
                    url.Layout = null;
                }
                else if (page.Contains("Selection.aspx"))
                {
                    url.View = null;
                    url.Layout = SSBPxUrl.NoLayout;
                }
            }

            // Analyse and call the right link method
            if (url.Database == null)
            {
                return "http://i.ssb.no/";
            }
            else if (url.Database != null && url.Table == null && url.Path != null)
            {
                return CreateMenuLink(page);
            }
            else if (url.Table != null && url.Layout == null)
            {
                return CreateSelectionLink(page, url);
            }
            else if (url.Database != null && url.Table != null && url.Layout != null)
            {
                return CreatePresentationLink(page, url);
            }

            return "Default.aspx";
        }

        /// <summary>
        /// Create link to the start page
        /// </summary>
        /// <returns></returns>
        private static string CreateDefaultLink(IPxUrl pxUrl)
        {
            StringBuilder url = new StringBuilder();
            url.Append(GetVirtualPath());
            url.Append("/");
            url.Append(PxUrl.PX_START + "/");

            if (pxUrl.Language != PXWeb.Settings.Current.General.Language.DefaultLanguage)
            {
                url.Append(pxUrl.Language + "/");
            }
            
            AddQuerystringParameters(pxUrl, url);

            return url.ToString();
        }

        private static string GetTableListNameFromTablePath(string tablePath)
        {
            string tableListPath = RouteInstance.RouteExtender.GetTableListPath(tablePath);
            return RouteInstance.RouteExtender.GetLastNodeFromPath(tableListPath);
        }

        /// <summary>
        /// Create link to the menu page
        /// </summary>
        /// <returns></returns>
        private static string CreateMenuLink(string page)
        {
            StringBuilder url = new StringBuilder();

            var pxUrl = RouteInstance.PxUrlProvider.Create(null);
            if (string.IsNullOrEmpty(pxUrl.Table)) return "";
            
            var tableListName = GetTableListNameFromTablePath(pxUrl.Path);
            var menuPath = RouteExtender.Instance.GetListUrl(tableListName);

            url.Append(GetVirtualPath());
            url.Append("/");
            url.Append(menuPath);

            return url.ToString();
        }

        /// <summary>
        /// Create link to the selection page
        /// </summary>
        /// <returns></returns>
        private static string CreateSelectionLink(string page, IPxUrl pxUrl)
        {
            StringBuilder url = new StringBuilder();

            url.Append(GetVirtualPath());
            url.Append("/");

            System.Web.UI.Page pageHandler = HttpContext.Current.Handler as System.Web.UI.Page;
            
            string tableId = null;
            string tableName = null;
            string tableIdOrName = ValidationManager.GetValue(pageHandler.RouteData.Values[SSBUrl.TableIdOrName_KEY] as string);

            if (string.IsNullOrEmpty(tableIdOrName))
            {
                tableId = RouteInstance.RouteExtender.GetTableIdByName(pxUrl.Table);
            }
            else
            {
                if (tableIdOrName.Any(x => !Char.IsDigit(x)))
                {
                    tableName = tableIdOrName;
                }
                else
                {
                    tableId = tableIdOrName;
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    tableId = RouteInstance.RouteExtender.GetTableIdByName(tableName);
                }
            }

            url.Append(RouteExtender.Instance.GetSelectionUrl(tableId));
            
            // Check if it is a selection sub page
            if (page != null)
            {
                if (page.Contains(".aspx"))
                {
                    if (page.Contains("InformationSelection.aspx") || page.Contains("FootnotesSelection.aspx") || page.Contains("MarkingTips.aspx"))
                    {
                        url.Append(GetSelectionView(page) + "/");
                    }
                }
                else
                {
                    //If the last part of the friendly URL is not the table it is the view of the selection sub page
                    char[] separator = { '/' };
                    string[] parts = page.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    if (parts[parts.Length - 1] != pxUrl.Table)
                    {
                        url.Append(parts[parts.Length - 1] + "/");
                    }
                }
            }

            AddQuerystringParameters(pxUrl, url);

            return url.ToString();
        }

        /// <summary>
        /// Create link to the presentation page
        /// </summary>
        /// <returns></returns>
        private static string CreatePresentationLink(string page, IPxUrl pxUrl)
        {
            StringBuilder url = new StringBuilder();

            url.Append(GetVirtualPath());
            url.Append("/");

            System.Web.UI.Page pageHandler = HttpContext.Current.Handler as System.Web.UI.Page;
            
            string tableId = ValidationManager.GetValue(pageHandler.RouteData.Values[SSBUrl.TableIdOrName_KEY] as string);
            url.Append(RouteExtender.Instance.GetPresentationUrl(tableId, pxUrl.Layout));
            
            AddQuerystringParameters(pxUrl, url);

            return url.ToString();
        }

        /// <summary>
        /// Get table name
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static string GetTableName(string table)
        {
            if (table.IndexOf(":") > 0)
            {
                // Remove database from CNMM table
                table = table.Substring(table.IndexOf(":") + 1);
            }
            return table;
        }


        /// <summary>
        /// Get view
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private static string GetSelectionView(string page)
        {
            if (page.Contains("MarkingTips.aspx"))
            {
                return PxUrl.VIEW_TIPS_IDENTIFIER;
            }
            else if (page.Contains("FootnotesSelection.aspx"))
            {
                return PxUrl.VIEW_FOOTNOTES_IDENTIFIER;
            }
            else if (page.Contains("InformationSelection.aspx"))
            {
                return PxUrl.VIEW_INFORMATION_IDENTIFIER;
            }

            return PxUrl.VIEW_FOOTNOTES_IDENTIFIER;
        }

        /// <summary>
        /// Get view
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private static string GetView(string page, string layout)
        {
            if (page.Contains("Table.aspx"))
            {
                return "";
            }
            else if (page.Contains("Chart.aspx"))
            {
                return PxUrl.VIEW_CHART_IDENTIFIER;
            }
            else if (page.Contains("InformationPresentation.aspx"))
            {
                return PxUrl.VIEW_INFORMATION_IDENTIFIER;
            }
            else if (page.Contains("DataSort.aspx"))
            {
                return PxUrl.VIEW_SORTEDTABLE_IDENTIFIER;
            }
            else if (page.Contains("/" + layout + "/"))
            {
                char[] separator = { '/' };
                string[] parts = page.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    if ((parts[i] == layout) && (i > 0))
                    {
                        return parts[i - 1];
                    }
                }
            }

            return "";

        }

        /// <summary>
        /// Add querysring parameters to URL
        /// </summary>
        /// <param name="pxUrl">PXUrl object</param>
        /// <param name="url">Stringbuilder object</param>
        private static void AddQuerystringParameters(IPxUrl pxUrl, StringBuilder url)
        {
            bool first = true;

            if (pxUrl.QuerystringParameters.Count > 0)
            {
                url.Append("?");

                foreach (KeyValuePair<string, string> param in pxUrl.QuerystringParameters)
                {
                    if (!first)
                    {
                        url.Append("&");
                    }

                    url.Append(param.Key + "=" + param.Value);
                    first = false;
                }
            }
        }
    }
}