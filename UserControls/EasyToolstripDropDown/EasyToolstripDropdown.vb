Imports System.Runtime.InteropServices

Public Class EasyToolstripDropDown
    Inherits ToolStripDropDown

    '(C)opyRight By EBS (Erik's Brilliant Solutions)

    <DllImport("user32.dll")> _
    Private Shared Function GetAsyncKeyState(ByVal vKey As Int32) As Short
    End Function


    Private bAutoClose As Boolean = True
    Private WithEvents MyTimer As System.Windows.Forms.Timer
    Private MouseUpGeweest As Boolean = False

    Public Overloads Property AutoClose() As Boolean
        Get
            Return bAutoClose
        End Get
        Set(ByVal value As Boolean)
            bAutoClose = value
        End Set
    End Property

    Public Sub New()
        'Er zit een fout in de autoclose van Microsoft... 
        'Nooit gebruiken dus, we maken onze eigen autoclose.
        MyBase.AutoClose = False
        Me.Padding = Padding.Empty
    End Sub

    Private Sub EasyToolstripDropDown_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If Not MyTimer Is Nothing Then
            MyTimer.Stop()
            MyTimer.Dispose()
            MyTimer = Nothing
        End If
    End Sub

    Private Sub EasyToolstripDropDown_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
        If Me.Visible And bAutoClose Then
            StartTimer()
        Else
            If Not MyTimer Is Nothing Then MyTimer.Stop()
        End If
    End Sub

    Private Sub StartTimer()
        MyTimer = New System.Windows.Forms.Timer
        MouseUpGeweest = False
        MyTimer.Interval = 10
        MyTimer.Start()
    End Sub

    Private Sub MyTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyTimer.Tick
        If Not bAutoClose Then
            MyTimer.Stop()
            Return
        End If

        CheckMouseDown()
    End Sub

    Private Sub CheckMouseDown()
        Dim pntLinksBoven As Point = New Point(Me.Left, Me.Top)
        Dim ControlRectInScreen As New Rectangle(pntLinksBoven, Me.ClientRectangle.Size)

        If GetAsyncKeyState(27) OrElse GetAsyncKeyState(9) OrElse My.Computer.Keyboard.AltKeyDown OrElse My.Computer.Keyboard.CtrlKeyDown Then 'Escape
            MyTimer.Stop()
            Me.Close()
            Return
        End If

        If Control.MouseButtons = Windows.Forms.MouseButtons.None Then
            MouseUpGeweest = True
            Return
        End If

   

        If Not MouseUpGeweest Then Return
        'We moeten de control verbergen als er BUITEN de control wordt geklikt.

        If Control.MouseButtons <> Windows.Forms.MouseButtons.None Then
            If Not ControlRectInScreen.Contains(MousePosition) Then
                MyTimer.Stop()
                Me.Close()
            End If
        End If
    End Sub
End Class

