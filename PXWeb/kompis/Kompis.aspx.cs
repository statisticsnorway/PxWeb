using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace PXWeb.kompis
{
    public partial class Kompis : System.Web.UI.Page
    {
        string komisApiUrlBase;
        string id;
        string ver;
        string val;
        string lang;
        string KompisUrl;
        string json;
        protected void Page_Load(object sender, EventArgs e)
        {
            init();
            showKompisInfo();
        }
        private void init()
        {
            komisApiUrlBase = WebConfigurationManager.AppSettings["KompisApiUrl"];
            id = PCAxis.Web.Core.Management.ValidationManager.GetValue(Request.QueryString["id"].ToString()) ;
            ver = PCAxis.Web.Core.Management.ValidationManager.GetValue(Request.QueryString["ver"].ToString());
            val = PCAxis.Web.Core.Management.ValidationManager.GetValue(Request.QueryString["val"].ToString());
            lang = PCAxis.Web.Core.Management.ValidationManager.GetValue(Request.QueryString["lang"].ToString());
            KompisUrl = komisApiUrlBase + "/" + id + "/" + ver + "/" + val + "/";
        }
        private void showKompisInfo()
        {
            try
            {
                var client = new WebClientWithTimeout();
                client.Headers["Content_type"] = "application/json;charset=UTF-8";
                client.Encoding = System.Text.Encoding.UTF8;
                json = client.DownloadString(KompisUrl);

                Kompisresult kompisresult = JsonConvert.DeserializeObject<Kompisresult>(json);

                if (lang == "no")
                {
                    lblTitleValue.Text = Server.HtmlEncode(kompisresult.variable.labelNo);
                    lblDescription.Text = Server.HtmlEncode(kompisresult.variable.descriptionNo);
                    //lblFormulaHeading.Text = "Formel:";
                    btnClose.Text = "Lukk";
                }
                else
                {
                    lblTitleValue.Text = Server.HtmlEncode(kompisresult.variable.labelEn);
                    lblDescription.Text = Server.HtmlEncode(kompisresult.variable.descriptionEn);
                    //lblFormulaHeading.Text = "Formel:";
                    btnClose.Text = "Close";
                }

                //if (kompisresult.formula != null)
                //{
                //    lblFormulaHeading.Visible = true;
                //    lblFormula.Text = kompisresult.formula.ToString();
                //}
            }
            catch (Exception e)
            {
                if (lang == "no")
                {
                    lblTitleValue.Text = "Beskrivelse midlertidig utilgjengelig";
                    btnClose.Text = "Lukk";
                }
                else
                {
                    lblTitleValue.Text = "Description temporary unavailable";
                    btnClose.Text = "Close";
                }
            }
        }
    }
}