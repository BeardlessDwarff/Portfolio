<ToolboxBitmap(GetType(System.Windows.Forms.ListBox))> _
Public Class ListBox
    Protected Overrides Function IsInputKey(keyData As System.Windows.Forms.Keys) As Boolean

        Select Case keyData
            Case Keys.Tab
                Return True
        End Select

        Return MyBase.IsInputKey(keyData)
    End Function
End Class
