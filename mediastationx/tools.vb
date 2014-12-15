Imports System.Net
Imports MediastationX.youtubedownload
Imports FFLib
Public Class tools
    Private WithEvents webc As New WebClient
    Private WithEvents encoders As New FFLib.Encoder
    Dim itemurl As String = ""
    Dim itemnome As String = ""
    Dim itemcombobox As Integer = Nothing
    Dim nometotalficheiro As String
    Dim cancelado As Boolean = False
    Private qualidade As Integer
#Region "delegades sub"
    Private Delegate Sub inseririnfo(ByVal item As Integer, ByVal subitem As Integer, ByVal recolha As String) ' serve para escrever a informação para a listview noutro thread
    Private Delegate Sub getinfourl(ByVal item As Integer) ' para ler link
    Private Delegate Sub getinfonome(ByVal item As Integer, ByVal subitem As Integer)
    Private Delegate Sub getinfocombobox(ByVal item As Integer)
   
    Private Sub recolha(ByVal item As Integer, ByVal subitem As Integer, ByVal recolha As String)
        ListView1.Items.Item(item).SubItems(subitem).Text = recolha
    End Sub

    Private Sub getnomeitem(ByVal item As Integer, ByVal subitem As Integer)
        itemnome = ListView1.Items.Item(item).SubItems(subitem).Text
    End Sub
    Private Sub geturlitem(ByVal item As Integer)
        itemurl = ListView1.Items.Item(item).Text

    End Sub
    Private Sub getitemcombobox(ByVal itemcombo As Integer)
        itemcombobox = ComboBox1.SelectedIndex
    End Sub
#End Region
#Region "download"
    'adicionar url
    Private Sub ToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem2.Click
        Dim input = InputBox(insertlink)
        If input <> "" Then
            input = verificarlinkyoutube(input)
            If input = "Error" Then
                MsgBox(erro)
                Exit Sub
            End If
            Dim item As New ListViewItem(input)
            item.SubItems.Add("")
            item.SubItems.Add("")
            item.SubItems.Add("")
            item.SubItems.Add("")
            ListView1.Items.AddRange(New ListViewItem() {item})
        Else
            Exit Sub
        End If
    End Sub
    'adiciona pasta do video para converter
    Private Sub ToolStripMenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem3.Click
        Dim stFilePathAndName As String = ""
        Dim openFileDialog1 As New OpenFileDialog
        openFileDialog1.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.Desktop
        openFileDialog1.Title = ""
        openFileDialog1.Filter = "Music and video files|*.mp3;*.mp4;*.wmv;*.mkv;*.avi;*.flv;*.webm"
        openFileDialog1.AddExtension = True
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = True
        openFileDialog1.FileName = ""
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
            stFilePathAndName = openFileDialog1.FileName
        Else
            Exit Sub
        End If
        Dim item As New ListViewItem(stFilePathAndName)
        item.SubItems.Add("")
        item.SubItems.Add("")
        item.SubItems.Add("")
        item.SubItems.Add("")
        ListView1.Items.AddRange(New ListViewItem() {item})
    End Sub
    Private Sub addurl_Click(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles addurl.Click
        menuadd.Show(addurl, New Point(e.X, e.Y))
    End Sub
    Dim numerodeitemsi As Integer = 0
    Private Sub downloadfile()
        If numerodeitemsi <= ListView1.Items.Count - 1 Then
            Me.Invoke(New getinfourl(AddressOf geturlitem), New Object() {numerodeitemsi})
            If Not itemurl.StartsWith("http://") Then
                nometotalficheiro = itemurl
                conversorsel()
                Exit Sub
            End If
        End If
        If numerodeitemsi <= ListView1.Items.Count - 1 Then
            Me.Invoke(New getinfourl(AddressOf geturlitem), New Object() {numerodeitemsi})
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, recolhernome})
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 1, videonome(itemurl)})
                Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, recolherlink})
                Dim linkhttp As String = geturl(itemurl, qualidade)
                If linkhttp = "Error" Then
                MsgBox(erro)
                Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
                Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
                Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 3, ""})
                Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, ""})
                    Exit Sub
                End If
                Me.Invoke(New getinfonome(AddressOf getnomeitem), New Object() {numerodeitemsi, 1})
                If cancelado = True Then
                    Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
                    Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
                    Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 3, ""})
                    Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, ""})
                    webc.Dispose()
                    linkhttp = ""
                    numerodeitemsi = 0
                    cancelado = False
                    Exit Sub
                End If
                nometotalficheiro = itemnome & "." & linkhttp.Substring(linkhttp.LastIndexOf(".") + 1, (linkhttp.Length - linkhttp.LastIndexOf(".") - 1))
                nometotalficheiro = outfoldecaminho.Text & "\" & nometotalficheiro
                webc.DownloadFileAsync(New Uri(linkhttp), nometotalficheiro)
                Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, recolhacompleta})
                AddHandler webc.DownloadProgressChanged, AddressOf webc_DownloadProgressChanged
                AddHandler webc.DownloadFileCompleted, AddressOf webc_DownloadFileCompleted
                Exit Sub
                itemnome = Nothing
                itemurl = Nothing
                linkhttp = Nothing
            End If
            If numerodeitemsi > ListView1.Items.Count - 1 Then
                RemoveHandler webc.DownloadProgressChanged, AddressOf webc_DownloadProgressChanged
                RemoveHandler webc.DownloadFileCompleted, AddressOf webc_DownloadFileCompleted
                If BackgroundWorker1.IsBusy Then
                    BackgroundWorker1.CancelAsync()
                End If
                numerodeitemsi = 0
                Exit Sub
            End If
    End Sub

    Private Sub webc_DownloadProgressChanged(sender As Object, e As System.Net.DownloadProgressChangedEventArgs)
        Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 3, "" & Format(e.TotalBytesToReceive / 1024 / 1024, "#0.00") & "Mb"})
        Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, e.ProgressPercentage.ToString + "%"})

    End Sub
    Private Sub webc_DownloadFileCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs)
        If Not e.Cancelled Then
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, concluido})
            If numerodeitemsi <= ListView1.Items.Count - 1 Then
                conversorsel()
                RemoveHandler webc.DownloadProgressChanged, AddressOf webc_DownloadProgressChanged
                RemoveHandler webc.DownloadFileCompleted, AddressOf webc_DownloadFileCompleted
            Else : Exit Sub
            End If
        Else
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
        End If
    End Sub
