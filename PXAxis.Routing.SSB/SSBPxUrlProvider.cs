using PXWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCAxis.Web.Core.Management;

namespace PXAxis.Routing.SSB
{
    public class SSBPxUrlProvider : IPxUrlProvider
    {
        public IPxUrl Create()
        {
            return new SSBPxUrl();
        }

        public IPxUrl Create(params LinkManager.LinkItem[] links)
        {
            return new SSBPxUrl(links);
        }
    }
}
