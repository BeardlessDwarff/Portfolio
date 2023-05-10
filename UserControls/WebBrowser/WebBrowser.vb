Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.WebBrowser))> _
Public Class WebBrowser
    Inherits System.Windows.Forms.WebBrowser

    Private cookie As AxHost.ConnectionPointCookie
    Private helper As WebBrowser2EventHelper
    Private MyCredentials As BrowserCredentials
    ' Private WithEvents axWebBrowser As SHDocVw.WebBrowser_V1

    'NEW EVENTS THAT WILL NOW BE EXPOSED
    Public Event NewWindow2 As WebBrowserNewWindow2EventHandler
    Public Event NavigateError As WebBrowserNavigateErrorEventHandler

    'DELEGATES TO HANDLE PROCESSING OF THE EVENTS
    Public Delegate Sub WebBrowserNewWindow2EventHandler(ByVal sender As Object, ByVal e As WebBrowserNewWindow2EventArgs)
    Public Delegate Sub WebBrowserNavigateErrorEventHandler(ByVal sender As Object, ByVal e As WebBrowserNavigateErrorEventArgs)

    Public Sub NavigateWithLogin(URL As String, UserName As String, Password As String)
        Dim hdr As String = ""

        If UserName <> "" Then
            hdr = "Authorization: Basic " & Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(UserName & ":" & Password)) + System.Environment.NewLine
        End If

        MyBase.Navigate(URL, Nothing, Nothing, hdr)
    End Sub

    Public WriteOnly Property HTML As String
        Set(value As String)
            If Me.Document Is Nothing Then
                Dim sTempFile As String
                sTempFile = My.Computer.FileSystem.GetTempFileName
                sTempFile = sTempFile.Substring(0, sTempFile.LastIndexOf(".")) & ".html"
                My.Computer.FileSystem.WriteAllText(sTempFile, value, False)
                FrameworkNS.Functions.API.SetRedraw(Me.Handle, False)
                Me.Navigate(sTempFile)
                FrameworkNS.Functions.API.SetRedraw(Me.Handle, True)
            Else
                FrameworkNS.Functions.API.SetRedraw(Me.Handle, False)
                Me.Document.Body.InnerHtml = value
                FrameworkNS.Functions.API.SetRedraw(Me.Handle, True)
            End If
        End Set
    End Property

    Public Sub SetCredentials(ByVal Username As String, ByVal Password As String)
        If Username = "" And Password = "" Then
            MyCredentials = Nothing
            Return
        End If

        MyCredentials = New BrowserCredentials(Username, Password)
        MyCredentials.AttachToWebbrowser(Me)
    End Sub

    Public Sub ClearCredentials()
        MyCredentials = Nothing
    End Sub

#Region " PROTECTED METHODS FOR EXTENDED EVENTS "
    Protected Overridable Sub OnNewWindow2(ByVal e As WebBrowserNewWindow2EventArgs)
        RaiseEvent NewWindow2(Me, e)
    End Sub

    Protected Overridable Sub OnNavigateError(ByVal e As WebBrowserNavigateErrorEventArgs)
        RaiseEvent NavigateError(Me, e)
    End Sub
#End Region

#Region "WB SINK ROUTINES"

    <PermissionSetAttribute(SecurityAction.LinkDemand, Name:="FullTrust")> _
    Protected Overrides Sub CreateSink()
        MyBase.CreateSink()
        helper = New WebBrowser2EventHelper(Me)
        cookie = New AxHost.ConnectionPointCookie(Me.ActiveXInstance, helper, GetType(DWebBrowserEvents2))
    End Sub

    <PermissionSetAttribute(SecurityAction.LinkDemand, Name:="FullTrust")> _
    Protected Overrides Sub DetachSink()
        If cookie IsNot Nothing Then
            cookie.Disconnect()
            cookie = Nothing
        End If
        MyBase.DetachSink()
    End Sub

#End Region

#Region "PROPERTIES EXPOSED THROUGH THE COM OBJECT"

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)> _
<System.Runtime.InteropServices.DispIdAttribute(200)> _
Public ReadOnly Property Application() As Object
        Get
            If IsNothing(Me.ActiveXInstance) Then
                Throw New AxHost.InvalidActiveXStateException("Application", AxHost.ActiveXInvokeKind.PropertyGet)
            End If

            Return CallByName(Me.ActiveXInstance, "Application", CallType.Get, Nothing)
            'THIS IS COMMENTED. UNCOMMENT AND REMOVE LINE BEFORE IF YOU CAN NOT USE CALLBYNAME()
            'Return Me.ActiveXInstance.Application
        End Get
    End Property

    <System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)> _
    <System.Runtime.InteropServices.DispIdAttribute(552)> _
    Public Property RegisterAsBrowser() As Boolean
        Get
            If IsNothing(Me.ActiveXInstance) Then
                Throw New AxHost.InvalidActiveXStateException("RegisterAsBrowser", AxHost.ActiveXInvokeKind.PropertyGet)
            End If

            Dim RetVal As Boolean = False
            If Not Boolean.TryParse(CallByName(Me.ActiveXInstance, "RegisterAsBrowser", CallType.Get, Nothing).ToString, RetVal) Then RetVal = False
            Return RetVal
            'THIS IS COMMENTED. UNCOMMENT AND REMOVE 3 LINES BEFORE IF YOU CAN NOT USE CALLBYNAME()
            'Return Me.ActiveXInstance.RegisterAsBrowser

        End Get
        Set(ByVal value As Boolean)
            If IsNothing(Me.ActiveXInstance) Then
                Throw New AxHost.InvalidActiveXStateException("RegisterAsBrowser", AxHost.ActiveXInvokeKind.PropertySet)
            End If

            CallByName(Me.ActiveXInstance, "RegisterAsBrowser", CallType.Let, True)
            'THIS IS COMMENTED. UNCOMMENT AND REMOVE LINE BEFORE IF YOU CAN NOT USE CALLBYNAME()
            'Me.ActiveXInstance.RegisterAsBrowser = value
        End Set
    End Property

