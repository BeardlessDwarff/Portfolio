Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.Button))>
Public Class Button

    Private MyTooltipTekst As String = ""
    Private MyTooltipTitle As String = ""

    Public Property TooltipDontWait As Boolean = False

    Public Property ToolTipText As String
        Get
            Return MyTooltipTekst
        End Get
        Set(value As String)
            MyTooltipTekst = value
        End Set
    End Property

    Public Property ToolTipTitle As String
        Get
            Return MyTooltipTitle
        End Get
        Set(value As String)
            MyTooltipTitle = value
        End Set
    End Property

    Private Sub Button_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        MCSCTRLS2.EasyTooltip.HideTooltip()
    End Sub

    Private Sub Button_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If Me.ToolTipText.Trim = "" Then Return
        MCSCTRLS2.EasyTooltip.SetTooltip(Me.ToolTipTitle, Me.ToolTipText)
        MCSCTRLS2.EasyTooltip.ShowTooltip(True)
    End Sub

    Private Sub Button_Click(sender As Object, e As EventArgs) Handles Me.Click
        RaiseControlValueChanged("Click")
    End Sub

    Public Shadows Event Click(sender As Object, e As EventArgs)

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        RaiseEvent Click(Me, e)
    End Sub

    Private Sub RaiseControlValueChanged(Key As String)
        Dim MyReceiver As iControlValueChangedReceiver = Me.FirstParent(Of iControlValueChangedReceiver)

        If MyReceiver Is Nothing Then Return

        MyReceiver.ControlValueChanged(Me, Key)
    End Sub

    Private Sub Button_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
        RaiseControlValueChanged("MouseEnter")
    End Sub
End Class
