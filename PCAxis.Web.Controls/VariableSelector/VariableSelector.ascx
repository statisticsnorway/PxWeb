<%@ control inherits="PCAxis.Web.Controls.VariableSelectorCodebehind" %>
<%@ Register Assembly="PCAxis.Web.Controls" Namespace="PCAxis.Web.Controls" TagPrefix="pxc" %>
<%@ Import Namespace="PCAxis.Paxiom"%>
<asp:Panel ID="VariableSelectorPanel" runat="server" >
    <div id="pxcontent"> 
        <pxc:UserManualScreenReader ID="UserManualVariableSelector"
            headerCode="PxWebRegionSelectionUserManualScreenReader"
            textCode="PxWebSkipToSelectionLinkScreenReader"
            runat="server" ClientIDMode="Static"/>
         <pxc:UserManualScreenReader ID="MarkingTipsScreenReader"
            headerCode="PxWebMarkingTipsHeaderScreenReader"
            textCode="PxWebMarkingTipsTextScreenReader"
            runat="server" ClientIDMode="Static"/>
    </div>
    <pxc:VariableSelectorMarkingTips runat="server" ID="VariableSelectorMarkingTips"  />    
     <div role="alert" id="validationsummarynotifyscreenreader" >
       <asp:ValidationSummary ID="SelectionValidationSummary" runat="server" DisplayMode="BulletList"  ShowValidationErrors="true" ShowMessageBox="false" ShowSummary="true" CssClass="variableselector_error_summary" ForeColor="" />   
    </div>
<asp:Repeater ID="VariableSelectorValueSelectRepeater" runat="server" EnableViewState="true">      
        <HeaderTemplate>
            <div class="variableselector_variable_box_container">
        </HeaderTemplate>      
        <ItemTemplate>  
                <asp:PlaceHolder ID="ValueSelectPlaceHolder" runat="server"></asp:PlaceHolder>            
        </ItemTemplate>   
        <FooterTemplate>
            </div>
            <div class="variableselector_clearboth"></div>
        </FooterTemplate>         
    </asp:Repeater>    
 
   
    <div class ="flex-row justify-center m-margin-top">
        <asp:Button ID="ButtonViewTable" runat="server" CssClass="pxweb-btn primary-btn variableselector_continue_button justify-center" CausesValidation="true"/>
            </div>
    <div class ="flex-row justify-center">
     <pxc:VariableSelectorSelectionInformation runat="server" ID="VariableSelectorSelectionInformation" />
    </div>
   <%-- <div role="region" id="selectionerrornotifyscreenreader" aria-atomic="true" aria-live="polite">--%>
        <div class="flex-row justify-center" aria-atomic="true" aria-live="polite">
            <asp:Label ID="SelectionErrorlabel" runat="server" visible="true" CssClass="variableselector_selectionerror_label"/>
            <asp:Label ID="SelectionErrorlabelTextCells" runat="server" CssClass="variableselector_selectionerror_label_text" />
            <asp:Label ID="SelectionErrorlabelTextColumns" runat="server" CssClass="variableselector_selectionerror_label_text" />
            <asp:Label ID="SelectionErrorlabelTextRows" runat="server" CssClass="variableselector_selectionerror_label_text" />
        </div>
    <%--</div>--%>
</asp:Panel>

<asp:Panel ID="SearchVariableValuesPanel" runat="server" Visible="false">
    <pxc:SearchValues ID="SearchVariableValues" runat="server" EnableViewState="true" />     
</asp:Panel>

<asp:Panel runat="server" ID="HierarchicalSelectPanel" Visible="false">   
        <pxc:Hierarchical runat="server" ID="SelectHierarchichalVariable" ShowButtonLabels="true" EnableViewState="true" />
</asp:Panel>

<asp:Panel ID="SelectFromGroupPanel" runat="server" Visible="false">
    <pxc:SelectFromGroup ID="SelectValuesFromGroup" runat="server" />
</asp:Panel>
<script>

    jQuery(document).ready(function () {
        var containerclass = document.getElementsByClassName('variableselector_variable_box_container');
        var boxelement = document.getElementsByClassName('variableselector_valuesselect_box');
    if(containerclass.length > 0 && boxelement.length > 0)
    {
            if (isSelectionLayoutCompact()) {
                containerclass[0].classList.add('flex-row');
                containerclass[0].classList.add('flex-wrap');
                for (index = 0; index < boxelement.length; ++index) {
                    boxelement[index].classList.add('variableselector_valuesselect_box_compact');
                }  
                jQuery(".variableselector_valuesselect_box").resizable({ handles: 'e', minWidth: 150 });
                var group = jQuery(".variableselector_valuesselect_box");
            }
            else {
                containerclass[0].classList.add('flex-column');
                for (index = 0; index < boxelement.length; ++index) {
                    boxelement[index].classList.add('variableselector_valuesselect_box_list');
                }   
            }
    }

        //Prevent resize to propagate down to option-tags
        jQuery("select").mousedown(function(event) {
            event.stopPropagation();
        });
        PCAxis_HideElement(".variableselector_valuesselect_action");
    });

    function ValidateAll()
    {
        var isValid = false;
        isValid = Page_ClientValidate();
        return isValid;
    }
</script>    
