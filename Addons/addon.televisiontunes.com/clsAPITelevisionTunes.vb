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
' ################################################################################

Imports EmberAPI
Imports NLog
Imports System.Text.RegularExpressions

Public Class Scraper

#Region "Fields"

    Shared logger As Logger = LogManager.GetCurrentClassLogger()

#End Region 'Fields

#Region "Methods"

    Public Function Scrape_Movie_TVShow(ByVal strOriginalTitle As String) As Interfaces.AddonResult
        Dim nAddonResult As New Interfaces.AddonResult
        Dim strBaseURL As String = "http://www.televisiontunes.com/search.php?q="
        Dim strDownloadURL As String = "http://www.televisiontunes.com/song/download/"
        Dim strSearchTitle As String
        Dim strSearchURL As String

        If Not String.IsNullOrEmpty(strOriginalTitle) Then
            strSearchTitle = HttpUtility.UrlEncode(strOriginalTitle)
            strSearchURL = String.Concat(strBaseURL, strSearchTitle)
        Else
            strSearchURL = String.Empty
        End If

        Try
            If Not String.IsNullOrEmpty(strSearchURL) Then
                Dim tTitle As String = String.Empty
                Dim tID As String = String.Empty
                Dim tWebURL As String = String.Empty
                Dim tURL As String = String.Empty
                Dim tLength As String = String.Empty
                Dim tBitrate As String = String.Empty

                Dim sPattern As String = "<div class=""jp-title"">.*?<ul>.*?<li><a href=""\/(?<URL>.*?)"">(?<TITLE>.*?)<\/a>"
                Dim nPattern As String = "<\/a><br><a href=""(?<NEXTURL>.*?)""><b>Next<\/b><\/a>"

                Dim sHTTP As New HTTP
                Dim Html As String = sHTTP.DownloadData(strSearchURL)
                sHTTP = Nothing

                While Not String.IsNullOrEmpty(Html)
                    Dim sResult As MatchCollection = Regex.Matches(Html, sPattern, RegexOptions.Singleline)

                    For ctr As Integer = 0 To sResult.Count - 1
                        tWebURL = String.Concat("http://www.televisiontunes.com/", HttpUtility.HtmlDecode(sResult.Item(ctr).Groups("URL").Value).Trim)
                        tTitle = sResult.Item(ctr).Groups("TITLE").Value.Trim
                        tURL = GetDownloadURL(tWebURL)

                        If Not String.IsNullOrEmpty(tURL) Then
                            nAddonResult.ScraperResult_Themes.Add(New MediaContainers.Theme With {.URLAudioStream = tURL, .Description = tTitle, .Duration = tLength, .Bitrate = tBitrate, .URLWebsite = tWebURL, .Scraper = "TelevisionTunes"})
                        End If
                    Next

                    'check if there is a "next" page
                    If Regex.IsMatch(Html, nPattern) Then
                        sHTTP = New HTTP
                        Html = sHTTP.DownloadData(String.Concat("http://www.televisiontunes.com/", Regex.Match(Html, nPattern).Groups(1).Value))
                        sHTTP = Nothing
                    Else
                        Html = String.Empty
                    End If
                End While
            End If

        Catch ex As Exception
            logger.Error(ex, New StackFrame().GetMethod().Name)
        End Try

        Return nAddonResult
    End Function

    Private Function GetDownloadURL(ByVal strURL As String) As String
        If Not String.IsNullOrEmpty(strURL) Then
            Dim sHTTP As New HTTP
            Dim Html As String = sHTTP.DownloadData(strURL)
            sHTTP = Nothing

            If Not String.IsNullOrEmpty(Html) Then
                Dim sResult As MatchCollection = Regex.Matches(Html, "<input id=""song_name"" type=""hidden"" value=""(?<URL>.*?)""", RegexOptions.Singleline)
                If sResult.Count > 0 Then
                    Return sResult.Item(0).Groups("URL").Value.Trim
                End If
            End If
        End If
        Return String.Empty
    End Function

#End Region 'Methods

End Class