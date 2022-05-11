<%@ Page Title="No data" Language="C#" AutoEventWireup="true" CodeBehind="NoData.aspx.cs" Inherits="PXWeb.NoData" %>
<!DOCTYPE html>

<html lang="<%= lang %>">
  <head runat="server">
    <title>
         <%= HeadTitle %>
    </title>
    <link href="<%= ResolveUrl("~/Resources/Styles/main-common.css")%>" rel="stylesheet" type="text/css" media="screen" />
    <link href="<%= ResolveUrl("~/Resources/Styles/main-pxweb.css")%>" rel="stylesheet" type="text/css" media="screen" />
    <link href="<%= ResolveUrl("~/Resources/Styles/main-custom.css")%>" rel="stylesheet" type="text/css" media="screen" />

    </head>
<body style="display:flex;justify-content:center" >
    <div id="pxwebcontent">
        <div class="flex-column">
            <div class="flex-row">
                <img src="~/Resources/Images/SSB_LOGO.png" class="imgSiteLogo" alt="PX-Web" runat="server">
            </div>
            <h1><%= NoDataH1 %> </h1>
            <%= NoDataInfo %>
            <div class="flex-row m-margin-top">
                <a class="pxweb-link" href="https://www.ssb.no/statbank/">Statistikkbanken</a>
            </div>
            <div class="flex-row">
                 <asp:Image id="ErrorLadyImage" runat="server" />
            </div>
        </div>

    </div>
</body>
</html>

