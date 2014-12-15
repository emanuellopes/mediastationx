Imports System.Runtime.InteropServices

Public Class msn
    Private Declare Function FindWindowEx Lib "user32" Alias "FindWindowExA" ( _
            ByVal hWnd1 As Integer, _
            ByVal hWnd2 As Integer, _
            ByVal lpsz1 As String, _
            ByVal lpsz2 As String) _
        As Integer

    Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" ( _
        ByVal Hwnd As Integer, _
        ByVal wMsg As Integer, _
        ByVal wParam As Integer, _
        ByVal lParam As Integer) _
    As Integer

    Private Const WM_COPYDATA As Short = 74

    Private Structure COPYDATASTRUCT
        Public dwData As Integer
        Public cbData As Integer
        Public lpData As Integer
    End Structure

    Public Enum EnumCategory As Integer
        Music = 0
        Games = 1
        Office = 2
    End Enum

    Public Shared Sub SendStatusMessage(ByVal Enable As Boolean, ByVal Category As EnumCategory, Optional ByVal Message As String = "")
        Try
            Dim Data As COPYDATASTRUCT
            Dim Buffer As String = "\0" & Category.ToString + "\0" & IIf(Enable, "1", "0") & "\0{0}\0" & Message & "\0\0\0\0" & Chr(0) & ""
            Dim Handle As Integer = 0

            Data.dwData = 1351
            Data.lpData = VarPtr(Buffer)
            Data.cbData = Buffer.Length * 2

            Handle = FindWindowEx(0, Handle, "MsnMsgrUIManager", Nothing)

            If Handle > 0 Then
                SendMessage(Handle, WM_COPYDATA, 0, VarPtr(Data))
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Shared Function VarPtr(ByVal e As Object) As Integer
        Dim GC As GCHandle = GCHandle.Alloc(e, GCHandleType.Pinned)
        Dim GC2 As Integer = GC.AddrOfPinnedObject.ToInt32
        GC.Free()
        Return GC2
    End Function
End Class
