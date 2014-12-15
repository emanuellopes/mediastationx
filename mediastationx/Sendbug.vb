Imports System.Net.Mail
Imports System.Management
Imports System.Net

Public Class Sendbug

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        If TextBox2.Text <> "" Then
            Try
                My.Computer.Network.Ping("gmail.com", 500)
            Catch ex As Exception
                MsgBox(Form1.semnet)
                Exit Sub
            End Try
            Try
                Dim MyMailMessage As New MailMessage()
                MyMailMessage.From = New MailAddress("joaolopesslb@gmail.com")
                MyMailMessage.To.Add("joaolopesslb@gmail.com")
                MyMailMessage.Subject = "mediastationx"
                MyMailMessage.Body = TextBox2.Text
                Dim SMPT As New SmtpClient("smtp.gmail.com")
                SMPT.Port = 587
                SMPT.EnableSsl = True
                SMPT.Credentials = New System.Net.NetworkCredential("joaolopesslb@gmail.com", "carbon10")
                SMPT.Send(MyMailMessage)
                MsgBox("Sended, tanks to sended the report")
            Catch ex As Exception
            End Try
        End If
    End Sub
    ' hextostring
    Function HexToString(ByVal hex As String) As String
        Dim text As New System.Text.StringBuilder(hex.Length \ 2)
        For i As Integer = 0 To hex.Length - 2 Step 2
            text.Append(Chr(Convert.ToByte(hex.Substring(i, 2), 16)))
        Next
        Return text.ToString
    End Function
    Private Sub Sendbug_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim objOS As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")
        Dim objCS As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem")
        Dim ObjP As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_Processor")
        Dim objHD As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive")

        For Each ob In objOS.Get
            TextBox2.Text += "OS:" + ob("Name").ToString().Split("|".ToCharArray())(0) + vbNewLine
            TextBox2.Text += "version:" + ob("version").ToString() + vbNewLine
            TextBox2.Text += "Utilizador:" + ob("csname").ToString() + vbNewLine
            TextBox2.Text += "RAM: " + ob("TotalVisibleMemorySize").ToString() + " MB" + vbNewLine
            Exit For
        Next
        For Each ob In objCS.Get
            TextBox2.Text += "System type:" + ob("systemtype").ToString + vbNewLine
            Exit For
        Next
        For Each ob In ObjP.Get
            TextBox2.Text += "Processor:" + ob("Name").ToString() + vbNewLine
            Exit For
        Next
        For Each ob In objHD.Get
            TextBox2.Text += "HD Size:" + ob("Size").ToString() + "KB" + vbNewLine
            Exit For
        Next
        TextBox2.Text += "========================" + vbNewLine + vbNewLine
    End Sub
End Class