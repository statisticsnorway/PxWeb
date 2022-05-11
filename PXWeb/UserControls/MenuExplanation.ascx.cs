using PCAxis.Web.Core.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PXWeb.UserControls
{
    public partial class MenuExplanation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Localize();
        }
        private void Localize()
        {
            explanationTextRegionHeader.Text = PCAxis.Web.Core.Management.LocalizationManager.GetLocalizedString("PxWebMenuExplanationRegionHeader");
            explanationTextRegion.Text =  PCAxis.Web.Core.Management.LocalizationManager.GetLocalizedString("PxWebMenuExplanationRegion");
            explanationTextTimePeriodHeader.Text = PCAxis.Web.Core.Management.LocalizationManager.GetLocalizedString("PxWebMenuExplanationTimePeriodHeader");
            explanationTextTimePeriod.Text = PCAxis.Web.Core.Management.LocalizationManager.GetLocalizedString("PxWebMenuExplanationTimePeriod");
        }


        private void SetExplanationRegionVisible(bool regionVisible )
        {
            explanationRegion.Visible = regionVisible;
        }

        private void SetExplanationTimePeriodVisible(bool timePeriodVisible)
        {
            explanationTimePeriod.Visible = timePeriodVisible;
        }

        public void SetExplanationRegionTimePeriodVisible(bool regionVisible, bool timePeriodVisible)
        {
            SetExplanationRegionVisible(regionVisible);
            SetExplanationTimePeriodVisible(timePeriodVisible);
            Visible = Settings.Current.Menu.ShowMenuExplanation;
            if (!(regionVisible || timePeriodVisible))
                Visible = false;
        }
    }
}