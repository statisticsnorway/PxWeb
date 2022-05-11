using PCAxis.Sql.DbConfig;
using PXWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXAxis.Routing.SSB
{
    internal interface ISSBRouteExtender : IRouteExtender
    {
        string GetListUrl(string tableListName);
        string GetSelectionUrl(string tableId);
        string GetPresentationUrl(string tableId, string presentationLayout);
    }
}
