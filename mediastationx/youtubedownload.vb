Imports System.Net
Imports System.Text.RegularExpressions
Imports mediastationx.Form1
Imports Newtonsoft.Json.Linq
Imports System.Collections.Specialized
Imports System.Web

Public Class youtubedownload
#Region "youtubedownload videos e url"
    Private Shared arrayurl(0) As String
    Private Shared arrayformato(0) As String
    Public Shared qualidadevideo As Integer
    Shared Function geturl(ByVal url As String, ByVal qualidade As Integer) As String
        Try
            Dim wc As New WebClient
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html")
            Dim i = 0
            Dim sourcecode As String = ""
            Dim formatos As String = ""
            sourcecode = wc.DownloadString(New Uri(url))
            wc.Dispose()
            Dim startconfig As String = "yt.playerConfig = "
            Dim playerconfigindex As Integer = sourcecode.IndexOf(startconfig)
            If playerconfigindex > -1 Then
                Dim signature As String
                signature = sourcecode.Substring(playerconfigindex)
                sourcecode = Nothing
                Dim endofjson As Integer = signature.IndexOf(");")
                signature = signature.Substring(startconfig.Length, endofjson - 17).Trim()
                Dim playerconfig As JObject = JObject.Parse(signature)
                Dim playerargs As JObject = JObject.Parse(playerconfig("args").ToString())
                formatos = CStr(playerargs("url_encoded_fmt_stream_map"))
                Dim argument As String = "url="
                Dim endofquerystring As String = "&quality"
                If formatos <> "" Then
                    Dim urlList As New List(Of String)(Regex.Split(formatos, argument))
                    For Each u In urlList
                        If u <> "" Then
                            ReDim Preserve arrayformato(arrayformato.Length)
                            ReDim Preserve arrayurl(arrayurl.Length)
                            Dim uri As New Uri(uri.UnescapeDataString(u.Substring(0, u.IndexOf(endofquerystring))))
                            Dim querys As NameValueCollection = HttpUtility.ParseQueryString(uri.Query)
                            Dim codigo As Byte
                            codigo = Byte.Parse(querys("itag"))
                            arrayformato(i) = codigo.ToString
                            arrayurl(i) = uri.ToString
                            i += 1
                        End If
                    Next
                    i = 0
                End If
            End If
        Catch ex As Exception
            Return "Error"
            Exit Function
        End Try
        Dim bestformat As Integer = 5
        For i = 0 To arrayformato.Length - 1
            Dim qualidadeatestar As Integer = 360
            Dim qualidadejatestada As Integer = Nothing
            Select Case arrayformato(i)
                Case 18, 34, 43 'qualidade 360
                    qualidadejatestada = 360
                    If qualidadeatestar < qualidadejatestada Then
                        qualidadeatestar = qualidadejatestada
                        bestformat = arrayformato(i)
                    End If
                Case 35, 44   'qualidade 480p
                    qualidadejatestada = 480
                    If qualidadeatestar < qualidadejatestada Then
                        qualidadeatestar = qualidadejatestada
                        bestformat = arrayformato(i)
                    End If
                Case 22, 45 'qualidade 720p
                    qualidadejatestada = 720
                    If qualidadeatestar < qualidadejatestada Then
                        qualidadeatestar = qualidadejatestada
                        bestformat = arrayformato(i)
                    End If
                Case 37, 46 'qualidade 1080p
                    qualidadejatestada = 1080
                    If qualidadeatestar < qualidadejatestada Then
                        qualidadeatestar = qualidadejatestada
                        bestformat = arrayformato(i)
                    End If
            End Select
        Next
        For j = 0 To arrayformato.Length - 1
            If arrayformato(j) <> "" Then
                If arrayformato(j) = 18 And qualidade = 18 Or qualidade = 34 Then 'qualidade 360p 
                    Return arrayurl(j) + "&title=video.mp4"
                ElseIf arrayformato(j) = 34 And qualidade = 18 Or qualidade = 34 Or qualidade = 43 Then
                    Return arrayurl(j) + "&title=video.mp4"
                ElseIf arrayformato(j) = 43 And qualidade = 18 Or qualidade = 34 Or qualidade = 43 Then
                    Return arrayurl(j) + "&title=video.webm"
                ElseIf arrayformato(j) = 35 And qualidade = 35 Or qualidade = 44 Then 'qualidade 480p
                    Return arrayurl(j) + "&title=video.mp4"
                ElseIf arrayformato(j) = 44 And qualidade = 35 Or qualidade = 44 Then
                    Return arrayurl(j) + "&title=video.webm"
                ElseIf arrayformato(j) = 22 And qualidade = 22 Or qualidade = 45 Then 'qualidade 720p
                    Return arrayurl(j) + "&title=video.mp4"
                ElseIf arrayformato(j) = 45 And qualidade = 22 Or qualidade = 45 Then 'qualidade 720p
                    Return arrayurl(j) + "&title=video.webm"
                ElseIf arrayformato(j) = 37 And qualidade = 37 Or qualidade = 46 Then 'qualidade 1080p
                    Return arrayurl(j) + "&title=video.mp4"
                ElseIf arrayformato(j) = 46 And qualidade = 37 Or qualidade = 46 Then 'qualidade 1080p
                    Return arrayurl(j) + "&title=video.webm"
                End If
            End If
        Next
        For jformat = 0 To arrayformato.Length - 1
            If arrayformato(jformat) <> "" Then
                If arrayformato(jformat) = bestformat Then
                    Return arrayurl(jformat) & "&title=video.webm"
                End If
            End If
        Next
        Return "Error"
    End Function

    Shared Function videonome(ByVal url As String) As String
        Dim sourcecode As String = ""
        Dim titulo As String = ""
        Dim wc As New WebClient
        wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html")
        Try
            sourcecode = wc.DownloadString(New Uri(url))
            wc.Dispose()
            Dim videotitle As String = "\<meta name=""title"" content=""(?<title>.*)""\>"
            Dim videoregex As Regex = New Regex(videotitle, RegexOptions.IgnoreCase And RegexOptions.Compiled)
            Dim videomatch As Match = videoregex.Match(sourcecode)

            If videomatch.Success Then
                titulo = videomatch.Groups("title").Value
                titulo = titulo.Replace("\\", "-").Replace("/", "-").Trim()
                titulo = titulo.Replace("&quot;", " ")
                Return titulo
            End If
        Catch ex As Exception
            Return "Error"
            Exit Function
        End Try
        url = ""
        sourcecode = ""
        Return "Error"
        titulo = ""
    End Function
    'verifica o url do video
    Shared Function verificarlinkyoutube(ByVal link As String)
        If link.StartsWith("https://") Then
            link = "http://" & link.Substring(8)
        ElseIf Not link.StartsWith("http://") Then
            link = "http://" & link
        End If
        link = link.Replace("youtu.be/", "www.youtube.com/watch?v=")
        If link.StartsWith("http://youtube.com/v/") Then
            link = link.Replace("youtube.com/v/", "youtube.com/watch?v=")
        ElseIf link.StartsWith("http://youtube.com/watch#") Then
            link = link.Replace("youtube.com/watch#", "youtube.com/watch?")
        End If
        If Not link.StartsWith("http://www.youtube.com/watch?v") Then
            link = "http://www.youtube.com/watch?v" + link.Substring(link.IndexOf("v=") + 1)
        End If
        If link.IndexOf("&") <> 0 Then
            Dim parte(1) As String
            parte = link.Split("&")
            link = parte(0)
            Return link
        End If
        If link <> "http://www.youtube" Then
            Return "Error"
            Exit Function
        End If
        Return link
    End Function
#End Region
End Class
