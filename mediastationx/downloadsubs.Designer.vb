<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class downloadsubs
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(downloadsubs))
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.ListView1 = New System.Windows.Forms.ListView()
        Me.selecionada = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.nomelegenda = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.website = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.language = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Code = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.concluido = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.opensubtitlesbgw = New System.ComponentModel.BackgroundWorker()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.download = New System.Windows.Forms.PictureBox()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.download, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ProgressBar1
        '
        Me.ProgressBar1.Location = New System.Drawing.Point(-1, 289)
        Me.ProgressBar1.Name = "ProgressBar1"
        Me.ProgressBar1.Size = New System.Drawing.Size(698, 23)
        Me.ProgressBar1.TabIndex = 1
        '
        'ListView1
        '
        Me.ListView1.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.ListView1.CheckBoxes = True
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.selecionada, Me.nomelegenda, Me.website, Me.language, Me.Code, Me.concluido})
        Me.ListView1.ForeColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.ListView1.Location = New System.Drawing.Point(-1, 0)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.Size = New System.Drawing.Size(698, 283)
        Me.ListView1.TabIndex = 4
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        '
        'selecionada
        '
        Me.selecionada.Text = ""
        Me.selecionada.Width = 30
        '
        'nomelegenda
        '
        Me.nomelegenda.Text = "Name"
        Me.nomelegenda.Width = 240
        '
        'website
        '
        Me.website.Text = "Website"
        Me.website.Width = 200
        '
        'language
        '
        Me.language.Text = "language"
        Me.language.Width = 100
        '
        'Code
        '
        Me.Code.Text = "Code"
        '
        'concluido
        '
        Me.concluido.Text = "Finish"
        '
        'opensubtitlesbgw
        '
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = Global.mediastationx.My.Resources.Resources.world
        Me.PictureBox1.Location = New System.Drawing.Point(613, 312)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(33, 34)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox1.TabIndex = 6
        Me.PictureBox1.TabStop = False
        '
        'download
        '
        Me.download.Image = Global.mediastationx.My.Resources.Resources.download
        Me.download.Location = New System.Drawing.Point(652, 312)
        Me.download.Name = "download"
        Me.download.Size = New System.Drawing.Size(33, 34)
        Me.download.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.download.TabIndex = 5
        Me.download.TabStop = False
        '
        'downloadsubs
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(697, 350)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.download)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.ProgressBar1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "downloadsubs"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.Text = "downloadsubs"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.download, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents ListView1 As System.Windows.Forms.ListView
    Friend WithEvents selecionada As System.Windows.Forms.ColumnHeader
    Friend WithEvents nomelegenda As System.Windows.Forms.ColumnHeader
    Friend WithEvents website As System.Windows.Forms.ColumnHeader
    Friend WithEvents language As System.Windows.Forms.ColumnHeader
    Friend WithEvents Code As System.Windows.Forms.ColumnHeader
    Friend WithEvents opensubtitlesbgw As System.ComponentModel.BackgroundWorker
    Friend WithEvents download As System.Windows.Forms.PictureBox
    Friend WithEvents concluido As System.Windows.Forms.ColumnHeader
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
End Class
