<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MenuExplanation.ascx.cs" Inherits="PXWeb.UserControls.MenuExplanation" %>
<div class="menuExplanationFrame">
    <div id="explanationRegion" runat="server">
        <asp:Label ID="explanationTextRegionHeader" CssClass="menuExplanationHeader" runat="server" Text="" ></asp:Label>
        <asp:Label ID="explanationTextRegion" CssClass="menuExplanation" runat="server" Text="" ></asp:Label>
    </div>

    <div id="explanationTimePeriod" runat="server">
        <asp:Label ID="explanationTextTimePeriodHeader" CssClass="menuExplanationHeader" runat="server" Text="" ></asp:Label>
        <asp:Label ID="explanationTextTimePeriod" CssClass="menuExplanation" runat="server" Text="" ></asp:Label>
    </div>
</div>