#End Region
#Region "CONVERSOR"
    Private Sub conversorsel()
        Me.Invoke(New getinfocombobox(AddressOf getitemcombobox), New Object() {numerodeitemsi})
        If itemcombobox = 0 Or itemcombobox = -1 Then
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, concluido})
            numerodeitemsi = numerodeitemsi + 1
            downloadfile()
        Else
            conversor()
            numerodeitemsi = numerodeitemsi + 1
            downloadfile()
        End If
    End Sub


    Private Sub conversor()

        Select Case itemcombobox
            Case 1 'mp3 320
                encoders.AudioBitrate = "320k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_MP3
                encoders.Video_Codec = encoders.Vcodec_NONE

            Case 2 'mp3 256
                encoders.AudioBitrate = "256k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_MP3
                encoders.Video_Codec = encoders.Vcodec_NONE


            Case 3 'mp3 224
                encoders.AudioBitrate = "224k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_MP3
                encoders.Video_Codec = encoders.Vcodec_NONE


            Case 4 'mp3 192
                encoders.AudioBitrate = "192k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_MP3
                encoders.Video_Codec = encoders.Vcodec_NONE


            Case 5 'mp3 128
                encoders.AudioBitrate = "128k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_MP3
                encoders.Video_Codec = encoders.Vcodec_NONE


            Case 6 'mp3 64
                encoders.AudioBitrate = "64k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_MP3
                encoders.Video_Codec = encoders.Vcodec_NONE



            Case 7 'aac
                encoders.AudioBitrate = "192k"
                encoders.AudioCodec = encoders.AudioCodec_AAC
                encoders.Format = encoders.Format_AAC
                encoders.Video_Codec = encoders.Vcodec_NONE


            Case 8 'avi
                encoders.AudioBitrate = "320k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_AVI
                encoders.Video_Codec = encoders.Vcodec_Xvid
                encoders.VideoBitrate = "512k"


            Case 9 'flv
                encoders.AudioBitrate = "320k"
                encoders.AudioCodec = encoders.AudioCodec_mp3
                encoders.Format = encoders.Format_FLV
                encoders.Video_Codec = encoders.Vcodec_flv
                encoders.VideoBitrate = "512k"


            Case 10 'mkv
                encoders.AudioBitrate = "320k"
                encoders.AudioCodec = encoders.AudioCodec_AAC
                encoders.Format = encoders.Format_MKV
                encoders.Video_Codec = encoders.Vcodec_h264
                encoders.VideoBitrate = "512k"
                encoders.Libx264_Preset_pass1 = encoders.libx264_normal

            Case 11 'mov
                encoders.AudioBitrate = "320k"
                encoders.AudioCodec = encoders.AudioCodec_AAC
                encoders.Format = encoders.Format_MOV
                encoders.Video_Codec = encoders.Vcodec_h264
                encoders.VideoBitrate = "512k"
                encoders.Libx264_Preset_pass1 = encoders.libx264_normal
            Case 12 'mp4
                encoders.AudioBitrate = "320k"
                encoders.AudioCodec = encoders.AudioCodec_AAC
                encoders.Format = encoders.Format_MP4
                encoders.Video_Codec = encoders.Vcodec_Xvid
                encoders.VideoBitrate = "512k"
        End Select

        encoders.OverWrite = False
        encoders.Threads = 0
        encoders.RateControl = encoders.RateControl_CRF
        encoders.OutputPath = System.IO.Path.GetTempPath()
        encoders.SourceFile = nometotalficheiro
        encoders.Encode()
        IO.File.Move(System.IO.Path.GetTempPath() & encoders.Filename, outfoldecaminho.Text & "\" & "[MX] " + encoders.Filename)
    End Sub
    Private Sub ConOut(ByVal prog As String, ByVal tl As String) Handles encoders.Progress
        Try
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, prog & "%"})
            Application.DoEvents()
        Catch ex As Exception
        End Try
    End Sub
    Private Sub stat(ByVal status) Handles encoders.Status
        Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, status})
        Application.DoEvents()
        Me.Invoke(New getinfonome(AddressOf getnomeitem), New Object() {numerodeitemsi, 2})
        If itemnome = "Encoding Finished" Then
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, concluido})
        End If
    End Sub

