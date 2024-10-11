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
using System.Web.Security;
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
           //     this.breadcrumb1.HomePage = "Default.aspx";
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

            cmsHelper.GetCMSContents(Language,KortNavnWeb, AbackupCmsCss, AbackupCmsImg,Cache, pathToBackupFiles,pageUrl, Page_Request_Url_AbsoluteUri);

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

