Imports MCS_Interfaces
Imports System.Threading
Imports System.ComponentModel

<ToolboxBitmap(GetType(System.ComponentModel.BackgroundWorker))> _
Public Class BackgroundWorker
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.WorkerSupportsCancellation = True
        Me.WorkerReportsProgress = True


    End Sub

    Public Event BeforeRun(Argument As Object, ASync As Boolean)

    Public Overloads Sub RunWorkerAsync()
        Me.WaitForReady

        RaiseEvent BeforeRun(Nothing, True)

        MyBase.RunWorkerAsync()
    End Sub

    Public Overloads Sub RunWorkerAsync(Argument As Object)
        Me.WaitForReady
        RaiseEvent BeforeRun(Argument, True)

        MyBase.RunWorkerAsync(Argument)
    End Sub

    Public Sub RunWorkerSync(Argument As Object)
        MCS_Interfaces.EasyCare.Hourglass = True

        RaiseEvent BeforeRun(Argument, False)

        Dim e As New DoWorkEventArgs(Argument)
        Me.OnDoWork(e)
        Dim X As New RunWorkerCompletedEventArgs(e.Result, Nothing, False)
        Me.OnRunWorkerCompleted(X)

        MCS_Interfaces.EasyCare.Hourglass = False
    End Sub

    Public Sub RunWorkerSync()
        Me.RunWorkerSync(Nothing)
    End Sub
End Class
