Imports System.Xml
Imports System.IO
Imports Microsoft.Win32

Public Class Settings
    Private msgrestart, msgerro, idiomatemp, ficheirosettingsnotsave, selecioneumitem, concluido As String
    Private qualidade, qualidadedown As Integer
    Private ficheirosass As New ArrayList
#Region "ler settings"
    Private Sub Settings_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml") Then
            MsgBox(ficheirosettingsnotsave)
            e.Cancel = True
        End If
        Me.Dispose()
        memoria.FlushMemory()
    End Sub
    Private Sub Settings_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Try
            If Not IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings") Then
                IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings")
            End If
            If Not IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml") Then
                ComboBox1.SelectedIndex = 0
                ComboBox2.SelectedIndex = 3
                ComboBox3.SelectedIndex = 0
                CheckBox1.Checked = True
                CheckBox2.Checked = True
                TextBox1.Text = Form1.fonttype
                NumericUpDown1.Value = Form1.poslegendas
                NumericUpDown2.Value = Form1.legendatamanho
                For i = 0 To ComboBox4.Items.Count - 1
                    If ComboBox4.Items.Item(i).Contains(My.Application.Culture.TextInfo.ANSICodePage) Then
                        ComboBox4.SelectedIndex = i
                    End If
                Next
                arraylangopensubtitles.Add("eng")
                arraylangsubti.Add("eng")
                ListBox1.Items.Add("English")
                For i = 0 To ListView1.Items.Count - 1
                    If ListView1.Items(i).Text.Contains(System.Globalization.CultureInfo.InstalledUICulture.EnglishName.Substring(0, System.Globalization.CultureInfo.InstalledUICulture.EnglishName.IndexOf("(") - 1)) Then
                        arraylangopensubtitles.Add(ListView1.Items(i).SubItems(1).Text)
                        arraylangsubti.Add(ListView1.Items(i).SubItems(2).Text)
                        ListBox1.Items.Add(ListView1.Items(i).Text)
                    End If
                Next
                idiomasub()
                Exit Sub
            Else
                Dim xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
                If xml...<pluginmsn>.Value = True Then
                    CheckBox1.Checked = True
                Else
                    CheckBox1.Checked = False
                End If
                If xml...<legendasdown>.Value = True Then
                    CheckBox2.Checked = True
                Else
                    CheckBox2.Checked = False
                End If
                idiomatemp = xml...<idioma>.Value
                If idiomatemp = Application.StartupPath & "\settings\language\en.xml" Then
                    ComboBox3.SelectedIndex = 0
                    ComboBox3.Text = "English"
                ElseIf idiomatemp = Application.StartupPath & "\settings\language\pt-pt.xml" Then
                    ComboBox3.SelectedIndex = 1
                    ComboBox3.Text = "português"
                ElseIf idiomatemp = Application.StartupPath & "\settings\language\spa.xml" Then
                    ComboBox3.SelectedIndex = 2
                    ComboBox3.Text = "Español"
                Else
                    ComboBox1.SelectedIndex = 0
                End If
                qualidade = xml...<qualidadevideos>.Value
                qualidadedown = xml...<qualidadedown>.Value
                verificarqualidade(qualidade, ComboBox1)
                verificarqualidade(qualidadedown, ComboBox2)
                ListBox1.Items.Clear()
                Dim lang As String = xml...<langtotal>.Value
                Dim langs As String() = Split(lang, ",")
                For Each l In langs
                    ListBox1.Items.Add(l)
                Next
                'font type
                TextBox1.Text = xml...<font>.Value
                NumericUpDown1.Value = xml...<poslegendas>.Value
                NumericUpDown2.Value = xml...<legendastamanho>.Value
                For i = 0 To ComboBox4.Items.Count - 1
                    If ComboBox4.Items(i).ToString.Contains(xml...<codlegendas>.Value) Then
                        ComboBox4.SelectedIndex = i
                    End If
                Next
                'get lang subtitles.com.br
                lang = ""
                Array.Clear(langs, 0, langs.Length)
                lang = xml...<langsubtitlescombr>.Value
                langs = Split(lang, ",")
                arraylangsubti.Clear()
                For Each l In langs
                    arraylangsubti.Add(l)
                Next
                'get lang of opensubtitles
                lang = ""
                Array.Clear(langs, 0, langs.Length)
                lang = xml...<langopensubtitles>.Value
                langs = Split(lang, ",")
                arraylangopensubtitles.Clear()
                For Each l In langs
                    arraylangopensubtitles.Add(l)
                Next
                idiomasub()
            End If
            If File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\fileass.xml") Then
                Dim xml = XDocument.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\fileass.xml")
                For Each co In xml.Descendants("audio")
                    ficheirosass.Add(co.Value)
                    For i = 0 To audiofiles.Items.Count - 1
                        If audiofiles.Items.Item(i).Text.Contains(co.Value.ToLower) Then
                            audiofiles.Items(i).Checked = True
                        End If
                    Next
                Next
                For Each co In xml.Descendants("video")
                    ficheirosass.Add(co.Value)
                    For i = 0 To videofiles.Items.Count - 1
                        If videofiles.Items(i).Text.Contains(co.Value.ToLower) Then
                            videofiles.Items(i).Checked = True
                        End If
                    Next
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub verificarqualidade(ByVal qualidade As String, ByVal combo As ComboBox)
        If qualidade = 18 Or qualidade = 34 Or qualidade = 43 Then 'qualidade 360p 
            combo.SelectedIndex = 0
        ElseIf qualidade = 35 Or qualidade = 44 Then 'qualidade 480p
            combo.SelectedIndex = 1
        ElseIf qualidade = 22 Or qualidade = 45 Then 'qualidade 720p
            combo.SelectedIndex = 2
        ElseIf qualidade = 37 Or qualidade = 46 Then 'qualidade 1080p
            combo.SelectedIndex = 3
        End If
    End Sub
    Private Sub idiomasub()
        Try
            Dim xml = XDocument.Load(Form1.idioma)
            ComboBox3.Items.Clear()
            ComboBox3.Items.Add(xml...<enidioma>.Value)
            ComboBox3.Items.Add(xml...<ptidioma>.Value)
            ComboBox3.Items.Add(xml...<spidioma>.Value)
            Button1.Text = xml...<botaoaplicar>.Value
            Me.ToolTip1.SetToolTip(Label1, xml...<tooltipmsn>.Value)
            Me.ToolTip1.SetToolTip(CheckBox1, xml...<tooltipmsn>.Value)
            Me.ToolTip1.SetToolTip(Label2, xml...<tooltipidioma>.Value)
            Me.ToolTip1.SetToolTip(ComboBox3, xml...<tooltipidioma>.Value)
            Me.ToolTip1.SetToolTip(Label5, xml...<tooltipautosub>.Value)
            Me.ToolTip1.SetToolTip(CheckBox2, xml...<tooltipautosub>.Value)
            Me.ToolTip1.SetToolTip(Label3, xml...<tooltipqualidade>.Value)
            Me.ToolTip1.SetToolTip(ComboBox1, xml...<tooltipqualidade>.Value)
            Me.ToolTip1.SetToolTip(Label4, xml...<tooltipqualdonw>.Value)
            Me.ToolTip1.SetToolTip(ComboBox2, xml...<tooltipqualdonw>.Value)
            Me.ToolTip1.SetToolTip(add1, xml...<addsublang>.Value)
            Me.ToolTip1.SetToolTip(remove, xml...<removesublang>.Value)
            Me.ToolTip1.SetToolTip(TabPage3, xml...<tooltipfont>.Value)
            Me.ToolTip1.SetToolTip(TabPage2, xml...<tooltipsubdown>.Value)
            Me.ToolTip1.SetToolTip(ListBox1, xml...<tooltipsubdown>.Value)
            Me.ToolTip1.SetToolTip(ListView1, xml...<tooltipsubdown>.Value)
            Me.ToolTip1.SetToolTip(Label10, xml...<toolasscociate>.Value)
            Me.ToolTip1.SetToolTip(audiofiles, xml...<toolasscociate>.Value)
            Me.ToolTip1.SetToolTip(videofiles, xml...<toolasscociate>.Value)
            Me.ToolTip1.SetToolTip(TabPage4, xml...<toolasscociate>.Value)
            Me.Text = xml...<preferences>.Value
            Button1.Text = xml...<botaoaplicar>.Value
            Button2.Text = xml...<botaoaplicar>.Value
            Button3.Text = xml...<botaoaplicar>.Value
            Button4.Text = xml...<botaoaplicar>.Value
            Button5.Text = xml...<checkbutton>.Value
            Button6.Text = xml...<uncheckbutton>.Value
            msgrestart = xml...<msgrestart>.Value
            msgerro = xml...<msgerro>.Value
            Label1.Text = xml...<labelmsnestado>.Value
            Label2.Text = xml...<labellang>.Value
            Label5.Text = xml...<labeldownloadsub>.Value
            Label3.Text = xml...<labelqualidadevideo>.Value
            Label4.Text = xml...<labelquadlidadedownload>.Value
            Label6.Text = xml...<tabfont>.Value & ":"
            Label7.Text = xml...<positionsub>.Value
            Label8.Text = xml...<fontscale>.Value & ":"
            Label9.Text = xml...<codificationsubs>.Value
            TabPage1.Text = xml...<taballsettings>.Value
            TabPage3.Text = xml...<tabfont>.Value
            TabPage2.Text = xml...<tablang>.Value
            TabPage4.Text = xml...<tabtypefiles>.Value
            ficheirosettingsnotsave = xml...<settingsfilenotsave>.Value
            Label10.Text = xml...<labelassociate>.Value
            audiofiles.Columns.Item(0).Text = xml...<audiotype>.Value
            videofiles.Columns.Item(0).Text = xml...<videotype>.Value
            selecioneumitem = xml...<selecioneitem>.Value
            concluido = xml...<concluido>.Value
        Catch ex As Exception
        End Try
    End Sub
