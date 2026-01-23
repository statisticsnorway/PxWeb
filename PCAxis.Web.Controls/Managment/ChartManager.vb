Imports PCAxis.Chart
Imports PCAxis.Web.Core.StateProvider

Public NotInheritable Class ChartManager

    Const CHART_SETTINGS As String = "CharSettings"
    Private Shared _logger As log4net.ILog = log4net.LogManager.GetLogger(GetType(ChartManager))

    Public Shared Property Settings() As ChartSettings
        Get
            _logger.Info("ChartManager: start")
            'Fetch the current model from the stateprovider
            Dim s As ChartSettings = CType(StateProviderFactory.GetStateProvider().Item(GetType(ChartSettings), CHART_SETTINGS), ChartSettings)
            If s Is Nothing Then
                _logger.Info("ChartManager: if s is nothing")
                s = New ChartSettings()
                ChartManager.Settings = s
                If _initializer IsNot Nothing Then
                    _logger.Info("ChartManager: if _initializer IsNot Nothing")
                    _initializer(s)
                Else
                    _logger.Info("ChartManager: else _initializer IsNot Nothing")
                End If
            Else
                _logger.Info("ChartManager: else s is nothing")
            End If

            _logger.Info("ChartManager: end")
            Return s
        End Get
        Set(ByVal value As ChartSettings)
            StateProviderFactory.GetStateProvider().Add(GetType(ChartSettings), CHART_SETTINGS, value)
        End Set
    End Property

    Public Delegate Sub InitializeSettings(ByVal settings As ChartSettings)

    Private Shared _initializer As InitializeSettings

    Public Shared Property SettingsInitializer() As InitializeSettings
        Get
            Return _initializer
        End Get
        Set(ByVal value As InitializeSettings)
            _logger.Info("ChartManager: start SettingsInitializer")

            _initializer = value
            If _initializer IsNot Nothing Then
                _logger.Info("ChartManager: if _initializer IsNot Nothing")
            Else
                _logger.Info("ChartManager: else _initializer IsNot Nothing")
            End If
            _logger.Info("ChartManager: end SettingsInitializer")
        End Set
    End Property


End Class