#End Region
#Region "iniciar background e cancelar webclient e backgoround"
    Private Sub downloadbutton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles downloadbutton.Click
        If outfoldecaminho.Text = "" Then
            MsgBox(msgerrocaminho)
        ElseIf ListView1.Items.Count = 0 Then
            MsgBox(msgsnoitems)
        ElseIf ListView1.Items.Count <> 0 Then
            Dim itemsi As Integer = (Me.ListView1.Items.Count - 1)
            Do While (itemsi >= 0)
                Dim item As ListViewItem = Me.ListView1.Items.Item(itemsi)
                If (item.SubItems.Item(4).Text.Contains(concluido)) Then
                    Me.ListView1.Items.Remove(item)
                End If
                itemsi = (itemsi + -1)
            Loop
            If Not BackgroundWorker1.IsBusy Then
                BackgroundWorker1.RunWorkerAsync()
            End If
        End If
    End Sub
    Private Sub stopdownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles stopdownload.Click
        If BackgroundWorker1.IsBusy = True Then
            Me.BackgroundWorker1.CancelAsync()
            cancelado = True
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 3, ""})
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, ""})
        End If
        If webc.IsBusy = True Then
            webc.CancelAsync()
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 4, cancel})
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 3, ""})
            Me.Invoke(New inseririnfo(AddressOf recolha), New Object() {numerodeitemsi, 2, ""})
        End If
        webc.Dispose()
    End Sub


    Private Sub bgw_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        downloadfile()
    End Sub
