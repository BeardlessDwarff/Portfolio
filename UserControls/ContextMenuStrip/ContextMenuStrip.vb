Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.ContextMenuStrip)), Obsolete("mcsctrls2.contextmenu gebruiken.")>
Public Class ContextMenuStrip
    Private MyFramework As FrameWorkNS.FrameWork = EasyCare.FrameWork
    '    <Obsolete("mcsctrls2.contextmenu gebruiken.")> _
    Public Shadows Event ItemClicked(Sender As Object, E As ToolStripItemClickedEventArgs)

    Public Sub RaiseItemClicked(Sender As Object, E As ToolStripItemClickedEventArgs) Handles MyBase.ItemClicked
        RaiseEvent ItemClicked(Sender, E)
    End Sub


    Public Sub ShowOnDefaultLocation()
        Dim MyPoint As New Point(Cursor.Position.X, Cursor.Position.Y)
        MyPoint.Offset(5, 5)
        MyBase.Show(MyPoint)
    End Sub

    Public Function GetItemByText(Text As String) As ToolStripMenuItem
        For Each Item As ToolStripMenuItem In Me.Items
            If Item.Text.ToLower = Text.ToLower Then
                Return Item
            End If
        Next

        Return Nothing
    End Function

    Private Sub ContextMenuStrip_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Opening
        'For Each Item As ToolStripItem In Me.Items
        '    If TypeOf Item Is ToolStripMenuItem Then SetItemText(Item)
        'Next
    End Sub

    Public Enum eDefaultItem
        Verwijderen = 1
        Seperator = 2
        Naam_wijzigen = 3
    End Enum

    Public Function AddDefaultItem(Item As eDefaultItem) As ToolStripItem
        Dim MyItem As ToolStripItem

        Select Case Item
            Case eDefaultItem.Verwijderen
                MyItem = Me.Items.Add("Verwijderen", EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Delete))
                MyItem.Tag = "DELETE"

            Case eDefaultItem.Seperator
                MyItem = Me.Items.Add("-")

            Case eDefaultItem.Naam_wijzigen
                MyItem = Me.Items.Add("Naam wijzigen", EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Rename))
                MyItem.Tag = "RENAME"

            Case Else
                Return Nothing
        End Select

        Return MyItem
    End Function

    Private Sub SetItemText(ByVal Item As ToolStripMenuItem)
        Item.Text = MyFramework.GeefVertaling(Item.Text)
        Item.ToolTipText = MyFramework.GeefVertaling(Item.ToolTipText)

        For Each SubItem As ToolStripItem In Item.DropDownItems
            If Not TypeOf SubItem Is ToolStripMenuItem Then Continue For
            SetItemText(SubItem)
        Next
    End Sub

End Class
