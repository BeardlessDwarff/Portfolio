<ToolboxBitmap(GetType(System.Windows.Forms.LinkLabel))> _
Public Class LinkLabel
    Public Property URL As String = ""


    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.ForeColor = Color.Blue
        Me.Font = New System.Drawing.Font(Control.DefaultFont, FontStyle.Underline)

        Me.Cursor = Cursors.Hand
    End Sub

    Public Shadows Event Click(sender As Object, e As EventArgs)

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        RaiseEvent Click(Me, e)
    End Sub



End Class
