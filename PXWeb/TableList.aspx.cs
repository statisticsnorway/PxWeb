using log4net;
using PCAxis.Menu;
using PCAxis.Web.Controls;
using PCAxis.Web.Core.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PXWeb
{
    public interface IUrlResolver
    {
        string ResolveUrl(string relativeUrl);
    }

    public class UrlResolver : IUrlResolver
    {
        private Page _page;

        public UrlResolver(Page page)
        {
            _page = page;
        }

        public string ResolveUrl(string relativeUrl)
        {
            return _page.ResolveUrl(relativeUrl);
        }
    }

    public class TableLinkItem
    {
        public TableLinkItem(TableLink tableLink, IUrlResolver urlResolver)
        {
           
            var linkItems = new List<LinkManager.LinkItem>();
            string tableName = tableLink.ID.Selection.Split(':').Last();
            linkItems.Add(new LinkManager.LinkItem(PxUrl.TABLE_KEY, tableName));
            
            Link = LinkManager.CreateLinkMethod("Selection.aspx", false, linkItems.ToArray());
            LinkTableId = GetLinkTableId(tableLink);
            LinkSpanContent = BuildLinkSpanContent(tableLink, urlResolver);
            LinkSpanPeriod = BuildLinkPeriod(tableLink);
        }


        
        private string BuildLinkSpanContent(TableLink tableLink, IUrlResolver urlResolver)
        {
            var result = new StringBuilder();

            result.Append(GetTextWithoutPeriodAndTableId(tableLink));

            if (tableLink.HasAttribute("modified") && tableLink.GetAttribute("modified").ToString().Length > 0)
            {
                var altStr = LocalizationManager.GetLocalizedString("CtrlTableOfContentModifiedHeading") + " " + GetFormattedDate(tableLink.GetAttribute("modified").ToString());
                LinkSpanContentHover = altStr;
            }

            if (tableLink.HasAttribute("updated") && tableLink.GetAttribute("updated").ToString().Length > 0)
            {
                var altStr = LocalizationManager.GetLocalizedString("CtrlTableOfContentUpdatedHeading") + " " + GetFormattedDate(tableLink.GetAttribute("updated").ToString());
                LinkSpanContentHover = altStr;
            }

            return result.ToString();
        }

        private string BuildLinkPeriod(TableLink tableLink)
        {
            bool linkHasDates = (tableLink.StartTime != null && tableLink.EndTime != null);

            if (!linkHasDates) return string.Empty;
            if (tableLink.StartTime == tableLink.EndTime) return tableLink.StartTime.ToString();

            return tableLink.StartTime.ToString() + " - " + tableLink.EndTime.ToString();
        }

        private string GetFormattedDate(string date)
        {
            string formattedDate = string.Empty;

            if (!string.IsNullOrEmpty(date))
            {
                var tempDate = DateTime.ParseExact(date, "yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
                var lang = LocalizationManager.CurrentCulture.Name;
                var format = PCAxis.Paxiom.Settings.GetLocale(lang).DateFormat;
                formattedDate = tempDate.ToString(format);
            }

            return formattedDate;
        }

        private string GetTextWithoutPeriodAndTableId(TableLink tableLink)
        {
            bool linkHasDates = (!string.IsNullOrEmpty(tableLink.StartTime) && !string.IsNullOrEmpty(tableLink.EndTime));

            string noDateText = null;

            if (linkHasDates)
            {
                if (tableLink.StartTime == tableLink.EndTime)
                {
                    noDateText = tableLink.Text.Replace(tableLink.StartTime.ToString(), "");
                }
                else
                {
                    noDateText = tableLink.Text.Replace(tableLink.StartTime + " - " + tableLink.EndTime, "");
                }
            }
            else
            {
                noDateText = tableLink.Text;
            }
            
            var firstColonIndex = noDateText.IndexOf(':');

            //Remove the table id
            return (firstColonIndex == -1) ? noDateText : noDateText.Substring(firstColonIndex + 1);
        }

        private string GetLinkTableId(TableLink tableLink)
        {
            var firstColonIndex = tableLink.Text.IndexOf(':');
            //Remove the table id
            return (firstColonIndex == -1) ? tableLink.Text : tableLink.Text.Substring(0, firstColonIndex);
        }

        private string GetCategoryText(PresCategory category)
        {
            switch (category)
            {
                case PresCategory.Internal:
                    return " (I)";
                case PresCategory.Official:
                    return " (O)";
                case PresCategory.Private:
                    return " (P)";
                default:
                    return " (S)";
            }
        }

     

        public string LinkTableId { get; private set; }
        public string LinkSpanContent { get; private set; }
        public string LinkSpanContentHover { get; private set; }
        public string LinkSpanPeriod { get; private set; }
        public string Link { get; private set; }
    }


    public partial class TableList : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PxWeb));

        //For row mouseover
        private int rowCounter = 0;

        protected string CreateTableLinkText(object obj)
        {
            return "Konsumprisindeks for varer og tjenester, etter leveringssektor (2015=100)";
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


        private string GetLastNodeFromPath(string path)
        {
            var pathIndex = path.LastIndexOf(PathHandler.NODE_DIVIDER);
            if (pathIndex == -1) return null;
            return path.Substring(pathIndex).Replace(PathHandler.NODE_DIVIDER, "");
        }

        private bool timePeriodExplanation = false;
        private bool regionExplanation = false;

        protected void Page_Init(object sender, EventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            PCAxis.Web.Core.Management.PaxiomManager.PaxiomModelBuilder = null;
            PCAxis.Web.Core.Management.PaxiomManager.PaxiomModel = null;
            PCAxis.Web.Core.Management.PaxiomManager.QueryModel = null;
            PCAxis.Web.Controls.VariableSelector.SelectedVariableValues.Clear();

            if (!IsPostBack)
            {
                Master.SetBreadcrumb(PCAxis.Web.Controls.Breadcrumb.BreadcrumbMode.Selection);
                Master.SetNavigationFlowMode(PCAxis.Web.Controls.NavigationFlow.NavigationFlowMode.First);
                Master.SetNavigationFlowVisibility(PXWeb.Settings.Current.Navigation.ShowNavigationFlow);
            }

            var tableListName = GetLastNodeFromPath(PxUrlObj.Path);//path:START__al__al06__aku
            if (string.IsNullOrEmpty(tableListName)) return;

            var path = RouteInstance.RouteExtender.GetTableListPath(PxUrlObj.Path);
            if (string.IsNullOrEmpty(path)) return;
            string lastNodeFromPath = GetLastNodeFromPath(path);

            if (lastNodeFromPath != tableListName)
            {
                RouteInstance.RouteExtender.RedirectMenuRoutePath(PxUrlObj.Path, lastNodeFromPath);
            }
            else
            {
                var siteTitle = LocalizationManager.GetLocalizedString("SiteTitle");

                var menu = GetMenu(path); //path=START__al__al06__aku

                if (!PXWeb.Settings.Current.Selection.StandardApplicationHeadTitle)
                {
                    Master.HeadTitle = menu.CurrentItem.Text + ". " + siteTitle;
                }

                var currentMenuitem = (PxMenuItem)menu.CurrentItem;

                List<string> outList = new List<string>();

                if (currentMenuitem.SubItems.Count() > 0)
                {
                    outList = ReadDataSource(currentMenuitem, path, new List<string>(), 0);
                }
                else
                {
                    outList.Add(GetEmptyFolderWarning(""));
                }

                foreach (string litString in outList)
                {
                    Literal tmpLit = new Literal();
                    tmpLit.Text = litString;
                    MenuTableList.Controls.Add(tmpLit);
                }

                regionExplanation = IsRegionExplanation(outList);
                timePeriodExplanation=IsTimePeriod(outList);
            }
            MenuExplanation.SetExplanationRegionTimePeriodVisible(regionExplanation, timePeriodExplanation);
        }

        private bool IsRegionExplanation(List<string> outList )
        {
            bool region = false;
            foreach (string litString in outList)
            {
                if (litString.Contains("(F)") ||
                    litString.Contains("(K)") ||
                    litString.Contains("(G)") ||
                    litString.Contains("(B)") ||
                    litString.Contains("(T)") ||
                    litString.Contains("(C)") ||
                    litString.Contains("(M)") ||
                    litString.Contains("(BU)") ||
                    litString.Contains("(UD)") ||
                    litString.Contains("(US)")
                )
                {
                    region= true;
                    break;
                }
            }
            return region;
        }

        private bool IsTimePeriod(List<string> outList)
        {
            bool tmePeriod = false;
            // Matches 2010K4 2020M09 2020U42 2020Q09 2020W42
            Regex r = new Regex(@"\d\d\d\d[KUMQW]\d");

            foreach (string litString in outList)
            {
                if (r.Match(litString).Success)
                {
                    tmePeriod = true;
                    break;
                }
            }
            return tmePeriod;
        }



        private List<string> ReadDataSource(PxMenuItem menuItem, string path, List<string> belongsToAccordions, int accordionLevel)
        {
            List<string> myOut = new List<string>();
            var urlResolver = new UrlResolver(this);

            //the classed dealing with belonging to a accordion
            string accordionClasses = GetAccordionClasses(belongsToAccordions);

            //this is used to see if we are inside a <table><tablebody> 
            bool isInTableElement = false;

            foreach (var subItem in menuItem.SubItems)
            {
                string myType = subItem.GetType().Name;
                var headLine = subItem as Headline;
                var tableLink = subItem as TableLink;
                var subMenuItem = subItem as PxMenuItem;

                if (isInTableElement && !myType.Equals("TableLink"))
                {
                    myOut.Add(GetCloseTable(accordionClasses));
                    isInTableElement = false;
                }
                else if (!isInTableElement && myType.Equals("TableLink"))
                {
                    myOut.Add(GetStartTable(accordionClasses));
                    isInTableElement = true;
                }


                if (headLine != null)
                {

                    myOut.Add(GetHeading(headLine.Text, accordionClasses));
                }
                else if (tableLink != null)
                {
                    myOut.Add(GetLinkRow(tableLink, urlResolver, accordionClasses));
                }
                else if (subMenuItem != null)
                {

                    var childPath = path + PathHandler.NODE_DIVIDER + subMenuItem.ID.Selection;

                    myOut.Add(GetAccordionStart(childPath, subMenuItem, accordionClasses, childPath, accordionLevel));

                    var childMenu = GetMenu(childPath);
                    var childMenuItem = (PxMenuItem)childMenu.CurrentItem;

                    if (childMenuItem.SubItems.Count() > 0)
                    {
                        List<string> tmpFolders = new List<string>();
                        tmpFolders.AddRange(belongsToAccordions);
                        tmpFolders.Add(childPath);
                        var tempList =  ReadDataSource(childMenuItem, childPath, tmpFolders, (accordionLevel + 1));
                        myOut.AddRange(tempList);
                    } else
                    {
                        myOut.Add(GetEmptyFolderWarning(accordionClasses));
                    }
                    myOut.Add(GetAccordionEnd(accordionClasses));
                }
            }

            if (isInTableElement)
            {
                myOut.Add(GetCloseTable(accordionClasses));
            }

            return myOut;
        }


        private string GetEmptyFolderWarning(string accordionClasses)
        {
            return WrapInFullRow(StartDiv(accordionClasses) + GetSpan(LocalizationManager.GetLocalizedString("PxWebTableListEmptyFolder")) + "</div>");
        }

        private string GetHeading(string headerText, string accordionClasses) {
            return WrapInFullRow(StartDiv(accordionClasses) + "<h2>" + headerText + "</h2></div>");
        }

        private string GetAccordionStart(string path, PxMenuItem menuItem, string accordionClasses, string accordionId, int accordionLevel)
        {
            string headerText = Server.HtmlEncode(menuItem.Text);
            StringBuilder sbAccordion = new StringBuilder();

            sbAccordion.Append(WrapInFullRow("<div class='pxweb-cell-accordion-start" + accordionClasses + "'></div>"));

            sbAccordion.Append("<div class='grid-row-1-3" + accordionClasses + "'>");
            sbAccordion.Append("<div class='pxweb-accordion without-borders'>");
            sbAccordion.Append("<button type = 'button' class='accordion-header closed' onclick='cellAccordionToggle(MenuTableList, this, \"Accordion_"+ accordionId+"\",\"closed_level_"+(accordionLevel +1)+ "\"  )'>");
            sbAccordion.Append("<span role = 'heading' aria-level='2' class='header-text'>"+headerText+"</span>");
            sbAccordion.Append("</button>");
            sbAccordion.Append("</div>");
            sbAccordion.Append("</div>");

            /* Removed "<div role='region' aria-label='" + headerText + "' 
               since there is a the role=heading with the same text.(Better ?) */
            return sbAccordion.ToString();
        }


        private string GetAccordionEnd(string accordionClasses)
        {
            return  WrapInFullRow("<div  class='pxweb-cell-accordion-end" + accordionClasses + "'></div>");
        }

        /*Its not really a table, but.*/
        private string GetStartTable(string accordionClasses)
        {
            StringBuilder outText = new StringBuilder();

            if (PxUrlObj.Language.Equals("en"))
            {
                outText.Append("<div class='menu-tablelist grid-container'>");
                outText.Append(StartDiv(accordionClasses + " colhead col1") + "Table no</div>");
                outText.Append(StartDiv(accordionClasses + " colhead col2") + "Title</div>");
                outText.Append(StartDiv(accordionClasses + " colhead col3") + "Time period:</div>");
                outText.Append("</div>");
            }
            else
            {
                outText.Append("<div class='menu-tablelist grid-container'>");
                outText.Append(StartDiv(accordionClasses + " colhead col1") + "Tabellnr.</div>");
                outText.Append(StartDiv(accordionClasses + " colhead col2") + "Tittel</div>");
                outText.Append(StartDiv(accordionClasses + " colhead col3") + "Tidsperiode:</div>");
                outText.Append("</div>");
            }

            return outText.ToString();
        }

        private string GetCloseTable(string accordionClasses)
        {
            return WrapInFullRow("<div class='pxweb-table-end" + accordionClasses + "'></div>");
        }


        private string GetLinkRow(TableLink tableLink, UrlResolver urlResolver, string accordionClasses)
        {
            rowCounter++;
            string forRowMouseover = " row_number_" + rowCounter;

            var tableLinkItem = new TableLinkItem(tableLink, urlResolver);

            StringBuilder outText = new StringBuilder();

            outText.Append("<a aria-label='" + "FRA YTRE A" + "' class='pxweb-link menu-tablelist grid-container' href = '" + Server.HtmlEncode(tableLinkItem.Link) + "'>");
            outText.Append(StartDiv(accordionClasses + forRowMouseover + " col1", tableLinkItem.LinkSpanContentHover));
            outText.Append(GetSpan(tableLinkItem.LinkTableId, "font-normal-text"));
            outText.Append("</div>");
            string ariaLabelText = Server.HtmlEncode(tableLinkItem.LinkSpanContent + " " + tableLinkItem.LinkSpanContentHover);
            //outText.Append(StartDiv(accordionClasses + forRowMouseover + " col2", tableLinkItem.LinkSpanContentHover) + "<a aria-label='" + ariaLabelText + "' class='pxweb-link' href = '" + Server.HtmlEncode(tableLinkItem.Link) + "'>");
            outText.Append(StartDiv(accordionClasses + forRowMouseover + " col2", tableLinkItem.LinkSpanContentHover) );
            outText.Append(GetSpan(tableLinkItem.LinkSpanContent, "font-normal-text"));
            //outText.Append("</a></div>");
            outText.Append("</div>");
            outText.Append(StartDiv(accordionClasses + forRowMouseover + " col3", tableLinkItem.LinkSpanContentHover));
            outText.Append(GetSpan(tableLinkItem.LinkSpanPeriod, "font-normal-text"));
            outText.Append("</div>");
            outText.Append("</a>");

            return outText.ToString();
        }



        private string GetAccordionClasses(List<string> belongsToAccordions)
        {
            StringBuilder outText = new StringBuilder();

            if (belongsToAccordions.Count > 0)
            {
                int accordionLevel = 1;
                outText.Append(" accordion-cell");
                foreach (string belongsToAccordion in belongsToAccordions)
                {
                    outText.Append(" Accordion_" + belongsToAccordion);
                    outText.Append(" closed_level_" + accordionLevel);
                    accordionLevel++;
                }
            }

            return outText.ToString();
        }


        private string StartDiv(string classes, string titletext)
        {

            return "<div  class='" + classes + "' title = '" + Server.HtmlEncode(titletext) + "'>";
        }
        private string StartDiv(string classes)
        {
            string classString = "";
            if (!String.IsNullOrWhiteSpace(classes))
            {
                classString = " class='" + classes + "'";
            }
            return "<div" + classString + ">";
        }


        private string GetSpan(string text)
        {
            return "<span>" + Server.HtmlEncode(text) + "</span>";
        }

        private string GetSpan(string text, string cssClass)
        {
            return "<span class='" + cssClass + "'>" + Server.HtmlEncode(text) + "</span>";
        }

        private string WrapInFullRow(string inString)
        {
            return String.Format("<div class='grid-row-1-3'>{0}</div>", inString);
        }




        /// <summary>
        /// Gets the menu object
        /// </summary>
        /// <returns>returns the menu object</returns> 
    private PxMenuBase GetMenu(string path)
        {
            var lang = LocalizationManager.CurrentCulture.Name;
            //  Checks that the necessary parameters are present
            if (String.IsNullOrEmpty(PxUrlObj.Database))
            {
            //    if parameters is missing redirect to the start page
                Response.Redirect("Default.aspx", false);
            }

            try
            {
                PxMenuBase menubase = null;
                if (PXWeb.Management.PxContext.CacheService != null)
                {
                    string key = $"pxc_menu_{PxUrlObj.Database}_{lang}_{path}";
                    menubase = PXWeb.Management.PxContext.CacheService.Get<PxMenuBase>(key);
                    if (menubase != null)
                    {
                        return menubase;
                    }
                }

                menubase = PXWeb.Management.PxContext.GetMenu(PxUrlObj.Database, path);

                if (PXWeb.Management.PxContext.CacheService != null)
                {
                    string key = $"pxc_menu_{PxUrlObj.Database}_{lang}_{path}";

                    if (menubase != null)
                    {
                        PXWeb.Management.PxContext.CacheService.Set(key, menubase);
                    }
                }
                return menubase;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
