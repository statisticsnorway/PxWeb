<%@ Page Title="" Language="C#" MasterPageFile="~/PxWeb.Master" AutoEventWireup="true" CodeBehind="TableList.aspx.cs" Inherits="PXWeb.TableList" %>

<%@ MasterType VirtualPath="~/PxWeb.Master" %>
<%@ Register Src="~/UserControls/MenuExplanation.ascx" TagPrefix="uc1" TagName="MenuExplanation" %>

<%@ Register TagPrefix="pxwebCC" Namespace="PXWeb.CustomControls" Assembly="PXWeb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolderHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolderMain" runat="server">
    <div id="pxcontent">
        <pxwebCC:UserManualScreenReader ID="UserManualMenu" manualFor="TableList"
            headerCode="UserManualScreenReader_TableList_Header"
            textCode="UserManualScreenReader_TableList_Text"
            runat="server" ClientIDMode="Static"/>
    </div>
    
    <uc1:MenuExplanation runat="server" ID="MenuExplanation" class="tablelist_explanation" />
    <asp:Panel runat="server" class="menu-tablelist grid-container" ClientIDMode="Static" ID="MenuTableList" >
    </asp:Panel>
   <script>
       function cellAccordionToggle(panel, button, accordionclass, closeClass) {
           //closeClass f.x. close_level_2
     
           jQuery(panel).find('.' + accordionclass).each(function (i, element) {
               
               element.classList.toggle(closeClass);

           })

           /*accordion head*/
           button.classList.toggle("closed");
           if (button.classList.contains('closed')) {
               button.setAttribute('aria-expanded', 'false');
           } else {
               button.setAttribute('aria-expanded', 'true');
           }
           return true;
       };

        //row_number_N   div[class^='row_number_']

       jQuery(document).ready(function () {

           jQuery("#MenuTableList div[class*='row_number_']").on("mouseover mouseout",
               function (e) {

                   var rowClass = "";

                   var classList = $(this).attr('class').trim();
                   var classArr = classList.split(/\s+/);
                   $.each(classArr, function (index, value) {
                       if (value.includes("row_number_")) {
                           rowClass = value;
                           return false; //exit loop
                       }
                   });

                   //There are 3 div with this row*Class. dont want to loop the full tree, checking 2 before and 2 after
                   
                   var rowMembers = [jQuery(this)];

                   if (jQuery(this).prev() && jQuery(this).prev().hasClass(rowClass)) {
                       rowMembers.push($(this).prev());
                       if (jQuery(this).prev().prev() && jQuery(this).prev().prev().hasClass(rowClass)) {
                           rowMembers.push(jQuery(this).prev().prev());
                       }
                   }

                   if (jQuery(this).next() && jQuery(this).next().hasClass(rowClass)) {
                       rowMembers.push(jQuery(this).next());

                       if (jQuery(this).next().next() && jQuery(this).next().next().hasClass(rowClass)) {
                           rowMembers.push(jQuery(this).next().next());
                       }
                   }

                   var i;
                   for (i = 0; i < rowMembers.length; i++) {
                       if (e.type == 'mouseover') {
                           rowMembers[i].addClass("table-hover");
                       } else {
                           rowMembers[i].removeClass("table-hover");
                       };
                   }
                   
               });
       });

   </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolderFooter" runat="server">
</asp:Content>





