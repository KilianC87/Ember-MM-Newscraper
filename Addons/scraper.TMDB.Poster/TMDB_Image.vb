﻿' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ###############################################################################

Imports System.IO
Imports EmberAPI
Imports WatTmdb
Imports ScraperModule.TMDBg
Imports NLog
Imports System.Diagnostics

Public Class TMDB_Image
    Implements Interfaces.ScraperModule_Image_Movie
    Implements Interfaces.ScraperModule_Image_MovieSet


#Region "Fields"
    Shared logger As Logger = NLog.LogManager.GetCurrentClassLogger()
    Public Shared ConfigOptions_Movie As New Structures.MovieScrapeOptions
    Public Shared ConfigOptions_MovieSet As New Structures.MovieSetScrapeOptions
    Public Shared ConfigScrapeModifier_Movie As New Structures.ScrapeModifier
    Public Shared ConfigScrapeModifier_MovieSet As New Structures.ScrapeModifier
    Public Shared _AssemblyName As String

    Private TMDBId As String
    Private _TMDBg As TMDBg.Scraper
    Private TMDB As TMDB.Scraper

    ''' <summary>
    ''' Scraping Here
    ''' </summary>
    ''' <remarks></remarks>
    Private strPrivateAPIKey As String = String.Empty
    Private _MySettings_Movie As New sMySettings_Movie
    Private _MySettings_MovieSet As New sMySettings_MovieSet
    Private _Name As String = "TMDB_Image"
    Private _ScraperEnabled_Movie As Boolean = False
    Private _ScraperEnabled_MovieSet As Boolean = False
    Private _setup_Movie As frmTMDBMediaSettingsHolder_Movie
    Private _setup_MovieSet As frmTMDBMediaSettingsHolder_MovieSet
    Private _TMDBConf As V3.TmdbConfiguration
    Private _TMDBConfE As V3.TmdbConfiguration
    Private _TMDBApi As V3.Tmdb 'preferred language
    Private _TMDBApiE As V3.Tmdb 'english language
    Private _TMDBApiA As V3.Tmdb 'all languages

#End Region 'Fields

#Region "Events"

    Public Event ModuleSettingsChanged_Movie() Implements Interfaces.ScraperModule_Image_Movie.ModuleSettingsChanged

    Public Event MovieScraperEvent_Movie(ByVal eType As Enums.ScraperEventType_Movie, ByVal Parameter As Object) Implements Interfaces.ScraperModule_Image_Movie.ScraperEvent

    Public Event SetupScraperChanged_Movie(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModule_Image_Movie.ScraperSetupChanged

    Public Event SetupNeedsRestart_Movie() Implements Interfaces.ScraperModule_Image_Movie.SetupNeedsRestart

    Public Event ImagesDownloaded_Movie(ByVal Posters As List(Of MediaContainers.Image)) Implements Interfaces.ScraperModule_Image_Movie.ImagesDownloaded

    Public Event ProgressUpdated_Movie(ByVal iPercent As Integer) Implements Interfaces.ScraperModule_Image_Movie.ProgressUpdated


    Public Event ModuleSettingsChanged_MovieSet() Implements Interfaces.ScraperModule_Image_MovieSet.ModuleSettingsChanged

    Public Event MovieScraperEvent_MovieSet(ByVal eType As Enums.ScraperEventType_MovieSet, ByVal Parameter As Object) Implements Interfaces.ScraperModule_Image_MovieSet.ScraperEvent

    Public Event SetupScraperChanged_MovieSet(ByVal name As String, ByVal State As Boolean, ByVal difforder As Integer) Implements Interfaces.ScraperModule_Image_MovieSet.ScraperSetupChanged

    Public Event SetupNeedsRestart_MovieSet() Implements Interfaces.ScraperModule_Image_MovieSet.SetupNeedsRestart

    Public Event ImagesDownloaded_MovieSet(ByVal Posters As List(Of MediaContainers.Image)) Implements Interfaces.ScraperModule_Image_MovieSet.ImagesDownloaded

    Public Event ProgressUpdated_MovieSet(ByVal iPercent As Integer) Implements Interfaces.ScraperModule_Image_MovieSet.ProgressUpdated

#End Region 'Events

#Region "Properties"

    ReadOnly Property ModuleName() As String Implements Interfaces.ScraperModule_Image_Movie.ModuleName, Interfaces.ScraperModule_Image_MovieSet.ModuleName
        Get
            Return _Name
        End Get
    End Property

    ReadOnly Property ModuleVersion() As String Implements Interfaces.ScraperModule_Image_Movie.ModuleVersion, Interfaces.ScraperModule_Image_MovieSet.ModuleVersion
        Get
            Return System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly.Location).FileVersion.ToString
        End Get
    End Property

    Property ScraperEnabled_Movie() As Boolean Implements Interfaces.ScraperModule_Image_Movie.ScraperEnabled
        Get
            Return _ScraperEnabled_Movie
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_Movie = value
        End Set
    End Property

    Property ScraperEnabled_MovieSet() As Boolean Implements Interfaces.ScraperModule_Image_MovieSet.ScraperEnabled
        Get
            Return _ScraperEnabled_MovieSet
        End Get
        Set(ByVal value As Boolean)
            _ScraperEnabled_MovieSet = value
        End Set
    End Property

