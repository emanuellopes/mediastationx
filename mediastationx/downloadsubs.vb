Imports GetHash.Main
Imports mediastationx.com.getsubtitle.api
Imports System.IO
Imports Ionic.Zip
Public Class downloadsubs
    Public file As String = ""
    Public hash As String = ""
    Private pasta As String = ""
    Private nome As String = ""
    Private nr As Integer = 0
    Private WithEvents wc As New System.Net.WebClient
    Private Sub downloadsubs_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            If ListView1.Items.Count <> 0 Then
                ListView1.Items.Clear()
            End If
            file = ""
            hash = ""
            nr = -1
            For Each ficheiros As String In My.Computer.FileSystem.GetFiles(Path.GetTempPath, FileIO.SearchOption.SearchTopLevelOnly, "*.zip", "*.srt", "*.nfo")
                IO.File.Delete(ficheiros)
            Next
            ProgressBar1.Value = 0
            Me.Dispose()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub downloadsubs_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
        If file <> "" Then
            pasta = file.Substring(0, file.LastIndexOf("\"))
            nome = file.Substring(file.LastIndexOf("\"))
            nome = nome.Substring(0, nome.LastIndexOf("."))
            If Not opensubtitlesbgw.IsBusy Then
                opensubtitlesbgw.RunWorkerAsync(xml...<langopensubtitles>.Value)
            End If
            subtitlecombr(xml...<langsubtitlescombr>.Value)
        End If
    End Sub
#Region "Open subtitles"
    'pesuisa
    Private Sub opensubtitlesbgw_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles opensubtitlesbgw.DoWork
        Me.Invoke(New opensubdelegate(AddressOf opensubtitles), New Object() {e.Argument})
    End Sub
    Private Delegate Sub opensubdelegate(ByVal lang As String)
    Private Sub opensubtitles(ByVal lang As String)
        Try
            My.Computer.Network.Ping("www.opensubtitles.org", 500) ' verifica se o computador esta ligado a internet
        Catch ex As Exception
            make_download()
            Exit Sub
        End Try
        Try
            Dim rssdoc As XDocument = XDocument.Load("http://www.opensubtitles.org/en/search/sublanguageid-" + lang + "/moviehash-" + hash + "/simplexml")
            Dim xroot = XElement.Parse(rssdoc.ToString())
            Dim nores = rssdoc.Descendants("subtitle").Value
            Dim rssresult = From rssdoelements In rssdoc.Descendants("subtitle") _
                            Select New With { _
                                .relesename = rssdoelements.Descendants("releasename").Value, _
                                .language = rssdoelements.Descendants("language").Value, _
                                .download = rssdoelements.Descendants("download").Value}
            For Each co In rssresult
                Dim item As New ListViewItem("")
                item.Checked = False
                item.SubItems.Add(co.relesename)
                item.SubItems.Add("Open Subtitles")
                item.SubItems.Add(co.language)
                item.SubItems.Add(co.download)
                item.SubItems.Add("")
                ListView1.Items.AddRange(New ListViewItem() {item})
            Next
        Catch ex As Exception
        End Try
    End Sub
    'download
    Private Sub wc_DownloadProgressChanged(sender As Object, e As System.Net.DownloadProgressChangedEventArgs) Handles wc.DownloadProgressChanged
        ProgressBar1.Value = e.ProgressPercentage / 2
    End Sub
    Private Sub wc_DownloadFileCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs) Handles wc.DownloadFileCompleted
        Using zip1 As ZipFile = ZipFile.Read(Path.GetTempPath() & "\" & "subs(" & nr & ").zip")
            ProgressBar1.Value = 60
            Dim z As ZipEntry
            Dim ficheirosrt As String = ""
            For Each z In zip1
                z.Extract(Path.GetTempPath(), ExtractExistingFileAction.OverwriteSilently)
                If z.FileName.Contains(".srt") Then
                    ficheirosrt = z.FileName
                    ProgressBar1.Value = 80
                If Not Directory.Exists(pasta & "\sub") Then
                    Directory.CreateDirectory(pasta & "\sub")
                End If
                ProgressBar1.Value = 90
                Try
                    If IO.File.Exists(pasta & "\sub" & nome & "(" & nr & ").srt") Then
                        IO.File.Delete(pasta & "\sub" & nome & "(" & nr & ").srt")
                    End If
                    If ficheirosrt <> "" Then
                        IO.File.Move(Path.GetTempPath() & "\" & ficheirosrt, pasta & "\sub" & nome & "(" & nr & ").srt")
                    End If
                Catch ex As Exception
                End Try
                ProgressBar1.Value = 100
                    ListView1.Items.Item(nr).SubItems(5).Text = "Finish"
                    Form1.arraylegendas.Add(pasta & "\sub" & nome & "(" & nr & ").srt")
                    Form1.additemmenu(nome.Replace("\", "") & "(" & nr & ")")
                    ficheirosrt = ""
                Else
                    ficheirosrt = ""
                End If
            Next
            nr += 1
            make_download()
        End Using
    End Sub
#End Region
#Region "subtitle.com.br"
    'pesquisa
    Private Sub subtitlecombr(ByVal lang As String)
        Try
            My.Computer.Network.Ping("www.subtitles.com.br", 500) ' verifica se o computador esta ligado a internet
        Catch ex As Exception
            make_download()
            Exit Sub
        End Try
        Dim search As New com.getsubtitle.api.searchSubtitles_wsdl
        Try
            search.searchSubtitlesByHashAsync(hash, lang, 0, 500)
            AddHandler search.searchSubtitlesByHashCompleted, AddressOf searchbyhash
        Catch ex As Exception
        End Try
    End Sub
    Private Sub searchbyhash(ByVal sender As Object, ByVal e As searchSubtitlesByHashCompletedEventArgs)
        Dim i As Integer = 0
        Try
            For Each resultado In e.Result
                Dim item As New ListViewItem("")
                item.Checked = False
                item.SubItems.Add(resultado.file_name)
                item.SubItems.Add("Subtitle.com.br")
                item.SubItems.Add(resultado.language)
                item.SubItems.Add(resultado.cod_subtitle_file)
                item.SubItems.Add("")
                ListView1.Items.AddRange(New ListViewItem() {item})
            Next
        Catch ex As Exception
        End Try
    End Sub
    'download
    Private Sub subtitlecombrdownload(ByVal code As String)
        Dim a As New com.getsubtitle.api.searchSubtitles_wsdl
        Dim Subtitles(1) As SubtitleDownload
        Subtitles(0) = New SubtitleDownload
        Subtitles(0).cod_subtitle_file = code
        a.downloadSubtitlesAsync(Subtitles, 0)
        ProgressBar1.Value = 20
        AddHandler a.downloadSubtitlesCompleted, AddressOf down
    End Sub
    Private Sub down(ByVal sender As Object, ByVal e As downloadSubtitlesCompletedEventArgs)
        For Each s In e.Result
            ProgressBar1.Value = 50
            Dim decodedtext As Byte() = Convert.FromBase64String(s.data)
            ProgressBar1.Value = 70
            Dim fs As FileStream = New FileStream(pasta & "\sub" & nome & "(" & nr & ").srt", FileMode.Create)
            ProgressBar1.Value = 90
            fs.Write(decodedtext, 0, decodedtext.Length)
            fs.Close()
            ProgressBar1.Value = 100
            ListView1.Items.Item(nr).SubItems(5).Text = "Finish"
            Form1.arraylegendas.Add(pasta & "\sub" & nome & "(" & nr & ").srt")
            Form1.additemmenu(nome.Replace("\", "") & "(" & nr & ")")
        Next
        nr += 1
        make_download()
    End Sub
#End Region
    Private Sub make_download()
        If nr <= ListView1.Items.Count - 1 Then
            If ListView1.Items.Item(nr).Checked = True Then
                If ListView1.Items.Item(nr).SubItems(2).Text.StartsWith("Subtitle.com.br") Then
                    If Not Directory.Exists(pasta & "\sub") Then
                        Directory.CreateDirectory(pasta & "\sub")
                    End If
                    subtitlecombrdownload(ListView1.Items.Item(nr).SubItems(4).Text)
                End If
                If ListView1.Items.Item(nr).SubItems(2).Text.StartsWith("Open Subtitles") Then
                    wc.DownloadFileAsync(New Uri(ListView1.Items.Item(nr).SubItems(4).Text), Path.GetTempPath() & "\" & "subs(" & nr & ").zip")
                End If
            Else
                nr += 1
                make_download()
            End If
        End If
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles download.Click
        Try
            My.Computer.Network.Ping("www.google.com", 500) ' verifica se o computador esta ligado a internet
        Catch ex As Exception
            MsgBox(Form1.semnet)
            Exit Sub
        End Try
        If wc.IsBusy <> True And opensubtitlesbgw.IsBusy <> True Then
            nr = 0
            make_download()
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As System.Object, e As System.EventArgs) Handles PictureBox1.Click
        Settings.Show()
        Settings.TabControl1.SelectedTab = Settings.TabPage2
    End Sub
#Region "efeitos botoes"
    Private Sub download_MouseEnter(sender As Object, e As System.EventArgs) Handles download.MouseEnter
        download.Image = My.Resources.download2
    End Sub

    Private Sub download_MouseLeave(sender As Object, e As System.EventArgs) Handles download.MouseLeave
        download.Image = My.Resources.download2
    End Sub

    Private Sub PictureBox1_MouseEnter(sender As Object, e As System.EventArgs) Handles PictureBox1.MouseEnter
        PictureBox1.Image = My.Resources.world2
    End Sub

    Private Sub PictureBox1_MouseLeave(sender As Object, e As System.EventArgs) Handles PictureBox1.MouseLeave
        PictureBox1.Image = My.Resources.world
    End Sub
#End Region
End Class