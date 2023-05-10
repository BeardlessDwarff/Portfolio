Imports MCS_Interfaces.EasyCare.Functions.MemoryManagement

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ListView
    Inherits System.Windows.Forms.ListView

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            Try

            Catch ex As Exception

            End Try
        End If

        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(tmrCheckSelectedItem)
        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(Me.ContextMenu)
        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(MyFrameWork)
        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(MySettings)
        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(oComparer)
        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(Me.LargeImageList)
        MCS_Interfaces.EasyCare.Functions.MemoryManagement.DisposeObject(Me.SmallImageList)

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
