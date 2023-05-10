Imports MCS_Interfaces

Public Class frmAutoCorrectielijst

    Private WithEvents MyPrintDocument As MCSCTRLS1.PrintDocument

    Private Sub btnOk_Click(sender As System.Object, e As System.EventArgs) Handles btnOk.Click
        Me.Close()
    End Sub

    Private Sub frmAutoCorrectielijst_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim Item As FrameWorkNS.TextControl.AutoCorrectieItem
        Dim MyListItems As New List(Of ListViewItem)
        Dim MyItem As ListViewItem

        For Each Item In FrameWorkNS.TextControl.AutoCorrectieItems.Values
            MyItem = New ListViewItem(Item.Word)
            MyItem.SubItems.Add(Item.Correction)
            MyItem.SubItems.Add(Item.Nivo.ToString)
            MyListItems.Add(MyItem)
        Next

        Me.ListView1.BeginUpdate()
        Me.ListView1.Items.Clear()
        Me.ListView1.Items.AddRange(MyListItems.ToArray)
        Me.ListView1.EndUpdate()
    End Sub


    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.tsbPrintPreview.Image = FrameWorkNS.Functions.Forms.GetDefaultImage(FrameWorkNS.Functions.Forms.eDefaultImage.PrintPreview)
        Me.tsbPrint.Image = EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Print)
    End Sub



    Private Sub tsbPrintPreview_Click(sender As System.Object, e As System.EventArgs) Handles tsbPrintPreview.Click
        MyPrintDocument = New MCSCTRLS1.PrintDocument

        Using MyDialog As New MCSCTRLS1.PrintPreviewDialog
            MyDialog.Document = MyPrintDocument
            MyDialog.ShowDialog()
        End Using
    End Sub

    Private iPagina As Integer

    Private Sub MyPrintDocument_BeginPrint(sender As Object, e As System.Drawing.Printing.PrintEventArgs) Handles MyPrintDocument.BeginPrint
        iPagina = 1
        Me.ListView1.BeginPrint()
    End Sub

    Private Sub MyPrintDocument_PrintPage(sender As Object, e As System.Drawing.Printing.PrintPageEventArgs) Handles MyPrintDocument.PrintPage
        Dim MyEasyPrint As New FrameWorkNS.EasyPrint(e, MyPrintDocument)
        Dim MyRect As Rectangle

        MyEasyPrint.Title = "Autocorrectielijst"
        MyEasyPrint.AddHeaderItem("Datum", FrameWorkNS.Functions.Algemeen.GiveDateString(Now), FrameWorkNS.EasyPrint.eColumn.Left)
        MyEasyPrint.AddHeaderItem("Pagina", iPagina.ToString, FrameWorkNS.EasyPrint.eColumn.Right)

        MyRect = MyEasyPrint.Print

        e.HasMorePages = Me.ListView1.Print(e.Graphics, MyRect)

        iPagina += 1
    End Sub

    Private Sub tsbPrint_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles tsbPrint.MouseUp
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Right
                FrameWorkNS.Functions.PrintFuncties.SetPrinterSettings("AUTOCORRECTIE")

            Case Windows.Forms.MouseButtons.Left
                Dim Settings As Printing.PrinterSettings = FrameWorkNS.Functions.PrintFuncties.GetPrinterSettings("AUTOCORRECTIE")
                If Settings Is Nothing Then
                    Settings = FrameWorkNS.Functions.PrintFuncties.SetPrinterSettings("AUTOCORRECTIE")
                    If Settings Is Nothing Then
                        MCSCTRLS2.Functions.Msgbox("Er is geen printer ingesteld, kies een printer bij dit onderdeel" & vbCrLf & vbCrLf & "Het is niet mogelijk om te printen", MsgBoxStyle.OkOnly + MsgBoxStyle.Information, "Niet mogelijk")
                        Return
                    End If
                End If
                MyPrintDocument = New MCSCTRLS1.PrintDocument
                MyPrintDocument.PrinterSettings = Settings
                MyPrintDocument.Print()
        End Select
    End Sub


End Class