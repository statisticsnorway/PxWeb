using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PXWeb;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using PCAxis.Web.Core.Management;
using PCAxis.Sql.DbConfig;

namespace PXAxis.Routing.SSB
{
    internal class SSBUrl
    {
        public const string SitePathStart = "statbank";
        public const string TABELL_REFIRECT_IDENTIFER = "tabell"; //soon removed?

        public const string LIST_IDENTIFIER = "list";
        public const string TABLE_IDENTIFER = "table";
        public const string SAVEDQUERY_IDENTIFER = "sq";
        
        public const string VIEW_FOOTNOTES_IDENTIFIER = "footnotes";
        public const string VIEW_INFORMATION_IDENTIFIER = "information";
        public const string VIEW_TIPS_IDENTIFIER = "tips";
        public const string TableListNameOrTableId_KEY = "TABLELISTORTABLEID_KEY";//soon removed?

        //public const string TableId_KEY = "TABLEID_KEY";
        public const string TableIdOrName_KEY = "TABLEIDORNAME_KEY";
        public const string TableListName_KEY = "TABLELIST_KEY";
        public const string QueryName_KEY = "QueryName";

        private static HashSet<string> tableViewHashSet;
        private static HashSet<string> chartViewHashSet;

        public static HashSet<string> GetTableViewHashSet()
        {
            if (tableViewHashSet == null)
            {
                var result = new HashSet<string>();

                result.Add(PCAxis.Web.Controls.Plugins.Views.TABLE_LAYOUT1);
                result.Add(PCAxis.Web.Controls.Plugins.Views.TABLE_LAYOUT2);

                var lowerceaseResult = new HashSet<string>();

                foreach (var view in result)
                {
                    lowerceaseResult.Add(view.ToLower());
                }

                tableViewHashSet = lowerceaseResult;
            }

            return tableViewHashSet;
        }