#End Region

    'HELPER CLASS TO FIRE OFF THE EVENTS
    Private Class WebBrowser2EventHelper
        Inherits StandardOleMarshalObject
        Implements DWebBrowserEvents2

        Private parent As MCSCTRLS1.WebBrowser

        Public Sub New(ByVal parent As MCSCTRLS1.WebBrowser)
            Me.parent = parent
        End Sub

        Public Sub NewWindow2(ByRef ppDisp As Object, ByRef cancel As Boolean) Implements DWebBrowserEvents2.NewWindow2
            Dim e As New WebBrowserNewWindow2EventArgs(ppDisp)
            Me.parent.OnNewWindow2(e)
            ppDisp = e.ppDisp
            cancel = e.Cancel
        End Sub

        Public Sub NavigateError(ByVal pDisp As Object, ByRef URL As Object, ByRef frame As Object, ByRef statusCode As Object, ByRef cancel As Boolean) Implements DWebBrowserEvents2.NavigateError
            ' Raise the NavigateError event.
            Me.parent.OnNavigateError( _
                New WebBrowserNavigateErrorEventArgs( _
                CStr(URL), CStr(frame), CInt(statusCode), cancel))
        End Sub
    End Class

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.IsWebBrowserContextMenuEnabled = False

    End Sub

    Private Sub WebBrowser_NewWindow(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles Me.NewWindow
        ' Stop
    End Sub

    Private Sub WebBrowser_NewWindow2(sender As Object, e As WebBrowserNewWindow2EventArgs) Handles Me.NewWindow2
        'EasyCare.FrameWork.ShowNotification(Me.Url.ToString)
    End Sub

    Private Sub WebBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles Me.DocumentCompleted
        If bLoadBlankPage Then
            If Me.ReadyState = WebBrowserReadyState.Complete Then
                bBlankPageLoaded = True
            End If
        End If
    End Sub

    Private bLoadBlankPage As Boolean = False
    Private bBlankPageLoaded As Boolean

    Public Function NavigateToBlankPage() As Boolean
        Try
            bLoadBlankPage = True

            Me.Navigate("about:blank")

            Dim dStart As Date = Date.Now

            Do While Not bBlankPageLoaded
                Threading.Thread.Sleep(10)
                System.Windows.Forms.Application.DoEvents()

                If DateDiff(DateInterval.Second, dStart, Date.Now) > 3 Then
                    Return False
                End If
            Loop

        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

End Class


Public Class WebBrowserNewWindow2EventArgs
    Inherits System.ComponentModel.CancelEventArgs

    Private ppDispValue As Object

    Public Sub New(ByVal ppDisp As Object)
        Me.ppDispValue = ppDisp
    End Sub

    Public Property ppDisp() As Object
        Get
            Return ppDispValue
        End Get
        Set(ByVal value As Object)
            ppDispValue = value
        End Set
    End Property

End Class
Public Class WebBrowserNavigateErrorEventArgs
    Inherits EventArgs

    Private urlValue As String
    Private frameValue As String
    Private statusCodeValue As Int32
    Private cancelValue As Boolean

    Public Sub New( _
        ByVal url As String, ByVal frame As String, _
        ByVal statusCode As Int32, ByVal cancel As Boolean)

        Me.urlValue = url
        Me.frameValue = frame
        Me.statusCodeValue = statusCode
        Me.cancelValue = cancel

    End Sub

    Public Property Url() As String
        Get
            Return urlValue
        End Get
        Set(ByVal value As String)
            urlValue = value
        End Set
    End Property

    Public Property Frame() As String
        Get
            Return frameValue
        End Get
        Set(ByVal value As String)
            frameValue = value
        End Set
    End Property

    Public Property StatusCode() As Int32
        Get
            Return statusCodeValue
        End Get
        Set(ByVal value As Int32)
            statusCodeValue = value
        End Set
    End Property

    Public Property Cancel() As Boolean
        Get
            Return cancelValue
        End Get
        Set(ByVal value As Boolean)
            cancelValue = value
        End Set
    End Property

End Class

<ComImport(), Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D"), _
InterfaceType(ComInterfaceType.InterfaceIsIDispatch), _
TypeLibType(TypeLibTypeFlags.FHidden)> _
Public Interface DWebBrowserEvents2

    <DispId(DISPID.NEWWINDOW2)> Sub NewWindow2( _
        <InAttribute(), OutAttribute(), MarshalAs(UnmanagedType.IDispatch)> ByRef ppDisp As Object, _
        <InAttribute(), OutAttribute()> ByRef cancel As Boolean)

    <DispId(DISPID.NAVIGATERROR)> Sub NavigateError( _
        <InAttribute(), MarshalAs(UnmanagedType.IDispatch)> _
         ByVal pDisp As Object, _
        <InAttribute()> ByRef URL As Object, _
        <InAttribute()> ByRef frame As Object, _
        <InAttribute()> ByRef statusCode As Object, _
        <InAttribute(), OutAttribute()> ByRef cancel As Boolean)
End Interface

Public Enum DISPID
    NEWWINDOW2 = 251
    NAVIGATERROR = 271
End Enum



