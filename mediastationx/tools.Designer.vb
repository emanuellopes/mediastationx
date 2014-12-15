<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class tools
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(tools))
        Me.menudelete = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.ComboBox1 = New System.Windows.Forms.ComboBox()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()
        Me.outfoldecaminho = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.downloadbutton = New System.Windows.Forms.PictureBox()
        Me.pathfolder = New System.Windows.Forms.PictureBox()
        Me.addurl = New System.Windows.Forms.PictureBox()
        Me.stopdownload = New System.Windows.Forms.PictureBox()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.menuadd = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripMenuItem()
        Me.menudelete.SuspendLayout()
        CType(Me.downloadbutton, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.pathfolder, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.addurl, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.stopdownload, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel2.SuspendLayout()
        Me.menuadd.SuspendLayout()
        Me.SuspendLayout()
        '
        'menudelete
        '
        Me.menudelete.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem1})
        Me.menudelete.Name = "menu"
        Me.menudelete.Size = New System.Drawing.Size(108, 26)
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        Me.ToolStripMenuItem1.Size = New System.Drawing.Size(107, 22)
        Me.ToolStripMenuItem1.Text = "Delete"
        '
        'ComboBox1
        '
        Me.ComboBox1.FormattingEnabled = True
        Me.ComboBox1.Items.AddRange(New Object() {"Não converter", "Formato .MP3 320 kbps", "Formato .MP3 256 kbps", "Formato .MP3 224 kbps", "Formato .MP3 192 kbps", "Formato .MP3 128 kbps", "Formato .MP3 64 kbps", "Formato .AAC", "Formato AVI", "Formato FLV", "Formato MKV", "Formato MOV", "Formato MP4"})
        Me.ComboBox1.Location = New System.Drawing.Point(195, 296)
        Me.ComboBox1.Name = "ComboBox1"
        Me.ComboBox1.Size = New System.Drawing.Size(121, 21)
        Me.ComboBox1.TabIndex = 20
        Me.ComboBox1.Text = "Formato final"
        Me.ToolTip1.SetToolTip(Me.ComboBox1, "Se tiveres uma insternet, mais fraca é recomendado escolher o formato standart, s" & _
                "e não o programa escolherá o melhor fotmato para fazero download")
        '
        'BackgroundWorker1
        '
        Me.BackgroundWorker1.WorkerSupportsCancellation = True
        '
        'outfoldecaminho
        '
        Me.outfoldecaminho.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.outfoldecaminho.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.outfoldecaminho.Location = New System.Drawing.Point(78, 297)
        Me.outfoldecaminho.Name = "outfoldecaminho"
        Me.outfoldecaminho.Size = New System.Drawing.Size(63, 20)
        Me.outfoldecaminho.TabIndex = 21
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.Label1.Location = New System.Drawing.Point(15, 304)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(46, 13)
        Me.Label1.TabIndex = 22
        Me.Label1.Text = "Save in:"
        '
        'downloadbutton
        '
        Me.downloadbutton.BackColor = System.Drawing.Color.Transparent
        Me.downloadbutton.Image = Global.mediastationx.My.Resources.Resources.download
        Me.downloadbutton.Location = New System.Drawing.Point(654, 289)
        Me.downloadbutton.Name = "downloadbutton"
        Me.downloadbutton.Size = New System.Drawing.Size(27, 31)
        Me.downloadbutton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.downloadbutton.TabIndex = 23
        Me.downloadbutton.TabStop = False
        '
        'pathfolder
        '
        Me.pathfolder.BackColor = System.Drawing.Color.Transparent
        Me.pathfolder.Image = Global.mediastationx.My.Resources.Resources.pasta
        Me.pathfolder.Location = New System.Drawing.Point(147, 293)
        Me.pathfolder.Name = "pathfolder"
        Me.pathfolder.Size = New System.Drawing.Size(27, 27)
        Me.pathfolder.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pathfolder.TabIndex = 24
        Me.pathfolder.TabStop = False
        '
        'addurl
        '
        Me.addurl.Image = Global.mediastationx.My.Resources.Resources.add
        Me.addurl.Location = New System.Drawing.Point(324, 293)
        Me.addurl.Name = "addurl"
        Me.addurl.Size = New System.Drawing.Size(27, 27)
        Me.addurl.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.addurl.TabIndex = 25
        Me.addurl.TabStop = False
        '
        'stopdownload
        '
        Me.stopdownload.BackColor = System.Drawing.Color.Transparent
        Me.stopdownload.Image = Global.mediastationx.My.Resources.Resources.parardownload
        Me.stopdownload.Location = New System.Drawing.Point(621, 290)
        Me.stopdownload.Name = "stopdownload"
        Me.stopdownload.Size = New System.Drawing.Size(27, 27)
        Me.stopdownload.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.stopdownload.TabIndex = 26
        Me.stopdownload.TabStop = False
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.stopdownload)
        Me.Panel2.Controls.Add(Me.addurl)
        Me.Panel2.Controls.Add(Me.pathfolder)
        Me.Panel2.Controls.Add(Me.downloadbutton)
        Me.Panel2.Controls.Add(Me.Label1)
        Me.Panel2.Controls.Add(Me.outfoldecaminho)
        Me.Panel2.Controls.Add(Me.ComboBox1)
        Me.Panel2.Controls.Add(Me.ListView1)
        Me.Panel2.Location = New System.Drawing.Point(-3, 0)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(692, 329)
        Me.Panel2.TabIndex = 21
        '
        'ListView1
        '
        Me.ListView1.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5})
        Me.ListView1.ContextMenuStrip = Me.menudelete
        Me.ListView1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.ListView1.Location = New System.Drawing.Point(3, 3)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(686, 280)
        Me.ListView1.TabIndex = 1
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Link"
        Me.ColumnHeader1.Width = 150
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Nome"
        Me.ColumnHeader2.Width = 250
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Estado"
        Me.ColumnHeader3.Width = 86
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Tamanho"
        Me.ColumnHeader4.Width = 95
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Progresso"
        Me.ColumnHeader5.Width = 87
        '
        'menuadd
        '
        Me.menuadd.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem2, Me.ToolStripMenuItem3})
        Me.menuadd.Name = "menuadd"
        Me.menuadd.Size = New System.Drawing.Size(161, 48)
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        Me.ToolStripMenuItem2.Size = New System.Drawing.Size(160, 22)
        Me.ToolStripMenuItem2.Text = "Add url youtube"
        '
        'ToolStripMenuItem3
        '
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        Me.ToolStripMenuItem3.Size = New System.Drawing.Size(160, 22)
        Me.ToolStripMenuItem3.Text = "add video path"
        '
        'tools
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(685, 332)
        Me.Controls.Add(Me.Panel2)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "tools"
        Me.Text = "Converter/download"
        Me.menudelete.ResumeLayout(False)
        CType(Me.downloadbutton, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.pathfolder, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.addurl, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.stopdownload, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.menuadd.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents menudelete As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ComboBox1 As System.Windows.Forms.ComboBox
    Friend WithEvents outfoldecaminho As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents downloadbutton As System.Windows.Forms.PictureBox
    Friend WithEvents pathfolder As System.Windows.Forms.PictureBox
    Friend WithEvents addurl As System.Windows.Forms.PictureBox
    Friend WithEvents stopdownload As System.Windows.Forms.PictureBox
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents menuadd As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem3 As System.Windows.Forms.ToolStripMenuItem
End Class
