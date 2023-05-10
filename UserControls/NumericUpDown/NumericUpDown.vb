<ToolboxBitmap(GetType(System.Windows.Forms.NumericUpDown))> _
Public Class NumericUpDown

    Private bAutoCorrectValueToMinMax As Boolean = False
    Public Property IgnoreMouseWheel As Boolean = True 'Het scrollen in een numericUpDOwn met het mousewheel gebeurt vaak per ongeluk, heeft box wel de focus maar willen niet scrollen. Standaard negeren we het mousewheel-bericht.

    Public Property AutoCorrectValueToMinMax() As Boolean
        Get
            Return bAutoCorrectValueToMinMax
        End Get
        Set(ByVal value As Boolean)
            bAutoCorrectValueToMinMax = value
        End Set
    End Property

    Public Overloads Property Value() As Decimal
        Get
            Return MyBase.Value
        End Get
        Set(ByVal value As Decimal)
            If bAutoCorrectValueToMinMax Or True Then
                If value > MyBase.Maximum Then value = MyBase.Maximum
                If value < MyBase.Minimum Then value = MyBase.Minimum
            End If

            If Debugger.IsAttached Then
                If value > MyBase.Maximum Then Stop
                If value < MyBase.Minimum Then Stop
            End If

            MyBase.Value = value
        End Set
    End Property

    Private Sub NumericUpDown_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        'If Me.Focus Then Return
        DirectCast(e, HandledMouseEventArgs).Handled = IgnoreMouseWheel
    End Sub
End Class