#End Region 'Properties

#Region "Methods"

    Function QueryScraperCapabilities_Movie(ByVal cap As Enums.ScraperCapabilities) As Boolean Implements Interfaces.ScraperModule_Image_Movie.QueryScraperCapabilities
        Select Case cap
            Case Enums.ScraperCapabilities.Fanart
                Return ConfigScrapeModifier_Movie.Fanart
            Case Enums.ScraperCapabilities.Poster
                Return ConfigScrapeModifier_Movie.Poster
        End Select
        Return False
    End Function

    Function QueryScraperCapabilities_MovieSet(ByVal cap As Enums.ScraperCapabilities) As Boolean Implements Interfaces.ScraperModule_Image_MovieSet.QueryScraperCapabilities
        Select Case cap
            Case Enums.ScraperCapabilities.Fanart
                Return ConfigScrapeModifier_MovieSet.Fanart
            Case Enums.ScraperCapabilities.Poster
                Return ConfigScrapeModifier_MovieSet.Poster
        End Select
        Return False
    End Function

    Private Sub Handle_ModuleSettingsChanged_Movie()
        RaiseEvent ModuleSettingsChanged_Movie()
    End Sub

    Private Sub Handle_SetupNeedsRestart_Movie()
        RaiseEvent SetupNeedsRestart_Movie()
    End Sub

    Private Sub Handle_ModuleSettingsChanged_MovieSet()
        RaiseEvent ModuleSettingsChanged_MovieSet()
    End Sub

    Private Sub Handle_SetupNeedsRestart_MovieSet()
        RaiseEvent SetupNeedsRestart_MovieSet()
    End Sub

    Private Sub Handle_SetupScraperChanged_Movie(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_Movie = state
        RaiseEvent SetupScraperChanged_Movie(String.Concat(Me._Name, "_Movie"), state, difforder)
    End Sub

    Private Sub Handle_SetupScraperChanged_MovieSet(ByVal state As Boolean, ByVal difforder As Integer)
        ScraperEnabled_MovieSet = state
        RaiseEvent SetupScraperChanged_MovieSet(String.Concat(Me._Name, "_MovieSet"), state, difforder)
    End Sub

    Sub Init_Movie(ByVal sAssemblyName As String) Implements Interfaces.ScraperModule_Image_Movie.Init
        _AssemblyName = sAssemblyName
        LoadSettings_Movie()
        'Must be after Load settings to retrieve the correct API key
        _TMDBApi = New WatTmdb.V3.Tmdb(_MySettings_Movie.TMDBAPIKey, _MySettings_Movie.TMDBLanguage)
        If IsNothing(_TMDBApi) Then
            logger.Error(Master.eLang.GetString(938, "TheMovieDB API is missing or not valid"), _TMDBApi.Error.status_message)
        Else
            If Not IsNothing(_TMDBApi.Error) AndAlso _TMDBApi.Error.status_message.Length > 0 Then
                logger.Error(_TMDBApi.Error.status_message, _TMDBApi.Error.status_code.ToString())
            End If
        End If
        _TMDBConf = _TMDBApi.GetConfiguration()
        _TMDBApiE = New WatTmdb.V3.Tmdb(_MySettings_Movie.TMDBAPIKey)
        _TMDBConfE = _TMDBApiE.GetConfiguration()
        _TMDBApiA = New WatTmdb.V3.Tmdb(_MySettings_Movie.TMDBAPIKey, "")
        _TMDBg = New TMDBg.Scraper(_TMDBConf, _TMDBConfE, _TMDBApi, _TMDBApiE, _TMDBApiA, True)
        TMDB = New TMDB.Scraper(_TMDBConf, _TMDBConfE, _TMDBApi, _TMDBApiE, _TMDBApiA, _MySettings_Movie)
    End Sub

    Sub Init_MovieSet(ByVal sAssemblyName As String) Implements Interfaces.ScraperModule_Image_MovieSet.Init
        _AssemblyName = sAssemblyName
        LoadSettings_MovieSet()
        'Must be after Load settings to retrieve the correct API key
        _TMDBApi = New WatTmdb.V3.Tmdb(_MySettings_MovieSet.TMDBAPIKey, _MySettings_MovieSet.TMDBLanguage)
        If IsNothing(_TMDBApi) Then
            logger.Error(Master.eLang.GetString(938, "TheMovieDB API is missing or not valid"), _TMDBApi.Error.status_message)
        Else
            If Not IsNothing(_TMDBApi.Error) AndAlso _TMDBApi.Error.status_message.Length > 0 Then
                logger.Error(_TMDBApi.Error.status_message, _TMDBApi.Error.status_code.ToString())
            End If
        End If
        _TMDBConf = _TMDBApi.GetConfiguration()
        _TMDBApiE = New WatTmdb.V3.Tmdb(_MySettings_MovieSet.TMDBAPIKey)
        _TMDBConfE = _TMDBApiE.GetConfiguration()
        _TMDBApiA = New WatTmdb.V3.Tmdb(_MySettings_MovieSet.TMDBAPIKey, "")
        _TMDBg = New TMDBg.Scraper(_TMDBConf, _TMDBConfE, _TMDBApi, _TMDBApiE, _TMDBApiA, True)
        TMDB = New TMDB.Scraper(_TMDBConf, _TMDBConfE, _TMDBApi, _TMDBApiE, _TMDBApiA, _MySettings_Movie) 'todo: _MySettings_MovieSet
    End Sub

    Function InjectSetupScraper_Movie() As Containers.SettingsPanel Implements Interfaces.ScraperModule_Image_Movie.InjectSetupScraper
        Dim Spanel As New Containers.SettingsPanel
        _setup_Movie = New frmTMDBMediaSettingsHolder_Movie
        LoadSettings_Movie()
        _setup_Movie.cbEnabled.Checked = _ScraperEnabled_Movie
        _setup_Movie.chkScrapePoster.Checked = ConfigScrapeModifier_Movie.Poster
        _setup_Movie.chkScrapeFanart.Checked = ConfigScrapeModifier_Movie.Fanart
        _setup_Movie.txtTMDBApiKey.Text = strPrivateAPIKey
        _setup_Movie.cbTMDBLanguage.Text = _MySettings_Movie.TMDBLanguage
        _setup_Movie.chkFallBackEng.Checked = _MySettings_Movie.FallBackEng
        _setup_Movie.chkTMDBLanguagePrefOnly.Checked = _MySettings_Movie.TMDBLanguagePrefOnly
        _setup_Movie.Lang = _setup_Movie.cbTMDBLanguage.Text
        _setup_Movie.API = _setup_Movie.txtTMDBApiKey.Text

        _setup_Movie.orderChanged()

        Spanel.Name = String.Concat(Me._Name, "_Movie")
        Spanel.Text = Master.eLang.GetString(937, "TMDB")
        Spanel.Prefix = "TMDBMovieMedia_"
        Spanel.Order = 110
        Spanel.Parent = "pnlMovieMedia"
        Spanel.Type = Master.eLang.GetString(36, "Movies")
        Spanel.ImageIndex = If(Me._ScraperEnabled_Movie, 9, 10)
        Spanel.Panel = Me._setup_Movie.pnlSettings

        AddHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
        AddHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
        AddHandler _setup_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_Movie
        Return Spanel
    End Function

    Function InjectSetupScraper_MovieSet() As Containers.SettingsPanel Implements Interfaces.ScraperModule_Image_MovieSet.InjectSetupScraper
        Dim Spanel As New Containers.SettingsPanel
        _setup_MovieSet = New frmTMDBMediaSettingsHolder_MovieSet
        LoadSettings_MovieSet()
        _setup_MovieSet.cbEnabled.Checked = _ScraperEnabled_MovieSet
        _setup_MovieSet.chkScrapePoster.Checked = ConfigScrapeModifier_MovieSet.Poster
        _setup_MovieSet.chkScrapeFanart.Checked = ConfigScrapeModifier_MovieSet.Fanart
        _setup_MovieSet.txtTMDBApiKey.Text = strPrivateAPIKey
        _setup_MovieSet.cbTMDBLanguage.Text = _MySettings_MovieSet.TMDBLanguage
        _setup_MovieSet.chkFallBackEng.Checked = _MySettings_MovieSet.FallBackEng
        _setup_MovieSet.chkTMDBLanguagePrefOnly.Checked = _MySettings_MovieSet.TMDBLanguagePrefOnly
        _setup_MovieSet.Lang = _setup_MovieSet.cbTMDBLanguage.Text
        _setup_MovieSet.API = _setup_MovieSet.txtTMDBApiKey.Text

        _setup_MovieSet.orderChanged()

        Spanel.Name = String.Concat(Me._Name, "_MovieSet")
        Spanel.Text = Master.eLang.GetString(937, "TMDB")
        Spanel.Prefix = "TMDBMovieSetMedia_"
        Spanel.Order = 110
        Spanel.Parent = "pnlMovieSetMedia"
        Spanel.Type = Master.eLang.GetString(1203, "MovieSets")
        Spanel.ImageIndex = If(Me._ScraperEnabled_MovieSet, 9, 10)
        Spanel.Panel = Me._setup_MovieSet.pnlSettings

        AddHandler _setup_MovieSet.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_MovieSet
        AddHandler _setup_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_MovieSet
        AddHandler _setup_MovieSet.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_MovieSet
        Return Spanel
    End Function

    Sub LoadSettings_Movie()

        strPrivateAPIKey = clsAdvancedSettings.GetSetting("TMDBAPIKey", "", , Enums.Content_Type.Movie)
        _MySettings_Movie.TMDBAPIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)
        _MySettings_Movie.FallBackEng = clsAdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.Content_Type.Movie)
        _MySettings_Movie.TMDBLanguage = clsAdvancedSettings.GetSetting("TMDBLanguage", "en", , Enums.Content_Type.Movie)
        _MySettings_Movie.TMDBLanguagePrefOnly = clsAdvancedSettings.GetBooleanSetting("TMDBLanguagePrefOnly", True, , Enums.Content_Type.Movie)

        ConfigScrapeModifier_Movie.Poster = clsAdvancedSettings.GetBooleanSetting("DoPoster", True, , Enums.Content_Type.Movie)
        ConfigScrapeModifier_Movie.Fanart = clsAdvancedSettings.GetBooleanSetting("DoFanart", True, , Enums.Content_Type.Movie)

    End Sub

    Sub LoadSettings_MovieSet()

        strPrivateAPIKey = clsAdvancedSettings.GetSetting("TMDBAPIKey", "", , Enums.Content_Type.MovieSet)
        _MySettings_MovieSet.TMDBAPIKey = If(String.IsNullOrEmpty(strPrivateAPIKey), "44810eefccd9cb1fa1d57e7b0d67b08d", strPrivateAPIKey)
        _MySettings_MovieSet.FallBackEng = clsAdvancedSettings.GetBooleanSetting("FallBackEn", False, , Enums.Content_Type.MovieSet)
        _MySettings_MovieSet.TMDBLanguage = clsAdvancedSettings.GetSetting("TMDBLanguage", "en", , Enums.Content_Type.MovieSet)
        _MySettings_MovieSet.TMDBLanguagePrefOnly = clsAdvancedSettings.GetBooleanSetting("TMDBLanguagePrefOnly", True, , Enums.Content_Type.MovieSet)

        ConfigScrapeModifier_MovieSet.Poster = clsAdvancedSettings.GetBooleanSetting("DoPoster", True, , Enums.Content_Type.MovieSet)
        ConfigScrapeModifier_MovieSet.Fanart = clsAdvancedSettings.GetBooleanSetting("DoFanart", True, , Enums.Content_Type.MovieSet)

    End Sub

    Function Scraper(ByRef DBMovie As Structures.DBMovie, ByVal Type As Enums.ScraperCapabilities, ByRef ImageList As List(Of MediaContainers.Image)) As Interfaces.ModuleResult Implements Interfaces.ScraperModule_Image_Movie.Scraper

        LoadSettings_Movie()

        If String.IsNullOrEmpty(DBMovie.Movie.TMDBID) Then
            _TMDBg.GetMovieID(DBMovie)
        End If

        Dim Settings As TMDB.Scraper.sMySettings_ForScraper
        Settings.FallBackEng = _MySettings_Movie.FallBackEng
        Settings.TMDBAPIKey = _MySettings_Movie.TMDBAPIKey
        Settings.TMDBLanguage = _MySettings_Movie.TMDBLanguage
        Settings.TMDBLanguagePrefOnly = _MySettings_Movie.TMDBLanguagePrefOnly

        ImageList = TMDB.GetTMDBImages(DBMovie.Movie.TMDBID, Type, Settings)

        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Function Scraper(ByRef DBMovieSet As Structures.DBMovieSet, ByVal Type As Enums.ScraperCapabilities, ByRef ImageList As List(Of MediaContainers.Image)) As Interfaces.ModuleResult Implements Interfaces.ScraperModule_Image_MovieSet.Scraper

        LoadSettings_MovieSet()

        Dim Settings As TMDB.Scraper.sMySettings_ForScraper
        Settings.FallBackEng = _MySettings_MovieSet.FallBackEng
        Settings.TMDBAPIKey = _MySettings_MovieSet.TMDBAPIKey
        Settings.TMDBLanguage = _MySettings_MovieSet.TMDBLanguage
        Settings.TMDBLanguagePrefOnly = _MySettings_MovieSet.TMDBLanguagePrefOnly

        ImageList = TMDB.GetTMDBImages(DBMovieSet.MovieSet.ID, Type, Settings)

        Return New Interfaces.ModuleResult With {.breakChain = False}
    End Function

    Sub SaveSettings_Movie()
        Using settings = New clsAdvancedSettings()
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_Movie.Poster, , , Enums.Content_Type.Movie)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_Movie.Fanart, , , Enums.Content_Type.Movie)

            settings.SetSetting("TMDBAPIKey", _setup_Movie.txtTMDBApiKey.Text, , , Enums.Content_Type.Movie)
            settings.SetBooleanSetting("FallBackEn", _MySettings_Movie.FallBackEng, , , Enums.Content_Type.Movie)
            settings.SetBooleanSetting("TMDBLanguagePrefOnly", _MySettings_Movie.TMDBLanguagePrefOnly, , , Enums.Content_Type.Movie)
            settings.SetSetting("TMDBLanguage", _MySettings_Movie.TMDBLanguage, , , Enums.Content_Type.Movie)
        End Using
    End Sub

    Sub SaveSettings_MovieSet()
        Using settings = New clsAdvancedSettings()
            settings.SetBooleanSetting("DoPoster", ConfigScrapeModifier_MovieSet.Poster, , , Enums.Content_Type.MovieSet)
            settings.SetBooleanSetting("DoFanart", ConfigScrapeModifier_MovieSet.Fanart, , , Enums.Content_Type.MovieSet)

            settings.SetSetting("TMDBAPIKey", _setup_MovieSet.txtTMDBApiKey.Text, , , Enums.Content_Type.MovieSet)
            settings.SetBooleanSetting("FallBackEn", _MySettings_MovieSet.FallBackEng, , , Enums.Content_Type.MovieSet)
            settings.SetBooleanSetting("TMDBLanguagePrefOnly", _MySettings_MovieSet.TMDBLanguagePrefOnly, , , Enums.Content_Type.MovieSet)
            settings.SetSetting("TMDBLanguage", _MySettings_MovieSet.TMDBLanguage, , , Enums.Content_Type.Movie)
        End Using
    End Sub

    Sub SaveSetupScraper_Movie(ByVal DoDispose As Boolean) Implements Interfaces.ScraperModule_Image_Movie.SaveSetupScraper
        _MySettings_Movie.TMDBLanguage = _setup_Movie.cbTMDBLanguage.Text
        _MySettings_Movie.FallBackEng = _setup_Movie.chkFallBackEng.Checked
        _MySettings_Movie.TMDBLanguagePrefOnly = _setup_Movie.chkTMDBLanguagePrefOnly.Checked
        ConfigScrapeModifier_Movie.Poster = _setup_Movie.chkScrapePoster.Checked
        ConfigScrapeModifier_Movie.Fanart = _setup_Movie.chkScrapeFanart.Checked
        SaveSettings_Movie()
        'ModulesManager.Instance.SaveSettings()
        If DoDispose Then
            RemoveHandler _setup_Movie.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_Movie
            RemoveHandler _setup_Movie.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_Movie
            RemoveHandler _setup_Movie.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_Movie
            _setup_Movie.Dispose()
        End If
    End Sub

    Sub SaveSetupScraper_MovieSet(ByVal DoDispose As Boolean) Implements Interfaces.ScraperModule_Image_MovieSet.SaveSetupScraper
        _MySettings_MovieSet.TMDBLanguage = _setup_MovieSet.cbTMDBLanguage.Text
        _MySettings_MovieSet.FallBackEng = _setup_MovieSet.chkFallBackEng.Checked
        _MySettings_MovieSet.TMDBLanguagePrefOnly = _setup_MovieSet.chkTMDBLanguagePrefOnly.Checked
        ConfigScrapeModifier_MovieSet.Poster = _setup_MovieSet.chkScrapePoster.Checked
        ConfigScrapeModifier_MovieSet.Fanart = _setup_MovieSet.chkScrapeFanart.Checked
        SaveSettings_MovieSet()
        'ModulesManager.Instance.SaveSettings()
        If DoDispose Then
            RemoveHandler _setup_MovieSet.SetupScraperChanged, AddressOf Handle_SetupScraperChanged_MovieSet
            RemoveHandler _setup_MovieSet.ModuleSettingsChanged, AddressOf Handle_ModuleSettingsChanged_MovieSet
            RemoveHandler _setup_MovieSet.SetupNeedsRestart, AddressOf Handle_SetupNeedsRestart_MovieSet
            _setup_MovieSet.Dispose()
        End If
    End Sub

    Public Sub ScraperOrderChanged_Movie() Implements EmberAPI.Interfaces.ScraperModule_Image_Movie.ScraperOrderChanged
        _setup_Movie.orderChanged()
    End Sub

    Public Sub ScraperOrderChanged_MovieSet() Implements EmberAPI.Interfaces.ScraperModule_Image_MovieSet.ScraperOrderChanged
        _setup_MovieSet.orderChanged()
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Structure sMySettings_Movie

#Region "Fields"
        Dim TMDBAPIKey As String
        Dim TMDBLanguage As String
        Dim TMDBLanguagePrefOnly As Boolean
        Dim FallBackEng As Boolean
#End Region 'Fields

    End Structure

    Structure sMySettings_MovieSet

#Region "Fields"
        Dim TMDBAPIKey As String
        Dim TMDBLanguage As String
        Dim TMDBLanguagePrefOnly As Boolean
        Dim FallBackEng As Boolean
#End Region 'Fields

    End Structure

#End Region 'Nested Types

End Class