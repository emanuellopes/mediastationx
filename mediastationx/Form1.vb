Imports System.Web
Imports LibMPlayerCommon
Imports System.Net
Imports Newtonsoft.Json.Linq
Imports System.Text.RegularExpressions
Imports System.Collections.Specialized
Imports System.Xml
Imports System.IO
Imports Microsoft.Win32
Imports GetHash.Main
Imports System.Runtime.InteropServices
Imports Microsoft.WindowsAPICodePack.Taskbar
Imports ErrorReport
Public Class Form1
#Region "declarações para nao adormecer pc"
    <DllImport("user32", EntryPoint:="SystemParametersInfo", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function SystemParametersInfo(ByVal uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As String, ByVal fuWinIni As Integer) As Integer
    End Function
    Private Const SPI_SETSCREENSAVETIMEOUT As Int32 = 15
    Private Sub No_sleeptimer_Tick(sender As System.Object, e As System.EventArgs) Handles No_sleeptimer.Tick
        SystemParametersInfo(SPI_SETSCREENSAVETIMEOUT, 30, 0, 0)
    End Sub
#End Region
#Region "delcarações de fuçoes de fullscreen"
    'fullscreen
    Private Declare Function SetWindowPos Lib "user32.dll" Alias "SetWindowPos" (ByVal hWnd As IntPtr, ByVal hWndIntertAfter As IntPtr, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As Integer) As Boolean
    Private Declare Function GetSystemMetrics Lib "user32.dll" Alias "GetSystemMetrics" (ByVal Which As Integer) As Integer
    Private Const SM_CXSCREEN As Integer = 0
    Private Const SM_CYSCREEN As Integer = 1
    Private Shared HWND_TOP As IntPtr = IntPtr.Zero
    Private Const SWP_SHOWWINDOW As Integer = 64
    Public ReadOnly Property ScreenX() As Integer
        Get
            Return GetSystemMetrics(SM_CXSCREEN)
        End Get
    End Property
    Public ReadOnly Property ScreenY() As Integer
        Get
            Return GetSystemMetrics(SM_CYSCREEN)
        End Get
    End Property
#End Region
#Region "variaveis globais"
    'estado do player
    Public Enum Estadoenum As Integer
        parado = 0
        pause = 1
        play = 2
    End Enum
    Private estado As Integer = Estadoenum.parado
    Private fullc As Boolean = False
    'variaveis dos paineis movimeis lista de videos e lista de musicas
    Private painelyou As Boolean
    Private painellist As Boolean
    'variaveis da zona de pesquisa de videos
    Private caixa As String
    Private index As Integer
    Private linksyou(9) As String
    'listbox
    Private mouseindex As Integer
    Private selecionado As Integer = 0
    'declarações mplayer
    Private player As MPlayer
    'variavel url 
    Private url As String
    Private clicada As Integer = -1 'nr do item selecionado na lista
    'variaveis das legendas
    Private legendasv As Boolean = True 'verifica se as legendas estaovisiveis
    Private legendason As Boolean = False
    ''' '''''''''''''''
    Public poslegendas As Integer = 100
    Public legendatamanho As Double
    Public fonttype As String
    Public codlegendas As String
    '''''''''''''''
    ' Private resolucaodevideo As String
    Public estadomsn As Boolean
    Public downloadlegendas As Boolean
    Public arraylegendas As New ArrayList
    Public idioma As String
    ''variaveis para a actualização
    Dim versao As String
    Dim filedownload As Integer
#End Region
#Region "efeitos lista de reprodução"
    Private Sub ListBox1_DrawItem(sender As Object, e As System.Windows.Forms.DrawItemEventArgs) Handles ListBox1.DrawItem
        e.DrawBackground()
        e.DrawFocusRectangle()
        Dim textb As Brush = Brushes.Orange
        If e.Index = mouseindex Then
            e.Graphics.FillRectangle(Brushes.Gray, e.Bounds)
            textb = Brushes.Black
        End If
        If (e.State And DrawItemState.Selected) = DrawItemState.Selected Then
            e.Graphics.FillRectangle(Brushes.Gray, e.Bounds)
            textb = Brushes.Black
        End If
        If e.Index >= 0 AndAlso e.Index < ListBox1.Items.Count Then
            e.Graphics.DrawString(CType(ListBox1.Items(e.Index), listbox_item_data).nomedovideo, New Font(FontFamily.GenericSansSerif, 8, FontStyle.Regular), textb, e.Bounds.X, e.Bounds.Y)
        End If
    End Sub
    Private Sub ListBox1_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles ListBox1.MouseMove
        Try
            Dim index As Integer = ListBox1.IndexFromPoint(e.Location)
            If index <> mouseindex Then
                mouseindex = index
                ListBox1.Invalidate()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub ListBox1_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.MouseLeave
        If mouseindex > -1 Then
            mouseindex = -1
            ListBox1.Invalidate()
        End If
    End Sub
#End Region

#Region "efeitos nos botões"
    'botão next
    Private Sub button_next_MouseEnter(sender As Object, e As System.EventArgs) Handles button_next.MouseEnter
        button_next.Image = My.Resources.next2
    End Sub
    Private Sub button_next_MouseLeave(sender As Object, e As System.EventArgs) Handles button_next.MouseLeave
        button_next.Image = My.Resources.next1
    End Sub

    'botão play
    Private Sub button_play_MouseEnter(sender As Object, e As System.EventArgs) Handles button_play.MouseEnter
        If estado = Estadoenum.play Then
            button_play.Image = My.Resources.pause2
        ElseIf estado = Estadoenum.pause Or estado = Estadoenum.parado Then
            button_play.Image = My.Resources.play2
        End If
    End Sub
    Private Sub button_play_MouseLeave(sender As Object, e As System.EventArgs) Handles button_play.MouseLeave
        If estado = Estadoenum.play Then
            button_play.Image = My.Resources.pause
        ElseIf estado = Estadoenum.pause Or estado = Estadoenum.parado Then
            button_play.Image = My.Resources.play
        End If
    End Sub

    'botão prev
    Private Sub button_prev_MouseEnter(sender As Object, e As System.EventArgs) Handles button_prev.MouseEnter
        button_prev.Image = My.Resources.previous2
    End Sub
    Private Sub button_prev_MouseLeave(sender As Object, e As System.EventArgs) Handles button_prev.MouseLeave
        button_prev.Image = My.Resources.previous
    End Sub

    'botao stop
    Private Sub button_stop_MouseEnter(sender As Object, e As System.EventArgs) Handles button_stop.MouseEnter
        button_stop.Image = My.Resources.stop2
    End Sub
    Private Sub button_stop_MouseLeave(sender As Object, e As System.EventArgs) Handles button_stop.MouseLeave
        button_stop.Image = My.Resources.stop1
    End Sub

    'botoes adicionar videos a lista
    Private Sub add1_MouseEnter(sender As Object, e As System.EventArgs) Handles add1.MouseEnter
        add1.Image = My.Resources.add2
    End Sub
    Private Sub add1_MouseLeave(sender As Object, e As System.EventArgs) Handles add1.MouseLeave
        add1.Image = My.Resources.add
    End Sub
    Private Sub add2_MouseEnter(sender As Object, e As System.EventArgs) Handles add2.MouseEnter
        add2.Image = My.Resources.add2
    End Sub
    Private Sub add2_MouseLeave(sender As Object, e As System.EventArgs) Handles add2.MouseLeave
        add2.Image = My.Resources.add
    End Sub
    Private Sub add3_MouseEnter(sender As Object, e As System.EventArgs) Handles add3.MouseEnter
        add3.Image = My.Resources.add2
    End Sub
    Private Sub add3_MouseLeave(sender As Object, e As System.EventArgs) Handles add3.MouseLeave
        add3.Image = My.Resources.add
    End Sub
    Private Sub add4_MouseEnter(sender As Object, e As System.EventArgs) Handles add4.MouseEnter
        add4.Image = My.Resources.add2
    End Sub
    Private Sub add4_MouseLeave(sender As Object, e As System.EventArgs) Handles add4.MouseLeave
        add4.Image = My.Resources.add
    End Sub
    Private Sub add5_MouseEnter(sender As Object, e As System.EventArgs) Handles add5.MouseEnter
        add5.Image = My.Resources.add2
    End Sub
    Private Sub add5_MouseLeave(sender As Object, e As System.EventArgs) Handles add5.MouseLeave
        add5.Image = My.Resources.add
    End Sub
    Private Sub add6_MouseEnter(sender As Object, e As System.EventArgs) Handles add6.MouseEnter
        add6.Image = My.Resources.add2
    End Sub
    Private Sub add6_MouseLeave(sender As Object, e As System.EventArgs) Handles add6.MouseLeave
        add6.Image = My.Resources.add
    End Sub
    Private Sub add7_MouseEnter(sender As Object, e As System.EventArgs) Handles add7.MouseEnter
        add7.Image = My.Resources.add2
    End Sub
    Private Sub add7_MouseLeave(sender As Object, e As System.EventArgs) Handles add7.MouseLeave
        add7.Image = My.Resources.add
    End Sub
    Private Sub add8_MouseEnter(sender As Object, e As System.EventArgs) Handles add8.MouseEnter
        add8.Image = My.Resources.add2
    End Sub
    Private Sub add8_MouseLeave(sender As Object, e As System.EventArgs) Handles add8.MouseLeave
        add8.Image = My.Resources.add
    End Sub
    Private Sub add9_MouseEnter(sender As Object, e As System.EventArgs) Handles add9.MouseEnter
        add9.Image = My.Resources.add2
    End Sub
    Private Sub add9_MouseLeave(sender As Object, e As System.EventArgs) Handles add9.MouseLeave
        add9.Image = My.Resources.add
    End Sub
    Private Sub add10_MouseEnter(sender As Object, e As System.EventArgs) Handles add10.MouseEnter
        add10.Image = My.Resources.add2
    End Sub
    Private Sub add10_MouseLeave(sender As Object, e As System.EventArgs) Handles add10.MouseLeave
        add10.Image = My.Resources.add
    End Sub

    'botoes download lista de videos
    Private Sub download1_MouseEnter(sender As Object, e As System.EventArgs) Handles download1.MouseEnter
        download1.Image = My.Resources.download2
    End Sub
    Private Sub download1_MouseLeave(sender As Object, e As System.EventArgs) Handles download1.MouseLeave
        download1.Image = My.Resources.download
    End Sub
    Private Sub download2_MouseEnter(sender As Object, e As System.EventArgs) Handles download2.MouseEnter
        download2.Image = My.Resources.download2
    End Sub
    Private Sub download2_MouseLeave(sender As Object, e As System.EventArgs) Handles download2.MouseLeave
        download2.Image = My.Resources.download
    End Sub
    Private Sub download3_MouseEnter(sender As Object, e As System.EventArgs) Handles download3.MouseEnter
        download3.Image = My.Resources.download2
    End Sub
    Private Sub download3_MouseLeave(sender As Object, e As System.EventArgs) Handles download3.MouseLeave
        download3.Image = My.Resources.download
    End Sub
    Private Sub download4_MouseEnter(sender As Object, e As System.EventArgs) Handles download4.MouseEnter
        download4.Image = My.Resources.download2
    End Sub
    Private Sub download4_MouseLeave(sender As Object, e As System.EventArgs) Handles download4.MouseLeave
        download4.Image = My.Resources.download
    End Sub
    Private Sub download5_MouseEnter(sender As Object, e As System.EventArgs) Handles download5.MouseEnter
        download5.Image = My.Resources.download2
    End Sub
    Private Sub download5_MouseLeave(sender As Object, e As System.EventArgs) Handles download5.MouseLeave
        download5.Image = My.Resources.download
    End Sub
    Private Sub download6_MouseEnter(sender As Object, e As System.EventArgs) Handles download6.MouseEnter
        download6.Image = My.Resources.download2
    End Sub
    Private Sub download6_MouseLeave(sender As Object, e As System.EventArgs) Handles download6.MouseLeave
        download6.Image = My.Resources.download
    End Sub
    Private Sub download7_MouseEnter(sender As Object, e As System.EventArgs) Handles download7.MouseEnter
        download7.Image = My.Resources.download2
    End Sub
    Private Sub download7_MouseLeave(sender As Object, e As System.EventArgs) Handles download7.MouseLeave
        download7.Image = My.Resources.download
    End Sub
    Private Sub download8_MouseEnter(sender As Object, e As System.EventArgs) Handles download8.MouseEnter
        download8.Image = My.Resources.download2
    End Sub
    Private Sub download8_MouseLeave(sender As Object, e As System.EventArgs) Handles download8.MouseLeave
        download8.Image = My.Resources.download
    End Sub
    Private Sub download9_MouseEnter(sender As Object, e As System.EventArgs) Handles download9.MouseEnter
        download9.Image = My.Resources.download2
    End Sub
    Private Sub download9_MouseLeave(sender As Object, e As System.EventArgs) Handles download9.MouseLeave
        download9.Image = My.Resources.download
    End Sub
    Private Sub download10_MouseEnter(sender As Object, e As System.EventArgs) Handles download10.MouseEnter
        download10.Image = My.Resources.download2
    End Sub
    Private Sub download10_MouseLeave(sender As Object, e As System.EventArgs) Handles download10.MouseLeave
        download10.Image = My.Resources.download
    End Sub

    ' botões proximo, anterior da lista de videos
    Private Sub pesquisardireita_MouseEnter(sender As Object, e As System.EventArgs) Handles pesquisardireita.MouseEnter
        If index = 50 Then
            pesquisardireita.Image = My.Resources.direitadisabled
        Else
            pesquisardireita.Image = My.Resources.direita2
        End If
    End Sub
    Private Sub pesquisardireita_MouseLeave(sender As Object, e As System.EventArgs) Handles pesquisardireita.MouseLeave
        If index = 50 Then
            pesquisardireita.Image = My.Resources.direitadisabled
        Else
            pesquisardireita.Image = My.Resources.direita
        End If
    End Sub
    Private Sub pesquisaresquerda_MouseEnter(sender As Object, e As System.EventArgs) Handles pesquisaresquerda.MouseEnter
        If index <= 10 Then
            pesquisaresquerda.Image = My.Resources.esquerdadisabled
        Else
            pesquisaresquerda.Image = My.Resources.esquerda2
        End If
    End Sub
    Private Sub pesquisaresquerda_MouseLeave(sender As Object, e As System.EventArgs) Handles pesquisaresquerda.MouseLeave
        If index <= 10 Then
            pesquisaresquerda.Image = My.Resources.esquerdadisabled
        Else
            pesquisaresquerda.Image = My.Resources.esquerda
        End If
    End Sub

    'botao fullscreen
    Private Sub botaofullsc_MouseEnter(sender As Object, e As System.EventArgs) Handles botaofullsc.MouseEnter
        If fullc = True Then
            botaofullsc.Image = My.Resources.fullscreenon2
        Else
            botaofullsc.Image = My.Resources.fullscreen2
        End If
    End Sub
    Private Sub botaofullsc_MouseLeave(sender As Object, e As System.EventArgs) Handles botaofullsc.MouseLeave
        If fullc = True Then
            botaofullsc.Image = My.Resources.fullscreenon
        Else
            botaofullsc.Image = My.Resources.fullscreen
        End If
    End Sub

    'botaopesquisar videos
    Private Sub pesquisarbutton_MouseEnter(sender As Object, e As System.EventArgs) Handles pesquisarbutton.MouseEnter
        pesquisarbutton.Image = My.Resources.procurar2
    End Sub
    Private Sub pesquisarbutton_MouseLeave(sender As Object, e As System.EventArgs) Handles pesquisarbutton.MouseLeave
        pesquisarbutton.Image = My.Resources.procurar
    End Sub

    'botao opçoes
    Private Sub botaoopt_MouseEnter(sender As Object, e As System.EventArgs) Handles botaoopt.MouseEnter
        botaoopt.Image = My.Resources.opt2
    End Sub
    Private Sub botaoopt_MouseLeave(sender As Object, e As System.EventArgs) Handles botaoopt.MouseLeave
        botaoopt.Image = My.Resources.opt
    End Sub

    'botao add all
    Private Sub addbuttonall_MouseEnter(sender As Object, e As System.EventArgs) Handles addbuttonall.MouseEnter
        addbuttonall.Image = My.Resources.add2
    End Sub
    Private Sub addbuttonall_MouseLeave(sender As Object, e As System.EventArgs) Handles addbuttonall.MouseLeave
        addbuttonall.Image = My.Resources.add
    End Sub

    'botao shuffle
    Private Sub shufflle_button_MouseEnter(sender As Object, e As System.EventArgs) Handles shufflle_button.MouseEnter
        shufflle_button.Image = My.Resources.shuffle2
    End Sub
    Private Sub shufflle_button_MouseLeave(sender As Object, e As System.EventArgs) Handles shufflle_button.MouseLeave
        shufflle_button.Image = My.Resources.shuffle
    End Sub
#End Region

#Region "botoes na taskbar"
    Public WithEvents trastaskbar As ThumbnailToolBarButton
    Public WithEvents frentetaskbar As ThumbnailToolBarButton
    Public WithEvents playpausetaskbar As ThumbnailToolBarButton

    Private Sub Form1_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        Try
            trastaskbar = New ThumbnailToolBarButton(My.Resources.previoustaskbar, ToolTip1.GetToolTip(button_prev))
            frentetaskbar = New ThumbnailToolBarButton(My.Resources.nexttaskbar, ToolTip1.GetToolTip(button_next))
            playpausetaskbar = New ThumbnailToolBarButton(My.Resources.playtaskbar, ToolTip1.GetToolTip(button_play))
            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(Me.Handle, trastaskbar, playpausetaskbar, frentetaskbar)
            TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(Me.Handle, New Rectangle(painelplayer.Location.X + 4, painelplayer.Location.Y, painelplayer.Size.Width - 1, painelplayer.Size.Height - 4))
        Catch ex As Exception
        End Try
    End Sub

    Private Sub frentetaskbar_Click(ByVal sender As Object, ByVal e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles frentetaskbar.Click
        seguinte()
    End Sub

    Private Sub trastaskbar_Click(ByVal sender As Object, ByVal e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles trastaskbar.Click
        anterior()
    End Sub

    Private Sub playpausetaskbar_Click(ByVal sender As Object, ByVal e As Microsoft.WindowsAPICodePack.Taskbar.ThumbnailButtonClickedEventArgs) Handles playpausetaskbar.Click
        button_play_Click(sender, e)
    End Sub
#End Region

#Region "lista de videos e musica moviveis"
    'mostra/esconde lista de videos
    Private Sub youtubemenuvideos_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles youtubemenuvideos.Click, HideShowYoutubeVideosToolStripMenuItem.Click
        If painelyou = True Then
            painelyou = False
        ElseIf painelyou = False Then
            painelyou = True
        End If
        timeryoutube.Enabled = True
    End Sub

    Private Sub timeryoutube_Tick(sender As System.Object, e As System.EventArgs) Handles timeryoutube.Tick
        If painelyou = True Then
            painelplayer.Left = painelplayer.Left + 2
            painelyoutube.Left = painelyoutube.Left + 2
            painelplayer.Width = painelplayer.Width - 2
            If painelyoutube.Right >= painelyoutube.Width Then
                timeryoutube.Enabled = False
                TextBox1.Enabled = False
                youtubemenuvideos.Image = My.Resources.painelesquerda
            End If
        End If
        If painelyou = False Then
            painelplayer.Left = painelplayer.Left - 2
            painelyoutube.Left = painelyoutube.Left - 2
            painelplayer.Width = painelplayer.Width + 2
            If painelyoutube.Left <= -painelyoutube.Width Then
                timeryoutube.Enabled = False
                TextBox1.Enabled = False
                youtubemenuvideos.Image = My.Resources.paineldireita
            End If
        End If
    End Sub

    'mostra/esconde lista de musicas
    Private Sub listamenu_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles listamenu.Click, hideslistmenu.Click
        If painellist = True Then
            painellist = False
        ElseIf painellist = False Then
            painellist = True
        End If
        timerlista.Enabled = True
    End Sub

    Private Sub timerlista_Tick(sender As System.Object, e As System.EventArgs) Handles timerlista.Tick
        If painellist = False Then
            painelplayer.Width = painelplayer.Width + 2
            painellista.Left = painellista.Left + 2
            If fullc = True Then
                If painellista.Left >= Me.Width Then
                    timerlista.Enabled = False
                    listamenu.Image = My.Resources.esquerda
                End If
            Else
                If painellista.Left >= Me.Width - 15 Then
                    timerlista.Enabled = False
                    listamenu.Image = My.Resources.painelesquerda
                End If
            End If
        End If
        If painellist = True Then
            painellista.Left = painellista.Left - 2
            painelplayer.Width = painelplayer.Width - 2
            If painellista.Right <= Me.Width - 13 Then
                timerlista.Enabled = False
                listamenu.Image = My.Resources.paineldireita
            End If
        End If
    End Sub
#End Region

#Region "movimento do rato, para listas e barra de menu"
    'movimento do rato é utilizado quando o programa está em fullscreen
    Private Sub mousemoveall(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseMove, painellista.MouseMove, painelyoutube.MouseMove, painelplayer.MouseMove, painelmenu.MouseMove
        If fullc = True Then
            If MousePosition.Y - Me.Location.Y > painelmenu.Top Then
                painelmenu.Show()
            Else
                painelmenu.Hide()
            End If
            If MousePosition.X - Me.Location.X < painelyoutube.Right + 40 Then
                painelyoutube.Show()
            Else
                painelyoutube.Hide()
            End If
            If MousePosition.X - Me.Location.X + 31 > painellista.Left Then
                painellista.Show()
            Else
                painellista.Hide()
            End If
            If cursorpos <> MousePosition Then
                cursorpos = MousePosition
                Cursor = Cursors.Default
                Cursor.Show()
                If cursortimer.Enabled = False Then
                    cursortimer.Interval = 3000
                    cursortimer.Start()
                End If
            End If
        End If
    End Sub
#End Region

#Region "drag and drop listbox e painel"
    Private Sub ListBox1_DragOver(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles ListBox1.DragOver
        If e.Data.GetDataPresent(DataFormats.Text) Then
            e.Effect = DragDropEffects.Move
        End If
    End Sub

    Private Sub ListBox1_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles ListBox1.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())
            For Each filename As String In files
                Dim fileinfo As New FileInfo(filename)
                If filetype.Contains(fileinfo.Extension) = True Then
                    If fileinfo.Extension = ".mspl" Or fileinfo.Extension = ".MSPL" Then
                        abrir_lista(filename)
                    ElseIf Not fileinfo.Extension = ".srt" Or Not fileinfo.Extension = ".SRT" Then
                        ListBox1.Items.Add(New listbox_item_data(filename, fileinfo.Name.Replace(fileinfo.Extension, "")))
                    End If
                End If
            Next
        End If
    End Sub

    Private Sub ListBox1_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles ListBox1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub painelplayer_DragEnter(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles painelplayer.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Sub painelplayer_Dragover(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles painelplayer.DragOver
        If e.Data.GetDataPresent(DataFormats.Text) Then
            e.Effect = DragDropEffects.Move
        End If
    End Sub

    Private Sub painelplayer_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles painelplayer.DragDrop
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim files As String() = CType(e.Data.GetData(DataFormats.FileDrop), String())
            For Each filename As String In files
                Dim fileinfo As New FileInfo(filename)
                If filetype.Contains(fileinfo.Extension) = True Then
                    If fileinfo.Extension = ".srt" Or fileinfo.Extension = ".SRT" Then
                        adicionar_legendas(filename)
                    Else
                        playfile(filename)
                    End If
                End If
            Next
        End If
    End Sub
#End Region

#Region "label em movimento"
    Private Sub timerlblmusica_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles timerlblmusica.Tick
        If lbl_musica.Left = menudir.Left Then
            lbl_musica.Left = listento.Right + 1 - lbl_musica.Width
        Else
            lbl_musica.Left = (lbl_musica.Left + 1)
        End If
    End Sub
#End Region

#Region "pesquisar videos"
    ' botão pesquisar
    Private Sub pesquisarbutton_Click(sender As System.Object, e As System.EventArgs) Handles pesquisarbutton.Click
        pesquisaresquerda.Image = My.Resources.esquerdadisabled
        index = 10
        If Not pesquisarbackgorundworker.IsBusy Then
            pesquisarbackgorundworker.RunWorkerAsync(index - 9)
        End If
    End Sub

    'botoes de navegação de lista
    Private Sub pesquisardireita_Click(sender As System.Object, e As System.EventArgs) Handles pesquisardireita.Click
        If pesquisarbackgorundworker.IsBusy Then
            Exit Sub
        End If
        If index = 50 Then
            Exit Sub
        End If
        If index >= 10 Then
            pesquisaresquerda.Enabled = True
            pesquisaresquerda.Image = My.Resources.esquerda
        End If
        index = index + 10
        If Not pesquisarbackgorundworker.IsBusy Then
            If index = 50 Then
                pesquisardireita.Enabled = False
                pesquisardireita.Image = My.Resources.direitadisabled
            End If
            pesquisarbackgorundworker.RunWorkerAsync(index - 10)
        End If
    End Sub

    Private Sub pesquisaresquerda_Click(sender As System.Object, e As System.EventArgs) Handles pesquisaresquerda.Click
        If pesquisarbackgorundworker.IsBusy Then
            Exit Sub
        End If
        If index <= 50 Then
            pesquisardireita.Enabled = True
            pesquisardireita.Image = My.Resources.direita
        End If
        Dim indexfinal As Integer
        If index = 10 Then
            Exit Sub
        End If
        If index = 20 Then
            index = index - 10
            indexfinal = index - 9
        Else
            index = index - 10
            indexfinal = index - 10
        End If
        If Not pesquisarbackgorundworker.IsBusy Then
            If index = 10 Then
                pesquisaresquerda.Image = My.Resources.esquerdadisabled
                pesquisaresquerda.Enabled = True
            End If
            pesquisarbackgorundworker.RunWorkerAsync(indexfinal)
        End If
    End Sub

    'inicia  a pesquisa dentro do backgroundworker
    Private Sub pesquisar(ByVal pesquisa As String, inicio As Integer)
        Try
            My.Computer.Network.Ping("www.youtube.com", 500) ' verifica se o computador esta ligado a internet
        Catch ex As Exception
            MsgBox(semnet)
            Me.Invoke(New mplayerglobaldelegate(AddressOf removecursorfromtextbox))
            Exit Sub
        End Try
        Try
            If TextBox1.Text = "" Then                       'verifica se a caixa de texto encontra-se em branco
                MsgBox(caixaembranco)
                Me.Invoke(New mplayerglobaldelegate(AddressOf removecursorfromtextbox))
                Exit Sub
            End If
            Me.Invoke(New carregardelegade(AddressOf disableprocurar), New Object() {pesquisarbutton})
            Me.Invoke(New carregardelegade(AddressOf mostrar), New Object() {loading})
            Me.Invoke(New Delegateglobal(AddressOf apagar))
            caixa = HttpUtility.UrlEncode(pesquisa)
            Dim rssfeedurl As String = "http://gdata.youtube.com/feeds/api/videos?q=" & caixa & "&start-index=" & inicio & "&max-results=" & index & "&v=2&alt=rss&orderby=relevance"
            Me.Invoke(New progresso(AddressOf progressototal), New Object() {20})
            Dim rssdoc As XDocument = XDocument.Load(rssfeedurl)
            Dim xroot = XElement.Parse(rssdoc.ToString())
            Dim nores = rssdoc.Descendants("item").Value
            If nores Is Nothing Then
                MsgBox(semresultados)
                Me.Invoke(New progresso(AddressOf progressototal), New Object() {0})
                Me.Invoke(New mplayerglobaldelegate(AddressOf removecursorfromtextbox))
                Exit Sub
            End If
            Me.Invoke(New progresso(AddressOf progressototal), New Object() {30})
            Dim rssresult = From rssdoelements In rssdoc.Descendants("item") _
                            Select New With { _
                                .link = rssdoelements.Descendants("link").Value, _
                                .title = rssdoelements.Descendants("title").Value}
            Me.Invoke(New progresso(AddressOf progressototal), New Object() {40})
            For Each co In rssresult
                Me.Invoke(New progresso(AddressOf progressototal), New Object() {100})
                Dim linkvideo(1) As String
                linkvideo = Split(co.link, "v=")
                Dim parteurl(2) As String
                parteurl = Split(linkvideo(1), "&")
                Dim myImage As System.Drawing.Image = LoadImageFromUrl("http://i.ytimg.com/vi/" + parteurl(0) + "/default.jpg")
                Dim urllink As String = ("http://www.youtube.com/watch?v=" & parteurl(0))
                Me.Invoke(New inserirdados(AddressOf inserir_dados), New Object() {myImage, co.title, urllink})
            Next
            Me.Invoke(New carregardelegade(AddressOf mostrar), New Object() {loading})
            Me.Invoke(New carregardelegade(AddressOf disableprocurar), New Object() {pesquisarbutton})
        Catch ex As Exception
            MsgBox(falhapesquisa)
        End Try
        Me.Invoke(New mplayerglobaldelegate(AddressOf removecursorfromtextbox))
        caixa = Nothing
    End Sub
    Private Sub removecursorfromtextbox()
        Me.ActiveControl = Nothing
    End Sub
    Private Sub pesquisarbackgorundworker_DoWork(sender As System.Object, e As System.ComponentModel.DoWorkEventArgs) Handles pesquisarbackgorundworker.DoWork
        pesquisar(TextBox1.Text, e.Argument)
    End Sub

    'private delegates usado para modificar objectos noutra threat
    Private Delegate Sub carregardelegade(ByVal imagem As PictureBox) ' modifica as imagens
    Private Delegate Sub Delegateglobal() ' para apagar os videos
    Private Delegate Sub inserirdados(ByVal imagem As Image, ByVal titulo As String, ByVal url As String)
    Private Delegate Sub progresso(ByVal progresso As Double) ' barra de progresso

    ' mostrar a imagem, a carregar a lista de videos
    Private Sub apagar()
        Label1.Text = ""
        Label2.Text = ""
        Label3.Text = ""
        Label4.Text = ""
        Label5.Text = ""
        Label6.Text = ""
        Label7.Text = ""
        Label8.Text = ""
        Label9.Text = ""
        PictureBox1.Image = Nothing
        PictureBox2.Image = Nothing
        PictureBox3.Image = Nothing
        PictureBox4.Image = Nothing
        PictureBox5.Image = Nothing
        PictureBox6.Image = Nothing
        PictureBox7.Image = Nothing
        PictureBox8.Image = Nothing
        PictureBox9.Image = Nothing
        PictureBox10.Image = Nothing
        GroupBox1.Visible = False
        GroupBox2.Visible = False
        GroupBox3.Visible = False
        GroupBox4.Visible = False
        GroupBox5.Visible = False
        GroupBox6.Visible = False
        GroupBox7.Visible = False
        GroupBox8.Visible = False
        GroupBox9.Visible = False
        GroupBox10.Visible = False
        Array.Clear(linksyou, 0, 9)
    End Sub ' apaga todos os videos da lista 

    'insere novos dados na lista
    Private Sub inserir_dados(ByVal imagem As Image, ByVal titulo As String, ByVal url As String)
        If PictureBox1.Image Is Nothing Or Label1.Text = "" Then
            escrevenopanel(PictureBox1, GroupBox1, Label1, imagem, titulo, url)
        ElseIf PictureBox2.Image Is Nothing Or Label2.Text = "" Then
            escrevenopanel(PictureBox2, GroupBox2, Label2, imagem, titulo, url)
        ElseIf PictureBox3.Image Is Nothing Or Label3.Text = "" Then
            escrevenopanel(PictureBox3, GroupBox3, Label3, imagem, titulo, url)
        ElseIf PictureBox4.Image Is Nothing Or Label4.Text = "" Then
            escrevenopanel(PictureBox4, GroupBox4, Label4, imagem, titulo, url)
        ElseIf PictureBox5.Image Is Nothing Or Label5.Text = "" Then
            escrevenopanel(PictureBox5, GroupBox5, Label5, imagem, titulo, url)
        ElseIf PictureBox6.Image Is Nothing Or Label6.Text = "" Then
            escrevenopanel(PictureBox6, GroupBox6, Label6, imagem, titulo, url)
        ElseIf PictureBox7.Image Is Nothing Or Label7.Text = "" Then
            escrevenopanel(PictureBox7, GroupBox7, Label7, imagem, titulo, url)
        ElseIf PictureBox8.Image Is Nothing Or Label8.Text = "" Then
            escrevenopanel(PictureBox8, GroupBox8, Label8, imagem, titulo, url)
        ElseIf PictureBox9.Image Is Nothing Or Label9.Text = "" Then
            escrevenopanel(PictureBox9, GroupBox9, Label9, imagem, titulo, url)
        ElseIf PictureBox10.Image Is Nothing Or Label10.Text = "" Then
            escrevenopanel(PictureBox10, GroupBox10, Label10, imagem, titulo, url)
        End If
    End Sub

    Public Sub escrevenopanel(ByVal imagemp As PictureBox, ByVal box As GroupBox, ByVal labela As Label, ByVal imagem As Drawing.Image, ByVal titulo As String, ByVal urlf As String)
        Try
            Dim tamanho() As String = SplitBySize(titulo, 31)
            imagemp.Image = imagem
            box.Visible = True
            labela.Text = titulo
            Try
                If titulo.Length >= 31 Then
                    If tamanho.Length = 2 Then
                        labela.Text = tamanho(0) & vbNewLine & tamanho(1)
                    ElseIf tamanho.Length = 3 Then
                        labela.Text = tamanho(0) & vbNewLine & tamanho(1) & vbNewLine & tamanho(2)
                    ElseIf tamanho.Length = 4 Then
                        labela.Text = tamanho(0) & vbNewLine & tamanho(1) & vbNewLine & tamanho(2) & vbNewLine & tamanho(3)
                    ElseIf tamanho.Length = 5 Then
                        labela.Text = tamanho(0) & vbNewLine & tamanho(1) & vbNewLine & tamanho(2) & vbNewLine & tamanho(3) & vbNewLine & tamanho(4)
                    ElseIf tamanho.Length = 6 Then
                        labela.Text = tamanho(0) & vbNewLine & tamanho(1) & vbNewLine & tamanho(2) & vbNewLine & tamanho(3) & vbNewLine & tamanho(4) & tamanho(5)
                    Else
                        labela.Text = titulo
                    End If
                End If
            Catch ex As Exception
            End Try
            If linksyou(0) = "" Then
                linksyou(0) = urlf
            ElseIf linksyou(1) = "" Then
                linksyou(1) = urlf
            ElseIf linksyou(2) = "" Then
                linksyou(2) = urlf
            ElseIf linksyou(3) = "" Then
                linksyou(3) = urlf
            ElseIf linksyou(4) = "" Then
                linksyou(4) = urlf
            ElseIf linksyou(5) = "" Then
                linksyou(5) = urlf
            ElseIf linksyou(6) = "" Then
                linksyou(6) = urlf
            ElseIf linksyou(7) = "" Then
                linksyou(7) = urlf
            ElseIf linksyou(8) = "" Then
                linksyou(8) = urlf
            ElseIf linksyou(9) = "" Then
                linksyou(9) = urlf
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub mostrar(ByVal imagemp As PictureBox)
        If imagemp.Visible = False Then
            imagemp.Show()
        Else
            imagemp.Hide()
        End If
    End Sub
    Private Sub disableprocurar(ByVal imagemp As PictureBox)
        If imagemp.Enabled = False Then
            imagemp.Enabled = True
        Else
            imagemp.Enabled = False
        End If
    End Sub
    Private Sub progressototal(ByVal progresso As Double) ' progresso da barra
        ProgressBar1.Value = progresso
    End Sub

    'função para carregar a imagem vinda da internet
    Private Function LoadImageFromUrl(ByVal url As String) As System.Drawing.Image 'função para descarregar imagem do video
        Dim theRequest As System.Net.WebRequest = System.Net.WebRequest.Create(url)
        Dim theResponse As System.Net.WebResponse = theRequest.GetResponse()
        Dim theImage As System.Drawing.Image = System.Drawing.Image.FromStream(theResponse.GetResponseStream())
        Return theImage
    End Function

    'divide uma string por um numero determinado
    Public Function SplitBySize(ByVal strInput As String, ByVal iSize As Integer) As String() ' função para dividir o texto por tamanho
        Dim strA() As String
        Dim iLength As Integer = strInput.Length()
        Dim iWords As Integer = iLength / iSize + IIf((iLength Mod iSize <> 0), 1, 0)
        ReDim strA(iWords)

        Dim j As Integer = 0, i As Integer
        For i = 0 To iLength Step iSize
            strA(j) = Mid(strInput, i + 1, iSize)
            j = j + 1
        Next i
        Return strA
    End Function
#End Region

#Region "botao fullscreen"
    Private cursorpos As Point
    Private lastwindows As Integer

    Private Sub botaofullsc_Click(sender As System.Object, e As System.EventArgs) Handles botaofullsc.Click
        Dim tamanho As Integer = MenuStrip1.Height + painelmenu.Height
        If fullc = False Then
            lastwindows = Me.WindowState
            Me.WindowState = FormWindowState.Maximized
            Me.FormBorderStyle = FormBorderStyle.None
            Me.TopMost = True
            SetWindowPos(Me.Handle, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW)
            painelplayer.Location = New Point(painelplayer.Location.X, 0)
            painelyoutube.Location = New Point(painelyoutube.Location.X, 0)
            painellista.Location = New Point(painellista.Location.X, 0)
            painelplayer.Height += tamanho
            painelyoutube.Height += tamanho
            painellista.Height += tamanho
            If painelyou = True Then
                painelplayer.Left = 0
                painelplayer.Width += painelyoutube.Width
            End If
            If painellist = True Then
                painelplayer.Width += painellista.Width
            End If
            MenuStrip1.Hide()
            fullc = True
            cursortimer.Enabled = True
            cursortimer.Interval = 3000
            cursorpos = MousePosition
            youtubemenuvideos.Enabled = False
            listamenu.Enabled = False
        Else
            cursortimer.Enabled = False
            fullc = False
            Me.WindowState = lastwindows
            Me.FormBorderStyle = FormBorderStyle.Sizable
            Me.TopMost = False
            painelplayer.Location = New Point(painelplayer.Location.X, MenuStrip1.Height)
            painelyoutube.Location = New Point(painelyoutube.Location.X, MenuStrip1.Height)
            painellista.Location = New Point(painellista.Location.X, MenuStrip1.Height)
            painelplayer.Height -= tamanho
            painelyoutube.Height -= tamanho
            painellista.Height -= tamanho
            If painelyou = True Then
                painelplayer.Left += painelyoutube.Width
                painelplayer.Width -= painelyoutube.Width
                painelyoutube.Show()
            End If
            If painellist = True Then
                painelplayer.Width -= painellista.Width
            End If
            painelmenu.Show()
            MenuStrip1.Show()
            painellista.Show()
            Cursor = Cursors.Default
            cursorpos = Nothing
            youtubemenuvideos.Enabled = True
            listamenu.Enabled = True
        End If
    End Sub

    Private WithEvents cursortimer As New Timer
    Private Sub cursortimer_Tick(sender As Object, e As System.EventArgs) Handles cursortimer.Tick
        If MousePosition = cursorpos Then
            Dim ms As New System.IO.MemoryStream(My.Resources.Blank)
            Me.Cursor = New Cursor(ms)
            cursorpos = MousePosition
        End If
        cursortimer.Enabled = False
    End Sub

    Private Sub painelplayer_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles painelplayer.DoubleClick
        botaofullsc_Click(sender, e)
    End Sub
#End Region

#Region "mplayer eventos| sair e progresso"
    Private Delegate Sub pos(ByVal pos As Integer) ' posiçao do video em tempo
    Private Delegate Sub mplayerglobaldelegate()   'delegate global
    Private Sub videoexited(sender As Object, e As MplayerEvent)
        Try
            Me.Invoke(New mplayerglobaldelegate(AddressOf resettitems))
        Catch ex As Exception
        End Try
    End Sub
    'tempo ocorrido do ficheiro múltimédia
    Private Sub posicao(ByVal sender As Object, ByVal e As MplayerEvent)
        Try
            Me.Invoke(New pos(AddressOf tempototalprog), New Object() {e.Value})
        Catch ex As Exception
        End Try
    End Sub

    Private Sub tempototalprog(ByVal tempo As Integer)
        Dim tempoc As Integer = tempo / 10
        minutohoras(tempoc, lbl_pos)
        progressotrackbar.Value = tempoc
        tempoc = Nothing
    End Sub

    'converter minutos em horas
    Private Sub minutohoras(ByVal time As Double, ByVal label As Label)
        Dim horas, min, segundos As Integer
        horas = Int(Int(time) / 3600)
        segundos = Int(time) Mod 60
        min = Int((Int(time) - (horas * 3600)) / 60)
        If horas = 0 Then
            label.Text = Format(min, "00") & ":" & Format(segundos, "00")
        Else
            label.Text = Format(horas, "00") & ":" _
               & Format(min, "00") & ":" & Format(segundos, "00")
        End If
        horas = Nothing
        min = Nothing
        segundos = Nothing
    End Sub

    'tempo total do ficheiro
    Private Sub tempototal()
        Dim tempoc As Integer = player.CurrentPlayingFileLength() / 100
        minutohoras(tempoc, lbl_totalpos)
        progressotrackbar.Maximum = tempoc
        tempoc = Nothing
    End Sub

    'ver a cache do ficheiro 
    Private Delegate Sub mplayermessage(ByVal valor As String)
    Private Sub chachemplayer(ByVal valor As String)
        listento.Text = "Cache:" & valor
    End Sub
    Private Sub cacched(ByVal sender As Object, ByVal e As MplayerEvent)
        Me.Invoke(New mplayermessage(AddressOf chachemplayer), New Object() {e.Message})
    End Sub
#End Region

#Region "mplayer iniciar o video"

    'inicia o mpalyer num backgroundworker
    Public WithEvents playerbginit As New System.ComponentModel.BackgroundWorker
    Private Sub player_init()
        Try
            player = New MPlayer(fonttype, 3, legendatamanho, codlegendas, poslegendas, volumebar.Value, painelplayer.Handle.ToInt32, MplayerBackends.Direct3D, Application.StartupPath & "\mplayer\mplayer.exe")
            AddHandler player.VideoExited, AddressOf videoexited
            AddHandler player.CurrentPosition, AddressOf posicao
            AddHandler player.cache, AddressOf cacched
            AddHandler player.finalfile, AddressOf fim_doficheiro
            AddHandler player.consola, AddressOf consola
            AddHandler player.scanfonts, AddressOf scanfonts
            AddHandler player.filesub, AddressOf filesubs
            AddHandler player.audiochannel, AddressOf audiochannels
            AddHandler player.setaudiolang, AddressOf setaudiolang
        Catch ex As Exception
        End Try
    End Sub

    'selecionar o canal de audio
    Private Sub setaudiolang(ByVal sender As Object, ByVal e As MplayerEvent)
        Me.Invoke(New stringinsertdelegate(AddressOf audioselect), New Object() {e.Message.Substring(e.Message.IndexOf("=") + 1)})
    End Sub

    Private Sub audiochannels(ByVal sender As Object, ByVal e As MplayerEvent)
        Me.Invoke(New stringinsertdelegate(AddressOf addaudiochannel), New Object() {e.Message.Substring(0, e.Message.IndexOf("_")) & " " & e.Message.Substring(e.Message.IndexOf("=") + 1)})
    End Sub

    Private Sub audioselect(ByVal msg As String)
        For i = 0 To audiomenustrip.DropDownItems.Count - 1
            If audiomenustrip.DropDownItems.Item(i).Text.Contains(msg) Then
                audiomenustrip.DropDownItems.Item(i).Image = My.Resources.checkbox
                audiocontextmenu.DropDownItems.Item(i).Image = My.Resources.checkbox
            End If
        Next
    End Sub

    Private Sub addaudiochannel(ByVal msg As String)
        If audiocontextmenu.DropDownItems.Item(0).Text = "Channel 1" Then
            audiomenustrip.DropDownItems.Item(0).Text = msg
            audiocontextmenu.DropDownItems.Item(0).Text = msg
        Else
            audiomenustrip.DropDownItems.Add(msg)
            audiocontextmenu.DropDownItems.Add(msg)
        End If
        AddHandler audiomenustrip.DropDownItems.Item(audiomenustrip.DropDownItems.Count - 1).Click, AddressOf audioclicked
        AddHandler audiocontextmenu.DropDownItems.Item(audiomenustrip.DropDownItems.Count - 1).Click, AddressOf audioclicked
    End Sub

    Private Sub audioclicked(ByVal sender As Object, ByVal e As EventArgs)
        For i = 0 To audiomenustrip.DropDownItems.Count - 1
            audiomenustrip.DropDownItems.Item(i).Image = Nothing
            audiocontextmenu.DropDownItems.Item(i).Image = Nothing
        Next
        CType(sender, ToolStripMenuItem).Image = My.Resources.checkbox
        For i = 0 To audiomenustrip.DropDownItems.Count - 1
            If Not audiomenustrip.DropDownItems.Item(i).Image Is Nothing Then
                player.changeaudio(audiomenustrip.DropDownItems.Item(i).Text.Substring(0, audiomenustrip.DropDownItems.Item(i).Text.IndexOf(" ")))
                Exit For
            End If
        Next
    End Sub

    'Leitura de todos os tipos de letra do windows
    Private Sub scanfonts(ByVal sender As Object, ByVal e As MplayerEvent)
        Me.Invoke(New mplayermessage(AddressOf scanfontssub), New Object() {e.Message})
    End Sub
    Private Sub scanfontssub(ByVal msg As String)
        If msg.Contains("Scanning file") Then
            If infofonts.Visible = False Then
                infofonts.TopLevel = False
                painelplayer.Controls.Add(infofonts)
                infofonts.Parent = painelplayer
                infofonts.Show()
            End If
            infofonts.RichTextBox1.Text += msg + vbNewLine
            infofonts.RichTextBox1.SelectionStart = mplayerlog.RichTextBox1.TextLength
        End If
        If msg.Contains("get_path") Then
            If infofonts.Visible = True Then
                infofonts.Close()
            End If
            Exit Sub
        End If
    End Sub

    'Log do mplayer
    Private Sub consola(ByVal sender As Object, ByVal e As MplayerEvent)
        Try
            Me.Invoke(New mplayermessage(AddressOf consoladelegate), New Object() {e.Message})
        Catch ex As Exception
        End Try
    End Sub
    Private Sub consoladelegate(ByVal valor As String)
        mplayerlog.RichTextBox1.Text += valor + vbNewLine
        mplayerlog.RichTextBox1.SelectionStart = mplayerlog.RichTextBox1.TextLength
    End Sub

    'reporta quando o ficheiro chega ao final
    Private Sub fim_doficheiro(ByVal sender As Object, ByVal e As MplayerEvent)
        Try
            Me.Invoke(New mplayermessage(AddressOf fimficheirodelegate), New Object() {e.Message})
        Catch ex As Exception
        End Try
    End Sub
    Private Sub fimficheirodelegate(ByVal valor As String)
        If valor.Contains("1") Then
            seguinte()
        End If
    End Sub

    'reinicia os items do programa
    Private Sub resettitems()
        If estadomsn = True Then
            msn.SendStatusMessage(False, msn.EnumCategory.Music, "")
        End If
        timerlblmusica.Enabled = False
        lbl_musica.Left = listento.Right + 1 - lbl_musica.Width
        lbl_musica.Text = ""
        progressotrackbar.Value = 0
        lbl_musica.Text = ""
        lbl_pos.Text = "00:00"
        lbl_totalpos.Text = "00:00"
        button_play.Image = My.Resources.play
        imagemalbum.Image = My.Resources.album
        arraylegendas.Clear()
        SubtitleToolStripMenuItem.DropDownItems.Clear()
        SubtitlesToolStripMenuItem1.DropDownItems.Clear()
        audiomenustrip.DropDownItems.Clear()
        audiocontextmenu.DropDownItems.Clear()
        audiomenustrip.DropDownItems.Add(canal1)
        audiocontextmenu.DropDownItems.Add(canal1)
        audiocontextmenu.DropDownItems.Item(0).Image = My.Resources.checkbox
        audiomenustrip.DropDownItems.Item(0).Image = My.Resources.checkbox
        SubtitleToolStripMenuItem.DropDownItems.Add(nosubs)
        SubtitleToolStripMenuItem.DropDownItems.Item(0).Image = My.Resources.checkbox
        AddHandler SubtitleToolStripMenuItem.DropDownItems.Item(0).Click, AddressOf menulegendas
        SubtitlesToolStripMenuItem1.DropDownItems.Add(nosubs)
        SubtitlesToolStripMenuItem1.DropDownItems.Item(0).Image = My.Resources.checkbox
        AddHandler SubtitlesToolStripMenuItem1.DropDownItems.Item(0).Click, AddressOf menulegendas
        delete_checkimage_item()
        aspect_auto_menu.Image = My.Resources.checkbox
        aspect_auto_c.Image = My.Resources.checkbox
        legendason = False
        legendasv = True
        estado = Estadoenum.parado
        Try
            player.Stop()
            System.Threading.Thread.Sleep(500)
        Catch ex As Exception
        End Try
        memoria.FlushMemory()
    End Sub

    'menu de legendas 
    Private Sub menulegendas(ByVal sender As Object, ByVal e As EventArgs)
        For i = 0 To SubtitleToolStripMenuItem.DropDownItems.Count - 1
            SubtitleToolStripMenuItem.DropDownItems.Item(i).Image = Nothing
            SubtitlesToolStripMenuItem1.DropDownItems.Item(i).Image = Nothing
        Next
        CType(sender, ToolStripMenuItem).Image = My.Resources.checkbox
        For i = 0 To SubtitleToolStripMenuItem.DropDownItems.Count - 1
            If Not SubtitleToolStripMenuItem.DropDownItems.Item(i).Image Is Nothing Or Not SubtitlesToolStripMenuItem1.DropDownItems.Item(i).Image Is Nothing Then
                SubtitlesToolStripMenuItem1.DropDownItems.Item(i).Image = My.Resources.checkbox
                SubtitleToolStripMenuItem.DropDownItems.Item(i).Image = My.Resources.checkbox
                If SubtitleToolStripMenuItem.DropDownItems.Item(i).Text.Contains(nosubs) Or SubtitlesToolStripMenuItem1.DropDownItems.Item(i).Text.Contains(nosubs) Then
                    Try
                        Me.Invoke(New mplayerglobaldelegate(AddressOf removerlegendas))
                        legendason = False
                    Catch ex As Exception
                    End Try
                Else
                    Try
                        Me.Invoke(New mplayerglobaldelegate(AddressOf removerlegendas))
                        System.Threading.Thread.Sleep(500)
                        Me.Invoke(New stringinsertdelegate(AddressOf adicionar_legendas), New Object() {arraylegendas.Item(i - 1)})
                    Catch ex As Exception
                    End Try
                End If
            End If
        Next
    End Sub

    Private Sub filesubs(ByVal sender As Object, ByVal e As MplayerEvent)
        Me.Invoke(New stringinsertdelegate(AddressOf checkitems), New Object() {e.Message})
    End Sub

    Private Sub checkitems(ByVal msg As String)
        For i = 0 To SubtitleToolStripMenuItem.DropDownItems.Count - 1
            SubtitleToolStripMenuItem.DropDownItems.Item(i).Image = Nothing
            SubtitlesToolStripMenuItem1.DropDownItems.Item(i).Image = Nothing
        Next
        For i = 0 To SubtitleToolStripMenuItem.DropDownItems.Count - 1
            If i <= arraylegendas.Count - 1 Then
                If arraylegendas.Item(i).ToString.Contains(msg) Then
                    SubtitlesToolStripMenuItem1.DropDownItems.Item(i + 1).Image = My.Resources.checkbox
                    SubtitleToolStripMenuItem.DropDownItems.Item(i + 1).Image = My.Resources.checkbox
                    legendason = True
                    legendasv = True
                End If
            Else
                Exit For
            End If
        Next
    End Sub

    Private Sub playerbginit_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles playerbginit.DoWork
        Me.Invoke(New mplayerglobaldelegate(AddressOf player_init))
        playerbginit.Dispose()
    End Sub

    Private Delegate Sub statedelegate(ByVal statetext As String, ByVal statehs As Boolean)
    Private Sub statetext(ByVal statetext As String, ByVal statehs As Boolean)
        state.Text = statetext
        state.Visible = statehs
    End Sub

    Private Delegate Sub showalbumdelegate(ByVal image As String, ByVal online As Boolean)
    Private Sub mostraalbum(ByVal image As String, ByVal online As Boolean)
        If online = True Then
            Dim linkimagem(1) As String
            linkimagem = Split(image, "v=")
            imagemalbum.Image = LoadImageFromUrl(("http://i.ytimg.com/vi/" + linkimagem(1) + "/default.jpg"))
        Else
            imagemalbum.ImageLocation = image
        End If
    End Sub

    Private Delegate Sub stringinsertdelegate(ByVal text As String)
    Private Sub labeltimeri(ByVal text As String)
        lbl_musica.Text = text.Replace(vbNewLine, "")
        timerlblmusica.Enabled = True
    End Sub

    'reprodução do ficheiro
    Private Sub bgwplayer_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgwplayer.DoWork
        Dim eventargs As EventArgs = Nothing
        Me.Invoke(New mplayerglobaldelegate(AddressOf resettitems))
        Dim args As Object() = DirectCast(e.Argument, Object())
        Dim arg1 As String = CStr(args(0))
        Dim arg2 As String = CStr(args(1))
        If arg1.StartsWith("http://") Then
            Me.Invoke(New statedelegate(AddressOf statetext), New Object() {recolherurl, True})
            url = youtubedownload.geturl(arg1, youtubedownload.qualidadevideo)
            If url.Contains("Error") Then
                MsgBox(msgerro)
                Me.Invoke(New statedelegate(AddressOf statetext), New Object() {"", False})
                Exit Sub
            End If
            Me.Invoke(New statedelegate(AddressOf statetext), New Object() {"", False})
            Me.Invoke(New showalbumdelegate(AddressOf mostraalbum), New Object() {arg1, True})
            player.Play(url)
            playpausetaskbar.Icon = My.Resources.pausetaskbar
            If player.FullScreen = False Then
                System.Threading.Thread.Sleep(500)
                player.ToggleFullScreen()
            End If
            Me.Invoke(New mplayerglobaldelegate(AddressOf tempototal))
            Me.Invoke(New stringinsertdelegate(AddressOf labeltimeri), New Object() {arg2})
            estado = Estadoenum.play
            button_play.Image = My.Resources.pause
            If estadomsn = True Then
                msn.SendStatusMessage(True, msn.EnumCategory.Music, arg2)
            End If
        Else
            Dim List As List(Of String) = Nothing
            If arg1.EndsWith(".avi") Or arg1.EndsWith(".mkv") Or arg1.EndsWith(".mp4") Then
                List = GetFiles(arg1.Substring(0, arg1.LastIndexOf("\")), "*.srt")
                For Each path In List
                    Me.Invoke(New stringinsertdelegate(AddressOf additemmenu), New Object() {path.Substring(path.LastIndexOf("\") + 1).Replace(".srt", "")})
                    arraylegendas.Add(path)
                Next
                If downloadlegendas = True Then
                    downloadsubs.file = arg1
                    Dim hash As Byte() = ComputeHash(arg1)
                    downloadsubs.hash = ToHexadecimal(hash)
                    Me.Invoke(New mplayerglobaldelegate(AddressOf downloadsubs.ShowDialog))
                End If
            End If
            player.Play(arg1)
            If File.Exists(arg1.Substring(0, arg1.LastIndexOf("\")) & "\folder.jpg") Then
                Me.Invoke(New showalbumdelegate(AddressOf mostraalbum), New Object() {(arg1.Substring(0, arg1.LastIndexOf("\")) & "\folder.jpg"), False})
                If File.Exists(arg1.Substring(0, arg1.LastIndexOf("\")) & "\albumart.jpg") Then
                    Me.Invoke(New showalbumdelegate(AddressOf mostraalbum), New Object() {(arg1.Substring(0, arg1.LastIndexOf("\")) & "\albumart.jpg"), False})
                End If
            End If
            url = arg1
            If legendason = False And Not List Is Nothing Then
                For i = 0 To List.Count - 1
                    System.Threading.Thread.Sleep(1000)
                    Me.Invoke(New stringinsertdelegate(AddressOf adicionar_legendas), New Object() {List.Item(0).ToString})
                    Exit For
                Next
            End If
            List = Nothing
            playpausetaskbar.Icon = My.Resources.pausetaskbar
            If player.FullScreen = False Then
                System.Threading.Thread.Sleep(500)
                player.ToggleFullScreen()
            End If
            Me.Invoke(New mplayerglobaldelegate(AddressOf tempototal))
            Me.Invoke(New stringinsertdelegate(AddressOf labeltimeri), New Object() {arg2})
            estado = Estadoenum.play
            button_play.Image = My.Resources.pause
            If estadomsn = True Then
                msn.SendStatusMessage(True, msn.EnumCategory.Music, arg2)
            End If
        End If
    End Sub

    'adiconar items da legenda ao menu
    Public Sub additemmenu(ByVal ficheiro As String)
        SubtitleToolStripMenuItem.DropDownItems.Add(ficheiro)
        SubtitlesToolStripMenuItem1.DropDownItems.Add(ficheiro)
        AddHandler SubtitlesToolStripMenuItem1.DropDownItems.Item(SubtitlesToolStripMenuItem1.DropDownItems.Count - 1).Click, AddressOf menulegendas
        AddHandler SubtitleToolStripMenuItem.DropDownItems.Item(SubtitleToolStripMenuItem.DropDownItems.Count - 1).Click, AddressOf menulegendas
    End Sub
#End Region

#Region "youtube lista"
#Region "cliques na lista de videos para visualizar os videos"
    Private Sub PictureBox1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox1.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(0), Label1.Text})
        End If
    End Sub

    Private Sub PictureBox2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox2.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(1), Label2.Text})
        End If
    End Sub

    Private Sub PictureBox3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox3.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(2), Label3.Text})
        End If
    End Sub

    Private Sub PictureBox4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox4.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(3), Label4.Text})
        End If
    End Sub

    Private Sub PictureBox5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox5.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(4), Label5.Text})
        End If
    End Sub

    Private Sub PictureBox6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox6.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(5), Label6.Text})
        End If
    End Sub

    Private Sub PictureBox7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox7.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(6), Label7.Text})
        End If
    End Sub

    Private Sub PictureBox8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox8.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(7), Label8.Text})
        End If
    End Sub

    Private Sub PictureBox9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox9.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(8), Label9.Text})
        End If
    End Sub

    Private Sub PictureBox10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PictureBox10.Click
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {linksyou(9), Label10.Text})
        End If
    End Sub
#End Region
#Region "cliques para adicionar os videos na lista"
    Private Sub add1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add1.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(0), Label1.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add2.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(1), Label2.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add3.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(2), Label3.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add4_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add4.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(3), Label4.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add5_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add5.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(4), Label5.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add6_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add6.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(5), Label6.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add7_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add7.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(6), Label7.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add8_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add8.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(7), Label8.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add9_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add9.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(8), Label9.Text.Replace(vbNewLine, "")))
    End Sub

    Private Sub add10_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles add10.Click
        ListBox1.Items.Add(New listbox_item_data(linksyou(9), Label10.Text.Replace(vbNewLine, "")))
    End Sub
#End Region

#Region "adicionar links para a lista de downloads"
    Private Sub download1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download1.Click
        tools.addlink(linksyou(0))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download2.Click
        tools.addlink(linksyou(1))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download3.Click
        tools.addlink(linksyou(2))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download4.Click
        tools.addlink(linksyou(3))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download5.Click
        tools.addlink(linksyou(4))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download6.Click
        tools.addlink(linksyou(5))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download7.Click
        tools.addlink(linksyou(6))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download8.Click
        tools.addlink(linksyou(7))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download9.Click
        tools.addlink(linksyou(8))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub download10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles download10.Click
        tools.addlink(linksyou(9))
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub
#End Region
#End Region

#Region "listas de reproduçao"
    'gravar lista
    Private Sub lbl_savelist_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbl_savelist.Click
        If ListBox1.Items.Count <> 0 Then
            Dim saveFileDialog1 As New SaveFileDialog
            saveFileDialog1.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.Desktop
            saveFileDialog1.Title = "MediaStationX Playlist"
            saveFileDialog1.Filter = "MediaStationX playlist (*.mspl)|*.mspl"
            saveFileDialog1.AddExtension = True
            saveFileDialog1.FilterIndex = 1
            saveFileDialog1.RestoreDirectory = True
            saveFileDialog1.FileName = "Playlist"
            If saveFileDialog1.ShowDialog() = DialogResult.OK Then
                guardar(saveFileDialog1.FileName)
            Else
                Exit Sub
            End If
        End If
    End Sub

    Private Sub guardar(ByVal ficheiro As String)
        Dim xmlDoc As New XmlDocument
        Dim nome, link As String
        Dim no As XmlNode
        no = xmlDoc.CreateElement("Playlist")
        xmlDoc.AppendChild(no)
        For i = 0 To ListBox1.Items.Count - 1
            nome = CType(ListBox1.Items(i), listbox_item_data).nomedovideo
            link = CType(ListBox1.Items(i), listbox_item_data).linkdovideo
            If nome.Contains("►") Then
                nome = nome.Replace("►", "")
            End If
            Dim RootElement As XmlElement = xmlDoc.DocumentElement
            Dim Element As XmlElement = xmlDoc.CreateElement("musica")
            RootElement.AppendChild(Element)
            RootElement = xmlDoc.DocumentElement
            'cria o elemento nome dentro do elemento musica
            Element = xmlDoc.CreateElement("Nome")
            Dim TextVideo As XmlText = xmlDoc.CreateTextNode(nome)
            RootElement.LastChild.AppendChild(Element)
            RootElement.LastChild.LastChild.AppendChild(TextVideo)
            'cria o elemento link dentro do elemnto musica
            Element = xmlDoc.CreateElement("link")
            TextVideo = xmlDoc.CreateTextNode(link)
            RootElement.LastChild.AppendChild(Element)
            RootElement.LastChild.LastChild.AppendChild(TextVideo)
        Next
        xmlDoc.Save(ficheiro)
    End Sub

    'abrir lista
    Private Sub lbl_openlist_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbl_openlist.Click
        Dim openFileDialog1 As New OpenFileDialog
        openFileDialog1.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.Desktop
        openFileDialog1.Title = "MediaStationX Playlist"
        openFileDialog1.Filter = "MediaStationX playlist (*.mspl)|*.mspl"
        openFileDialog1.AddExtension = True
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = True
        openFileDialog1.FileName = "Playlist"
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
        Else
            Exit Sub
        End If
        abrir_lista(openFileDialog1.FileName)
    End Sub
  
    Public Sub abrir_lista(ByVal ficheiro As String)
        Dim xmlDoc As New XmlDocument
        xmlDoc.Load(ficheiro)
        Dim itemnodes As XmlNodeList = Nothing
        Dim itemnode As XmlNode = Nothing
        itemnodes = xmlDoc.SelectNodes("/Playlist/musica")
        For Each itemnode In itemnodes
            ListBox1.Items.Add(New listbox_item_data(itemnode.ChildNodes.Item(1).InnerText, itemnode.ChildNodes.Item(0).InnerText))
        Next
    End Sub
    'limpar lista
    Private Sub lbl_clearlist_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbl_clearlist.Click
        If ListBox1.Items.Count - 1 Then
            ListBox1.Items.Clear()
            clicada = -1
        End If
    End Sub
    'baralha os items da lista
    Private Sub ShuffleItems(ByVal Listbox As System.Windows.Forms.ListBox)
        Dim arraynome As New ArrayList()
        Dim arrayurl As New ArrayList()
        Dim Random As New System.Random
        Listbox.BeginUpdate()
        For i = 0 To Listbox.Items.Count - 1
            arraynome.Add(CType(ListBox1.Items(i), listbox_item_data).nomedovideo)
            arrayurl.Add(CType(ListBox1.Items(i), listbox_item_data).linkdovideo)
        Next
        Listbox.Items.Clear()
        While arraynome.Count > 0
            Dim Index As System.Int32 = Random.Next(0, arraynome.Count)
            ListBox1.Items.Add(New listbox_item_data(arrayurl(Index), arraynome(Index)))
            arraynome.RemoveAt(Index)
            arrayurl.RemoveAt(Index)
        End While
        Listbox.EndUpdate()
        arraynome = Nothing
        arrayurl = Nothing
        Random = Nothing
    End Sub
    Private Sub shufflle_button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles shufflle_button.Click
        If ListBox1.Items.Count - 1 > 0 Then
            ShuffleItems(ListBox1)
        End If
    End Sub
#End Region

#Region "botoes play, stop, next, previous"
    'play/pausa
    Private Sub button_play_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button_play.Click, PlayToolStripMenuItem.Click
        If estado = Estadoenum.parado Then
            If ListBox1.Items.Count > 0 Then
                clicada = 0
                ListBox1.SetSelected(clicada, True)
                If Not bgwplayer.IsBusy Then
                    bgwplayer.RunWorkerAsync(New Object() {CType(ListBox1.SelectedItem, listbox_item_data).linkdovideo, CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo})
                    CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo = "►" & CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo
                    ListBox1.Invalidate()
                End If
            End If
        End If
        If player.MplayerRunning = True Then
            If estado = Estadoenum.play Then
                playpausetaskbar.Icon = My.Resources.playtaskbar
                button_play.Image = My.Resources.play
                player.Pause()
                estado = Estadoenum.pause
            ElseIf estado = Estadoenum.pause Then
                estado = Estadoenum.play
                playpausetaskbar.Icon = My.Resources.pausetaskbar
                button_play.Image = My.Resources.pause
                player.Pause()
            End If
        End If
    End Sub

    'parar
    Private Sub button_stop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button_stop.Click, StopToolStripMenuItem.Click
        clicada = -1
        If ListBox1.Items.Count > 0 Then
            ListBox1.SetSelected(0, True)
        End If
        Try
            For i = 0 To ListBox1.Items.Count - 1
                If CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Contains("►") Then
                    CType(ListBox1.Items(i), listbox_item_data).nomedovideo = CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Replace("►", "")
                    ListBox1.Invalidate()
                End If
            Next
        Catch ex As Exception
        End Try
        resettitems()
        url = Nothing
        bgworkeradd.Dispose()
        bgwplayer.Dispose()
        playerbginit.Dispose()
        Settings.Dispose()
        downloadsubs.Dispose()
        mplayerlog.Dispose()
        tools.Dispose()
        memoria.FlushMemory()
    End Sub

    'seguinte
    Private Sub button_next_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button_next.Click, NextToolStripMenuItem.Click
        seguinte()
    End Sub


    Private Sub seguinte()
        Dim sender As Object = Nothing
        Dim e As System.EventArgs = Nothing
        If ListBox1.Items.Count = 0 Then
            Exit Sub
        End If
        If ListBox1.SelectedIndex = ListBox1.Items.Count - 1 Then
            Try
                For i = 0 To ListBox1.Items.Count - 1
                    If CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Contains("►") Then
                        CType(ListBox1.Items(i), listbox_item_data).nomedovideo = CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Replace("►", "")
                        ListBox1.Invalidate()
                    End If
                Next
            Catch ex As Exception
            End Try
            resettitems()
            Exit Sub
        End If
        clicada = clicada + 1
        ListBox1.SetSelected(clicada, True)
        ListBox1_DoubleClick(sender, e)
    End Sub

    'anterior
    Private Sub button_prev_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button_prev.Click, PreviousToolStripMenuItem.Click
        anterior()
    End Sub

    Private Sub anterior()
        resettitems()
        Dim sender As Object = Nothing
        Dim e As System.EventArgs = Nothing
        If ListBox1.Items.Count = 0 Then
            Exit Sub
        End If
        If ListBox1.SelectedIndex = 0 Then
            Exit Sub
        End If
        clicada = clicada - 1
        ListBox1.SetSelected(clicada, True)
        ListBox1_DoubleClick(sender, e)
    End Sub
    Private Sub ListBox1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ListBox1.DoubleClick
        Try
            For i = 0 To ListBox1.Items.Count - 1
                If CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Contains("►") Then
                    CType(ListBox1.Items(i), listbox_item_data).nomedovideo = CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Replace("►", "")
                    ListBox1.Invalidate()
                End If
            Next
        Catch ex As Exception
        End Try
        clicada = ListBox1.SelectedIndex
        If Not File.Exists(CType(ListBox1.SelectedItem, listbox_item_data).linkdovideo) And Not CType(ListBox1.SelectedItem, listbox_item_data).linkdovideo.StartsWith("http://") Then
            MsgBox(ficheirofail)
            ListBox1.Items.Remove(ListBox1.SelectedItem)
            clicada -= 1
            seguinte()
            Exit Sub
        End If
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {CType(ListBox1.SelectedItem, listbox_item_data).linkdovideo, CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo})
            ListBox1.TopIndex = ListBox1.SelectedIndex
        End If
        Try
            CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo = "►" & CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo
            ListBox1.Invalidate()
        Catch ex As Exception
        End Try
    End Sub
#End Region

#Region "Volume e barra de progresso"
    Private Sub progressotrackbar_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles progressotrackbar.MouseUp
        Me.ActiveControl = Nothing
        If player.MplayerRunning = True Then
            If estado = Estadoenum.pause Then
                button_play.Image = My.Resources.play
                playpausetaskbar.Icon = My.Resources.playtaskbar
                estado = Estadoenum.play
            End If
            player.MovePosition(progressotrackbar.Value)
        End If
    End Sub

    Private Sub progressotrackbar_ValueChanged(ByVal sender As Object, ByVal value As Decimal) Handles progressotrackbar.ValueChanged
        If listento.Text <> playing Then
            listento.Text = playing
            player.Volume(volumebar.Value)
        End If
    End Sub
    Private Sub volumebar_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles volumebar.MouseUp
        Me.Focus()
        listento.Text = playing
        If player.MplayerRunning = True Then
            player.Volume(volumebar.Value)
        End If
    End Sub
    Private Sub volumebar_ValueChanged(ByVal sender As System.Object, ByVal value As System.Decimal) Handles volumebar.ValueChanged
        listento.Text = msgvolume & volumebar.Value & "%"
    End Sub
#End Region

#Region "botao opçoes"
    Private Sub botaoopt_Click(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles botaoopt.Click
        menuoptions.Show(botaoopt, New Point(e.X, e.Y))
    End Sub
#End Region

#Region "lista de ficherios que podem ser reproduzidos"
    Private filetype As New ArrayList
    Private Sub tiposdeficheiros()
        filetype.Add(".aac")
        filetype.Add(".ac3")
        filetype.Add(".aiff")
        filetype.Add(".ape")
        filetype.Add(".dts")
        filetype.Add(".fla")
        filetype.Add(".flac")
        filetype.Add(".m4a")
        filetype.Add(".mka")
        filetype.Add(".mp+")
        filetype.Add(".mp1")
        filetype.Add(".mp2")
        filetype.Add(".mp3")
        filetype.Add(".mpa")
        filetype.Add(".mpc")
        filetype.Add(".mpp")
        filetype.Add(".mpg")
        filetype.Add(".nsa")
        filetype.Add(".ogg")
        filetype.Add(".shn")
        filetype.Add(".spx")
        filetype.Add(".tak")
        filetype.Add(".tta")
        filetype.Add(".wav")
        filetype.Add(".wma")
        filetype.Add(".wv")
        filetype.Add(".26l")
        filetype.Add(".3gp")
        filetype.Add(".asf")
        filetype.Add(".avi")
        filetype.Add(".divx")
        filetype.Add(".dv")
        filetype.Add(".evo")
        filetype.Add(".flv")
        filetype.Add(".jsv")
        filetype.Add(".m1v")
        filetype.Add(".m2p")
        filetype.Add(".m2ts")
        filetype.Add(".m2v")
        filetype.Add(".m4v")
        filetype.Add(".mkv")
        filetype.Add(".mov")
        filetype.Add(".mp4")
        filetype.Add(".mpe")
        filetype.Add(".mpeg")
        filetype.Add(".mpv")
        filetype.Add(".mqv")
        filetype.Add(".nsv")
        filetype.Add(".ogm")
        filetype.Add(".pva")
        filetype.Add(".qt")
        filetype.Add(".ra")
        filetype.Add(".rm")
        filetype.Add(".rmvb")
        filetype.Add(".ts")
        filetype.Add(".vcd")
        filetype.Add(".vfw")
        filetype.Add(".vob")
        filetype.Add(".wmv")
        filetype.Add(".webm")
        filetype.Add(".mspl")
        filetype.Add(".srt")
    End Sub
#End Region

#Region "Procura todos os ficheiros da pasta e subpastas"
    Private Function GetFiles(ByVal initial As String, ByVal ficheiros As String) As List(Of String)
        Dim result As New List(Of String)
        Dim stack As New Stack(Of String)
        stack.Push(initial)
        Do While (stack.Count > 0)
            Dim dir As String = stack.Pop
            Try
                result.AddRange(Directory.GetFiles(dir, ficheiros))
                Dim directoryName As String
                For Each directoryName In Directory.GetDirectories(dir)
                    stack.Push(directoryName)
                Next
            Catch ex As Exception
            End Try
        Loop
        Return result
    End Function
#End Region

#Region "menus"
    'adicionar ficheios
    Private Sub PictureBox12_Click(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles addbuttonall.Click
        menuadd.Show(addbuttonall, New Point(e.X, e.Y))
    End Sub

    'adiciona url's do youtube
    Private WithEvents bgworkeradd As New System.ComponentModel.BackgroundWorker
    Private Sub bgworkeradd_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgworkeradd.DoWork

        Dim input = InputBox("", linkdovideo)
        If input = "" Then
            Exit Sub
        End If
        Dim nome As String
        Me.Invoke(New statedelegate(AddressOf statetext), New Object() {verificarurl, True})
        input = youtubedownload.verificarlinkyoutube(input)
        If input = msgerro Then
            MsgBox(urlerrado)
            Exit Sub
        Else
            Me.Invoke(New statedelegate(AddressOf statetext), New Object() {recolhernome, True})
            nome = youtubedownload.videonome(input)
            Me.Invoke(New inserirparalistadelegate(AddressOf inserir_para_lista), New Object() {input, nome})
            Me.Invoke(New statedelegate(AddressOf statetext), New Object() {"", False})
            If e.Argument = True Then
                Me.Invoke(New mplayerglobaldelegate(AddressOf selecionaitem))
            End If
        End If
    End Sub

    Private Delegate Sub inserirparalistadelegate(ByVal url As String, ByVal nome As String)
    Private Sub inserir_para_lista(ByVal url As String, ByVal nome As String)
        ListBox1.Items.Add(New listbox_item_data(url, nome))
    End Sub

    Private Sub selecionaitem()
        clicada = ListBox1.Items.Count - 1
        ListBox1.SetSelected(clicada, True)
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {CType(ListBox1.SelectedItem, listbox_item_data).linkdovideo, CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo})
        End If
    End Sub

    'remover legendas
    Private Sub removerlegendas()
        If player.MplayerRunning = True Then
            player.removerlegendas()
            legendason = False
            player.MovePosition(progressotrackbar.Value - 2)
        End If
    End Sub

    Private Sub adicionar_legendas(ByVal legenda As String)
        player.inserirlegendas(legenda)
        legendason = True
        legendasv = True
    End Sub

    'carregar ficheiro
    Private Sub loadfilemenu_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadfilemenu.Click, LoadFileToolStripMenuItem1.Click, LoadFileToolStripMenuItem.Click
        Dim openFileDialog1 As New OpenFileDialog
        openFileDialog1.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyMusic
        openFileDialog1.Title = "MediaStationX"
        openFileDialog1.Filter = "Music and video files|*.aac;*.mpg;*.ac3;*.aiff;*.ape;*.dts;*.fla;*.flac;*.m4a;*.mka;*.mp+;*.mp1;*.mp2;*.mp3;*.mpa;*.mpc;*.mpp;*.nsa;*.ogg;*.shn;*.spx;*.tak;*.tta;*.wav;*.wma;*.wv;*.26l;*.3gp;*.asf;*.avi;*.bin;*.dat;*.divx;*.dv;*.evo;*.flv;*.iso;*.jsv;*.m1v;*.m2p;*.m2ts;*.m2v;*.m4v;*.mkv;*.mov;*.mp4;*.mpe;*.mpeg;*.mpv;*.mqv;*.nsv;*.ogm;*.pva;*.qt;*.ra;*.ram;*.rm;*.rmvb;*.ts;*.vcd;*.vfw;*.vob;*.wmv;*.webm;"
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = True
        openFileDialog1.Multiselect = False
        If openFileDialog1.ShowDialog() = DialogResult.OK Then
            playfile(openFileDialog1.FileName)
        Else
            Exit Sub
        End If
    End Sub

    Public Sub playfile(ByVal f As String)
        Dim MyFile As FileInfo = New FileInfo(f)
        ListBox1.Items.Add(New listbox_item_data(f, MyFile.Name.Replace(MyFile.Extension, "")))
        If Not bgwplayer.IsBusy Then
            bgwplayer.RunWorkerAsync(New Object() {f, MyFile.Name.Replace(MyFile.Extension, "")})
        End If
        clicada = ListBox1.Items.Count - 1
        ListBox1.SetSelected(clicada, True)
        Try
            For i = 0 To ListBox1.Items.Count - 1
                If CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Contains("►") Then
                    CType(ListBox1.Items(i), listbox_item_data).nomedovideo = CType(ListBox1.Items(i), listbox_item_data).nomedovideo.Replace("►", "")
                    ListBox1.Invalidate()
                End If
            Next
            CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo = "►" & CType(ListBox1.SelectedItem, listbox_item_data).nomedovideo
            ListBox1.Invalidate()
        Catch ex As Exception
        End Try
    End Sub

    'carregar video url
    Private Sub loadyoutubevideourlmenuc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadyoutubevideourlmenuc.Click, loadyoutubevideourl.Click, LoadYoutubeVideoToolStripMenuItem.Click
        bgworkeradd.RunWorkerAsync(True)
    End Sub

    'adicionar musicas alista
    Private Sub AdicionarMúsicaAListaToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AdicionarMúsicaAListaToolStripMenuItem.Click, listamenuaddfile.Click, addfile.Click, AddFileToListToolStripMenuItem.Click
        Dim openFileDialog1 As New OpenFileDialog
        openFileDialog1.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyMusic
        openFileDialog1.Title = "MediaStationX"
        openFileDialog1.Filter = "Music and video files|*.aac;*.ac3;*.aiff;*.ape;*.dts;*.fla;*.flac;*.m4a;*.mka;*.mp+;*.mp1;*.mp2;*.mp3;*.mpa;*.mpc;*.mpp;*.nsa;*.ogg;*.shn;*.spx;*.tak;*.tta;*.wav;*.wma;*.wv;*.26l;*.3gp;*.asf;*.avi;*.divx;*.dv;*.evo;*.flv;*.jsv;*.m1v;*.m2p;*.m2ts;*.m2v;*.m4v;*.mkv;*.mov;*.mp4;*.mpe;*.mpeg;*.mpv;*.mqv;*.nsv;*.ogm;*.pva;*.qt;*.ra;*.ram;*.rm;*.rmvb;*.ts;*.vcd;*.vfw;*.vob;*.wmv;*.mspl"
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = True
        openFileDialog1.Multiselect = True
        If openFileDialog1.ShowDialog() = DialogResult.OK Then

            For Each fi As String In openFileDialog1.FileNames
                Dim MyFile As FileInfo = New FileInfo(fi)
                If MyFile.Extension = ".mspl" Then
                    abrir_lista(openFileDialog1.FileName)
                Else
                    ListBox1.Items.Add(New listbox_item_data(fi, MyFile.Name.Replace(MyFile.Extension, "")))
                End If
            Next
        Else
            Exit Sub
        End If
    End Sub

    'adicionar video a lista
    Private Sub addtolistyouurl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles addtolistyouurl.Click, listamenuaddurlyoutube.Click, addyoutubeurl.Click, AddYoutubeUrlToListToolStripMenuItem.Click
        bgworkeradd.RunWorkerAsync(False)
    End Sub

    'carregar legendas
    Private Sub loadsubmenu_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles loadsubmenu.Click, LoadSubtitlesToolStripMenuItem.Click, LoadSubtitleToolStripMenuItem.Click
        Dim open As New OpenFileDialog
        open.Filter = "Subtitles|*.srt;"
        If open.ShowDialog = Windows.Forms.DialogResult.OK Then
            player.inserirlegendas(open.FileName)
            legendasv = True
            legendason = True
        End If
    End Sub

    'remover legendas
    Private Sub removesubs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles removesubs.Click, RemoveSubtitlesToolStripMenuItem.Click, RemoveSubtitleToolStripMenuItem.Click
        If player.MplayerRunning = True Then
            removerlegendas()
            SubtitlesToolStripMenuItem1.DropDownItems.Item(0).Image = My.Resources.checkbox
            SubtitleToolStripMenuItem.DropDownItems.Item(0).Image = My.Resources.checkbox
        End If
    End Sub

    'alinhar legendas
    'top
    Private Sub subaltop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles subaltop.Click, TopToolStripMenuItem.Click
        If player.MplayerRunning = True Then
            player.subpos(0)
            poslegendas = 0
        End If
    End Sub

    'center
    Private Sub subaligncenter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles subaligncenter.Click, CenterToolStripMenuItem.Click
        If player.MplayerRunning = True Then
            player.subpos(50)
            poslegendas = 50
        End If
    End Sub

    'bottom
    Private Sub subalignbottom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles subalignbottom.Click, BottomToolStripMenuItem.Click
        If player.MplayerRunning = True Then
            player.subpos(100)
            poslegendas = 100
        End If
    End Sub

    'sub pos -
    Private Sub SubPositionToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SubPositionToolStripMenuItem.Click, SubPositionToolStripMenuItem3.Click
        If player.MplayerRunning = True Then
            If poslegendas < 100 Then
                poslegendas -= 2
                player.subpos(poslegendas)
            End If
        End If
    End Sub

    'subpos +
    Private Sub SubPositionToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SubPositionToolStripMenuItem1.Click, SubPositionToolStripMenuItem2.Click
        If player.MplayerRunning = True Then
            If poslegendas > 0 Then
                poslegendas += 2
                player.subpos(poslegendas)
            End If
        End If
    End Sub

    'legendas atrasadas/certas
    '+
    Private Sub subdelaymais_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles subdelaymais.Click, SubDelayToolStripMenuItem1.Click
        If player.MplayerRunning = True Then
            If player.MplayerRunning = True Then
                player.subdelay(progressotrackbar.Value + 2)
            End If
        End If
    End Sub
    '-
    Private Sub subdelaymenos_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles subdelaymenos.Click, SubDelayToolStripMenuItem2.Click
        If player.MplayerRunning = True Then
            If player.MplayerRunning = True Then
                player.subdelay(progressotrackbar.Value - 2)
            End If
        End If
    End Sub

    'tamanho das legendas
    '+
    Private Sub fontscalemais_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles fontscalemais.Click, FontToolStripMenuItem.Click
        If player.MplayerRunning = True Then
            legendatamanho += 1.0
            player.subscale(legendatamanho)
        End If
    End Sub

    '-
    Private Sub fontscalemenos_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles fontscalemenos.Click, FontToolStripMenuItem1.Click
        If player.MplayerRunning = True Then
            legendatamanho -= 1
            player.subscale(legendatamanho)
        End If
    End Sub

    'hide/show legendas
    Private Sub hidessubs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles hidessubs.Click, HideShowSubtitlesToolStripMenuItem.Click
        If player.MplayerRunning = True Then
            If legendasv = True Then
                player.visibilidade(0)
                legendasv = False
            Else
                player.visibilidade(1)
                legendasv = True
            End If
        End If
    End Sub

    'eliminar item da lista de musicas
    Private Sub listamenudeleteitem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles listamenudeleteitem.Click
        ListBox1.Items.Remove(ListBox1.SelectedItem)
    End Sub

    'add item 
    Private Sub ToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem1.Click
        If ListBox1.SelectedIndex <> -1 Then
            tools.addlink(CType(ListBox1.SelectedItem, listbox_item_data).linkdovideo)
        End If
        If tools.Visible = False Then
            tools.Show()
        End If
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click, ExitToolStripMenuItem1.Click
        Me.Close()
    End Sub
    Private Sub MplayerLogToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MplayerLogToolStripMenuItem.Click
        mplayerlog.Show()
    End Sub

    Private Sub ConverterDownloadToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ConverterDownloadToolStripMenuItem.Click
        tools.Show()
    End Sub

    Private Sub DownloadSubtitlesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DownloadSubtitlesToolStripMenuItem.Click, downloadsubsmenu.Click
        If url <> "" Then
            downloadsubs.file = url
            Dim hash As Byte() = ComputeHash(url)
            downloadsubs.hash = ToHexadecimal(hash)
            downloadsubs.Show()
        End If
    End Sub

    Private Sub searchsub_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles searchsub.Click
        Dim open As New OpenFileDialog
        If open.ShowDialog() = Windows.Forms.DialogResult.OK Then
            downloadsubs.file = open.FileName
            Dim hash As Byte() = ComputeHash(open.FileName)
            downloadsubs.hash = ToHexadecimal(hash)
            Me.Invoke(New mplayerglobaldelegate(AddressOf downloadsubs.ShowDialog))
        End If
    End Sub

    Private Sub sendcomment_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles sendcomment.Click
        Sendbug.Show()
    End Sub

    Private Sub AboutToolStripMenuItem1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem1.Click
        Aboutform.Show()
    End Sub

    Private Sub PreferencesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PreferencesToolStripMenuItem.Click, preferencesmenu.Click
        Settings.ShowDialog()
    End Sub
#End Region

#Region "argumentos ao inicar o programa"
    Public Delegate Sub argumentosdelegate(ByVal argsss As String)
    Public Sub argumentos(ByVal Arg As String)
        Try
            If Arg.ToLower.Contains("-add") Then
                If Arg.Substring(Arg.IndexOf("-add") + 4).EndsWith(".mspl") Then
                    abrir_lista(Arg.Substring(Arg.IndexOf("-add") + 4))
                    Me.Show()
                Else
                    Dim MyFile As FileInfo = New FileInfo(Arg.Substring(Arg.IndexOf("-add") + 4))
                    ListBox1.Items.Add(New listbox_item_data(Arg.Substring(Arg.IndexOf("-add") + 4), MyFile.Name.Replace(MyFile.Extension, "")))
                    Me.Show()
                End If
            ElseIf Arg.ToLower.Contains("-play") Then
                playfile(Arg.Substring(Arg.IndexOf("-play") + 5))
                Me.Show()
            ElseIf Arg.ToLower.Contains("-fs") Then
                Dim e As EventArgs = Nothing
                Dim sender As Object = Nothing
                fullc = False
                botaofullsc_Click(sender, e)
                Me.Show()
            End If
        Catch ex As Exception
        End Try
    End Sub
#End Region

#Region "teclas de atalho"
    Public Const WM_HOTKEY As Integer = &H312
    Public Declare Function RegisterHotKey Lib "user32" (ByVal hwnd As IntPtr, ByVal id As Integer, ByVal fsModifiers As Integer, ByVal vk As Integer) As Integer
    Public Declare Function UnregisterHotKey Lib "user32" (ByVal hwnd As IntPtr, ByVal id As Integer) As Integer

    Private Sub carregar_teclas()
        Call RegisterHotKey(Me.Handle, 0, 0, Keys.MediaNextTrack)
        Call RegisterHotKey(Me.Handle, 1, 0, Keys.MediaPlayPause)
        Call RegisterHotKey(Me.Handle, 2, 0, Keys.MediaPreviousTrack)
        Call RegisterHotKey(Me.Handle, 3, 0, Keys.MediaStop)
        Call RegisterHotKey(Me.Handle, 4, 0, Keys.VolumeDown)
        Call RegisterHotKey(Me.Handle, 5, 0, Keys.VolumeUp)
    End Sub

    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Dim e As EventArgs = Nothing
        Dim sender As Object = Nothing
        If m.Msg = WM_HOTKEY Then
            If m.LParam = 11730944 Then
                'play
                button_play_Click(sender, e)
            ElseIf m.LParam = 11534336 Then
                'Next
                seguinte()
            ElseIf m.LParam = 11599872 Then
                'prev
                anterior()
            ElseIf m.LParam = 11665408 Then
                'stop
                button_stop_Click(sender, e)
            ElseIf m.LParam = 11403264 Then
                volumebar.Value -= 4
                player.Volume(volumebar.Value)
            ElseIf m.LParam = 11468800 Then
                volumebar.Value += 4
                player.Volume(volumebar.Value)
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    'clique no enter para pesquisar
    Private Sub TextBox1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter And pesquisarbackgorundworker.IsBusy = False Then
            pesquisarbutton_Click(sender, e)
            Me.Focus()
        End If
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        index = 10
    End Sub

    Private Sub TextBox1_click(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.Click
        TextBox1.Focus()
    End Sub

    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.F Then
            botaofullsc_Click(sender, e)
        ElseIf e.KeyCode = Keys.Escape Then
            If fullc = True Then
                botaofullsc_Click(sender, e)
            End If
        ElseIf e.KeyCode = Keys.Space Then
            button_play_Click(sender, e)
        ElseIf e.KeyCode = Keys.N Then
            seguinte()
        ElseIf e.KeyCode = Keys.P Then
            anterior()
        ElseIf e.KeyCode = Keys.PageUp Then
            volumebar.Value += 4
            player.Volume(volumebar.Value)
        ElseIf e.KeyCode = Keys.PageDown Then
            volumebar.Value -= 4
            player.Volume(volumebar.Value)
        ElseIf e.KeyCode = Keys.Left Then
            If player.MplayerRunning = True Then
                player.Seek(-4, Seek.Absolute)
            End If
        ElseIf e.KeyCode = Keys.Right Then
            If player.MplayerRunning = True Then
                player.Seek(+4, Seek.Absolute)
            End If
        ElseIf e.KeyCode = Keys.S Then
            If player.MplayerRunning = True Then
                hidessubs_Click(sender, e)
            End If
        End If
    End Sub

    Private Sub TextBox1_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.MouseEnter
        KeyPreview = False
    End Sub

    Private Sub TextBox1_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles TextBox1.MouseLeave
        KeyPreview = True
        TextBox1.Enabled = False
    End Sub

    Private Sub Form1_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        If fullc = True Then
            Try
                If e.Delta < 0 Then
                    volumebar.Value -= 4
                    player.Volume(volumebar.Value)
                ElseIf e.Delta > 0 Then
                    volumebar.Value += 4
                    player.Volume(volumebar.Value)
                End If
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub Panel1_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles painelyoutube.MouseWheel, GroupBox1.MouseWheel, GroupBox2.MouseWheel, _
        GroupBox3.MouseWheel, GroupBox4.MouseWheel, GroupBox5.MouseWheel, GroupBox6.MouseWheel, GroupBox7.MouseWheel, GroupBox8.MouseWheel, GroupBox10.MouseWheel, Label1.MouseWheel, Label2.MouseWheel, _
        Label3.MouseWheel, Label4.MouseWheel, Label5.MouseWheel, Label6.MouseWheel, Label7.MouseWheel, Label8.MouseWheel, Label9.MouseWheel, Label10.MouseWheel, add1.MouseWheel, add2.MouseWheel, _
        add3.MouseWheel, add4.MouseWheel, add5.MouseWheel, add6.MouseWheel, add7.MouseWheel, add8.MouseWheel, add9.MouseWheel, add10.MouseWheel, download1.MouseWheel, download2.MouseWheel, _
        download3.MouseWheel, download4.MouseWheel, download5.MouseWheel, download6.MouseWheel, download7.MouseWheel, download8.MouseWheel, download9.MouseWheel, download10.MouseWheel
        Try
            If e.Delta < 0 Then
                painelyoutube.VerticalScroll.Value += 3
            ElseIf e.Delta > 0 Then
                painelyoutube.VerticalScroll.Value -= 3
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Panel1_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles painelyoutube.MouseEnter, GroupBox1.MouseEnter, GroupBox2.MouseEnter, GroupBox3.MouseEnter, _
      GroupBox4.MouseEnter, GroupBox5.MouseEnter, GroupBox6.MouseEnter, GroupBox7.MouseEnter, GroupBox8.MouseEnter, GroupBox9.MouseEnter, GroupBox10.MouseEnter, PictureBox1.MouseEnter, _
      PictureBox2.MouseEnter, PictureBox2.MouseEnter, PictureBox3.MouseEnter, PictureBox4.MouseEnter, PictureBox5.MouseEnter, PictureBox6.MouseEnter, PictureBox7.MouseEnter, PictureBox8.MouseEnter, PictureBox9.MouseEnter, PictureBox10.MouseEnter, _
        add1.MouseEnter, add2.MouseEnter, add3.MouseEnter, add4.MouseEnter, add5.MouseEnter, add6.MouseEnter, add7.MouseEnter, add8.MouseEnter, add9.MouseEnter, add10.MouseEnter, download1.MouseEnter, download2.MouseEnter, _
        download3.MouseEnter, download4.MouseEnter, download5.MouseEnter, download6.MouseEnter, download7.MouseEnter, download8.MouseEnter, download9.MouseEnter, download10.MouseEnter, ProgressBar1.MouseEnter, PictureBox11.MouseEnter
        painelyoutube.Focus()
        TextBox1.Enabled = True
    End Sub

    Private Sub Panel1_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles painelyoutube.MouseLeave, GroupBox1.MouseLeave, GroupBox2.MouseLeave, GroupBox3.MouseLeave, _
        GroupBox4.MouseLeave, GroupBox5.MouseLeave, GroupBox6.MouseLeave, GroupBox7.MouseLeave, GroupBox8.MouseLeave, GroupBox9.MouseLeave, GroupBox10.MouseLeave, PictureBox1.MouseLeave, _
        PictureBox2.MouseLeave, PictureBox2.MouseLeave, PictureBox3.MouseLeave, PictureBox4.MouseLeave, PictureBox5.MouseLeave, PictureBox6.MouseLeave, PictureBox7.MouseLeave, PictureBox8.MouseLeave, PictureBox9.MouseLeave, PictureBox10.MouseLeave, _
        add1.MouseLeave, add2.MouseLeave, add3.MouseLeave, add4.MouseLeave, add5.MouseLeave, add6.MouseLeave, add7.MouseLeave, add8.MouseLeave, add9.MouseLeave, add10.MouseLeave, download1.MouseLeave, download2.MouseLeave, _
       download3.MouseLeave, download4.MouseLeave, download5.MouseLeave, download6.MouseLeave, download7.MouseLeave, download8.MouseLeave, download9.MouseLeave, download10.MouseLeave, ProgressBar1.MouseLeave, PictureBox11.MouseLeave
        Me.ActiveControl = Nothing
    End Sub

#End Region

#Region "scrollbar lista"
    Private Sub ListBox1_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles ListBox1.MouseWheel
        Try
            If e.Delta < 0 Then
                painellista.VerticalScroll.Value += 6
            ElseIf e.Delta > 0 Then
                painellista.VerticalScroll.Value -= 6
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Panel2_MouseEnter(sender As Object, e As System.EventArgs) Handles painellista.MouseEnter, ListBox1.MouseEnter
        ListBox1.Focus()
    End Sub

    Private Sub Panel2_MouseLeave(sender As Object, e As System.EventArgs) Handles painellista.MouseLeave, ListBox1.MouseLeave
        Me.ActiveControl = Nothing
    End Sub
#End Region

#Region "tamanho do video"
    Private Sub delete_checkimage_item()
        aspect_auto_menu.Image = Nothing
        aspect_auto_c.Image = Nothing
        aspect_1_1_menu.Image = Nothing
        aspect_1_1_c.Image = Nothing
        aspect_3_2_menu.Image = Nothing
        aspect_3_2_c.Image = Nothing
        aspect_4_3_menu.Image = Nothing
        aspect_4_3_c.Image = Nothing
        aspect_5_4_menu.Image = Nothing
        aspect_5_4_c.Image = Nothing
        aspect_14_9_menu.Image = Nothing
        aspect_14_9_c.Image = Nothing
        aspect_14_10_menu.Image = Nothing
        aspect_16_9_menu.Image = Nothing
        aspect_16_9.Image = Nothing
        aspect_16_10_menu.Image = Nothing
        aspect_16_10_c.Image = Nothing
        aspect_2_35_1.Image = Nothing
        aspect_2_35_1_c.Image = Nothing
    End Sub

    Private Sub aspect_auto_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_auto_menu.Click, aspect_auto_c.Click
        delete_checkimage_item()
        aspect_auto_menu.Image = My.Resources.checkbox
        aspect_auto_c.Image = My.Resources.checkbox
        player.aspect_ratio("0")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_1_1_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_1_1_menu.Click, aspect_1_1_c.Click
        delete_checkimage_item()
        aspect_1_1_menu.Image = My.Resources.checkbox
        aspect_1_1_c.Image = My.Resources.checkbox
        player.aspect_ratio("1:1")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_3_2_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_3_2_menu.Click, aspect_3_2_c.Click
        delete_checkimage_item()
        aspect_3_2_menu.Image = My.Resources.checkbox
        aspect_3_2_c.Image = My.Resources.checkbox
        player.aspect_ratio("3:2")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_4_3_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_4_3_menu.Click, aspect_4_3_c.Click
        delete_checkimage_item()
        aspect_4_3_menu.Image = My.Resources.checkbox
        aspect_4_3_c.Image = My.Resources.checkbox
        player.aspect_ratio("4:3")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_5_4_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_5_4_menu.Click, aspect_5_4_c.Click
        delete_checkimage_item()
        aspect_5_4_menu.Image = My.Resources.checkbox
        aspect_5_4_c.Image = My.Resources.checkbox
        player.aspect_ratio("5:4")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_14_9_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_14_9_menu.Click, aspect_14_9_c.Click
        delete_checkimage_item()
        aspect_14_9_menu.Image = My.Resources.checkbox
        aspect_14_9_c.Image = My.Resources.checkbox
        player.aspect_ratio("14:9")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_14_10_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_14_10_menu.Click, aspect_16_10_c.Click
        delete_checkimage_item()
        aspect_14_10_menu.Image = My.Resources.checkbox
        aspect_16_10_c.Image = My.Resources.checkbox
        player.aspect_ratio("14:10")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_16_9_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_16_9_menu.Click, aspect_16_9.Click
        delete_checkimage_item()
        aspect_16_9_menu.Image = My.Resources.checkbox
        aspect_16_9.Image = My.Resources.checkbox
        player.aspect_ratio("16:9")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_16_10_menu_Click(sender As System.Object, e As System.EventArgs) Handles aspect_16_10_menu.Click, aspect_16_10_c.Click
        delete_checkimage_item()
        aspect_16_10_menu.Image = My.Resources.checkbox
        aspect_16_10_c.Image = My.Resources.checkbox
        player.aspect_ratio("16:10")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub

    Private Sub aspect_2_35_1_Click(sender As System.Object, e As System.EventArgs) Handles aspect_2_35_1.Click, aspect_2_35_1_c.Click
        delete_checkimage_item()
        aspect_2_35_1.Image = My.Resources.checkbox
        aspect_2_35_1_c.Image = My.Resources.checkbox
        player.aspect_ratio("2.35:1")
        If player.FullScreen = False Then
            player.ToggleFullScreen()
        End If
    End Sub
#End Region

#Region "settings"
    Private Sub ler_settings()
        Try
            If Not File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml") Then
                painelyou = True
                painellist = True
                volumebar.Value = 100
                fonttype = Application.StartupPath & "\mplayer\mplayer\subfont.ttf"
                legendatamanho = 2.5
                poslegendas = 100
                idioma = Application.StartupPath & "\settings\language\en.xml"
                WindowState = FormWindowState.Normal
                Settings.Show()
                Settings.TopMost = True
                Exit Sub
            Else
                Dim xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
                If xml...<window>.Value = 1 Then
                    WindowState = FormWindowState.Normal
                Else
                    WindowState = xml...<window>.Value
                End If
                volumebar.Value = xml...<volume>.Value
                painelyou = xml...<youtubepanel>.Value
                fonttype = xml...<font>.Value
                poslegendas = xml...<poslegendas>.Value
                legendatamanho = xml...<legendastamanho>.Value
                If legendatamanho = 0.0 Then
                    legendatamanho = 2.5
                End If
                codlegendas = xml...<codlegendas>.Value
                If painelyou = False Then
                    painelplayer.Left = 0
                    painelplayer.Width = painelplayer.Width + painelyoutube.Width
                    painelyoutube.Left = -painelyoutube.Width
                    youtubemenuvideos.Image = My.Resources.paineldireita
                End If
                painellist = xml...<listbox>.Value
                If painellist = False Then
                    painelplayer.Width = painelplayer.Width + painellista.Width
                    painellista.Left = Me.Width - 15
                    listamenu.Image = My.Resources.painelesquerda
                End If
                estadomsn = xml...<pluginmsn>.Value
                idioma = xml...<idioma>.Value
                downloadlegendas = xml...<legendasdown>.Value
                youtubedownload.qualidadevideo = xml...<qualidadevideos>.Value
            End If
        Catch ex As Exception
        End Try
    End Sub

    'variaveis do idioma
    Private caixaembranco, falhapesquisa, semresultados, recolherurl, playing, msgvolume, linkdovideo, verificarurl, _
      msgerro, urlerrado, recolhernome, ficheirofail, nosubs, updatemsg, updatemsgfail, updatemsgon, canal1 As String
    Public semnet As String

    Public Sub idiomaxml()
        Try
            Dim xml = XDocument.Load(idioma)
            'labels 
            listento.Text = xml...<labelreproduzir>.Value 'label playing
            playing = listento.Text
            lbl_savelist.Text = xml...<labelguardarlista>.Value ' label guardar lista
            lbl_clearlist.Text = xml...<labellimpar>.Value ' label limpar lista
            lbl_openlist.Text = xml...<labelabrirlista>.Value ' label abrir lista
            infofonts.Label1.Text = xml...<scanfontswait>.Value
            'menu
            'menu abrir
            MenuStrip1.Items.Item(0).Text = xml...<menufile>.Value
            'loadfile
            loadfilemenu.Text = xml...<menucarregarficheiro>.Value
            LoadFileToolStripMenuItem1.Text = loadfilemenu.Text
            LoadFileToolStripMenuItem.Text = loadfilemenu.Text
            'loadvideourl
            loadyoutubevideourlmenuc.Text = xml...<menucarregaryoutube>.Value
            loadyoutubevideourl.Text = loadyoutubevideourlmenuc.Text
            LoadYoutubeVideoToolStripMenuItem.Text = loadyoutubevideourlmenuc.Text
            'add fileto list
            AdicionarMúsicaAListaToolStripMenuItem.Text = xml...<addmusicatolist>.Value
            listamenuaddfile.Text = AdicionarMúsicaAListaToolStripMenuItem.Text
            addfile.Text = AdicionarMúsicaAListaToolStripMenuItem.Text
            AddFileToListToolStripMenuItem.Text = AdicionarMúsicaAListaToolStripMenuItem.Text
            'add video url to list
            addtolistyouurl.Text = xml...<addyoutubetolist>.Value
            listamenuaddurlyoutube.Text = addtolistyouurl.Text
            addyoutubeurl.Text = addtolistyouurl.Text
            AddYoutubeUrlToListToolStripMenuItem.Text = addtolistyouurl.Text
            'exit
            ExitToolStripMenuItem.Text = xml...<exit>.Value
            ExitToolStripMenuItem1.Text = ExitToolStripMenuItem.Text
            'menu playing
            MenuStrip1.Items.Item(1).Text = xml...<playing>.Value
            PlayToolStripMenuItem.Text = xml...<playpause>.Value
            StopToolStripMenuItem.Text = xml...<stop>.Value
            PreviousToolStripMenuItem.Text = xml...<previous>.Value
            NextToolStripMenuItem.Text = xml...<next>.Value
            'menu audio
            MenuStrip1.Items.Item(2).Text = xml...<menuaudio>.Value
            'menu video
            MenuStrip1.Items.Item(3).Text = xml...<menuvideo>.Value
            aspect_ratio_menu.Text = xml...<videoaspect>.Value
            aspect_ratio_c.Text = xml...<videoaspect>.Value
            Video_c.Text = xml...<menuvideo>.Value
            'menu subtitles
            MenuStrip1.Items.Item(4).Text = xml...<subtitlesmenu>.Value
            SubtitlesToolStripMenuItem1.Text = xml...<subtitlesmenu>.Value
            SubtitleToolStripMenuItem.Text = xml...<subtitlesmenu>.Value
            nosubs = xml...<semlegendas>.Value
            'load subtitles
            LoadSubtitlesToolStripMenuItem.Text = xml...<loadsub>.Value
            loadsubmenu.Text = LoadSubtitlesToolStripMenuItem.Text
            LoadSubtitleToolStripMenuItem.Text = LoadSubtitlesToolStripMenuItem.Text
            'remover subs
            removesubs.Text = xml...<removesubs>.Value
            RemoveSubtitlesToolStripMenuItem.Text = removesubs.Text
            RemoveSubtitleToolStripMenuItem.Text = removesubs.Text
            'sub align
            SubAlignmentToolStripMenuItem.Text = xml...<subaling>.Value
            subalign.Text = SubAlignmentToolStripMenuItem.Text
            'top
            subaltop.Text = xml...<subtop>.Value
            TopToolStripMenuItem.Text = subaltop.Text
            'center
            subaligncenter.Text = xml...<subcenter>.Value
            CenterToolStripMenuItem.Text = subaligncenter.Text
            'bottom
            subalignbottom.Text = xml...<subbottom>.Value
            BottomToolStripMenuItem.Text = subalignbottom.Text
            'subposmais
            SubPositionToolStripMenuItem1.Text = xml...<subposmais>.Value
            SubPositionToolStripMenuItem2.Text = SubPositionToolStripMenuItem1.Text
            'sub pos maneos
            SubPositionToolStripMenuItem.Text = xml...<subposmenos>.Value
            SubPositionToolStripMenuItem3.Text = SubPositionToolStripMenuItem.Text
            'sub delay
            SubDelayToolStripMenuItem.Text = xml...<subdelay>.Value
            Subdelay.Text = SubDelayToolStripMenuItem.Text
            'sub delay +
            subdelaymais.Text = xml...<subdelaymais>.Value
            SubDelayToolStripMenuItem1.Text = subdelaymais.Text
            'subdelay -
            subdelaymenos.Text = xml...<subdelaymenos>.Value
            SubDelayToolStripMenuItem2.Text = subdelaymenos.Text
            'font scale
            FontScaleToolStripMenuItem.Text = xml...<fontscale>.Value
            fontssubsscale.Text = FontScaleToolStripMenuItem.Text
            'font mais
            fontscalemais.Text = xml...<fontmais>.Value
            FontToolStripMenuItem.Text = fontscalemais.Text
            'font menos
            fontscalemenos.Text = xml...<fontmenos>.Value
            FontToolStripMenuItem1.Text = fontscalemenos.Text
            'hideor show
            hidessubs.Text = xml...<hssubs>.Value
            HideShowSubtitlesToolStripMenuItem.Text = hidessubs.Text
            'download legendas
            DownloadSubtitlesToolStripMenuItem.Text = xml...<downloadsubs>.Value
            downloadsubsmenu.Text = xml...<downloadsubs>.Value
            searchsub.Text = xml...<downloadsubfile>.Value
            'menu options
            MenuStrip1.Items.Item(5).Text = xml...<options>.Value
            ToolStripMenuItem1.Text = xml...<converterdownload>.Value
            PreferencesToolStripMenuItem.Text = xml...<preferences>.Value
            preferencesmenu.Text = PreferencesToolStripMenuItem.Text
            'menu about
            MenuStrip1.Items.Item(6).Text = xml...<about>.Value
            MplayerLogToolStripMenuItem.Text = xml...<mplayerlog>.Value
            sendcomment.Text = xml...<formsendrelatorios>.Value
            AboutToolStripMenuItem1.Text = xml...<about>.Value
            Aboutform.Text = xml...<about>.Value
            Aboutform.Label1.Text = xml...<abouttext>.Value
            Aboutform.Label2.Text = xml...<mywebsite>.Value
            Aboutform.Label3.Text = xml...<othersite>.Value
            'menu update
            Updatemenu.Text = xml...<update>.Value
            'menu lista
            ToolStripMenuItem1.Text = xml...<downloadvideo>.Value
            listamenudeleteitem.Text = xml...<deleteitem>.Value
            'hide/show paineis
            'youtube video
            youtubemenuvideos.Text = xml...<youtubepanel>.Value
            HideShowYoutubeVideosToolStripMenuItem.Text = youtubemenuvideos.Text
            'lista de musicas/videos
            listamenu.Text = xml...<listapanel>.Value
            hideslistmenu.Text = listamenu.Text
            mplayerlog.Button1.Text = xml...<mpalyerlogclear>.Value
            Sendbug.Label1.Text = xml...<formsendrelatorios>.Value
            Sendbug.Button1.Text = xml...<sendrelatorios>.Value
            'tooltip
            'play.prev,next
            Me.ToolTip1.SetToolTip(Me.button_prev, xml...<toolprevious>.Value)
            Me.ToolTip1.SetToolTip(Me.button_stop, xml...<toolstop>.Value)
            Me.ToolTip1.SetToolTip(Me.button_play, xml...<toolplay>.Value)
            Me.ToolTip1.SetToolTip(Me.button_next, xml...<toolnext>.Value)
            'trackbar e labels tempo
            Me.ToolTip1.SetToolTip(Me.volumebar, xml...<toolvolume>.Value)
            Me.ToolTip1.SetToolTip(Me.progressotrackbar, xml...<toolprogresso>.Value)
            Me.ToolTip1.SetToolTip(Me.lbl_pos, xml...<tooltempoprogresso>.Value)
            Me.ToolTip1.SetToolTip(Me.lbl_totalpos, xml...<tooltempototal>.Value)
            'outros botoes
            Me.ToolTip1.SetToolTip(Me.shufflle_button, xml...<toolaleatorio>.Value)
            Me.ToolTip1.SetToolTip(Me.botaoopt, xml...<toolopcoes>.Value)
            Me.ToolTip1.SetToolTip(Me.botaofullsc, xml...<toolfull>.Value)
            'labels save, open clear
            Me.ToolTip1.SetToolTip(Me.lbl_savelist, xml...<toolsave>.Value)
            Me.ToolTip1.SetToolTip(Me.lbl_openlist, xml...<toolopen>.Value)
            Me.ToolTip1.SetToolTip(Me.lbl_clearlist, xml...<toolclear>.Value)
            Me.ToolTip1.SetToolTip(Me.imagemalbum, xml...<a>.Value)
            Me.ToolTip1.SetToolTip(Me.pesquisarbutton, xml...<toolpes>.Value)
            Me.ToolTip1.SetToolTip(Me.pesquisaresquerda, xml...<toolpesprev>.Value)
            Me.ToolTip1.SetToolTip(Me.pesquisardireita, xml...<toolpesnext>.Value)
            'mensagens de erro entre outras
            semnet = xml...<semnet>.Value
            caixaembranco = xml...<caixaembranco>.Value
            falhapesquisa = xml...<falhapesquisa>.Value
            semresultados = xml...<semresultados>.Value
            recolherurl = xml...<recolherurl>.Value
            msgvolume = xml...<msgvolume>.Value
            linkdovideo = xml...<msglinkvideo>.Value
            verificarurl = xml...<msgverificarurl>.Value
            msgerro = xml...<msgerro>.Value
            urlerrado = xml...<msgurlerrado>.Value
            recolhernome = xml...<recolhernome>.Value
            recolhernome = xml...<recolhernome>.Value
            ficheirofail = xml...<ficheirofail>.Value
            updatemsg = xml...<updatecheck>.Value
            updatemsgfail = xml...<updatefail>.Value
            updatemsgon = xml...<updatemsg>.Value
            canal1 = xml...<audiochannel1>.Value
        Catch ex As Exception
        End Try
    End Sub

    Private Sub escrever_settings()
        Try
            Dim xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
            xml...<volume>.Value = volumebar.Value
            xml...<youtubepanel>.Value = painelyou
            xml...<listbox>.Value = painellist
            xml...<poslegendas>.Value = poslegendas
            xml...<legendastamanho>.Value = legendatamanho
            xml...<window>.Value = WindowState
            xml.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
        Catch ex As Exception
        End Try
    End Sub

#End Region

#Region "update"
    Private WithEvents checkupdatetimer As New Timer
    Private Sub checkupdate()
        Using reader As StreamReader = New StreamReader(Application.StartupPath & "\version.mxvs")
            versao = reader.ReadLine
            reader.Close()
            reader.Dispose()
        End Using
        Dim wc As New WebClient
        Dim tamanho As String
        Try
            filedownload = wc.DownloadString(New Uri("http://dl.dropbox.com/u/22494369/mediastationx/version.mxvs"))
            wc.Dispose()
            tamanho = wc.DownloadString(New Uri("http://dl.dropbox.com/u/22494369/mediastationx/tamanho.mxvs"))
            wc.Dispose()
            checkupdatetimer.Enabled = False
            If filedownload > versao Then
                If MessageBox.Show(updatemsgon, "", MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.Yes Then
                    Process.Start(Application.StartupPath & "\msxfiletype.exe", "-update")
                End If
            End If
        Catch ex As Exception
            checkupdatetimer.Enabled = False
        End Try
    End Sub

    Private Sub checkupdatetimer_Tick(sender As Object, e As System.EventArgs) Handles checkupdatetimer.Tick
        checkupdate()
    End Sub

    Private Sub Updatemenu_Click(sender As System.Object, e As System.EventArgs) Handles Updatemenu.Click
        Try
            My.Computer.Network.Ping("www.youtube.com", 500) ' verifica se o computador esta ligado a internet
        Catch ex As Exception
            MsgBox(semnet)
            Exit Sub
        End Try
        MsgBox(updatemsg)
        checkupdate()
        If filedownload = versao Then
            MsgBox(updatemsgfail)
        ElseIf versao = "" Then
            checkupdate()
        End If
    End Sub

#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        UnhandledExceptionManager.AddHandler()
        Me.Focus()
        TextBox1.Enabled = False
        carregar_teclas()
        If File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\playlist.mspl") Then
            abrir_lista(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\playlist.mspl")
        End If
        ler_settings()
        idiomaxml()
        apagar()
        listento.Text = playing
        loading.Visible = False
        playerbginit.RunWorkerAsync()
        Me.Invoke(New mplayerglobaldelegate(AddressOf resettitems))
        state.Visible = False
        tiposdeficheiros()
        Try
            For i = 0 To My.Application.CommandLineArgs.Count - 1
                argumentos(My.Application.CommandLineArgs(i))
            Next
        Catch ex As Exception
        End Try
        checkupdatetimer.Interval = 60000
        checkupdatetimer.Enabled = True
        memoria.FlushMemory()
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Try
            Me.Invoke(New mplayerglobaldelegate(AddressOf resettitems))
            player.Quit()
        Catch ex As Exception
        End Try
        If estadomsn = True Then
            msn.SendStatusMessage(False, msn.EnumCategory.Music, "")
        End If
        If ListBox1.Items.Count <> 0 Then
            guardar(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\playlist.mspl")
        End If
        escrever_settings()
    End Sub

    Private Sub HotKeysToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles HotKeysToolStripMenuItem.Click
        Try
            Dim xml = XDocument.Load(idioma)
            MsgBox(xml...<hotkeys>.Value)
        Catch ex As Exception
        End Try
    End Sub
End Class
