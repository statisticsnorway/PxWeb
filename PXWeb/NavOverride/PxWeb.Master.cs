using log4net;
using PCAxis.Menu;
using PCAxis.Web.Core;
using PCAxis.Web.Core.Enums;
using PCAxis.Web.Core.Management;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net; //ssb
using System.Web;
using System.Web.Caching;
using System.Web.UI;


namespace PXWeb
{
    // The CMSHelper  is

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

        private readonly CMSHelper cmsHelper = new CMSHelper();
        public string templateHead
        {
            get
            {
                return cmsHelper.templateHead;
            }
        }

        public string templateTop
        {
            get
            {
                return cmsHelper.templateTop;
            }
        }

        public string templateFoot
        {
            get
            {
                return cmsHelper.templateFoot;
            }
        }


        public string Language
        {
            get
            {
                return PxUrlObj.Language;
            }
        }

        private string _cmsHost;

        public string CmsHost
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


        private string _kortNavnWeb;

        private string KortNavnWeb
        {
            get
            {
                if (_kortNavnWeb == null)
                {
                    _kortNavnWeb = "";// navn har ikke kortnavn, var: GetTableListName() ?? "";
                }

                return _kortNavnWeb;
            }
        }

        /* private string GetTableListName()
         {
             var tableListPath = RouteInstance.RouteExtender.GetTableListPath(PxUrlObj.Path);
             if (string.IsNullOrEmpty(tableListPath)) return tableListPath;
             return RouteInstance.RouteExtender.GetLastNodeFromPath(tableListPath);
         }*/


        /*
        private void CheckIfLoggedOut()
        {
            if (Request.Cookies["wasLoggedInPxWeb"] != null && Session["PXUSER"] == null && Session["PXPASSWORD"] == null)
            {
                Response.Cookies["wasLoggedInPxWeb"].Expires = DateTime.Now.AddDays(-1);
                FormsAuthentication.SignOut();
                Session.Clear();
                Session["wasLoggedOutPxWeb"] = true;
                Response.Redirect("~/" + PxUrl.PX_START + "/" + PxUrlObj.Language + "/?sessionExpired=true");
            }
        }
        */