#End Region
#Region "escrever settings"
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click, Button2.Click, Button3.Click
        Dim subopensubtitles As String = ""
        Dim subsubtitlecom As String = ""
        Dim langstotal As String = ""
        For i = 0 To arraylangopensubtitles.Count - 1
            subopensubtitles += arraylangopensubtitles.Item(i) + ","
        Next
        subopensubtitles.Replace(" ", "")
        For i = 0 To arraylangsubti.Count - 1
            subsubtitlecom += arraylangsubti.Item(i) + ","
        Next
        subsubtitlecom.Replace(" ", "")
        For Each item In ListBox1.Items
            langstotal += item + ","
        Next
        langstotal.Replace(" ", "")
        If subsubtitlecom = "" Then
            subsubtitlecom = ","
        End If
        If subopensubtitles = "" Then
            subopensubtitles = ","
        End If
        If langstotal = "" Then
            langstotal = ","
        End If
        Dim xmldoc As New XmlDocument
        Dim no As XmlNode
        no = xmldoc.CreateElement("setting")
        xmldoc.AppendChild(no)
        Dim RootElement As XmlElement = xmldoc.DocumentElement
        Dim Element As XmlElement
        RootElement = xmldoc.DocumentElement
        ''''volume
        Element = xmldoc.CreateElement("volume")
        Dim xmltexts As XmlText = xmldoc.CreateTextNode("100")
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        '''''pathfoler
        If Not IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml") Then
            Element = xmldoc.CreateElement("pathfolder")
            xmltexts = xmldoc.CreateTextNode(My.Computer.FileSystem.SpecialDirectories.MyMusic)
            RootElement.AppendChild(Element)
            RootElement.LastChild.AppendChild(xmltexts)
        End If
        ''''janela
        Element = xmldoc.CreateElement("window")
        xmltexts = xmldoc.CreateTextNode(Form1.WindowState)
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        '''''painel youtube true ou false
        Element = xmldoc.CreateElement("youtubepanel")
        xmltexts = xmldoc.CreateTextNode("True")
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        '''' painel lista true ou false
        Element = xmldoc.CreateElement("listbox")
        xmltexts = xmldoc.CreateTextNode("True")
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        '''''idioma
        Element = xmldoc.CreateElement("idioma")
        If ComboBox3.SelectedIndex = 0 Or ComboBox3.SelectedIndex = -1 Then
            xmltexts = xmldoc.CreateTextNode(Application.StartupPath & "\settings\language\en.xml")
        ElseIf ComboBox3.SelectedIndex = 1 Then
            xmltexts = xmldoc.CreateTextNode(Application.StartupPath & "\settings\language\pt-pt.xml")
        ElseIf ComboBox3.SelectedIndex = 2 Then
            xmltexts = xmldoc.CreateTextNode(Application.StartupPath & "\settings\language\spa.xml")
        End If
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        ''''qualidade videos
        Element = xmldoc.CreateElement("qualidadevideos")
        If ComboBox1.SelectedIndex = 0 Then
            youtubedownload.qualidadevideo = 18
            xmltexts = xmldoc.CreateTextNode("18") '360p
        ElseIf ComboBox1.SelectedIndex = 1 Then
            youtubedownload.qualidadevideo = 35
            xmltexts = xmldoc.CreateTextNode("35") '480p
        ElseIf ComboBox1.SelectedIndex = 2 Then
            youtubedownload.qualidadevideo = 22
            xmltexts = xmldoc.CreateTextNode("22") '720p
        ElseIf ComboBox1.SelectedIndex = 3 Then
            youtubedownload.qualidadevideo = 37
            xmltexts = xmldoc.CreateTextNode("37") '1080p
        End If
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        'qualidade videos download
        Element = xmldoc.CreateElement("qualidadedown")
        If ComboBox2.SelectedIndex = 0 Then
            xmltexts = xmldoc.CreateTextNode("18") '360p
        ElseIf ComboBox2.SelectedIndex = 1 Then
            xmltexts = xmldoc.CreateTextNode("35") '480p
        ElseIf ComboBox2.SelectedIndex = 2 Then
            xmltexts = xmldoc.CreateTextNode("22") '720p
        ElseIf ComboBox2.SelectedIndex = 3 Then
            xmltexts = xmldoc.CreateTextNode("37") '1080p
        End If
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        ''''plugin msn
        Element = xmldoc.CreateElement("pluginmsn")
        If CheckBox1.Checked = True Then
            Form1.estadomsn = True
            xmltexts = xmldoc.CreateTextNode("True")
        Else
            Form1.estadomsn = False
            xmltexts = xmldoc.CreateTextNode("False")
        End If
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        'font type
        Element = xmldoc.CreateElement("font")
        xmltexts = xmldoc.CreateTextNode(TextBox1.Text)
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)

        'posição legendas
        Form1.poslegendas = NumericUpDown1.Value
        Element = xmldoc.CreateElement("poslegendas")
        xmltexts = xmldoc.CreateTextNode(NumericUpDown1.Value)
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)

        'font tamanho
        Form1.legendatamanho = NumericUpDown2.Value
        Element = xmldoc.CreateElement("legendastamanho")
        xmltexts = xmldoc.CreateTextNode(NumericUpDown2.Value)
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)

        'font codicação
        Element = xmldoc.CreateElement("codlegendas")
        xmltexts = xmldoc.CreateTextNode(ComboBox4.SelectedItem.Substring(ComboBox4.SelectedItem.IndexOf("(") + 1).Replace(")", ""))
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        'download de legendas automaticamente
        Element = xmldoc.CreateElement("legendasdown")
        If CheckBox2.Checked = True Then
            Form1.downloadlegendas = True
            xmltexts = xmldoc.CreateTextNode("True")
        Else
            Form1.downloadlegendas = False
            xmltexts = xmldoc.CreateTextNode("False")
        End If
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        'langopensubtitles
        Element = xmldoc.CreateElement("langsubtitlescombr")
        xmltexts = xmldoc.CreateTextNode(subsubtitlecom.Remove(subsubtitlecom.LastIndexOf(",")))
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        'langsubtitles.com.br
        Element = xmldoc.CreateElement("langopensubtitles")
        xmltexts = xmldoc.CreateTextNode(subopensubtitles.Remove(subopensubtitles.LastIndexOf(",")))
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        'langtotal
        Element = xmldoc.CreateElement("langtotal")
        xmltexts = xmldoc.CreateTextNode(langstotal.Remove(langstotal.LastIndexOf(",")))
        RootElement.AppendChild(Element)
        RootElement.LastChild.AppendChild(xmltexts)
        xmldoc.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\settings.xml")
        If MessageBox.Show(msgrestart, "", MessageBoxButtons.YesNo) = Windows.Forms.DialogResult.Yes Then
            Me.Close()
            Form1.Close()
            Process.Start(Application.StartupPath & "\mediastationx.exe")
        End If
    End Sub
