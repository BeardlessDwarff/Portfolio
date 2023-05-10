Imports MCS_Interfaces

Public Class ToolStrip

    Public Sub New()
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Public Sub CheckButtonImagesSize()
        Dim MyImage As Image

        For Each Item As ToolStripItem In Me.Items
            If Item.Image Is Nothing Then Continue For
            If Item.Image.Width <> Item.Image.Height Then Continue For 'PreserveRatio eerst verder uitwerken.

            If Item.Image.Size.Width <> Me.ImageScalingSize.Width Then
                MyImage = Item.Image.Resize(Me.ImageScalingSize.Width, Me.ImageScalingSize.Width)
                Item.Image = MyImage
            End If
        Next
    End Sub

    Private Sub ItemTextChanged(ByVal Sender As Object, ByVal e As EventArgs)
        Dim sText As String = ""

        With DirectCast(Sender, ToolStripItem)
            .Text = FrameWorkNS.Functions.Language.GeefVertaling(.Text)
            .ToolTipText = FrameWorkNS.Functions.Language.GeefVertaling(.ToolTipText)
        End With


    End Sub

    Private Sub ToolStrip_ItemAdded(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemEventArgs) Handles Me.ItemAdded
        AddHandler e.Item.TextChanged, AddressOf ItemTextChanged
        AddHandler e.Item.MouseEnter, AddressOf ItemTextChanged
    End Sub

    Private Sub ToolStrip_ItemRemoved(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemEventArgs) Handles Me.ItemRemoved
        RemoveHandler e.Item.TextChanged, AddressOf ItemTextChanged
        RemoveHandler e.Item.MouseEnter, AddressOf ItemTextChanged
    End Sub
End Class
