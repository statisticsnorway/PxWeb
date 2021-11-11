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
using System.Web.Caching;

namespace PXWeb
{
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
        string Page_Request_Url_AbsoluteUri ;

        public void GetCMSContents(string Language, string KortNavnWeb, string backupCmsCss, string backupCmsImg, Cache Cache, string pathToBackupFiles,string pageUrlFromRequestPATH_INFO, string Page_Request_Url_AbsoluteUri)
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

            if (part == "top")
            {
                templatePart = insertChangeLanguage(templatePart);
            }

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

            if (part == "top")
            {
                templatePart = insertChangeLanguage(templatePart);
            }

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
                backupCMSramme = System.IO.File.ReadAllText(pathToBackupFiles+"BackupCmsFrame.html");
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
            var changeLanguageUrl = Language == "no" ? currentUrl.Replace("/statbank/", "/en/statbank/") : currentUrl.Replace("/en/statbank/", "/statbank/");
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