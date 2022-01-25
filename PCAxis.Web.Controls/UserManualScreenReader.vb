Public Class UserManualScreenReader

    ''' <summary>
    ''' Adds html to the two strings, using server to html encode them
    ''' </summary>
    ''' <param name="server"></param>
    ''' <param name="heading"></param>
    ''' <param name="longText"></param>
    ''' <returns></returns>
    Public Shared Function GetString(ByRef server As System.Web.HttpServerUtility, ByVal heading As String, ByVal longText As String) As String
        Dim builder As New System.Text.StringBuilder
        builder.Append("<section aria-label=""")
        builder.Append(server.HtmlEncode(heading))
        builder.Append("""><span class=""screenreader-only"">")
        builder.Append(server.HtmlEncode(longText))
        builder.Append("</span></section>")
        Return builder.ToString
    End Function

End Class
