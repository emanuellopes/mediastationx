Imports System.Drawing
Imports System.Windows.Forms
Imports System.Reflection
Imports System.Threading
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Management

'--
'-- Generic UNHANDLED error handling class
'--
'-- Intended as a last resort for errors which crash our application, so we can get feedback on what
'-- caused the error.
'-- 
'-- To use: UnhandledExceptionManager.AddHandler() in the STARTUP of your application
'--
'-- more background information on Exceptions at:
'--   http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnbda/html/exceptdotnet.asp
'--
'--
'-- Jeff Atwood
'-- http://www.codinghorror.com
'--
Public Class UnhandledExceptionManager

    Private Sub New()
        ' to keep this class from being creatable as an instance.
    End Sub

    Private Shared _blnLogToEventLog As Boolean
    Private Shared _blnLogToUI As Boolean

    'Private Shared _blnLogToFileOK As Boolean
    Private Shared _blnLogToEventLogOK As Boolean
    Private Shared _objParentAssembly As System.Reflection.Assembly = Nothing
    Private Shared _strException As String
    Private Shared _strExceptionType As String
    Private Shared _blnIgnoreDebugErrors As Boolean
    Private Shared _blnKillAppOnException As Boolean
    Private Const _strClassName As String = "UnhandledExceptionManager"

#Region "Properties"
    Public Shared Property IgnoreDebugErrors() As Boolean
        Get
            Return _blnIgnoreDebugErrors
        End Get
        Set(ByVal Value As Boolean)
            _blnIgnoreDebugErrors = Value
        End Set
    End Property

    Public Shared Property DisplayDialog() As Boolean
        Get
            Return _blnLogToUI
        End Get
        Set(ByVal Value As Boolean)
            _blnLogToUI = Value
        End Set
    End Property
    Public Shared Property KillAppOnException() As Boolean
        Get
            Return _blnKillAppOnException
        End Get
        Set(ByVal Value As Boolean)
            _blnKillAppOnException = Value
        End Set
    End Property
    Public Shared Property LogToEventLog() As Boolean
        Get
            Return _blnLogToEventLog
        End Get
        Set(ByVal Value As Boolean)
            _blnLogToEventLog = Value
        End Set
    End Property
