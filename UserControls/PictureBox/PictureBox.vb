Imports MCS_Interfaces
<ToolboxBitmap(GetType(System.Windows.Forms.PictureBox))>
Public Class PictureBox
    Public Property TooltipText As String = ""
    Public Property TooltipTitle As String = ""
    Private Sub PictureBox_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        MCSCTRLS2.EasyTooltip.ShowTooltip(True)
    End Sub
    Private Sub PictureBox_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        MCSCTRLS2.EasyTooltip.HideTooltip()
    End Sub
    Private Sub PictureBox_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        Dim sTitle As String
        sTitle = IIf(Me.TooltipTitle.isEmptyOrWhiteSpace, "Info", Me.TooltipTitle)
        MCSCTRLS2.EasyTooltip.SetTooltip(sTitle, Me.TooltipText)
    End Sub
End Class
