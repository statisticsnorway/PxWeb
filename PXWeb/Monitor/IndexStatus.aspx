<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IndexStatus.aspx.cs" Inherits="PXWeb.Monitor.IndexStatus" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
       body
       {
           font-family:Arial;
       }
        .indexOK {
            background: #b6ff00;
        }

        .indexError {
            background: #ff0000;
        }

        .melding {
            padding: 5px;
            margin-left: -20px;
            background: #F7F7F7;
        }

        .seksjon {
            border-bottom: 2px groove #EEEEEE;
            padding-bottom: 5px;
            margin-left: 20px;
            margin-right: 20px;
        }

        h1 {
            margin-left: 20px;
        }

        h2 {
            margin: 5px 0;
        }

        ul {
            margin-top: 0;
            margin-bottom: 0;
        }

        .info {
            background: #EFEFEF;
            color: #555555;
            font-size: 0.8em;
            text-align: left;
            margin-left: 20px;
            margin-right: 20px;
            margin-top: 5px;
        }
        .status {
            font-size: 0.8em;
            text-align: left;
            margin-left: 20px;
            margin-right: 20px;
            margin-top: 5px;
        }

            .info p {
                padding: 0 10px;
                margin: 0;
                display: inline;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <h1>Indekseringsstatus</h1>
        <div class="status">
            <div>
                <asp:Label ID="lblIndexStatus" runat="server" Text="Indeksstatus"></asp:Label>
                <asp:Label ID="txtIndexStatus" runat="server" Text=""></asp:Label>
            </div>
            <div>
                <asp:Label ID="lblLastIndexed" runat="server" Text="Sist indeksert"></asp:Label>
                <asp:Label ID="txtLastIndexed" runat="server" Text=""></asp:Label>
            </div>
        </div>
         <div class="info">
            <p>Server: <strong><%Response.Write(System.Environment.MachineName);%></strong></p>
            <p>Timestamp: <strong><%Response.Write(DateTime.Now.ToString());%></strong></p>
        </div>
    </form>
</body>
</html>