#End Region


    Private Shared Function ParentAssembly() As System.Reflection.Assembly
        If _objParentAssembly Is Nothing Then
            If System.Reflection.Assembly.GetEntryAssembly Is Nothing Then
                _objParentAssembly = System.Reflection.Assembly.GetCallingAssembly
            Else
                _objParentAssembly = System.Reflection.Assembly.GetEntryAssembly
            End If
        End If
        Return _objParentAssembly
    End Function

    '--
    '-- load some settings that may optionally be present in our .config file
    '-- if they aren't present, we get the defaults as defined here
    '--
    Private Shared Sub LoadConfigSettings()
        LogToEventLog = GetConfigBoolean("LogToEventLog", False)
        DisplayDialog = GetConfigBoolean("DisplayDialog", True)
        IgnoreDebugErrors = GetConfigBoolean("IgnoreDebug", True)
        KillAppOnException = GetConfigBoolean("KillAppOnException", True)
    End Sub

    '--
    '-- This *MUST* be called early in your application to set up global error handling
    '--
    Public Shared Sub [AddHandler](Optional ByVal blnConsoleApp As Boolean = False)
        '-- attempt to load optional settings from .config file
        LoadConfigSettings()

        '-- we don't need an unhandled exception handler if we are running inside
        '-- the vs.net IDE; it is our "unhandled exception handler" in that case
        If _blnIgnoreDebugErrors Then
            If Debugger.IsAttached Then Return
        End If
        '-- track the parent assembly that set up error handling
        '-- need to call this NOW so we set it appropriately; otherwise
        '-- we may get the wrong assembly at exception time!
        ParentAssembly()
        '-- for winforms applications
        RemoveHandler Application.ThreadException, AddressOf ThreadExceptionHandler
        AddHandler Application.ThreadException, AddressOf ThreadExceptionHandler
    End Sub

    '--
    '-- handles Application.ThreadException event
    '--
    Private Shared Sub ThreadExceptionHandler(ByVal sender As System.Object, ByVal e As System.Threading.ThreadExceptionEventArgs)
        GenericExceptionHandler(e.Exception)
    End Sub


    '--
    '-- handles AppDomain.CurrentDoamin.UnhandledException event
    '--
    Private Shared Sub UnhandledExceptionHandler(ByVal sender As System.Object, ByVal args As UnhandledExceptionEventArgs)
        Dim objException As Exception = CType(args.ExceptionObject, Exception)
        GenericExceptionHandler(objException)
    End Sub

    '--
    '-- exception-safe file attrib retrieval; we don't care if this fails
    '--
    Private Shared Function AssemblyFileTime(ByVal objAssembly As System.Reflection.Assembly) As DateTime
        Try
            Return System.IO.File.GetLastWriteTime(objAssembly.Location)
        Catch ex As Exception
            Return DateTime.MaxValue
        End Try
    End Function

    '--
    '-- returns build datetime of assembly
    '-- assumes default assembly value in AssemblyInfo:
    '-- <Assembly: AssemblyVersion("1.0.*")> 
    '--
    '-- filesystem create time is used, if revision and build were overridden by user
    '--
    Private Shared Function AssemblyBuildDate(ByVal objAssembly As System.Reflection.Assembly, _
                                       Optional ByVal blnForceFileDate As Boolean = False) As DateTime
        Dim objVersion As System.Version = objAssembly.GetName.Version
        Dim dtBuild As DateTime

        If blnForceFileDate Then
            dtBuild = AssemblyFileTime(objAssembly)
        Else
            dtBuild = CType("01/01/2000", DateTime). _
                AddDays(objVersion.Build). _
                AddSeconds(objVersion.Revision * 2)
            If TimeZone.IsDaylightSavingTime(DateTime.Now, TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year)) Then
                dtBuild = dtBuild.AddHours(1)
            End If
            If dtBuild > DateTime.Now Or objVersion.Build < 730 Or objVersion.Revision = 0 Then
                dtBuild = AssemblyFileTime(objAssembly)
            End If
        End If

        Return dtBuild
    End Function

    '--
    '-- turns a single stack frame object into an informative string
    '--
    Private Shared Function StackFrameToString(ByVal sf As StackFrame) As String
        Dim sb As New System.Text.StringBuilder
        Dim intParam As Integer
        Dim mi As MemberInfo = sf.GetMethod

        With sb
            '-- build method name
            .Append("   ")
            .Append(mi.DeclaringType.Namespace)
            .Append(".")
            .Append(mi.DeclaringType.Name)
            .Append(".")
            .Append(mi.Name)

            '-- build method params
            Dim objParameters() As ParameterInfo = sf.GetMethod.GetParameters()
            Dim objParameter As ParameterInfo
            .Append("(")
            intParam = 0
            For Each objParameter In objParameters
                intParam += 1
                If intParam > 1 Then .Append(", ")
                .Append(objParameter.Name)
                .Append(" As ")
                .Append(objParameter.ParameterType.Name)
            Next
            .Append(")")
            .Append(Environment.NewLine)

            '-- if source code is available, append location info
            .Append("       ")
            If sf.GetFileName Is Nothing OrElse sf.GetFileName.Length = 0 Then
                .Append(System.IO.Path.GetFileName(ParentAssembly.CodeBase))
                '-- native code offset is always available
                .Append(": N ")
                .Append(String.Format("{0:#00000}", sf.GetNativeOffset))

            Else
                .Append(System.IO.Path.GetFileName(sf.GetFileName))
                .Append(": line ")
                .Append(String.Format("{0:#0000}", sf.GetFileLineNumber))
                .Append(", col ")
                .Append(String.Format("{0:#00}", sf.GetFileColumnNumber))
                '-- if IL is available, append IL location info
                If sf.GetILOffset <> StackFrame.OFFSET_UNKNOWN Then
                    .Append(", IL ")
                    .Append(String.Format("{0:#0000}", sf.GetILOffset))
                End If
            End If
            .Append(Environment.NewLine)
        End With
        Return sb.ToString
    End Function

    '--
    '-- enhanced stack trace generator
    '--
    Private Overloads Shared Function EnhancedStackTrace(ByVal objStackTrace As StackTrace, _
        Optional ByVal strSkipClassName As String = "") As String
        Dim intFrame As Integer
        Dim sb As New System.Text.StringBuilder
        sb.Append(Environment.NewLine)
        sb.Append("--------- Stack Trace ---------")
        sb.Append(Environment.NewLine)
        For intFrame = 0 To objStackTrace.FrameCount - 1
            Dim sf As StackFrame = objStackTrace.GetFrame(intFrame)
            Dim mi As MemberInfo = sf.GetMethod

            If strSkipClassName <> "" AndAlso mi.DeclaringType.Name.IndexOf(strSkipClassName) > -1 Then
                '-- don't include frames with this name
            Else
                sb.Append(StackFrameToString(sf))
            End If
        Next
        sb.Append(Environment.NewLine)
        Return sb.ToString
    End Function

    '--
    '-- enhanced stack trace generator (exception)
    '--
    Private Overloads Shared Function EnhancedStackTrace(ByVal objException As Exception) As String
        Dim objStackTrace As New StackTrace(objException, True)
        Return EnhancedStackTrace(objStackTrace)
    End Function

    '--
    '-- enhanced stack trace generator (no params)
    '--
    Private Overloads Shared Function EnhancedStackTrace() As String
        Dim objStackTrace As New StackTrace(True)
        Return EnhancedStackTrace(objStackTrace, "ExceptionManager")
    End Function

    '--
    '-- generic exception handler; the various specific handlers all call into this sub
    '--
    Private Shared Sub GenericExceptionHandler(ByVal objException As Exception)

        '-- turn the exception into an informative string
        Try
            _strException = ExceptionToString(objException)
            _strExceptionType = objException.GetType.FullName
        Catch ex As Exception
            _strException = "Error '" & ex.Message & "' while generating exception string"
            _strExceptionType = ""
        End Try
        Cursor.Current = System.Windows.Forms.Cursors.WaitCursor
        '-- log this error to various locations
        Try
            If _blnLogToEventLog Then ExceptionToEventLog()
        Catch ex As Exception
        End Try
        Cursor.Current = System.Windows.Forms.Cursors.Default
        '-- display message to the user
        If _blnLogToUI Then ExceptionToUI()

        If _blnKillAppOnException Then
            KillApp()
            Application.Exit()
        End If

    End Sub

    '--
    '-- This is in a private routine for .NET security reasons
    '-- if this line of code is in a sub, the entire sub is tagged as full trust
    '--
    Private Shared Sub KillApp()
        System.Diagnostics.Process.GetCurrentProcess.Kill()
    End Sub

    '--
    '-- turns exception into something an average user can hopefully
    '-- understand; still very technical
    '--
    Private Shared Function FormatExceptionForUser(ByVal blnConsoleApp As Boolean) As String
        Dim objStringBuilder As New System.Text.StringBuilder
        With objStringBuilder
            .Append("Detailed error information follows:")
            .Append(Environment.NewLine)
            .Append(Environment.NewLine)
            .Append(_strException)
        End With
        Return objStringBuilder.ToString
    End Function

    '--
    '-- display a dialog to the user; otherwise we just terminate with no alert at all!
    '--
    Private Shared Sub ExceptionToUI()

        '  Const _strWhatHappened As String = "There was an unexpected error in MediaStationX. This may be due to a programming bug."
        Dim _strHowUserAffected As String
        ' Const _strWhatUserCanDo As String = "Restart MediaStationX, and try repeating your last action. Try alternative methods of performing the same action."

        If UnhandledExceptionManager.KillAppOnException Then
            _strHowUserAffected = "When you click OK, MediaStationX will close."
        Else
            _strHowUserAffected = "The action you requested was not performed."
        End If
        '-- pop the dialog
        HandledExceptionManager.ShowDialog(_strHowUserAffected, _
            FormatExceptionForUser(False), _
            MessageBoxButtons.OK, MessageBoxIcon.Stop)
    End Sub
    Private Shared Function GetApplicationPath() As String
        If ParentAssembly.CodeBase.StartsWith("http://") Then
            Return "c:\" + Regex.Replace(ParentAssembly.CodeBase(), "[\/\\\:\*\?\""\<\>\|]", "_") + "."
        Else
            Return System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName + "."
        End If
    End Function
    Private Shared Sub ExceptionToEventLog()
        Try
            System.Diagnostics.EventLog.WriteEntry(System.AppDomain.CurrentDomain.FriendlyName, _
                Environment.NewLine + _strException, _
                EventLogEntryType.Error)
            _blnLogToEventLogOK = True
        Catch ex As Exception
            _blnLogToEventLogOK = False
        End Try
    End Sub
    '--
    '-- exception-safe WindowsIdentity.GetCurrent retrieval returns "domain\username"
    '-- per MS, this sometimes randomly fails with "Access Denied" particularly on NT4
    '--
    Private Shared Function CurrentWindowsIdentity() As String
        Try
            Return System.Security.Principal.WindowsIdentity.GetCurrent.Name()
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Shared Function CurrentEnvironmentIdentity() As String
        Try
            Return System.Environment.UserDomainName + "\" + System.Environment.UserName
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Shared Function UserIdentity() As String
        Dim strTemp As String
        strTemp = CurrentWindowsIdentity()
        If strTemp = "" Then
            strTemp = CurrentEnvironmentIdentity()
        End If
        Return strTemp
    End Function

    Friend Shared Function SysInfoToString(Optional ByVal blnIncludeStackTrace As Boolean = False) As String
        Dim objStringBuilder As New System.Text.StringBuilder

        With objStringBuilder
            .Append("Date and Time:         ")
            .Append(DateTime.Now)
            .Append(Environment.NewLine)
            .Append("Machine Name:          ")
            Try
                .Append(Environment.MachineName)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append("Current User:          ")
            .Append(UserIdentity)
            .Append(Environment.NewLine)
            Try
                Dim objOS As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")
                Dim objCS As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem")
                Dim ObjP As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_Processor")
                Dim objHD As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive")
                For Each ob As Management.ManagementBaseObject In objOS.Get
                    .Append("OS:" + ob("Name").ToString().Split("|".ToCharArray())(0))
                    .Append(Environment.NewLine)
                    .Append("Version:" + ob("version").ToString())
                    .Append(Environment.NewLine)
                    .Append("RAM: " + ob("TotalVisibleMemorySize").ToString() + " MB")
                    .Append(Environment.NewLine)
                Next
                For Each ob As Management.ManagementBaseObject In objCS.Get
                    .Append("System type:" + ob("systemtype").ToString())
                    .Append(Environment.NewLine)
                    Exit For
                Next
                For Each ob As Management.ManagementBaseObject In ObjP.Get
                    .Append("Processor:" + ob("Name").ToString())
                    .Append(Environment.NewLine)
                    Exit For
                Next
                For Each ob As Management.ManagementBaseObject In objHD.Get
                    .Append("HD Size:" + ob("Size").ToString() + "KB")
                    .Append(Environment.NewLine)
                    Exit For
                Next
            Catch e As Exception
            End Try
            Try
                .Append("appdomain: " + System.AppDomain.CurrentDomain.FriendlyName())
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append("Assembly Codebase:     ")
            Try
                .Append(ParentAssembly.CodeBase())
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append("Assembly Full Name:    ")
            Try
                .Append(ParentAssembly.FullName)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append("Assembly Version:      ")
            Try
                .Append(ParentAssembly.GetName().Version().ToString)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)

            .Append("Assembly Build Date:   ")
            Try
                .Append(AssemblyBuildDate(ParentAssembly).ToString)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append(Environment.NewLine)
            .Append("------------------------------------")
            .Append(Environment.NewLine)
            If blnIncludeStackTrace Then
                .Append(EnhancedStackTrace())
            End If
        End With
        Return objStringBuilder.ToString
    End Function


    '--
    '-- translate exception object to string, with additional system info
    '--
    Friend Shared Function ExceptionToString(ByVal objException As Exception) As String
        Dim objStringBuilder As New System.Text.StringBuilder

        If Not (objException.InnerException Is Nothing) Then
            With objStringBuilder
                .Append("(Inner Exception)")
                .Append(Environment.NewLine)
                .Append(ExceptionToString(objException.InnerException))
                .Append(Environment.NewLine)
                .Append("(Outer Exception)")
                .Append(Environment.NewLine)
            End With
        End If
        With objStringBuilder
            .Append(SysInfoToString)
            .Append("Exception Source:      ")
            Try
                .Append(objException.Source)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append("Exception Type:        ")
            Try
                .Append(objException.GetType.FullName)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            .Append("Exception Message:     ")
            Try
                .Append(objException.Message)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)

            .Append("Exception Target: ")
            Try
                .Append(objException.TargetSite.Name)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
            Try
                Dim x As String = EnhancedStackTrace(objException)
                .Append(x)
            Catch e As Exception
                .Append(e.Message)
            End Try
            .Append(Environment.NewLine)
        End With
        Return objStringBuilder.ToString
    End Function
    '--
    '-- Returns the specified boolean value from the application .config file,
    '-- with many fail-safe checks (exceptions, key not present, etc)
    '--
    '-- this is important in an *unhandled exception handler*, because any unhandled exceptions will simply exit!
    '-- 
    Private Shared Function GetConfigBoolean(ByVal strKey As String, Optional ByVal blnDefault As Boolean = Nothing) As Boolean
        Dim strTemp As String
        Try
            strTemp = ConfigurationSettings.AppSettings.Get(_strClassName & "/" & strKey)
        Catch ex As Exception
            If blnDefault = Nothing Then
                Return False
            Else
                Return blnDefault
            End If
        End Try

        If strTemp = Nothing Then
            If blnDefault = Nothing Then
                Return False
            Else
                Return blnDefault
            End If
        Else
            Select Case strTemp.ToLower
                Case "1", "true"
                    Return True
                Case Else
                    Return False
            End Select
        End If
    End Function
End Class