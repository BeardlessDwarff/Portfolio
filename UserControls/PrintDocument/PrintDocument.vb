Imports System.Drawing.Printing
Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Drawing.Printing.PrintDocument))>
Public Class PrintDocument

    Private iPageCount As Integer = 0
    Private iPageNumber As Integer

    Public Function PageNumber() As Integer
        Return iPageNumber
    End Function

    Public Function PageCount() As Integer
        Return iPageCount
    End Function

    Public Sub SetPageCount()
        Cursor.Current = Cursors.WaitCursor
        iPageCount = FrameworkNS.Functions.PrintFuncties.PageCount(Me)
    End Sub

    Public Sub SetSmallestMargins()
        Return

        Try
            Me.OriginAtMargins = False

            Dim iMargeX As Integer = Me.DefaultPageSettings.HardMarginX + 5
            Dim iMargeY As Integer = Me.DefaultPageSettings.HardMarginY + 5

            iMargeX = iMargeX.Min(35)
            iMargeY = iMargeX.Min(35)

            Me.DefaultPageSettings.Margins = New Margins(iMargeX, iMargeX, iMargeY, iMargeY)
        Catch ex As Exception
            If Debugger.IsAttached Then Stop
        End Try

    End Sub

    Public Sub New()
        ' This call is required by the designer.

        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.PrintController = New MCSCTRLS2.EasyPrintController
        Me.OriginAtMargins = False

        'SetSmallestMargins()
    End Sub

    Public Shadows Sub Print()
        Try
            MyBase.Print()
        Catch ex As Exception
            Dim sException As String = ex.Message

            If Debugger.IsAttached Then Stop
        End Try
    End Sub

    Private Sub PrintDocument_BeginPrint(sender As Object, e As PrintEventArgs) Handles Me.BeginPrint
        iPageNumber = 0
    End Sub

    Private Sub PrintDocument_PrintPage(sender As Object, e As PrintPageEventArgs) Handles Me.PrintPage
        iPageNumber += 1
    End Sub
End Class
