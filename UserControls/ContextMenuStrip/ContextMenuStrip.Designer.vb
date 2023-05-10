<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ContextMenuStrip
    Inherits System.Windows.Forms.ContextMenuStrip

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        MyFramework = Nothing
        If disposing Then

            Try
            If Not Me.Items Is Nothing Then
                Me.Items.Clear()
            End If
            Catch ex As Exception

            End Try
            

        End If
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
    End Sub

End Class
