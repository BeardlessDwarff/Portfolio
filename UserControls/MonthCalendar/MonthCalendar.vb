<ToolboxBitmap(GetType(System.Windows.Forms.MonthCalendar))> _
Public Class MonthCalendar
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)

        'Bug van Microsoft...
        Select Case m.Msg
            Case FrameworkNS.Functions.API.Messages.WM_PAINT
                If (Me.MaxSelectionCount = 1) AndAlso (Me.SelectionEnd <> Me.SelectionStart) Then
                    Me.SelectionEnd = Me.SelectionStart
                End If
        End Select

        MyBase.WndProc(m)
    End Sub
End Class
