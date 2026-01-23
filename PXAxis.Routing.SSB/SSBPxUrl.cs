using Oracle.ManagedDataAccess.Client;
using PCAxis.Web.Core.Management;
using PXWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PXAxis.Routing.SSB
{
    public class SSBPxUrl : IPxUrl
    {
        #region "Constants"
        //Parts of the user friendly URL
        //------------------------------

        /// <summary>
        /// Start for all PX-Web URL:s
        /// </summary>
        public const string PX_START = "pxweb";

        /// <summary>
        /// Start for short-links
        /// </summary>
        public const string PX_GOTO = "goto";

        /// <summary>
        /// Defines language
        /// </summary>
        public const string LANGUAGE_IDENTIFIER = "lang";
        public const string LANGUAGE_KEY = "px_language";

        /// <summary>
        /// Defines database
        /// </summary>
        public const string DB_IDENTIFIER = "db";
        public const string DB_KEY = "px_db";

        /// <summary>
        /// Defines path within database
        /// </summary>
        public const string PATH_IDENTIFIER = "path";
        public const string PATH_KEY = "px_path";

        /// <summary>
        /// Defines table
        /// </summary>
        public const string TABLE_IDENTIFIER = "table";
        public const string TABLE_KEY = "px_tableid";
        /// <summary>
        /// Defines presentation view
        /// </summary>

        /// <summary>
        /// Defines presentation view layout
        /// </summary>
        public const string LAYOUT_IDENTIFIER = "layout";
        public const string LAYOUT_KEY = "layout";

        //Querystring parameters
        //----------------------


        #endregion

        #region "Private fields"

        /// <summary>
        /// Querystring parameters
        /// </summary>
        private List<KeyValuePair<string, string>> _params;

        #endregion

        private const string TableNameByIdCacheBase = "routeextender#tableid#{0}";
        private const string TableNameByLowercaseNameCaheBase = "routeextender#tablenamelowercase#{0}";
        private const string NodePathByNodeNameCacheBase = "routeextender#nodename#{0}";
        private const string HasPathTablesByPathBase = "routeextender#path#{0}";

        private static string GetTableNameByIdFromCache(string tableId)
        {
            return GetFromCache(TableNameByIdCacheBase, tableId);
        }

        private static void SetTableNameByIdFromCache(string tableId, string tableName)
        {
            SetInCache(TableNameByIdCacheBase, tableId, tableName);
        }

        private static string GetTableNameByLowercaseNameFromCache(string lowercaseName)
        {
            return GetFromCache(TableNameByLowercaseNameCaheBase, lowercaseName);
        }

        private static void SetTableNameByLowercaseNameFromCache(string lowercaseName, string tableName)
        {
            SetInCache(TableNameByLowercaseNameCaheBase, lowercaseName, tableName);
        }

        private static string GetNodePathByNodeNameFromCache(string nodeName)
        {
            return GetFromCache(NodePathByNodeNameCacheBase, nodeName);
        }

        private static void SetNodePathByNodeNameFromCache(string nodeName, string nodePath)
        {
            SetInCache(NodePathByNodeNameCacheBase, nodeName, nodePath);
        }

        private static bool? GetHasPathTablesByPathFromCache(string path)
        {
            var resultStr = GetFromCache(HasPathTablesByPathBase, path);
            if (resultStr == null) return null;
            return bool.Parse(resultStr);
        }

        private static void SetHasPathTablesByPathFromCache(string path, bool value)
        {
            SetInCache(HasPathTablesByPathBase, path, value.ToString());
        }

        private static string GetFromCache(string keyBase, string key)
        {
            if (RouteInstance.RouteExtender.MetaCacheService == null) return null;

            string cacheKey = string.Format(keyBase, key);
            return RouteInstance.RouteExtender.MetaCacheService.Get<string>(cacheKey);
        }

        private static void SetInCache(string keyBase, string key, string value)
        {
            if (RouteInstance.RouteExtender.MetaCacheService == null) return;

            string cacheKey = string.Format(keyBase, key);
            RouteInstance.RouteExtender.MetaCacheService.Set(cacheKey, value);
        }

        private static int? _topLevelNo;

        private static int TopLevelNo
        {
            get
            {
                if (_topLevelNo.HasValue) return _topLevelNo.Value;

                if (_topLevelNo == null)
                {
                    string sql = string.Format("select MA.VALUE from {0}METAADM MA where MA.PROPERTY = 'MENULEVELS'", GetMetatablesSchema());

                    using (var conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();

                        using (OracleCommand cmd = new OracleCommand(sql, conn))
                        {
                            _topLevelNo = int.Parse((string)cmd.ExecuteScalar());
                        }
                    }
                }

                if (!_topLevelNo.HasValue) throw new Exception("Could not find MENULEVELS in METAADM");

                return _topLevelNo.Value;
            }
        }

        #region "Public methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public SSBPxUrl()
        {
            _params = new List<KeyValuePair<string, string>>();

            SetDafaultValues();
            SetValuesBasedOnRoute();
        }

        /// <summary>
        /// Constructor. Initializes the PxUrl with the querystring parameters
        /// </summary>
        /// <param name="links">Array of querystring parameters</param>
        public SSBPxUrl(params PCAxis.Web.Core.Management.LinkManager.LinkItem[] links)
        {
            _params = new List<KeyValuePair<string, string>>();
            Dictionary<string, string> queries;

            // Get querystring items from subscribers
            queries = PCAxis.Web.Core.Management.LinkManager.GetQueries();
            foreach (KeyValuePair<string, string> querie in queries)
            {
                this.AddParameter(querie.Key, HttpUtility.UrlPathEncode(querie.Value));
            }

            SetDafaultValues();
            SetValuesBasedOnRoute();

            if (links != null)
            {
                foreach (PCAxis.Web.Core.Management.LinkManager.LinkItem itm in links)
                {
                    this.AddParameter(itm.Key, itm.Value);
                }
            }
        }

        private void SetDafaultValues()
        {
            Database = RouteInstance.RouteExtender.GetDatabase();
            Language = PXWeb.Settings.Current.General.Language.DefaultLanguage;
        }

        private void SetValuesBasedOnRoute()
        {
            System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;

            if (page != null)
            {
                string tableIdOrName = ValidationManager.GetValue(page.RouteData.Values[SSBUrl.TableIdOrName_KEY] as string);
                string tableListName = ValidationManager.GetValue(page.RouteData.Values[SSBUrl.TableListName_KEY] as string);

                string tableId = null;
                string tableName = null;

                if (!string.IsNullOrEmpty(tableIdOrName))
                {
                    if (tableIdOrName.Any(x => !Char.IsDigit(x)))
                    {
                        tableName = tableIdOrName;
                    }
                    else
                    {
                        tableId = tableIdOrName;
                    }
                }

                try
                {
                    if (!string.IsNullOrEmpty(tableId))
                    {
                        Table = GetTableById(tableId);
                        Path = GetPath(Table, false);
                    }
                    else if (!string.IsNullOrEmpty(tableName))
                    {
                        Table = GetTableByName(tableName);
                        Path = GetPath(Table, false);
                    }
                    else if (!string.IsNullOrEmpty(tableListName))
                    {
                        Path = GetPath(tableListName, true);

                        if (string.IsNullOrEmpty(Path))
                        {
                            throw new ArgumentException("Path cant be empty");
                        }
                        //TODO: Sjekk om det er mulig å ha gyldig tabell uten path for de to tilfellene over (tableId/tableName)
                    }
                }
                catch (Exception ex) when (ex is NullReferenceException || ex is ArgumentException)
                {
                    throw new HttpException(404, "HTTP/1.1 404 Not Found");
                }
            }
        }

        private static string GetConnectionString()
        {
            return RouteExtender.Instance.Db.GetInfoForDbConnection(null, null).ConnectionString;
        }

        private static string GetMetatablesSchema()
        {
            return RouteExtender.Instance.Db.MetatablesSchema;
        }


        private string GetTableByName(string tableName)
        {
            //table name is case sensitive in parser
            tableName = tableName.ToLower();

            var cacheResult = GetTableNameByLowercaseNameFromCache(tableName);
            if (!string.IsNullOrEmpty(cacheResult)) return cacheResult;

            string sql = string.Format("select MAINTABLE from {0}MAINTABLE WHERE UPPER(MAINTABLE) = UPPER(:MAINTABLE)", GetMetatablesSchema());

            using (var conn = new OracleConnection(GetConnectionString()))
            {
                conn.Open();

                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add("MAINTABLE", tableName);

                    var result = cmd.ExecuteScalar() as string;

                    if (!string.IsNullOrEmpty(result))
                    {
                        SetTableNameByLowercaseNameFromCache(tableName, result);
                    }

                    return result;
                }
            }
        }

        private string GetPath(string node, bool isTableList)
        {
            var result = GetPath(node, isTableList, true);

            if (result.StartsWith("START"))
            {
                return result;
            }
            else
            {
                return GetPath(node, isTableList, false);
            }
        }


        private string GetPath(string node, bool isTableList, bool onlyPreferredPlacement)
        {
            node = node.ToLower();
            string cacheResult = GetNodePathByNodeNameFromCache(node);
            if (!string.IsNullOrEmpty(cacheResult)) return cacheResult;

            var resultList = new List<string>();

            string onlyPreferredPlacementClause = "";

            if (onlyPreferredPlacement)
            {
                onlyPreferredPlacementClause = @"nvl(MS.prima_plass, 'H') <> 'N' AND";
            }

            string unformattedLeafMenuNodeSQL = @"select MS.SELECTION
                                                  from {0}MENUSELECTION_TAB MS
                                                  WHERE {1} MS.levelno < :MAXLEVELNO
                                                  AND UPPER(MS.SELECTION) = UPPER(:NODE)";

            if (!isTableList)
            {
                unformattedLeafMenuNodeSQL = @"select MS.MENU
                                                  from {0}MENUSELECTION_TAB MS
                                                  WHERE {1} MS.levelno = :MAXLEVELNO
                                                  AND UPPER(MS.SELECTION) = UPPER(:NODE)";
            }

            string leafMenuNodeSQL = string.Format(unformattedLeafMenuNodeSQL, GetMetatablesSchema(), onlyPreferredPlacementClause);


            using (var conn = new OracleConnection(GetConnectionString()))
            {
                conn.Open();

                using (OracleCommand cmd = new OracleCommand(leafMenuNodeSQL, conn))
                {
                    cmd.Parameters.Add("MAXLEVELNO", TopLevelNo);
                    cmd.Parameters.Add("NODE", node);

                    var selectionValue = cmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(selectionValue))
                    {
                        return "";
                        //return "START";
                    }

                    resultList.Add(selectionValue);
                }
            }

            string unformattedParentNodeSQL = @"select MS.MENU, MS.levelno
                                                 from {0}MENUSELECTION_TAB MS
                                                 where {1} UPPER(MS.SELECTION) = UPPER(:PARENTNODENAME)
                                                 and MS.levelno < :MAXLEVELNO ";

            string parentNodeSQL = string.Format(unformattedParentNodeSQL, GetMetatablesSchema(), onlyPreferredPlacementClause);

            string parentNodeName = resultList.Last();
            int loopCounter = 0;

            while (true)
            {
                loopCounter++;

                if (loopCounter >= TopLevelNo) break;

                using (var conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    using (OracleCommand cmd = new OracleCommand(parentNodeSQL, conn))
                    {
                        cmd.Parameters.Add("PARENTNODENAME", parentNodeName);
                        cmd.Parameters.Add("MAXLEVELNO", TopLevelNo);

                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                var nextParentNodeName = r["MENU"] as string;
                                var levelno = System.Convert.ToInt32(r["levelno"]);

                                if (levelno == 1)
                                {
                                    resultList.Add(nextParentNodeName);
                                    break;
                                }
                                if (nextParentNodeName == parentNodeName) break;

                                parentNodeName = nextParentNodeName;
                                resultList.Add(parentNodeName);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            string pathResult = string.Join(PCAxis.Web.Controls.PathHandler.NODE_DIVIDER, resultList.ToArray().Reverse());

            if (pathResult.StartsWith("START"))
            {
                SetNodePathByNodeNameFromCache(node, pathResult);
            }

            return pathResult;
        }

        private string GetTableById(string tableId)
        {
            string cacheResult = GetTableNameByIdFromCache(tableId);
            if (!string.IsNullOrEmpty(cacheResult)) return cacheResult;

            string sql = string.Format("select MAINTABLE from {0}MAINTABLE WHERE TABLEID = :TABLEID", GetMetatablesSchema());

            using (var conn = new OracleConnection(GetConnectionString()))
            {
                conn.Open();

                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    cmd.Parameters.Add("TABLEID", tableId);

                    var result = cmd.ExecuteScalar() as string;

                    if (!string.IsNullOrEmpty(result))
                    {
                        SetTableNameByIdFromCache(tableId, result);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Add parameter to URL
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddParameter(string key, string value)
        {
            // Check that the value do not contain illegal characters
            if (!QuerystringManager.CheckValue(value))
            {
                return;
            }

            switch (key)
            {
                case LANGUAGE_KEY:
                    Language = System.Web.HttpUtility.UrlDecode(value);
                    break;
                case DB_KEY:
                    Database = System.Web.HttpUtility.UrlDecode(value);
                    break;
                case PATH_KEY:
                    Path = System.Web.HttpUtility.UrlDecode(value);
                    break;
                case TABLE_KEY:
                    Table = System.Web.HttpUtility.UrlDecode(value);
                    break;
                case LAYOUT_KEY:
                    Layout = System.Web.HttpUtility.UrlDecode(value);
                    break;
                default:
                    _params.Add(new KeyValuePair<string, string>(key, value));
                    break;
            }
        }

        #endregion

        #region "Public properties"

        public string Language
        {
            get
            {
                System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;

                if (page == null) return "no";

                return RouteInstance.RouteExtender.GetLanguageFromUri(page.Request.Url);
            }
            set
            {
                //no action. Always derived from url
            }
        }
        public string Database { get; set; }
        public string Path { get; set; }
        public string Table { get; set; }
        public string View { get; set; }

        public const string NoLayout = "NoLayout";

        private string _layout;
        public string Layout
        {
            get
            {
                if (_layout == NoLayout) return null;
                if (!string.IsNullOrEmpty(_layout)) return _layout;

                System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;
                if (page == null) return null;

                string requestPath = ValidationManager.GetValue(page.Request.Path);
                var pathParts = requestPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                //different from localhost
                var sitePathStartIndex = Array.FindIndex(pathParts, s => s.StartsWith(SSBUrl.SitePathStart.ToLower()));

                int baseIndex = 0;

                if (sitePathStartIndex != -1)
                {
                    baseIndex = sitePathStartIndex + 1;
                }

                if (pathParts.Length < baseIndex + 3) return null;
                if (pathParts[baseIndex].ToLower() != SSBUrl.TABLE_IDENTIFER.ToLower()) return null;

                return pathParts[baseIndex + 2];
            }
            set
            {
                _layout = value;
            }
        }

        public List<KeyValuePair<string, string>> QuerystringParameters { get { return _params; } }
        public string TablePath { get { return Path + PCAxis.Web.Controls.PathHandler.NODE_DIVIDER + Table; } }

        #endregion

        #region "Public static methods"

        /// <summary>
        /// Get querystring or routedata value depending on if user friendly URL:s are used or not
        /// </summary>
        /// <param name="key">parameter key</param>
        /// <returns>Parameter value</returns>
        public static string GetParameter(string key)
        {
            if (PXWeb.Settings.Current.Features.General.UserFriendlyUrlsEnabled)
            {
                System.Web.UI.Page page = HttpContext.Current.Handler as System.Web.UI.Page;

                if (page != null)
                {
                    var routeDataValue = ValidationManager.GetValue(page.RouteData.Values[key] as string);
                    return routeDataValue;
                }
            }
            else
            {
                if (QuerystringManager.GetQuerystringParameter(key) != null)
                {
                    return ValidationManager.GetValue(QuerystringManager.GetQuerystringParameter(key));
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the view name by Page class
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public static string GetView(System.Web.UI.Page page)
        {
            if (page is Table)
            {
                return PxUrl.VIEW_TABLE_IDENTIFIER;
            }
            else if (page is Chart)
            {
                return PxUrl.VIEW_CHART_IDENTIFIER;
            }
            else if (page is InformationPresentation)
            {
                return PxUrl.VIEW_INFORMATION_IDENTIFIER;
            }
            else if (page is DataSort)
            {
                return PxUrl.VIEW_SORTEDTABLE_IDENTIFIER;
            }

            return PxUrl.VIEW_TABLE_IDENTIFIER;

        }

        #endregion
    }
}
