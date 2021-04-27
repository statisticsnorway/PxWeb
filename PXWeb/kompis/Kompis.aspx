<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kompis.aspx.cs" Inherits="PXWeb.kompis.Kompis" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <link href="../Resources/Styles/PxWeb.css" rel="stylesheet" type="text/css" media="screen" /> 
  <link href="../Resources/Styles/Custom.css" rel="stylesheet" type="text/css" media="screen" />  
  <title></title>
  <script lang="JavaScript" type="text/javascript">
        function resizeWindow() {
            window.moveTo(0, 0);
            window.resizeTo(400, 400);
        }
        function closeWindow() {
        var Browser = navigator.appName;
        var indexB = Browser.indexOf('Explorer');

        if (indexB > 0) {
            var indexV = navigator.userAgent.indexOf('MSIE') + 5;
            var Version = navigator.userAgent.substring(indexV, indexV + 1);

            if (Version >= 7) {
                window.open('', '_self', '');
                window.close();
            }
            else if (Version == 6) {
                window.opener = null;
                window.close();
            }
            else {
                window.opener = '';
                window.close();
            }

        }
        else {
            window.close();
            }
        }

  </script>
</head>
<body>
    
    <form id="form1" runat="server">
     <div id="wrap" style="border: 2px solid grey;padding: 30px;border-radius: 25px">
       <div>
            <asp:PlaceHolder ID="ValueNamePlaceholder" runat="server"  >
           <asp:Label ID="lblTitleValue" CssClass="tittel" runat="server"> </asp:Label>
        </asp:PlaceHolder>
           </div>
    
 
       <div id="content" class="dialogboks">

        <asp:PlaceHolder ID="DescriptionPlaceholder" runat="server" >
           <p><asp:Label ID="lblDescription"  CssClass="kompis_description_text" runat="server"> </asp:Label></p>
        </asp:PlaceHolder>
<%--        <asp:PlaceHolder ID="FormulaHeadingPlaceholder" runat="server" >
           <p><asp:Label ID="lblFormulaHeading" CssClass="kompis_formula_heading" Visible="false" runat="server"> </asp:Label></p>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="FormulaPlaceholder" runat="server" >
           <p><asp:Label ID="lblFormula" CssClass="kompis_formula_text" runat="server"> </asp:Label></p>
        </asp:PlaceHolder>--%>
       </div>
       <div id="close" class="kompis_close">
         <asp:LinkButton ID="btnClose" runat="server" OnClientClick=" closeWindow();" Text="Lukk"></asp:LinkButton>
       </div>
     </div>        
    </form>
</body>
</html>
