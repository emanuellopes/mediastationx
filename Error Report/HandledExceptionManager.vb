Imports System.Windows.Forms
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.Configuration

'--
'-- Generic HANDLED error handling class
'--
'-- It's like MessageBox, but specific to handled exceptions, and supports email notifications
'--
'-- Jeff Atwood
'-- http://www.codinghorror.com
'--
Public Class HandledExceptionManager

    Private Shared _blnHaveException As Boolean = False
    'Private Shared _blnEmailError As Boolean = True
    'Private Shared _strEmailBody As String
    Private Shared _strExceptionType As String



    Public Enum UserErrorDefaultButton
        [Default] = 0
        Button1 = 1
        Button2 = 2
        Button3 = 3
    End Enum

    '-- 
    '-- replace generic constants in strings with specific values
    '--
    Private Shared Function ReplaceStringVals(ByVal strOutput As String) As String
        Dim strTemp As String
        If strOutput Is Nothing Then
            strTemp = ""
        Else
            strTemp = strOutput
        End If
        strTemp = strTemp.Replace("(app)", AppSettings.AppProduct)
        strTemp = strTemp.Replace("(contact)", AppSettings.GetString("UnhandledExceptionManager/ContactInfo"))
        Return strTemp
    End Function

    '--
    '-- make sure "More" text is populated with something useful
    '--
    Private Shared Function GetDefaultMore(ByVal strMoreDetails As String) As String
        If strMoreDetails = "" Then
            Dim objStringBuilder As New System.Text.StringBuilder
            With objStringBuilder
                ' .Append(_strDefaultMore)
                .Append(Environment.NewLine)
                .Append(Environment.NewLine)
                .Append("Basic technical information follows: " + Environment.NewLine)
                .Append("---" + Environment.NewLine)
                .Append(UnhandledExceptionManager.SysInfoToString(True))
            End With
            Return objStringBuilder.ToString
        Else
            Return strMoreDetails
        End If
    End Function

    '--
    '-- converts exception to a formatted "more" string
    '--
    Private Shared Function ExceptionToMore(ByVal objException As System.Exception) As String
        Dim sb As New System.Text.StringBuilder
        With sb
            .Append("Detailed technical information follows: " + Environment.NewLine)
            .Append("---" + Environment.NewLine)
            Dim x As String = UnhandledExceptionManager.ExceptionToString(objException)
            .Append(x)
        End With
        Return sb.ToString
    End Function

    '--
    '-- perform our string replacements for (app) and (contact), etc etc
    '-- also make sure More has default values if it is blank.
    '--       
    Private Shared Sub ProcessStrings(ByRef strHowUserAffected As String, _
        ByRef strMoreDetails As String)
        strHowUserAffected = ReplaceStringVals(strHowUserAffected)
        strMoreDetails = ReplaceStringVals(GetDefaultMore(strMoreDetails))
    End Sub

    '-- 
    '-- simplest possible error dialog
    '--
    Public Overloads Shared Function ShowDialog(ByVal strHowUserAffected As String) As DialogResult
        _blnHaveException = False
        Return ShowDialogInternal(strHowUserAffected, "", MessageBoxButtons.OK, MessageBoxIcon.Warning, UserErrorDefaultButton.Default)
    End Function

    '--
    '-- advanced error dialog with Exception object
    '--
    Public Overloads Shared Function ShowDialog(ByVal strHowUserAffected As String, _
        ByVal objException As System.Exception, _
        Optional ByVal Buttons As MessageBoxButtons = MessageBoxButtons.OK, _
        Optional ByVal Icon As MessageBoxIcon = MessageBoxIcon.Warning, _
        Optional ByVal DefaultButton As UserErrorDefaultButton = UserErrorDefaultButton.Default) As DialogResult
        _blnHaveException = True
        _strExceptionType = objException.GetType.FullName
        Return ShowDialogInternal(strHowUserAffected, _
            ExceptionToMore(objException), Buttons, Icon, DefaultButton)
    End Function


    '--
    '-- advanced error dialog with More string
    '-- leave "more" string blank to get the default
    '--
    Public Overloads Shared Function ShowDialog(ByVal strHowUserAffected As String, _
        ByVal strMoreDetails As String, _
        Optional ByVal Buttons As MessageBoxButtons = MessageBoxButtons.OK, _
        Optional ByVal Icon As MessageBoxIcon = MessageBoxIcon.Warning, _
        Optional ByVal DefaultButton As UserErrorDefaultButton = UserErrorDefaultButton.Default _
        ) As DialogResult

        _blnHaveException = False
        Return ShowDialogInternal(strHowUserAffected, strMoreDetails, _
            Buttons, Icon, DefaultButton)
    End Function

    '-- 
    '-- internal method to show error dialog
    '--
    Private Shared Function ShowDialogInternal(ByVal strHowUserAffected As String, _
                 ByVal strMoreDetails As String, _
                ByVal Buttons As MessageBoxButtons, _
                ByVal Icon As MessageBoxIcon, _
                ByVal DefaultButton As UserErrorDefaultButton) As DialogResult

        '-- set default values, etc
        ProcessStrings(strHowUserAffected, strMoreDetails)

        Dim objForm As New ExceptionDialog
        With objForm
            .Text = ReplaceStringVals(objForm.Text)
            .ScopeBox.Text = strHowUserAffected
            .txtMore.Text = strMoreDetails
        End With

        '-- set the proper dialog icon
        Select Case Icon
            Case MessageBoxIcon.Error
                objForm.PictureBox1.Image = System.Drawing.SystemIcons.Error.ToBitmap
            Case MessageBoxIcon.Stop
                objForm.PictureBox1.Image = System.Drawing.SystemIcons.Error.ToBitmap
            Case MessageBoxIcon.Exclamation
                objForm.PictureBox1.Image = System.Drawing.SystemIcons.Exclamation.ToBitmap
            Case MessageBoxIcon.Information
                objForm.PictureBox1.Image = System.Drawing.SystemIcons.Information.ToBitmap
            Case MessageBoxIcon.Question
                objForm.PictureBox1.Image = System.Drawing.SystemIcons.Question.ToBitmap
            Case Else
                objForm.PictureBox1.Image = System.Drawing.SystemIcons.Error.ToBitmap
        End Select
        '-- show the user our error dialog
        Return objForm.ShowDialog()
    End Function
End Class