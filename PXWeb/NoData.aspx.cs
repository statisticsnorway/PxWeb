using PCAxis.Web.Core.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PXWeb
{
    public partial class NoData : System.Web.UI.Page
    {
        
        public string lang { get; set; }

        public string HeadTitle { get; set; }

        public string NoDataInfo { get; set; }

        public string NoDataH1 { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            HeadTitle = "Statistikkbanken";

            bool isEnglish = this.Page.Request.Url.AbsoluteUri.Contains("/en/");
            lang = isEnglish ? "en" : "no"; 
                

            string tableIdInfo = "";
            string tableId = ValidationManager.GetValue(this.Page.Request.QueryString["tableId"]);

            if (!string.IsNullOrEmpty(tableId) && isEnglish)
            {
                tableIdInfo = string.Format(" for table {0}", tableId);
            }
            else
            {
                tableIdInfo = string.Format(" for tabell {0}", tableId);
            }

            if (isEnglish)
            {
                NoDataH1 = "Temporarily unavailable";
                NoDataInfo = string.Format("The figures are temporarily unavailable{0}.", tableIdInfo);
                ErrorLadyImage.AlternateText = "Error 404";
                ErrorLadyImage.ImageUrl = "~/Resources/Images/svg/OhNoData_en.svg";
            }
            else
            {
                NoDataH1 = "Midlertidig utilgjengelig";
                NoDataInfo = string.Format("Tallene er midlertidig utilgjengelige{0}.", tableIdInfo);
                ErrorLadyImage.AlternateText = "404 feil";
                ErrorLadyImage.ImageUrl = "~/Resources/Images/svg/OhNoData_no.svg";
            }

        }
    }
}