#End Region
    Private Sub associar_extencao_ao_ficheiro()
        For i = 0 To audiofiles.Items.Count - 1
            If audiofiles.Items.Item(i).Checked = True Then
                Try
                    If My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text, False, Security.AccessControl.RegistryRights.ReadKey) Is Nothing Then
                        My.Computer.Registry.CurrentUser.CreateSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text, RegistryKeyPermissionCheck.Default, RegistryOptions.None)
                    End If
                    If My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text & "\UserChoice", False, Security.AccessControl.RegistryRights.ReadKey) Is Nothing Then
                        My.Computer.Registry.CurrentUser.CreateSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text & "\UserChoice")
                    End If
                    My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text, True).DeleteSubKey("UserChoice")
                    My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text, True).CreateSubKey("UserChoice").SetValue("Progid", "Mediasxfile" & audiofiles.Items(i).Text.ToUpper)

                    ficheirosass.Add(audiofiles.Items(i).Text)
                Catch ex As Exception
                    MsgBox(audiofiles.Items(i).Text & vbNewLine & vbNewLine & ex.ToString)
                End Try
            End If
        Next
        For i = 0 To videofiles.Items.Count - 1
            If videofiles.Items.Item(i).Checked = True Then
                Try
                    If My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text, False, Security.AccessControl.RegistryRights.ReadKey) Is Nothing Then
                        My.Computer.Registry.CurrentUser.CreateSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text, RegistryKeyPermissionCheck.Default, RegistryOptions.None)
                    End If
                    If My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text & "\UserChoice", False, Security.AccessControl.RegistryRights.ReadKey) Is Nothing Then
                        My.Computer.Registry.CurrentUser.CreateSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text & "\UserChoice")
                    End If
                    My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text, True).DeleteSubKey("UserChoice")
                    My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text, True).CreateSubKey("UserChoice").SetValue("Progid", "Mediasxfile" & videofiles.Items(i).Text.ToUpper)
                    ficheirosass.Add(videofiles.Items(i).Text)
                Catch ex As Exception
                    MsgBox(ex.ToString)
                End Try
            End If
        Next
        Dim ficheiroasstroca As New ArrayList
        For Each fich In ficheirosass
            For i = 0 To audiofiles.Items.Count - 1
                If audiofiles.Items.Item(i).Text.Contains(fich.ToString.ToLower) Then
                    If audiofiles.Items.Item(i).Checked = False Then
                        ficheiroasstroca.Add(fich.ToString.ToLower)
                        Try
                            My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text, True).DeleteSubKey("UserChoice")
                            My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & audiofiles.Items(i).Text, True).CreateSubKey("UserChoice").SetValue("Progid", "WMP11.AssocFile" & audiofiles.Items(i).Text.ToUpper)
                        Catch ex As Exception
                            MsgBox(ex.ToString)
                        End Try
                    End If
                End If
            Next
          
            For i = 0 To videofiles.Items.Count - 1
                If videofiles.Items.Item(i).Text.Contains(fich.ToString.ToLower) Then
                    If videofiles.Items.Item(i).Checked = False Then
                        ficheiroasstroca.Add(fich.ToString.ToLower)
                        Try
                            My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text, True).DeleteSubKey("UserChoice")
                            My.Computer.Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" & videofiles.Items(i).Text, True).CreateSubKey("UserChoice").SetValue("Progid", "WMP11.AssocFile" & videofiles.Items(i).Text.ToUpper)
                        Catch ex As Exception
                            MsgBox(ex.ToString)
                        End Try
                    End If
                End If
            Next
        Next
        For Each fich In ficheiroasstroca
            ficheirosass.Remove(fich)
        Next
        ficheiroasstroca.Clear()
        Dim element As XmlElement
        Dim document As New XmlDocument
        Dim newChild As XmlNode = document.CreateElement("files")
        document.AppendChild(newChild)
        Dim documentElement As XmlElement = document.DocumentElement
        documentElement = document.DocumentElement
        For i = 0 To audiofiles.Items.Count - 1
            If audiofiles.Items(i).Checked = True Then
                element = document.CreateElement("audio")
                Dim text As XmlText = document.CreateTextNode(audiofiles.Items(i).Text)
                documentElement.AppendChild(element)
                documentElement.LastChild.AppendChild(text)
            End If
        Next
        For i = 0 To videofiles.Items.Count - 1
            If videofiles.Items(i).Checked = True Then
                element = document.CreateElement("video")
                Dim text2 As XmlText = document.CreateTextNode(videofiles.Items(i).Text)
                documentElement.AppendChild(element)
                documentElement.LastChild.AppendChild(text2)
            End If
        Next
        document.Save(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\mediastationx\settings\fileass.xml")
        MsgBox(concluido)
    End Sub
#Region "lang subtitle search"
    Public arraylangopensubtitles As New ArrayList
    Public arraylangsubti As New ArrayList

    Private Sub add1_Click(sender As System.Object, e As System.EventArgs) Handles add1.Click
        For i = 0 To ListView1.SelectedItems.Count - 1
            ListBox1.Items.Add(ListView1.SelectedItems(i).Text)
            arraylangopensubtitles.Add(ListView1.SelectedItems(i).SubItems(1).Text)
            If ListView1.SelectedItems(i).SubItems(2).Text <> "" Then
                arraylangsubti.Add(ListView1.SelectedItems(i).SubItems(2).Text)
            End If
        Next
    End Sub
    Private Sub remove_Click(sender As System.Object, e As System.EventArgs) Handles remove.Click
        If ListBox1.SelectedItem = Nothing Then
            MsgBox(selecioneumitem)
            Exit Sub
        End If
        For i = 0 To ListView1.Items.Count - 1
            If ListView1.Items(i).Text.Contains(ListBox1.SelectedItem) Then
                arraylangopensubtitles.Remove(ListView1.Items(i).SubItems(1).Text)
                arraylangsubti.Remove(ListView1.Items(i).SubItems(2).Text)
            End If
        Next
        ListBox1.Items.Remove(ListBox1.SelectedItem)
    End Sub
#End Region

    Private Sub addfont_Click(sender As System.Object, e As System.EventArgs) Handles addfont.Click
        If FontDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = FontDialog1.Font.Name
            NumericUpDown2.Value = FontDialog1.Font.Size
        End If
    End Sub

    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        associar_extencao_ao_ficheiro()
    End Sub

    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs) Handles Button6.Click
        For i = 0 To audiofiles.Items.Count - 1
            audiofiles.Items(i).Checked = False
        Next
        For i = 0 To videofiles.Items.Count - 1
            videofiles.Items(i).Checked = False
        Next
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        For i = 0 To audiofiles.Items.Count - 1
            audiofiles.Items(i).Checked = True
        Next
        For i = 0 To videofiles.Items.Count - 1
            videofiles.Items(i).Checked = True
        Next
    End Sub
End Class