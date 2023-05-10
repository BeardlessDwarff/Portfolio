Imports System.ComponentModel
Imports System.IO
Imports System.Threading.Tasks
Imports CefSharp
Imports CefSharp.WinForms
Public Class frmInnerStatus

    Private WithEvents MyBrowser As ChromiumWebBrowser
    Private sHeader As String = "Een ogenblik geduld"
    Private sFooter As String = "Bezig met laden"

    Public Property StatusForm As frmStatus

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Icon = Nothing
        Me.ShowInTaskbar = False

        Me.Cursor = Cursors.WaitCursor
    End Sub

    Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
        Get
            Dim WS_EX_TOPMOST As Integer = &H8&
            Dim WS_EX_NOACTIVATE As Integer = &H8000000

            Dim MyParams As System.Windows.Forms.CreateParams = MyBase.CreateParams
            MyParams.ExStyle = MyParams.ExStyle Or &H80 Or WS_EX_TOPMOST Or WS_EX_NOACTIVATE
            Return MyParams
        End Get
    End Property

    Private Sub MyBrowser_FrameLoadEnd(sender As Object, e As FrameLoadEndEventArgs) Handles MyBrowser.FrameLoadEnd

    End Sub

    Private Sub frmInnerStatus_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Dim sHTML As String

        sHTML = My.Resources.StatusHTML

        sHTML = sHTML.Replace("HEADERTEXT", Me.Header.HtmlEncode)
        sHTML = sHTML.Replace("FOOTERTEXT", Me.Footer.HtmlEncode)

        Dim x As New Web.HtmlString(sHTML, False, Nothing)

        MyBrowser = New ChromiumWebBrowser(x, Nothing)

        Me.Padding = New Padding(0)

        MyBrowser.Dock = DockStyle.Fill
        Me.Controls.Add(MyBrowser)

        Me.Button1.BringToFront()
    End Sub

    Private Sub MyBrowser_LoadingStateChanged(sender As Object, e As LoadingStateChangedEventArgs) Handles MyBrowser.LoadingStateChanged

        If Me.InvokeRequired Then
            Me.Invoke(Sub()
                          MyBrowser_LoadingStateChanged(sender, e)
                      End Sub)
            Return
        End If

        If e.IsLoading Then Return

        StatusForm.LoadingCompleted()

        MyBrowser.Cursor = Cursors.WaitCursor
        'MyBrowser.ShowDevTools()
    End Sub

    Public Property Header As String
        Get
            If sHeader = "" Then Return "Een ogenblik geduld"
            Return sHeader
        End Get
        Set(value As String)
            If sHeader = value Then Return
            sHeader = value

            Me.UpdateHeaderText(value)
        End Set
    End Property

    Public Property Footer As String
        Get
            Return sFooter
        End Get
        Set(value As String)
            If sFooter = value Then Return
            sFooter = value

            Me.UpdateFooterText(value)
        End Set
    End Property
    Private Sub UpdateHeaderText(sText As String)
        If Me.MyBrowser Is Nothing Then Return

        If Not Me.MyBrowser.IsBrowserInitialized Then Return

        Dim sScript As String
        Dim MyTask As Task

        sScript = $"updateHeaderText(""{sText}"");"

        MyTask = MyBrowser.EvaluateScriptAsync(sScript, Nothing, False)

        MyTask.Wait(1000)

    End Sub

    Private Sub UpdateFooterText(sText As String)
        If MyBrowser Is Nothing Then Return

        If Not Me.MyBrowser.IsBrowserInitialized Then Return

        Dim sScript As String
        Dim MyTask As Task = Nothing

        sScript = $"updateFooterText(""{sText.HtmlEncode}"");"

        MyTask = MyBrowser.EvaluateScriptAsync(sScript, Nothing, False)

        'MyBrowser.EvaluateScriptAsync(sScript, Nothing, False)

        If Not MyTask Is Nothing Then MyTask.Wait(1000)

    End Sub

    Public Sub ShowSource()
        Dim ts As Task = GetSource()
    End Sub

    Private Async Function GetSource() As Task(Of String)

        Dim MySource = Await Me.MyBrowser.GetBrowser().MainFrame.GetSourceAsync()

        Dim f As String = Application.StartupPath + "/currentSource.txt"

        Dim wr As StreamWriter = New StreamWriter(f, False, System.Text.Encoding.Default)
        wr.Write(MySource)
        wr.Close()

        System.Diagnostics.Process.Start(f)

        Return Nothing

    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        MyBrowser.ShowDevTools
        Me.Button1.Visible = False
    End Sub
End Class