#End Region
#Region "outputfolder"
    Private Sub pathfolder_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles pathfolder.Click
        Dim saidafolder As New FolderBrowserDialog
        If saidafolder.ShowDialog = Windows.Forms.DialogResult.OK Then
            outfoldecaminho.Text = saidafolder.SelectedPath
            outfoldecaminho.Enabled = False
        End If
    End Sub
#End Region
    

    Public Sub addlink(ByVal link As String)
        Dim item As New ListViewItem(link)
        item.SubItems.Add("")
        item.SubItems.Add("")
        item.SubItems.Add("")
        item.SubItems.Add("")
        ListView1.Items.AddRange(New ListViewItem() {item})
    End Sub

  


#Region "efeitos nos botoes"
    Private Sub downloadbutton_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles downloadbutton.MouseEnter
        downloadbutton.Image = My.Resources.download2
    End Sub

    Private Sub downloadbutton_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles downloadbutton.MouseLeave
        downloadbutton.Image = My.Resources.download
    End Sub
    Private Sub addurl_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles addurl.MouseEnter
        addurl.Image = My.Resources.add2
    End Sub

    Private Sub addurl_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles addurl.MouseLeave
        addurl.Image = My.Resources.add
    End Sub
    Private Sub pathfolder_MouseEnter(sender As Object, e As System.EventArgs) Handles pathfolder.MouseEnter
        pathfolder.Image = My.Resources.pasta2
    End Sub

    Private Sub pathfolder_MouseLeave(sender As Object, e As System.EventArgs) Handles pathfolder.MouseLeave
        pathfolder.Image = My.Resources.pasta
    End Sub

    Private Sub stopdownload_MouseEnter(sender As Object, e As System.EventArgs) Handles stopdownload.MouseEnter
        stopdownload.Image = My.Resources.parardownload2
    End Sub

    Private Sub stopdownload_MouseLeave(sender As Object, e As System.EventArgs) Handles stopdownload.MouseLeave
        stopdownload.Image = My.Resources.parardownload
    End Sub
#End Region


  
    Private Sub ToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem1.Click
        For Each item As ListViewItem In ListView1.SelectedItems
            ListView1.Items.RemoveAt(item.Index)
        Next
    End Sub

    Private insertlink, recolhernome, recolherlink, cancel, recolhacompleta, concluido, msgerrocaminho, msgsnoitems, erro As String

    Private Sub tools_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If outfoldecaminho.Text <> "" Then
            Try
                Dim xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
                xml...<pathfolder>.Value = outfoldecaminho.Text
                xml.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
            Catch ex As Exception
            End Try
        End If
    End Sub
    Private Sub tools_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Try
            Dim xml = XDocument.Load(Form1.idioma)
            insertlink = xml...<recolherurl>.Value
            recolhernome = xml...<recolhernome>.Value
            recolherlink = xml...<recolherurl>.Value
            cancel = xml...<msgcancel>.Value
            recolhacompleta = xml...<recolhacompleta>.Value
            concluido = xml...<concluido>.Value
            msgerrocaminho = xml...<msgerrocaminho>.Value
            msgsnoitems = xml...<msgsnoitems>.Value
            erro = xml...<msgerro>.Value
            ComboBox1.Text = xml...<formato>.Value
            Label1.Text = xml...<labelsavetool>.Value
            Me.ToolTip1.SetToolTip(Label1, xml...<tooltipsave>.Value)
            Me.ToolTip1.SetToolTip(pathfolder, xml...<tooltipsave>.Value)
            Me.ToolTip1.SetToolTip(downloadbutton, xml...<tooltipiniciar>.Value)
            Me.ToolTip1.SetToolTip(stopdownload, xml...<tooltipparar>.Value)
            Me.ToolTip1.SetToolTip(ComboBox1, xml...<toolformato>.Value)
            Me.Text = xml...<converterdownload>.Value
            If Not IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml") Then
                qualidade = 37
            Else
                xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
                outfoldecaminho.Text = xml...<pathfolder>.Value
                outfoldecaminho.Enabled = False
            End If
        Catch ex As Exception
        End Try
    End Sub
End Class