        public static HashSet<string> GetChartViewHashSet()
        {
            if (chartViewHashSet == null)
            {
                var result = new HashSet<string>();

                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_AREA);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_AREASTACKED);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_AREASTACKED100);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_BAR);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_BARSTACKED);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_BARSTACKED100);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_COLUMN);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_COLUMNSTACKED100);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_LINE);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_PIE);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_POINT);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_POPULATIONPYRAMID);
                result.Add(PCAxis.Web.Controls.Plugins.Views.CHART_RADAR);

                var lowerceaseResult = new HashSet<string>();

                foreach (var view in result)
                {
                    lowerceaseResult.Add(view.ToLower());
                }

                chartViewHashSet = lowerceaseResult;
            }

            return chartViewHashSet;
        }

        public static string GeTableSortedIdentifier()
        {
            return PCAxis.Web.Controls.Plugins.Views.TABLE_SORTED;
        }

        public static bool IsTableLayout(string view)
        {
            return GetTableViewHashSet().Contains(view.ToLower());
        }

        public bool IsChartLayout(string view)
        {
            return GetChartViewHashSet().Contains(view.ToLower());
        }

        public bool IsTableSortedLayout(string view)
        {
            return view.ToLower() == PCAxis.Web.Controls.Plugins.Views.TABLE_SORTED.ToLower();
        }
    }

    internal class GotoTableListOrTableHttpHandler : IHttpHandler
    {
        private ISSBRouteExtender _routeExtender;
        
        public GotoTableListOrTableHttpHandler(ISSBRouteExtender routeExtender, string language)
        {
            if (routeExtender == null) throw new ArgumentNullException("routeExtender");

            _routeExtender = routeExtender;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var routeData = context.Items["RouteData"] as RouteData;
            string tableIdOrTableListName = ValidationManager.GetValue(routeData.Values[SSBUrl.TableListNameOrTableId_KEY] as string);
            string redirectUrl = _routeExtender.DefaultRedirectPage;


            if (!string.IsNullOrEmpty(tableIdOrTableListName))
            {
                if (!tableIdOrTableListName.Any(x => !Char.IsDigit(x)))
                {
                    string tableId = tableIdOrTableListName;

                    redirectUrl = _routeExtender.GetSelectionUrl(tableId);
                }
                else
                {
                    string tableListName = tableIdOrTableListName;
                    
                    redirectUrl = _routeExtender.GetListUrl(tableListName);
                }
            }

            StringBuilder url = new StringBuilder();

            url.Append(SSBLinkManager.GetVirtualPath());
            url.Append("/");
            url.Append(redirectUrl);

            context.Response.Redirect(url.ToString());
        }
    }

    internal class GotoTableListOrTableRouteHandler : IRouteHandler
    {
        private ISSBRouteExtender _routeExtender;

        public GotoTableListOrTableRouteHandler(ISSBRouteExtender routeExtender)
        {
            if (routeExtender == null) throw new ArgumentNullException("routeExtender");

            _routeExtender = routeExtender;
        }
        
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            // Store the route data on the requestcontext so we can avoid state class-variables.
            requestContext.HttpContext.Items["RouteData"] = requestContext.RouteData;
            string language = RouteInstance.RouteExtender.GetLanguageFromUri(requestContext.HttpContext.Request.Url);

            return new GotoTableListOrTableHttpHandler(_routeExtender, language);
            
        }
    }

    internal class DefaultHttpHandler : IHttpHandler
    {
        private IRouteExtender _routeExtender;
        private string _language;

        public DefaultHttpHandler(IRouteExtender routeExtender, string language)
        {
            if (routeExtender == null) throw new ArgumentNullException("routeExtender");

            _routeExtender = routeExtender;
            _language = language;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.Redirect(_routeExtender.DefaultRedirectPage);
        }
    }

    internal class DefaultRouteHandler : IRouteHandler
    {
        private DefaultHttpHandler _defaultHttpHandler;

        public DefaultRouteHandler(IRouteExtender routeExtender, string language)
        {
            if (routeExtender == null) throw new ArgumentNullException("routeExtender");

            _defaultHttpHandler = new DefaultHttpHandler(routeExtender, language);
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            // Store the route data on the requestcontext so we can avoid state class-variables.
            requestContext.HttpContext.Items["RouteData"] = requestContext.RouteData;
            return _defaultHttpHandler;
        }
    }
    
    public class RouteExtender : ISSBRouteExtender
    {
        internal static ISSBRouteExtender Instance
        {
            get
            {
                return (ISSBRouteExtender)RouteInstance.RouteExtender;
            }
        }

        private const int TableListIndex = 3;

        public string SitePathStart
        {
            get
            {
                return SSBUrl.SitePathStart;
            }
        }

        public ICacheService MetaCacheService { get; set; }
        public string HomeSitePage { get; set; }
        public string DefaultRedirectPage { get; set; }

        public SqlDbConfig Db { get; set; }

        public LinkManager.LinkMethod CreateLink
        {
            get
            {
                return SSBLinkManager.CreateLink;
            }
        }

        public IPxUrlProvider PxUrlProvider
        {
            get
            {
                return new SSBPxUrlProvider();
            }
        }

        public void RedirectMenuRoutePath(string language, string tableListName)
        {
            //for our case, language will make no difference for redirect url
            string path = GetListUrl(tableListName);

            var url = new StringBuilder();

            url.Append(SSBLinkManager.GetVirtualPath());
            url.Append("/");
            url.Append(path);

            System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;
            page.Response.Redirect(url.ToString(), true);
        }

        public string GetListUrl()
        {
            return string.Format("{0}/", SSBUrl.LIST_IDENTIFIER) + "{" + SSBUrl.TableListName_KEY +  "}/";
        }

        public string GetListUrl(string tableListName)
        {
            return string.Format("{0}/{1}/", SSBUrl.LIST_IDENTIFIER, tableListName);
        }

        public string GetSelectionUrl()
        {
            return string.Format("{0}/", SSBUrl.TABLE_IDENTIFER) + "{" + SSBUrl.TableIdOrName_KEY + "}/";
        }

        public string GetNoDataUrl()
        {
            return "nodata/";
        }

        public string GetSelectionUrl(string tableId)
        {
            return string.Format("{0}/{1}/", SSBUrl.TABLE_IDENTIFER, tableId);
        }

        public string GetSelectionInformationUrl()
        {
            return GetSelectionUrl() + SSBUrl.VIEW_INFORMATION_IDENTIFIER + "/";
        }

        public string GetSelectionFootnotesUrl()
        {
            return GetSelectionUrl() + SSBUrl.VIEW_FOOTNOTES_IDENTIFIER + "/";
        }

        public string GetSelectionTipsUrl()
        {
            return GetSelectionUrl() + SSBUrl.VIEW_TIPS_IDENTIFIER + "/";
        }
        
        public string GetPresentationUrl()
        {
            return GetSelectionUrl() + "/{" + SSBPxUrl.LAYOUT_IDENTIFIER + "}/";
        }

        public IEnumerable<string> GetTablePresentationUrls()
        {
            var result = new List<string>();
            
            foreach (var tableLayoutIdentifer in SSBUrl.GetTableViewHashSet())
            {
                result.Add(GetSelectionUrl() + string.Format("{0}/", tableLayoutIdentifer));
            }

            return result;
        }

        public IEnumerable<string> GetAllPresentationUrls()
        {
            var result = GetTablePresentationUrls().Union(GetChartPresentationUrls()).ToList();
            result.Add(GetTableSortedPresentationUrl());

            return result;
        }
        
        public IEnumerable<string> GetChartPresentationUrls()
        {
            var result = new List<string>();
            
            foreach (var chartLayoutIdentifer in SSBUrl.GetChartViewHashSet())
            {
                result.Add(GetSelectionUrl() + string.Format("{0}/", chartLayoutIdentifer));
            }

            return result;
        }
        public string GetTableSortedPresentationUrl()
        {
            return GetSelectionUrl() + string.Format("{0}/", SSBUrl.GeTableSortedIdentifier());
        }

        public string GetPresentationUrl(string tableId, string presentationLayout)
        {
            return GetSelectionUrl(tableId) + string.Format("{0}/", presentationLayout);
        }
        public string GetPresentationRedirectUrl(string tableId, string presentationLayout)
        {
            string presentationPath = GetPresentationUrl(tableId, presentationLayout);

            var pxUrl = RouteInstance.PxUrlProvider.Create(null);

            string redirectPath = string.Format("{0}/", presentationPath);

            StringBuilder url = new StringBuilder();
            
            url.Append(SSBLinkManager.GetVirtualPath());
            url.Append("/");
            url.Append(redirectPath);

            return url.ToString();
        }

        public string GetSavedQueryUrl()
        {
            return string.Format("{0}/", SSBUrl.SAVEDQUERY_IDENTIFER) + "{" + SSBUrl.QueryName_KEY + "}/";
        }

        public string GetSavedQueryUrl(string savedQueryId)
        {
            return string.Format("{0}/{1}", SSBUrl.SAVEDQUERY_IDENTIFER, savedQueryId);
        }

        private string GetTableListOrTableIdNorwegianUrl()
        {
            return SSBUrl.TABELL_REFIRECT_IDENTIFER + "/{" + SSBUrl.TableListNameOrTableId_KEY + "}/";
        }
        
        public string GetDatabase()
        {
            if (Db == null) throw new Exception("Db property not set!");

            return Db.Database.id;
        }

        public bool ShowBreadcrumb()
        {
            return false;
        }

        public string GetSavedQueryPath(string language, string queryId)
        {
            //langauge irrelevant for us
            return GetSavedQueryUrl(queryId);
        }

        public void AddSavedQueryRoute(RouteCollection routes)
        {
            string urlFormat = "SQ/{QueryName}.{Format}/";
            string url = "SQ/{QueryName}/";

            using (RouteTable.Routes.GetWriteLock())
            {
                RouteTable.Routes.Add(new Route
                (
                        urlFormat, new PXWeb.Management.SavedQueryRouteHandler()
                ));
                RouteTable.Routes.Add(new Route
                (
                        url, new PXWeb.Management.SavedQueryRouteHandler()
                ));
                RouteTable.Routes.Add(new Route
                (
                     "SQ", new PXWeb.Management.SavedQueryRouteHandler()
                ));
            }
        }

        public void RegisterCustomRoutes(RouteCollection routes)
        {
            string tableListOrTableIdNorwegianUrl = GetTableListOrTableIdNorwegianUrl();
            
            string listUrl = GetListUrl();
            string selectionUrl = GetSelectionUrl();
            string noDataUrl = GetNoDataUrl();
            string selectionFootnotesUrl = GetSelectionFootnotesUrl();
            string selectionInformationUrl = GetSelectionInformationUrl();
            string selectionTipsUrl = GetSelectionTipsUrl();
            var presentationTableUrls = GetTablePresentationUrls();
            var presentationChartUrls = GetChartPresentationUrls();

            using (RouteTable.Routes.GetWriteLock())
            {
                RouteTable.Routes.Add(new Route("", new DefaultRouteHandler(this, "no")));
                RouteTable.Routes.Add(new Route(tableListOrTableIdNorwegianUrl, new GotoTableListOrTableRouteHandler(this)));

                RouteTable.Routes.MapPageRoute("CustomListRoute", listUrl, "~/TableList.aspx");

                RouteTable.Routes.MapPageRoute("CustomSelectionRoute", selectionUrl, "~/Selection.aspx");
                RouteTable.Routes.MapPageRoute("NoDataRoute", noDataUrl, "~/NoData.aspx");
                
                RouteTable.Routes.MapPageRoute("CustomSelectionInformationRoute", selectionInformationUrl, "~/InformationSelection.aspx");
                RouteTable.Routes.MapPageRoute("CustomSelectionFootnotesRoute", selectionFootnotesUrl, "~/FootnotesSelection.aspx");
                RouteTable.Routes.MapPageRoute("CustomSelectionTipsRoute", selectionTipsUrl, "~/MarkingTips.aspx");
                
                foreach (var presentationTableUrl in presentationTableUrls)
                {
                    RouteTable.Routes.MapPageRoute("CustomPresentationTableRoute" + presentationTableUrl, presentationTableUrl, "~/Table.aspx");
                }

                foreach (var presentationChartUrl in presentationChartUrls)
                {
                    RouteTable.Routes.MapPageRoute("Custom" + presentationChartUrl + "Route", presentationChartUrl, "~/Chart.aspx");
                }

                RouteTable.Routes.MapPageRoute("CustomSortedTablePresentationRoute", GetTableSortedPresentationUrl(), "~/DataSort.aspx");
                
                foreach (var presentationUrl in GetAllPresentationUrls())
                {
                    RouteTable.Routes.MapPageRoute("Custom" + presentationUrl + "FootnoteRoute", presentationUrl + SSBUrl.VIEW_FOOTNOTES_IDENTIFIER, "~/FootnotesPresentation.aspx");
                    RouteTable.Routes.MapPageRoute("Custom" + presentationUrl + "InformationRoute", presentationUrl + SSBUrl.VIEW_INFORMATION_IDENTIFIER, "~/InformationPresentation.aspx");
                }
            }
        }
        
        public string GetTableListPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            
            var menuNodes = path.Split(new string[] { PCAxis.Web.Controls.PathHandler.NODE_DIVIDER }, StringSplitOptions.RemoveEmptyEntries);

            if (menuNodes.Length > TableListIndex)
            {
                return string.Join(PCAxis.Web.Controls.PathHandler.NODE_DIVIDER, menuNodes.Take(TableListIndex + 1).ToArray());
            }

            return null;
        }

        public string GetLastNodeFromPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return path.Split(new string[] { PCAxis.Web.Controls.PathHandler.NODE_DIVIDER }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        }
        
        private static string GetConnectionString()
        {
            return RouteExtender.Instance.Db.GetInfoForDbConnection(null, null).ConnectionString;
        }

        private static string GetMetatablesSchema()
        {
            return RouteExtender.Instance.Db.MetatablesSchema;
        }

        private const string TableIdByLowercaseNameCacheBase = "routeextender#tableidbylowercasename#{0}";

        private static string GetTableIdByNameFromCache(string tablename)
        {
            if (RouteInstance.RouteExtender.MetaCacheService == null) return null;

            string cacheKey = string.Format(TableIdByLowercaseNameCacheBase, tablename);
            return RouteInstance.RouteExtender.MetaCacheService.Get<string>(cacheKey);
        }

        private static void SetTableIdByNameInCache(string tableName, string tableId)
        {
            if (RouteInstance.RouteExtender.MetaCacheService == null) return;

            string cacheKey = string.Format(TableIdByLowercaseNameCacheBase, tableName);
            RouteInstance.RouteExtender.MetaCacheService.Set(cacheKey, tableId);
        }

        public string GetTableIdByName(string tablename)
        {
            string cacheResult = GetTableIdByNameFromCache(tablename);
            if (!string.IsNullOrEmpty(cacheResult)) return cacheResult;

            string sql = string.Format("select TABLEID from {0}MAINTABLE WHERE UPPER(MAINTABLE) = UPPER(:MAINTABLE)", GetMetatablesSchema());

            using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(GetConnectionString()))
            {
                conn.Open();

                using (Oracle.ManagedDataAccess.Client.OracleCommand cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add("MAINTABLE", tablename);

                    var tableId = cmd.ExecuteScalar() as string;

                    if (!string.IsNullOrEmpty(tableId))
                    {
                        SetTableIdByNameInCache(tablename, tableId);
                    }

                    return tableId;
                }
            }
        }

        public bool HasTableData(string tableId)
        {
            //Naturally no cache for this kind of information
            string sql = string.Format("SELECT COUNT(*) from {0}CONTENTSTIME C INNER JOIN {0}MAINTABLE M on C.MAINTABLE = M.MAINTABLE WHERE TABLEID = :TABLEID", GetMetatablesSchema());

            using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(GetConnectionString()))
            {
                conn.Open();

                using (Oracle.ManagedDataAccess.Client.OracleCommand cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add("TABLEID", tableId);

                    var contentsTimeCount = System.Convert.ToInt64(cmd.ExecuteScalar());
                    return contentsTimeCount > 0;
                }
            }
        }

        public string GetRedirectNoDataPath(string tableId)
        {
            string redirectUrl = GetNoDataUrl();

            StringBuilder url = new StringBuilder();

            url.Append(SSBLinkManager.GetVirtualPath());
            url.Append("/");
            url.Append(redirectUrl);
            url.Append("?tableId=");
            url.Append(tableId);

            return url.ToString();
        }

        public string GetLanguageFromUri(Uri uri)
        {
            string fullUrl = uri.OriginalString;
            var fullUrlPathParts = fullUrl.Split('/').Select(x => ValidationManager.GetValue(x)).ToArray();

            if (fullUrlPathParts.Any(x => x.ToLower() == SSBUrl.SitePathStart))
            {
                for (int i = 0; i < fullUrlPathParts.Length; i++)
                {
                    string fullUrlPathPart = fullUrlPathParts[i];

                    if (fullUrlPathPart.ToLower() == SSBUrl.SitePathStart.ToLower() && i > 0)
                    {
                        string maybeLanguage = fullUrlPathParts[i - 1];

                        if (maybeLanguage.ToLower() == "en")
                        {
                            return "en";
                        }

                        return "no";
                    }
                }
            }

            return "no";
        }

        public bool DoesSelectionPathContainTableName(System.Web.UI.Page page)
        {
            string tableIdOrName = ValidationManager.GetValue(page.RouteData.Values[SSBUrl.TableIdOrName_KEY] as string);
            
            if (!string.IsNullOrEmpty(tableIdOrName))
            {
                if (tableIdOrName.Any(x => !Char.IsDigit(x))) return true;
            }

            return false;
        }

        private const string MenuDepricatedCacheKeyPrefix = "menuDepricated#";

        public bool IsParentMenuItemDeprecated(string menu, string selection)
        {
            var cacheKey = String.Format("{0}#{1}#{2}", MenuDepricatedCacheKeyPrefix, menu, selection);
            var cachedValue = MetaCacheService.Get<string>(cacheKey);

            if (cachedValue != null) return bool.Parse(cachedValue);

            string sql = string.Format("select count(*) from O_STATMETA.menyval2 where presentation = 'A' and (nivanr = 4 or nivanr = 5) and meny = :MENU and val = :SELECTION");

            using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(GetConnectionString()))
            {
                conn.Open();

                using (Oracle.ManagedDataAccess.Client.OracleCommand cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add("MENU", menu);
                    cmd.Parameters.Add("SELECTION", selection);

                    var depricatedCount = System.Convert.ToInt64(cmd.ExecuteScalar());
                    bool isDepricated = depricatedCount > 0;

                    MetaCacheService.Set(cacheKey, isDepricated.ToString());
                    return isDepricated;
                }
            }
        }

        public string GetSelectionRedirectUrl(string tableId)
        {
            string selectionPath = GetSelectionUrl(tableId);

            var pxUrl = RouteInstance.PxUrlProvider.Create(null);

            string redirectPath = string.Format("{0}/", selectionPath);

            StringBuilder url = new StringBuilder();

            url.Append(SSBLinkManager.GetVirtualPath());
            url.Append("/");
            url.Append(redirectPath);

            return url.ToString();
        }
    }
}
