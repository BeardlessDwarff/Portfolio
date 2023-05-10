Imports System.Runtime.InteropServices

Public Class BrowserCredentials
    Implements IOleClientSite, IAuthenticate, IServiceProvider

    Public Shared IID_IAuthenticate As New Guid("79eac9d0-baf9-11ce-8c82-00aa004ba90b")
    Public Const INET_E_DEFAULT_ACTION As Integer = -2146697199
    Public Const S_OK As Integer = 0

    Private _UserName As String
    Private _Password As String

    Public Function Authenticate(ByRef phwnd As System.IntPtr, ByRef pszUsername As System.IntPtr, ByRef pszPassword As System.IntPtr) As Integer Implements IAuthenticate.Authenticate
        Dim sUser As IntPtr = Marshal.StringToCoTaskMemAuto(_UserName)
        Dim sPassword As IntPtr = Marshal.StringToCoTaskMemAuto(_Password)

        pszUsername = sUser
        pszPassword = sPassword
        Return S_OK
    End Function

    Public Sub GetContainer(ByVal ppContainer As Object) Implements IOleClientSite.GetContainer
        ppContainer = Me
    End Sub

    Public Sub GetMoniker(ByVal dwAssign As UInteger, ByVal dwWhichMoniker As UInteger, ByVal ppmk As Object) Implements IOleClientSite.GetMoniker

    End Sub
    Public Sub OnShowWindow(ByVal fShow As Boolean) Implements IOleClientSite.OnShowWindow

    End Sub
    Public Sub RequestNewObjectLayout() Implements IOleClientSite.RequestNewObjectLayout

    End Sub
    Public Sub SaveObject() Implements IOleClientSite.SaveObject

    End Sub
    Public Sub ShowObject() Implements IOleClientSite.ShowObject

    End Sub

    Public Function QueryService(ByRef guidService As System.Guid, ByRef riid As System.Guid, ByRef ppvObject As System.IntPtr) As Integer Implements IServiceProvider.QueryService
        Dim nRet As Integer = guidService.CompareTo(IID_IAuthenticate)
        ' Zero returned if the compared objects are equal 
        If nRet = 0 Then
            nRet = riid.CompareTo(IID_IAuthenticate)
            ' Zero returned if the compared objects are equal 
            If nRet = 0 Then
                ppvObject = Marshal.GetComInterfaceForObject(Me, GetType(IAuthenticate))
                Return S_OK
            End If
        End If
        ppvObject = New IntPtr()
        Return INET_E_DEFAULT_ACTION
    End Function

    Public Sub New(ByVal Username As String, ByVal Password As String)
        _UserName = Username
        _Password = Password
    End Sub

    Public Sub AttachToWebbrowser(ByVal Browser As MCSCTRLS1.WebBrowser)
        Browser.Navigate("about:blank")

        Dim obj As Object = Browser.ActiveXInstance
        Dim oc As IOleObject = TryCast(obj, IOleObject)
        oc.SetClientSite(TryCast(Me, IOleClientSite))
    End Sub
End Class
<ComImport(), Guid("00000118-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
Public Interface iOleClientSite
    Sub SaveObject()
    Sub GetMoniker(ByVal dwAssign As UInteger, ByVal dwWhichMoniker As UInteger, ByVal ppmk As Object)
    Sub GetContainer(ByVal ppContainer As Object)
    Sub ShowObject()
    Sub OnShowWindow(ByVal fShow As Boolean)
    Sub RequestNewObjectLayout()
End Interface
<ComImport(), GuidAttribute("79EAC9D0-BAF9-11CE-8C82-00AA004BA90B"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), ComVisible(False)> _
Public Interface iAuthenticate
    <PreserveSig()> _
    Function Authenticate(ByRef phwnd As IntPtr, ByRef pszUsername As IntPtr, ByRef pszPassword As IntPtr) As <MarshalAs(UnmanagedType.I4)> Integer
End Interface
<ComImport(), Guid("00000112-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface iOleObject
    Sub SetClientSite(ByVal pClientSite As IOleClientSite)
    Sub GetClientSite(ByVal ppClientSite As IOleClientSite)
    Sub SetHostNames(ByVal szContainerApp As Object, ByVal szContainerObj As Object)
    Sub Close(ByVal dwSaveOption As UInteger)
    Sub SetMoniker(ByVal dwWhichMoniker As UInteger, ByVal pmk As Object)
    Sub GetMoniker(ByVal dwAssign As UInteger, ByVal dwWhichMoniker As UInteger, ByVal ppmk As Object)
    Sub InitFromData(ByVal pDataObject As IDataObject, ByVal fCreation As Boolean, ByVal dwReserved As UInteger)
    Sub GetClipboardData(ByVal dwReserved As UInteger, ByVal ppDataObject As IDataObject)
    Sub DoVerb(ByVal iVerb As UInteger, ByVal lpmsg As UInteger, ByVal pActiveSite As Object, ByVal lindex As UInteger, ByVal hwndParent As UInteger, ByVal lprcPosRect As UInteger)
    Sub EnumVerbs(ByVal ppEnumOleVerb As Object)
    Sub Update()
    Sub IsUpToDate()
    Sub GetUserClassID(ByVal pClsid As UInteger)
    Sub GetUserType(ByVal dwFormOfType As UInteger, ByVal pszUserType As UInteger)
    Sub SetExtent(ByVal dwDrawAspect As UInteger, ByVal psizel As UInteger)
    Sub GetExtent(ByVal dwDrawAspect As UInteger, ByVal psizel As UInteger)
    Sub Advise(ByVal pAdvSink As Object, ByVal pdwConnection As UInteger)
    Sub Unadvise(ByVal dwConnection As UInteger)
    Sub EnumAdvise(ByVal ppenumAdvise As Object)
    Sub GetMiscStatus(ByVal dwAspect As UInteger, ByVal pdwStatus As UInteger)
    Sub SetColorScheme(ByVal pLogpal As Object)
End Interface
<ComImport(), GuidAttribute("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), ComVisible(False)> _
Public Interface iServiceProvider
    <PreserveSig()> _
    Function QueryService(ByRef guidService As Guid, ByRef riid As Guid, ByRef ppvObject As IntPtr) As <MarshalAs(UnmanagedType.I4)> Integer
End Interface

