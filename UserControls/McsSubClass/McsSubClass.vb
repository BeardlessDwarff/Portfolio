Public Class McsSubClass
    Inherits System.Windows.Forms.NativeWindow
    Implements IDisposable

    Public Event WinProc(ByRef m As Message, ByRef KillMessage As Boolean)

    Public Sub New()

    End Sub

    Public Sub New(ByVal Handle As IntPtr)
        MyBase.AssignHandle(Handle)
    End Sub

    <DebuggerNonUserCode()> _
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Dim bKill As Boolean = False
        RaiseEvent WinProc(m, bKill)

        If Not bKill Then MyBase.WndProc(m)
    End Sub

    '<DebuggerNonUserCode()> _
    'Protected Overrides Sub Finalize()
    '    MyBase.DestroyHandle()
    '    MyBase.Finalize()
    'End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                
            End If

            Me.ReleaseHandle()
            
            
        End If
        Me.disposedValue = True
    End Sub

    
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