        protected void Page_Init(object sender, EventArgs e)
        {

            var queryStringPart = ValidationManager.GetValue(Page.Request.QueryString[PCAxis.Web.Core.StateProvider.StateProviderFactory.REQUEST_ID]);

            // ekstern har ikke breadC .
            // if (DoNotUseBreadCrumb())
            // {
            //     Page.Controls.Remove(this.breadcrumb1);
            // ekstern har ikke breadC . men hvorfor er denne i nav:
            this.breadcrumb1.HomePage = "Default.aspx";
            //}

            //Add eventhandlers
            LinkManager.RegisterEnsureQueries(new EnsureQueriesEventHandler(LinkManager_EnsureQueries));

            var pxUrl = RouteInstance.PxUrlProvider.Create(null);
            string urlLanguage = pxUrl.Language;

            if (urlLanguage != null)
            {
                if (!(LocalizationManager.CurrentCulture.Name == urlLanguage))
                {
                    if (PxUrlObj.Database != null)
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

        private bool DoNotUseBreadCrumb()
        {
            return true;
            //   if (RouteInstance.RouteExtender == null) return false;
            //   return !RouteInstance.RouteExtender.ShowBreadcrumb();
        }

        /// <summary>
        /// Page load - set private properties and page content
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            GetCMSContents();
            string ctrlname = Request.Params.Get("__EVENTTARGET");
            SetCanonicalUrl();
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

        private void SetCanonicalUrl()
        {
            var pageUrl = Page.Request.Url.AbsoluteUri.ToLower().Replace("http", "https");
            int index = pageUrl.IndexOf('?');
            if (index > 0)
            {
                CanonicalUrl.Href = pageUrl.Substring(0, index).TrimEnd('/');
            }
            else
            {
                CanonicalUrl.Href = pageUrl.TrimEnd('/');
            }
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


        private void GetCMSContents()
        {
            string pageUrl = Request.ServerVariables["PATH_INFO"];

            string AbackupCmsCss = ResolveUrl("~/Resources/Styles/bundlebackup.css");
            string AbackupCmsImg = ResolveUrl("~/Resources/Images/svg/SSB_logo_black.svg");
            string pathToBackupFiles = Server.MapPath(@"~\App_Data\BackupCms\");

            string Page_Request_Url_AbsoluteUri = Page.Request.Url.AbsoluteUri;

            cmsHelper.GetCMSContents(Language, KortNavnWeb, AbackupCmsCss, AbackupCmsImg, Cache, pathToBackupFiles, pageUrl, Page_Request_Url_AbsoluteUri);

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

    public class CMSHelper
    {
        public string templateHead;
        public string templateTop;
        public string templateFoot;


        private string Language;
        private string KortNavnWeb;
        private string backupCmsCss;
        private string backupCmsImg;
        Cache Cache;
        private string pathToBackupFiles;

        private bool connectedToCMS = true;

        private Dictionary<string, string> _templateByIdSetOnRequestByPart = new Dictionary<string, string>();
        private Dictionary<string, string> _genericTemplateByIdSetOnRequestByPart = new Dictionary<string, string>();

        string cmsGenericTemplateUrl = "system/xpramme?xpframe=statbank";


        string pageUrlFromRequestPATH_INFO;
        string Page_Request_Url_AbsoluteUri;

        public void GetCMSContents(string Language, string KortNavnWeb, string backupCmsCss, string backupCmsImg, Cache Cache, string pathToBackupFiles, string pageUrlFromRequestPATH_INFO, string Page_Request_Url_AbsoluteUri)
        {
            this.Language = Language;
            this.KortNavnWeb = KortNavnWeb;
            this.backupCmsCss = backupCmsCss;
            this.backupCmsImg = backupCmsImg;
            this.Cache = Cache;
            this.pathToBackupFiles = pathToBackupFiles;
            this.pageUrlFromRequestPATH_INFO = pageUrlFromRequestPATH_INFO;
            this.Page_Request_Url_AbsoluteUri = Page_Request_Url_AbsoluteUri;



            //context frame not ready for XP
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
            templateTop = templateTop.Replace("href=\"#content\"", "href=\"#pxcontent\"");

            templateTop = LagNavBank(templateTop);

        }

        private const string SeHovedTall = "<div class=\"col-md-12\">\r\n                  <a href=\"https://www.qa.ssb.no/system/\" class=\"ssb-link roboto-plain\">\r\n                    <span class=\"link-text\">Se hovedtall for denne statistikken</span>\r\n                  </a>\r\n                </div>";
        private const string SeHovedTall_en = "<div class=\"col-md-12\">\r\n                  <a href=\"https://www.qa.ssb.no/en/system/\" class=\"ssb-link roboto-plain\">\r\n                    <span class=\"link-text\">View primary data for this statistic</span>\r\n                  </a>\r\n                </div>";

        private const string BreadSmuler = "<nav class=\"row mt-2\" aria-label=\"secondary\" data-reactroot=\"\"><div class=\"col-12\"><div class=\"ssb-breadcrumbs\"><div><a class=\"ssb-link\" href=\"https://www.qa.ssb.no/\"><span class=\"link-text\">Forsiden</span></a> / </div><span></span></div></div></nav>";
        private const string BreadSmuler_en = "<nav class=\"row mt-2\" aria-label=\"secondary\" data-reactroot=\"\"><div class=\"col-12\"><div class=\"ssb-breadcrumbs\"><div><a class=\"ssb-link\" href=\"https://www.qa.ssb.no/en\"><span class=\"link-text\">Home</span></a> / </div><span></span></div></div></nav>";

        private const string MainMenu_in_en = "<div class=\"col-md-12\">\r\n                  <a href=\"https://www.qa.ssb.no/statbank\" class=\"ssb-link roboto-plain\">\r\n                    <span class=\"link-text\">Statbank main menu</span>\r\n                  </a>\r\n                </div>";
        private const string MainMenu_out_en = "<div class=\"col-md-12\">\r\n                  <a href=\"https://www.qa.ssb.no/navbank/pxweb/en/nav\" class=\"ssb-link roboto-plain\">\r\n                    <span class=\"link-text\">Statbank NAV main menu</span>\r\n                  </a>\r\n                </div>";
        private const string MainMenu_in_no = "<div class=\"col-md-12\">\r\n                  <a href=\"https://www.qa.ssb.no/statbank\" class=\"ssb-link roboto-plain\">\r\n                    <span class=\"link-text\">Statistikkbankens forside</span>\r\n                  </a>\r\n                </div>";
        private const string MainMenu_out_no = "<div class=\"col-md-12\">\r\n                  <a href=\"https://www.qa.ssb.no/navbank/pxweb/no/nav\" class=\"ssb-link roboto-plain\">\r\n                    <span class=\"link-text\">Statistikkbanken NAVs forside</span>\r\n                  </a>\r\n                </div>";





        private string LagNavBank(string templateTop)
        {
            string myOut = templateTop.Replace(">Statistikkbanken<", ">Statistikkbanken NAV<");
            myOut = myOut.Replace(SeHovedTall, "");
            myOut = myOut.Replace(SeHovedTall_en, "");
            myOut = myOut.Replace(BreadSmuler, "");
            myOut = myOut.Replace(BreadSmuler_en, "");
            myOut = myOut.Replace(MainMenu_in_no, MainMenu_out_no);
            myOut = myOut.Replace(MainMenu_in_en, MainMenu_out_en);
            myOut = myOut.Replace("breadcrumbs-", "");  //To stop a javascript, from reintroducing BreadSmuler

            return myOut;
        }


        private const string _genericTemplateCacheId = "genericTemplateCachePage";

        private string GetGenericTemplateCacheId()
        {
            return _genericTemplateCacheId + "_" + Language;
        }

        private string GettGenericTemplateCacheId(string part)
        {
            return _genericTemplateCacheId + part + "_" + Language;
        }

        private string GetCacheTemplateId(string part)
        {
            return part + "_" + "_" + KortNavnWeb + "_" + "_" + Language;
        }



        private void getGenericTemplate()
        {

            string result = GetGenericTemplateHtml();

            string headRamme = extractHead(result);
            string topRamme = extractTop(result);
            string bottomRamme = extractBottom(result);

            if (ShouldUseAbsoluteReferences())
            {
                headRamme = MakeAbsoluteReferences(headRamme);
                topRamme = MakeAbsoluteReferences(topRamme);
                bottomRamme = MakeAbsoluteReferences(bottomRamme);
            }


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
                templatePart = _templateByIdSetOnRequestByPart[part];
            }

            //if (part == "top")
            //{
            //    templatePart = insertChangeLanguage(templatePart);
            //}

            return templatePart;
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

            //if (part == "top")
            //{
            //    templatePart = insertChangeLanguage(templatePart);
            //}

            templatePart = templatePart.Replace("href = \"/", string.Format("href = \"{0}", ConfigurationManager.AppSettings["HomeSitePage"]));
            templatePart = templatePart.Replace("src = \"/", string.Format("src = \"{0}", ConfigurationManager.AppSettings["HomeSitePage"]));

            return templatePart;
        }


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
            //url = url + "?fane=statbank-web";
            url = url + "?xpframe=statbank";


            string result;
            string headRamme;
            string topRamme;
            string bottomRamme;
            try
            {
                result = invokeHttp(url);
                headRamme = extractHead(result);
                topRamme = extractTop(result);
                bottomRamme = extractBottom(result);
            }
            catch
            {
                result = GetGenericTemplateHtml();
                headRamme = extractHead(result);
                topRamme = extractTop(result);
                bottomRamme = extractBottom(result);
            }
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


        private string GetBackupTemplateHtml()
        {
            var result = Cache[GetGenericTemplateCacheId()] as string;
            if (!string.IsNullOrEmpty(result)) return result;

            string backupCMSramme;
            if (Language == "no")
            {
                backupCMSramme = System.IO.File.ReadAllText(pathToBackupFiles + "BackupCmsFrame.html");
            }

            else
            {
                backupCMSramme = System.IO.File.ReadAllText(pathToBackupFiles + "BackupCmsFrameEn.html");
            }
            result = backupCMSramme.Replace("backupcmscss", backupCmsCss).Replace("backupcmsimg", backupCmsImg);
            return result;
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

            try
            {
                result = invokeHttp(url);
            }
            catch
            {
                result = GetBackupTemplateHtml();
                connectedToCMS = false;
            }
            Cache.Insert(GetGenericTemplateCacheId(), result, null, DateTime.Now.AddMinutes(CacheTimeInMinutesCMSloadedContent), System.Web.Caching.Cache.NoSlidingExpiration);

            return result;
        }



        private bool ShouldUseAbsoluteReferences()
        {
            return true;
            /* Se på dette
            string fullUrl = Request.Url.OriginalString;
            var fullUrlPathParts = fullUrl.Split('/');
            return !fullUrlPathParts.Any(x => !string.IsNullOrEmpty(x) && x.ToLower() == RouteInstance.RouteExtender.SitePathStart.ToLower());
            //RouteInstance.RouteExtender.SitePathStart er hardkodet til statbank i den externe. 
       */
        }

        private string MakeAbsoluteReferences(string html)
        {
            if (connectedToCMS)
            {
                html = html.Replace("href=\"/", string.Format("href=\"{0}", ConfigurationManager.AppSettings["HomeSitePage"]));
                html = html.Replace("src=\"/", string.Format("src=\"{0}", ConfigurationManager.AppSettings["HomeSitePage"]));
                html = html.Replace("logoUrl\":\"", string.Format("logoUrl\":\"{0}", ConfigurationManager.AppSettings["HomeSitePage"].Substring(0, ConfigurationManager.AppSettings["HomeSitePage"].Length - 1)));
                html = html.Replace("path\":\"", string.Format("path\":\"{0}", ConfigurationManager.AppSettings["HomeSitePage"].Substring(0, ConfigurationManager.AppSettings["HomeSitePage"].Length - 1)));
            }
            return html;
        }


        private string insertChangeLanguage(string topPart)
        {

            string pageUrl = pageUrlFromRequestPATH_INFO;
            string topPartAfterCLInsert = "";
            if (Language == "en")
            {
                pageUrl = pageUrl.Replace("/en/", "/");
                topPartAfterCLInsert = topPart.Replace("change-language\" lang=\"no\" href=\"#\"", "change-language\" href=" + pageUrl);
            }
            else if (Language == "no")
            {
                pageUrl = pageUrl.ToLower().Replace("/statistikkbanken/", "/en/statistikkbanken/");
                topPartAfterCLInsert = topPart.Replace("change-language\" lang=\"en\" href=\"#\"", "change-language\" href=" + pageUrl);
            }

            return topPartAfterCLInsert;
        }

        private string ReplaceLanguageLink(string topTemplateHtml)
        {
            if (string.IsNullOrEmpty(topTemplateHtml)) return topTemplateHtml;

            var langSearchIndex = topTemplateHtml.IndexOf("title=\"language-changer\"");
            if (langSearchIndex < 0)
            {
                return topTemplateHtml;
            }

            var temp1Search = topTemplateHtml.Substring(0, langSearchIndex);
            var linkIndex = temp1Search.LastIndexOf("<a");
            if (linkIndex == -1) return topTemplateHtml;
            var endLinkStartNodeRelativeIndex = topTemplateHtml.Substring(linkIndex).IndexOf(">") + 1;
            var linkNodeStart = topTemplateHtml.Substring(linkIndex, endLinkStartNodeRelativeIndex);

            var hrefStartIndex = linkNodeStart.LastIndexOf("href");
            var linkNodePart1 = linkNodeStart.Substring(0, hrefStartIndex);

            var endLinkEndNodeRelativeIndex = topTemplateHtml.Substring(langSearchIndex).IndexOf(">") + 1;
            var linkNodePart3 = topTemplateHtml.Substring(langSearchIndex, endLinkEndNodeRelativeIndex);



            var currentUrl = Page_Request_Url_AbsoluteUri;
            var changeLanguageUrl = Language == "no" ? currentUrl.Replace("/pxweb/no/", "/pxweb/en/") : currentUrl.Replace("/pxweb/en/", "/pxweb/no/");
            var presentationUrlPartStartIndex = changeLanguageUrl.ToLower().IndexOf("tableview");
            if (presentationUrlPartStartIndex < 0)
            {
                presentationUrlPartStartIndex = changeLanguageUrl.ToLower().IndexOf("chartview");
            }
            if (presentationUrlPartStartIndex > 0)
            {
                var deltapresentationUrlPartEndIndex = changeLanguageUrl.Substring(presentationUrlPartStartIndex).IndexOf("/");
                changeLanguageUrl = changeLanguageUrl.ToLower().Remove(presentationUrlPartStartIndex - 1, deltapresentationUrlPartEndIndex + 2);
            }
            var linkNodePart2 = string.Format(" href=\"{0}\" ", changeLanguageUrl);
            var newLinkNodeStart = linkNodePart1 + linkNodePart2 + linkNodePart3;

            return topTemplateHtml.Replace(linkNodeStart, newLinkNodeStart);

            //    return "test";
        }


        private string extractHead(string result)
        {
            // Henter ut head fra cms-malen
            //int linkStartIndex = result.IndexOf("<link");
            //int linkStartIndex = result.IndexOf("<!-- UA");
            int linkStartIndex = result.IndexOf("<head>") + 6;
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
            int mainContentIndex = result.IndexOf("<div id=\"statbank-placeholder\"></div>");
            result = result.Substring(0, mainContentIndex);


            return result;
        }

        private string extractBottom(string result)
        {
            // Henter ut foot fra cms-malen
            int mainContentIndex = result.IndexOf("<div id=\"statbank-placeholder\"></div>");
            result = result.Substring(mainContentIndex + 37);
            return result;
        }

        private string invokeHttp(string url)
        {

            //try
            //{
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
            return strResult.Replace("xpramme", "");

            //}
            //catch
            //{
            //  return GetBackupTemplateHtml();
            //return backupCMSramme;
            //}
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
                if (!_cacheTimeInMinutesCMSloadedContent.HasValue && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["CacheServiceExpirationInMinutes"]))
                {
                    _cacheTimeInMinutesCMSloadedContent = int.Parse(ConfigurationManager.AppSettings["CacheServiceExpirationInMinutes"]);
                }

                return _cacheTimeInMinutesCMSloadedContent.HasValue ? _cacheTimeInMinutesCMSloadedContent.Value : 10;
            }
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

    }

}

