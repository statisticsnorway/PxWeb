<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PxWeb.Monitor.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .applicationOK {
            background: #EEEEEE url(accept.png) no-repeat scroll 2px 5px;
            padding-left: 20px;
            border: solid 1px;
            margin-bottom: 5px;
            padding-top: 5px;
        }

        .applicationERROR {
            background: #EEEEEE url(exclamation.png) no-repeat scroll 2px 5px;
            padding-left: 20px;
            border: solid 1px;
            margin-bottom: 5px;
            padding-top: 5px;
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
            text-align: right;
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
    <h1><asp:Label runat="server" id="monitorTitle"></asp:Label></h1>

    <div class="seksjon">
        <h2 runat="server" id="IntegrationTests" >Tester integrasjonspunkter  </h2>
        <div>
          <asp:Panel runat="server" ID="connection"><strong>Database:</strong> Testet oppkobling <%Response.Write("(" +connectionString + ")");%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="luceneNoServer"><strong>API/Lucene:</strong> <%Response.Write(LuceneNoServerMessage);%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="luceneEnServer"><strong>API/Lucene:</strong> <%Response.Write(LuceneEnServerMessage);%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="triggerFil"><strong>API/Lucene:</strong> <%Response.Write(TriggerFilMessage);%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="indexDateNo"><strong>API/Lucene:</strong> <%Response.Write(IndexDateNoMessage);%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="indexDateEn"><strong>API/Lucene:</strong> <%Response.Write(IndexDateEnMessage);%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="IndexingLastRun"><strong>API/Lucene:</strong> <%Response.Write(IndexingLastRunMessage);%></asp:Panel>
        </div>
        <div>
            <asp:Panel runat="server" ID="IndexStatus"><strong>API/Lucene:</strong> <%Response.Write(IndexStatusMessage);%></asp:Panel>
        </div>
        <div class="info">
            <p>Server: <strong><%Response.Write(System.Environment.MachineName);%></strong></p>
            <p>Timestamp: <strong><%Response.Write(DateTime.Now.ToString());%></strong></p>
        </div>
    </div>


    <div class="seksjon">
        <h2>System</h2>

        <ul>
            <li>App version: <% Response.Write(AssemblyVersion); %></li>
            <li>.Net CLR version: <% Response.Write(DotNetVersion);%></li>
            <li>Server software: <% Response.Write(Request.ServerVariables["SERVER_SOFTWARE"].ToString());%></li>
        </ul>
    </div>
</body>
</html>
