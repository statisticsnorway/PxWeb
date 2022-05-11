''' <summary>
''' Default implementation for logging visitor statistics.
'''Logs visitor actions to file using Log4Net.
''' </summary>
''' <remarks></remarks>
Public Class PxSSBLogger
    Implements IActionLogger

    Private Shared _logger As log4net.ILog = log4net.LogManager.GetLogger(GetType(PxSSBLogger))

    Public Sub LogEvent(ByVal context As ActionContext, ByVal userid As String, ByVal lang As String, ByVal database As String, ByVal e As PxActionEventArgs) Implements IActionLogger.LogEvent
        Dim logMess As System.Text.StringBuilder = New System.Text.StringBuilder()


        Dim convertedAction = ActionConverter(e.ActionName)

        If convertedAction <> "p99" Then

            'for databaselogging
            log4net.LogicalThreadContext.Properties("System") = System.Configuration.ConfigurationManager.AppSettings("system")
            log4net.LogicalThreadContext.Properties("Context") = context.ToString()
            log4net.LogicalThreadContext.Properties("Userid") = userid
            log4net.LogicalThreadContext.Properties("Lang") = lang
            log4net.LogicalThreadContext.Properties("Database") = database
            log4net.LogicalThreadContext.Properties("Cached") = "0"
            log4net.LogicalThreadContext.Properties("ActionType") = e.ActionType.ToString()
            log4net.LogicalThreadContext.Properties("ActionName") = convertedAction
            log4net.LogicalThreadContext.Properties("NumberOfCells") = e.NumberOfCells.ToString()
            log4net.LogicalThreadContext.Properties("NumberOfContents") = e.NumberOfContents.ToString()
            log4net.LogicalThreadContext.Properties("TableId") = e.TableId

            'for filelogging
            logMess.Append("System=" & System.Configuration.ConfigurationManager.AppSettings("system").ToString() & ", ")
            logMess.Append("Context=" & context.ToString() & ", ")
            logMess.Append("UserId=" & userid & ", ")
            logMess.Append("Language=" & lang & ", ")
            logMess.Append("Database=" & database & ", ")
            logMess.Append("ActionName=" & e.ActionName + ", ")
            logMess.Append("ActionType=" & e.ActionType.ToString() + ", ")
            logMess.Append("TableId=" & e.TableId & ", ")
            logMess.Append("NumberOfCells=" & e.NumberOfCells.ToString() & ", ")
            logMess.Append("NumberOfContents=" & e.NumberOfContents.ToString())
            _logger.Info(logMess.ToString())
        End If

    End Sub

    Public Function ActionConverter(ByVal ActionName As String) As String
        Select Case ActionName
            Case "tableViewLayout1"
                Return "p01"
            Case "tableViewLayout2"
                Return "p02"
            Case "FileTypePX"
                Return "p05"
            Case "FileTypeJson"
                Return "p06"
            Case "FileTypeJsonStat"
                Return "p07"
            Case "FileTypeJsonStat2"
                Return "p08"
            Case "FileTypeExcelX"
                Return "p11"
            'Case "FileTypeExcelDoubleColumn"
            '    Return "p12"
            'Case "FileTypeExcelCompleteInfoFile"
            '    Return "p13"
            Case "FileTypeExcelXDoubleColumn"
                Return "p14"
            Case "FileTypeCsvWithHeadingAndTabulator"
                Return "p21"
            Case "FileTypeCsvWithoutHeadingAndTabulator"
                Return "p22"
            Case "FileTypeCsvWithHeadingAndComma"
                Return "p23"
            Case "FileTypeCsvWithoutHeadingAndComma"
                Return "p24"
            Case "FileTypeCsvWithHeadingAndSpace"
                Return "p25"
            Case "FileTypeCsvWithoutHeadingAndSpace"
                Return "p26"
            Case "FileTypeCsvWithHeadingAndSemiColon"
                Return "p27"
            Case "FileTypeCsvWithoutHeadingAndSemiColon"
                Return "p28"
            Case "FileTypeText"
                Return "p29"
            Case "FileTypeRelational"
                Return "p30"
            Case "FileTypeHtml"
                Return "p31"
            Case "FileTypeChartPng"
                Return "p32"
            Case "FileTypeChartGif"
                Return "p33"
            Case "FileTypeChartJpeg"
                Return "p34"
            Case "tableViewSorted"
                Return "p35"
            Case "chartViewColumn"
                Return "p36"
            Case "chartViewBar"
                Return "p37"
            Case "chartViewLine"
                Return "p38"
            Case "chartViewPie"
                Return "p39"
            Case "chartViewPopulationPyramid"
                Return "p40"
        End Select
        Return "p99"
    End Function
End Class

