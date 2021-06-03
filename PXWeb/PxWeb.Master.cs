using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using PCAxis.Web.Core.Management;
using System.Globalization;
using PCAxis.Menu;
using PCAxis.Menu.Implementations;
using PCAxis.Web.Controls;
using System.Configuration;
using log4net;
using PCAxis.Web.Core.Enums;
using PCAxis.Web.Core;
using System.Net; //ssb

namespace PXWeb
{
    public partial class PxWeb : System.Web.UI.MasterPage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PxWeb));

        public string HtmlLang
        {
            get
            {
                return LocalizationManager.GetTwoLetterLanguageCode();
            }
        }

        public string HeadTitle
        {
            get
            {
                return LiteralTitle.Text;
            }
            set
            {
                LiteralTitle.Text = value;
            }
        }

        public enum ModalDialogType
        {
            Information,
            Footnotes
        }

        private string _footertext = "";
        private string _imagesPath = "";
        private string _logoPath = "";

        public string FooterText
        {
            get
            {
                return _footertext;
            }
            set
            {
                _footertext = value;
            }
        }

        public string ImagesPath
        {
            get
            {
                return _imagesPath;
            }
        }
        public string OfficialStatisticsImage
        {
            get;
            set;
        }

        public string templateHead;
        public string templateTop;
        public string templateFoot;
        //string cmsGenericTemplateUrl = "system/ramme?markerrom=statistikk";
        string cmsGenericTemplateUrl = "system/xpramme";

        public string Language
        {
            get
            {
                return PxUrlObj.Language;
            }
        }

        private string _cmsHost;

        private string CmsHost
        {
            get
            {
                if (string.IsNullOrEmpty(_cmsHost))
                {
                    _cmsHost = ConfigurationManager.AppSettings["cmsHost"];
                }

                return _cmsHost;
            }
        }

        private int? _cacheTimeInMinutesCMSloadedContent;

        private int CacheTimeInMinutesCMSloadedContent
        {
            get
            {
                if (!_cacheTimeInMinutesCMSloadedContent.HasValue && ! string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CacheServiceExpirationInMinutes"]))
                {
                    _cacheTimeInMinutesCMSloadedContent = int.Parse(ConfigurationManager.AppSettings["CacheServiceExpirationInMinutes"]);
                }

                return _cacheTimeInMinutesCMSloadedContent.HasValue ? _cacheTimeInMinutesCMSloadedContent.Value : 10;
            }
        }
        
        private string _kortNavnWeb;

        private string KortNavnWeb
        {
            get
            {
                if (_kortNavnWeb == null)
                {
                    _kortNavnWeb = GetTableListName() ?? "";
                }

                return _kortNavnWeb;
            }
        }
        
        private string GetTableListName()
        {
            var tableListPath = RouteInstance.RouteExtender.GetTableListPath(PxUrlObj.Path);
            if (string.IsNullOrEmpty(tableListPath)) return tableListPath;
            return RouteInstance.RouteExtender.GetLastNodeFromPath(tableListPath);
        }
		
        protected void Page_Init(object sender, EventArgs e)
        {
            var queryStringPart = ValidationManager.GetValue(Page.Request.QueryString[PCAxis.Web.Core.StateProvider.StateProviderFactory.REQUEST_ID]);

            //Add eventhandlers
            LinkManager.RegisterEnsureQueries(new EnsureQueriesEventHandler(LinkManager_EnsureQueries));

            var pxUrl = RouteInstance.PxUrlProvider.Create(null);
            string urlLanguage = pxUrl.Language;

            if (urlLanguage != null)
            {
                if (!(LocalizationManager.CurrentCulture.Name == urlLanguage))
                {
                    if (urlLanguage != null)
                    {
                        DatabaseInfo dbi = null;

                        IPxUrl url = RouteInstance.PxUrlProvider.Create(null);
                        dbi = PXWeb.Settings.Current.General.Databases.GetDatabase(url.Database);

                        if (dbi.Type == DatabaseType.CNMM && !this.Page.Request.Url.AbsolutePath.Split('/').Contains("uttrekk"))
                        {
                            PCAxis.Web.Core.Management.PaxiomManager.PaxiomModelBuilder = null;
                            PCAxis.Web.Core.Management.PaxiomManager.PaxiomModel = null;
                        }
                    }
                    LocalizationManager.ChangeLanguage(urlLanguage);
                }
            }
            else
            {
                string lang = PXWeb.Settings.Current.General.Language.DefaultLanguage;
                LocalizationManager.ChangeLanguage(lang);

                List<LinkManager.LinkItem> linkItems = new List<LinkManager.LinkItem>();
                //linkItems.Add(new LinkManager.LinkItem(urlLanguage, lang));
                linkItems.Add(new LinkManager.LinkItem(PxUrl.LANGUAGE_KEY, lang));

                //Replaced Request.Url.AbsolutePath with Request.AppRelativeCurrentExecutionFilePath
                //so that the links will be right even if the site is running without UserFriendlyURL
                string url = PCAxis.Web.Core.Management.LinkManager.CreateLink(Request.AppRelativeCurrentExecutionFilePath, linkItems.ToArray());
                Response.Redirect(url);
            }

            ////Add eventhandlers
            //LinkManager.EnsureQueries += new LinkManager.EnsureQueriesEventHandler(LinkManager_EnsureQueries);
            _imagesPath = PXWeb.Settings.Current.General.Paths.ImagesPath;
            _logoPath = PXWeb.Settings.Current.General.Site.LogoPath;
            LoadPageContent();

            if (!IsPostBack)
            {
                if (PXWeb.Settings.Current.Navigation.ShowNavigationFlow)
                    InitializeNavigationFlow();
            }

            //navigationFlowControl.GetMenu = GetMenu;
        }

        /// <summary>
        /// Page load - set private properties and page content
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            GetCMSContents();
            string ctrlname = Request.Params.Get("__EVENTTARGET");
            bool languageChanged = false;
            if (!string.IsNullOrEmpty(ctrlname))
            {
                if ((ctrlname.Contains("cboSelectLanguages")))
                {
                    languageChanged = true;
                }
            }
            ToTheTopButtonLiteralText.Text = GetLocalizedString("PxWebToTheTopButtonLiteralText");
        }

        /// <summary>
        /// Set database name as H1 text
        /// </summary>
        public void SetH1TextDatabase()
        {
            if (!string.IsNullOrEmpty(PxUrlObj.Database) && !string.IsNullOrEmpty(PxUrlObj.Language))
            {
                DatabaseInfo dbi = PXWeb.Settings.Current.General.Databases.GetDatabase(PxUrlObj.Database);

                if (dbi != null)
                {
                    lblH1Title.Text = dbi.GetDatabaseName(PxUrlObj.Language);
                }
            }

        }

        /// <summary>
        /// Set menu level as H1 text
        /// </summary>
        public void SetH1TextMenuLevel()
        {
            if (PXWeb.Settings.Current.General.Site.MainHeaderForTables == MainHeaderForTablesType.StatisticArea)
            {
                if (!string.IsNullOrEmpty(PxUrlObj.Path))
                {
                    PCAxis.Menu.Item itm = GetMenu(PxUrlObj.Path);
                    if (itm != null && !string.IsNullOrEmpty(itm.Text))
                    {
                        lblH1Title.Text = itm.Text;
                    }
                }
            }
            else
            {
                lblH1Title.Visible = false;
            }
        }

        /// <summary>
        /// Set custom H1 text 
        /// </summary>
        /// <param name="text"></param>
        public void SetH1TextCustom(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                lblH1Title.Text = text;
            }
        }


        /// <summary>
        /// Page unload - remove eventhandler for LinkManager.EnsureQueries
        /// </summary>
        protected void Page_Unload(object sender, EventArgs e)
        {

            LinkManager.UnregisterEnsureQueries(LinkManager_EnsureQueries);
        }


        /// <summary>
        /// Set page content
        /// </summary>
        private void LoadPageContent()
        {
            //Title
            //litTitle.Text = Server.HtmlEncode(GetLocalizedString("PxWebApplicationTitle"));
            //if (string.IsNullOrEmpty(litTitle.Text))
            //{
            //    litTitle.Text = "PX-Web";
            //}

            //Logo
            if (PXWeb.Settings.Current.Selection.StandardApplicationHeadTitle)
            {
                HeadTitle = Server.HtmlEncode(GetLocalizedString("PxWebApplicationTitle"));
            }

            //Footer
            lblFooterText.Text = _footertext;
        }

        /// <summary>
        /// Eventhandler for LinkManager.EnsureQueries (calles from LinkManager.CreateLink) 
        /// that adds dictionaryitems to add to the created link.
        /// </summary>
        /// <param name="queries"></param>
        protected void LinkManager_EnsureQueries(object sender, EnsureQueriesEventArgs e)
        {
            Dictionary<string, string> queries = e.Queries;
            AddToQueries(queries, "px_db"); // Identifies selected PX- or SQL-database
            AddToQueries(queries, "px_language");
            AddToQueries(queries, "px_path"); // path within database 
            AddToQueries(queries, "px_tableid"); // Identifies selected PX-file or SQL-table
            AddToQueries(queries, "rxid");
        }


        protected void AddToQueries(Dictionary<string, string> queries, string key)
        {
            if (Page.RouteData.Values[key] != null)
            {
                if (queries.ContainsKey(key))
                {
                    queries[key] = ValidationManager.GetValue(Page.RouteData.Values[key].ToString());
                }
                else
                {
                    queries.Add(key, ValidationManager.GetValue(Page.RouteData.Values[key].ToString()));
                }
            }
            else if (HttpContext.Current.Request.QueryString[key] != null)
            {
                if (queries.ContainsKey(key))
                {
                    queries[key] = ValidationManager.GetValue(HttpContext.Current.Request.QueryString[key]);
                }
                else
                {
                    queries.Add(key, ValidationManager.GetValue(HttpContext.Current.Request.QueryString[key]));
                }
            }
        }

        /// <summary>
        /// Get text in the currently selected language
        /// </summary>
        /// <param name="key">Key identifying the string in the language file</param>
        /// <returns>Localized string</returns>
        public string GetLocalizedString(string key)
        {
            string lang = LocalizationManager.CurrentCulture.Name;
            return PCAxis.Web.Core.Management.LocalizationManager.GetLocalizedString(key, new CultureInfo(lang));
        }

        /// <summary>
        /// Set breadcrumb
        /// </summary>
        /// <param name="mode">Breadcrumb mode</param>
        /// <param name="subpage">Optional parameter breadcrumb name</param>
        public void SetBreadcrumb(PCAxis.Web.Controls.Breadcrumb.BreadcrumbMode mode, string subpage = "")
        {
            //breadcrumb1.Update(mode, subpage);
        }

        public void SetNavigationFlowMode(PCAxis.Web.Controls.NavigationFlow.NavigationFlowMode mode)
        {
            if (PXWeb.Settings.Current.Navigation.ShowNavigationFlow)
            {
                navigationFlowControl.UpdateNavigationFlowMode(mode);
            }
        }

        /// <summary>
        /// If the Navigationflow 
        /// </summary>
        /// <param name="show"></param>
        public void SetNavigationFlowVisibility(Boolean show)
        {
            navigationFlowControl.Visible = show;
        }

        private void InitializeNavigationFlow()
        {
            IPxUrl url = RouteInstance.PxUrlProvider.Create(null);
            DatabaseInfo dbi = null;

            navigationFlowControl.MenuPage = "Menu.aspx";
            navigationFlowControl.SelectionPage = "Selection.aspx";
            navigationFlowControl.TablePathParam = "px_path";
            navigationFlowControl.LayoutParam = "layout";

            if (string.IsNullOrEmpty(url.Database))
            {
                return;
            }

            dbi = PXWeb.Settings.Current.General.Databases.GetDatabase(url.Database);

            navigationFlowControl.DatabaseId = dbi.Id;
            navigationFlowControl.DatabaseName = dbi.GetDatabaseName(LocalizationManager.CurrentCulture.TwoLetterISOLanguageName);

            if (string.IsNullOrEmpty(url.Path))
            {
                return;
            }


            if (dbi.Type == PCAxis.Web.Core.Enums.DatabaseType.CNMM)
            {
                navigationFlowControl.DatabaseType = PCAxis.Web.Core.Enums.DatabaseType.CNMM;
            }
            else
            {
                navigationFlowControl.DatabaseType = PCAxis.Web.Core.Enums.DatabaseType.PX;
            }

            navigationFlowControl.TablePath = System.Web.HttpUtility.UrlDecode(url.Path);

            if (string.IsNullOrEmpty(url.Table))
            {
                return;
            }

            if ((dbi.Type == PCAxis.Web.Core.Enums.DatabaseType.CNMM) && (!url.Table.Contains(":")))
            {
                navigationFlowControl.Table = url.Database + ":" + url.Table;
            }
            else
            {
                navigationFlowControl.Table = url.Table;
            }
        }

        private IPxUrl _pxUrl = null;

        private IPxUrl PxUrlObj
        {
            get
            {
                if (_pxUrl == null)
                {
                    _pxUrl = RouteInstance.PxUrlProvider.Create(null);
                }

                return _pxUrl;
            }
        }

        /// <summary>
        /// Gets the menu object
        /// </summary>
        /// <returns>returns the menu object</returns>
        private Item GetMenu(string nodeId)
        {
            //Checks that the necessary parameters are present
            if (String.IsNullOrEmpty(PxUrlObj.Database))
            {
                //if parameters is missing redirect to the start page
                Response.Redirect("Default.aspx", false);
            }

            try
            {
                string db = PxUrlObj.Database;
                return PXWeb.Management.PxContext.GetMenuItem(db, nodeId);
            }
            catch (Exception e)
            {
                log.Error("An error occured in GetMenu(string nodeId). So it returns null after logging this message.", e);
                return null;
            }
        }

        private string ReplaceLanguageLink(string topTemplateHtml)
        {
            if (string.IsNullOrEmpty(topTemplateHtml)) return topTemplateHtml;

            var linkIndex = topTemplateHtml.IndexOf("<a id=\"change-language\"");
            if (linkIndex == -1) return topTemplateHtml;
            var endLinkStartNodeRelativeIndex = topTemplateHtml.Substring(linkIndex).IndexOf(">") + 1;
            var linkNodeStart = topTemplateHtml.Substring(linkIndex, endLinkStartNodeRelativeIndex);
            var hrefStartIndex = linkNodeStart.IndexOf(" href=");
            var linkNodePart1 = linkNodeStart.Substring(0, hrefStartIndex);
            var currentUrl = Page.Request.Url.AbsoluteUri;
            var changeLanguageUrl = PxUrlObj.Language == "no" ? currentUrl.Replace("/statbank/", "/en/statbank/") : currentUrl.Replace("/en/statbank/", "/statbank/");
            var presentationUrlPartStartIndex = changeLanguageUrl.ToLower().IndexOf("tableview");
            if (presentationUrlPartStartIndex < 0)
            {
                 presentationUrlPartStartIndex = changeLanguageUrl.ToLower().IndexOf("chartview");
            }
            if (presentationUrlPartStartIndex > 0)
            {
                var deltapresentationUrlPartEndIndex = changeLanguageUrl.Substring(presentationUrlPartStartIndex).IndexOf("/");
                changeLanguageUrl=changeLanguageUrl.ToLower().Remove(presentationUrlPartStartIndex - 1, deltapresentationUrlPartEndIndex + 2 );
            }
            var linkNodePart2 = string.Format(" href=\"{0}\">", changeLanguageUrl);
            var newLinkNodeStart = linkNodePart1 + linkNodePart2;

            return topTemplateHtml.Replace(linkNodeStart, newLinkNodeStart);
        }

        private void GetCMSContents()
        {
            string pageUrl = Request.ServerVariables["PATH_INFO"];
            
            if (string.IsNullOrEmpty(KortNavnWeb))
            {
                templateHead = getGenericTemplatePart("head").ToString();
                templateTop = getGenericTemplatePart("top").ToString();
                templateFoot = getGenericTemplatePart("foot").ToString();
            }
            else
            {
                templateHead = getTemplatePart("head").ToString();
                templateTop = getTemplatePart("top").ToString();
                templateFoot = getTemplatePart("foot").ToString();
            }

            templateTop = ReplaceLanguageLink(templateTop);
            templateTop = templateTop.Replace("class=\"mega-menu hidden-by-default\"", "class=\"mega-menu hidden-by-default\" style=\"display: none;\"");
            templateTop = templateTop.Replace("<a href=\"#content\"", "<a href=\"#pxcontent\"");
        }

        private string GetCacheTemplateId(string part)
        {
            return part + "_" + "_" + KortNavnWeb + "_" + "_" + Language;
        }

        private string getTemplatePart(string part)
        {
            string templateId = GetCacheTemplateId(part);
            if (Cache[templateId] == null)
            {
                getTemplate();
            }
            
            string templatePart = Cache[templateId] as string;

            if (templatePart == null)
            {
                templatePart = (string)_templateByIdSetOnRequestByPart[part];
            }
            
            if (part == "top")
            {
                templatePart = insertChangeLanguage(templatePart);
            }

            return templatePart;
        }

        private const string _genericTemplateCacheId = "genericTemplateCachePage";

        private string GettGenericTemplateCacheId(string part)
        {
            return _genericTemplateCacheId + part + "_" + Language;
        }

        private string getGenericTemplatePart(string part)
        {
            string genericTemplateCacheId = GettGenericTemplateCacheId(part);

            if (Cache[genericTemplateCacheId] == null)
            {
                getGenericTemplate();
            }

            string templatePart = Cache[genericTemplateCacheId] as string;

            if (templatePart == null)
            {
                templatePart = _genericTemplateByIdSetOnRequestByPart[part];
            }
            
            if (part == "top")
            {
                templatePart = insertChangeLanguage(templatePart);
            }

            templatePart = templatePart.Replace("href = \"/", string.Format("href = \"{0}", RouteInstance.RouteExtender.HomeSitePage));
            templatePart = templatePart.Replace("src = \"/", string.Format("src = \"{0}", RouteInstance.RouteExtender.HomeSitePage));

            return templatePart;
        }

        private string insertChangeLanguage(string topPart)
        {

            string pageUrl = Request.ServerVariables["PATH_INFO"];
            string topPartAfterCLInsert = "";
            if (PxUrlObj.Language == "en")
            {
                pageUrl = pageUrl.Replace("/en/", "/");
                topPartAfterCLInsert = topPart.Replace("change-language\" lang=\"no\" href=\"#\"", "change-language\" href=" + pageUrl);
            }
            else if (PxUrlObj.Language == "no")
            {
                pageUrl = pageUrl.ToLower().Replace("/statistikkbanken/", "/en/statistikkbanken/");
                topPartAfterCLInsert = topPart.Replace("change-language\" lang=\"en\" href=\"#\"", "change-language\" href=" + pageUrl);
            }

            return topPartAfterCLInsert;
        }

        private Dictionary<string, string> _genericTemplateByIdSetOnRequestByPart = new Dictionary<string, string>();

        private string GetGenericTemplateCacheId()
        {
            return _genericTemplateCacheId + "_" + Language;
        }

        private string GetGenericTemplateHtml()
        {
            var result = Cache[GetGenericTemplateCacheId()] as string;
            if (!string.IsNullOrEmpty(result)) return result;

            string cmsHostEn = CmsHost + "en/";
            string url;
            if (Language == "no")
            {
                url = CmsHost + cmsGenericTemplateUrl;
            }
            else
            {
                url = cmsHostEn + cmsGenericTemplateUrl;
            }

            result = invokeHttp(url);
            Cache.Insert(GetGenericTemplateCacheId(), result, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);

            return result;
        }

        private void getGenericTemplate()
        {
            string result = GetGenericTemplateHtml();

            string headRamme = extractHead(result);
            string topRamme = extractTop(result);
            string bottomRamme = extractBottom(result);

            _genericTemplateByIdSetOnRequestByPart["head"] = headRamme;
            _genericTemplateByIdSetOnRequestByPart["top"] = topRamme;
            _genericTemplateByIdSetOnRequestByPart["top"] = bottomRamme;

            string headerTemplateCacheId = GettGenericTemplateCacheId("head");
            string topTemplateCacheId = GettGenericTemplateCacheId("top");
            string footTemplateCacheId = GettGenericTemplateCacheId("foot");

            Cache.Insert(headerTemplateCacheId, headRamme, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);
            Cache.Insert(topTemplateCacheId, topRamme, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);
            Cache.Insert(footTemplateCacheId, bottomRamme, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);

        }

        private Dictionary<string, string> _templateByIdSetOnRequestByPart = new Dictionary<string, string>();
            
        private void getTemplate()
        {
            string cmsHost = ConfigurationManager.AppSettings["cmsHost"];
            string cmsHostEn = cmsHost + "en/";
            string url;
            if (Language == "no")
            {
                url = cmsHost;
            }
            else
            {
                url = cmsHostEn;
            }

            url = url + KortNavnWeb;
            url = url + "?fane=statbank-web";
            
            string result;
            try
            {
                result = invokeHttp(url);
            }
            catch
            {
                result = GetGenericTemplateHtml();
            }
            string headRamme = extractHead(result);
            string topRamme = extractTop(result);
            string bottomRamme = extractBottom(result);

            if (ShouldUseAbsoluteReferences())
            {
                headRamme = MakeAbsoluteReferences(headRamme);
                topRamme = MakeAbsoluteReferences(topRamme);
                bottomRamme = MakeAbsoluteReferences(bottomRamme);
            }

            _templateByIdSetOnRequestByPart["head"] = headRamme;
            _templateByIdSetOnRequestByPart["top"] = topRamme;
            _templateByIdSetOnRequestByPart["foot"] = bottomRamme;
            
            var headerTemplateCacheId = GetCacheTemplateId("head");
            var topTemplateCacheId = GetCacheTemplateId("top");
            var footTemplateCacheId = GetCacheTemplateId("foot");

            Cache.Insert(headerTemplateCacheId, headRamme, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);
            Cache.Insert(topTemplateCacheId, topRamme, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);
            Cache.Insert(footTemplateCacheId, bottomRamme, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);
        }

        private bool ShouldUseAbsoluteReferences()
        {
            string fullUrl = Request.Url.OriginalString;
            var fullUrlPathParts = fullUrl.Split('/');
            return !fullUrlPathParts.Any(x => !string.IsNullOrEmpty(x) && x.ToLower() == RouteInstance.RouteExtender.SitePathStart.ToLower());
        }

        private string MakeAbsoluteReferences(string html)
        {
            html = html.Replace("href=\"/", string.Format("href=\"{0}", RouteInstance.RouteExtender.HomeSitePage));
            html = html.Replace("src=\"/", string.Format("src=\"{0}", RouteInstance.RouteExtender.HomeSitePage));
            html = html.Replace("logoUrl\":\"", string.Format("logoUrl\":\"{0}",RouteInstance.RouteExtender.HomeSitePage.Substring(0,RouteInstance.RouteExtender.HomeSitePage.Length-1)));
            html = html.Replace("path\":\"", string.Format("path\":\"{0}", RouteInstance.RouteExtender.HomeSitePage.Substring(0, RouteInstance.RouteExtender.HomeSitePage.Length - 1)));

            return html;
        }

        private string extractHead(string result)
        {
            // Henter ut head fra cms-malen
            //int linkStartIndex = result.IndexOf("<link");
            int linkStartIndex = result.IndexOf("<!-- UA");
            result = result.Substring(linkStartIndex);
            int headStopIndex = result.IndexOf("</head>");
            result = result.Substring(0, headStopIndex);
            return result;
        }

        private string extractTop(string result)
        {
            //Henter ut body fra cms-malen til og med main-content <div id = "main-content">
            int indexOfBody = result.IndexOf("<body");
            result = result.Substring(indexOfBody);
            int mainContentIndex = result.IndexOf("row-cols-1");
            result = result.Substring(0, mainContentIndex + 12);
            return result;
        }

        private string extractBottom(string result)
        {
            // Henter ut foot fra cms-malen
            int mainContentIndex = result.IndexOf("row-cols-1");
            result = result.Substring(mainContentIndex + 23);
            result = result.Substring(0, result.Length);
            return result;
        }
        
        private string invokeHttp(string url)
        {
            String strResult = null;
            WebRequest objRequest = HttpWebRequest.Create(url);

            objRequest.Timeout = CMSloadedContentTimeout;

            using (WebResponse objResponse = objRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    strResult = sr.ReadToEnd();
                    sr.Close();
                }
            }
            
            return strResult;
        }

        private int? _CMSloadedContentTimeout = null;

        private int CMSloadedContentTimeout
        {
            get
            {
                if (!_CMSloadedContentTimeout.HasValue && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CMSloadedContentTimeout"]))
                {
                    _CMSloadedContentTimeout = int.Parse(ConfigurationManager.AppSettings["CMSloadedContentTimeout"]);
                }

                return _CMSloadedContentTimeout ?? 100000;
            }
        }

        private string _displayVersion = null;

        public string DisplayVersion
        {
            get
            {
                if (_displayVersion == null)
                {
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    var displayVersion = version.Major + "." + version.Minor + "." + version.Build;

                    if (version.Revision > 0)
                    {
                        displayVersion += "." + version.Revision;
                    }

                    _displayVersion = displayVersion;
                }

                return _displayVersion;
            }
        }
    }
}

