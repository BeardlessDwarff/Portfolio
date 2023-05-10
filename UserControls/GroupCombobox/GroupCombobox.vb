Imports System.ComponentModel
Imports Voorbeelden

Public Class GroupCombobox

    Public Event SelectedItemChanged(SelectedItem As Object)
    Public ItemIndent As Integer
    Public GroupIndent As Integer
    Public lstSortedGroups As New List(Of String)
    Public iDropDownButtonWidth As Integer = 17
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Items As New List(Of Object)
    Public Property DropdownHeight As Integer = 106
    Public Property ItemHeight As Integer = 14
    Public Property GroupFont As Font 'New Font("Microsoft Sans Serif", 8.25, FontStyle.Bold, GraphicsUnit.Point) 
    Public Property DisplayMember As String = ""
    Public Property GroupMember As String = ""
    Public Property ItemImageMember As String = ""

    Public Event GetGroupImage(Groupname As String, ByRef Image As Image)
    Public Event GetItemImage(ItemName As String, ByRef Image As Image)
    Public Event DrawGroup(ByRef G As Graphics, Group As String, ByRef Rect As Rectangle, ByRef Handled As Boolean)
    Public Event DrawItem(ByRef G As Graphics, ByRef Item As Object, ByRef Rect As Rectangle, ByRef Handled As Boolean)
    Public Event GetGroupSortResult(Groep1 As String, Groep2 As String, ByRef Result As Integer)
    Public Event GetItemSortResult(Item1 As Object, Item2 As Object, ByRef Result As Integer)

    Private MyDropDownButtonRect As Rectangle
    Private bMouseOver As Boolean
    Private MyPopup As GroupComboboxPopup
    Private mySelectedItem As Object
    Private MyControlHost As ToolStripControlHost
    Private WithEvents MyDropDown As ToolStripDropDown

    Public Property SelectedItem As Object
        Get
            Return mySelectedItem
        End Get
        Set(value As Object)
            If value Is mySelectedItem Then Return

            mySelectedItem = value

            RaiseEvent SelectedItemChanged(value)

            Me.Invalidate()
        End Set
    End Property

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw, True)

        Me.GroupFont = New Font(Me.Font, FontStyle.Bold)

    End Sub

    Public Sub ClosePopup()
        MyDropDown.Close()
    End Sub

    Public Function GetGroupName(O As Object) As String
        If Me.GroupMember = "" Then Return ""

        Dim sGroup As String

        sGroup = PropertyGetterNiels.GetProperty(Of String)(O, Me.GroupMember)
        'sGroup = extObject.GetPropertyByName(Of String)(O, Me.GroupMember)

        Return sGroup
    End Function

    Public Function GetDisplayMember(O As Object) As String
        If Me.DisplayMember = "" Then Return O.GetType.ToString

        Dim sResult As String = ""
        'sResult = extObject.GetPropertyByName(Of String)(O, Me.DisplayMember)

        sResult = PropertyGetterNiels.GetProperty(Of String)(O, Me.DisplayMember)

        Return sResult
    End Function

    Public Function GetItemImageMember(O As Object) As Image
        If Me.ItemImageMember = "" Then Return Nothing

        Dim result As Image

        result = PropertyGetterNiels.GetProperty(Of Image)(O, Me.ItemImageMember)
        'result = extObject.GetPropertyByName(Of Image)(O, Me.ItemImageMember)

        If Not result Is Nothing Then

        End If

        Return result
    End Function

    Public Function Groups() As List(Of String)
        Dim lstResult As New List(Of String)
        Dim sGroup As String

        For Each Item As Object In Me.Items
            sGroup = GetGroupName(Item)

            If sGroup = "" Then Continue For

            If lstResult.Contains(sGroup) Then Continue For

            lstResult.Add(sGroup)
        Next

        Return lstResult
    End Function

    Private Sub SetGroupOrder()
        lstSortedGroups.Clear()
        lstSortedGroups.AddRange(Me.Groups.ToArray)

        lstSortedGroups.Sort(AddressOf GetGroupSortResult_)
    End Sub

    Private Function GetGroupSortResult_(Groep1 As String, Groep2 As String) As Integer
        Dim Result As Integer

        Result = Groep1.CompareTo(Groep2)

        RaiseEvent GetGroupSortResult(Groep1, Groep2, Result)

        Return Result
    End Function

    Private Function GetSortResult(Item1 As Object, Item2 As Object) As Integer
        Dim sGroep1 As String
        Dim sGroep2 As String
        Dim sText1 As String
        Dim sText2 As String

        sGroep1 = GetGroupName(Item1)
        sGroep2 = GetGroupName(Item2)

        If sGroep1 <> sGroep2 Then Return lstSortedGroups.IndexOf(sGroep1).CompareTo(lstSortedGroups.IndexOf(sGroep2))

        Dim iResult As Integer

        sText1 = GetDisplayMember(Item1)
        sText2 = GetDisplayMember(Item2)

        iResult = sText1.CompareTo(sText2)

        RaiseEvent GetItemSortResult(Item1, Item2, iResult)

        Return iResult
    End Function

    Private Sub SortItems()
        SetGroupOrder()
        Me.Items.Sort(AddressOf GetSortResult)
    End Sub



    Private bPopupShown As Boolean = False

    Private Sub CreateGroupWrappers()
        Dim MyWrapper As GroupComboboxPopup.ObjectWrapper

        For Each sGroup In Me.lstSortedGroups
            MyWrapper = New GroupComboboxPopup.ObjectWrapper

            With MyWrapper
                .CanSelect = False
                .Text = sGroup
                .Group = sGroup
            End With

            MyPopup.dicGroupInfo.Add(sGroup, MyWrapper)
        Next
    End Sub

    Private Sub CreateItemWrappers()
        Dim MyWrapper As GroupComboboxPopup.ObjectWrapper

        For Each Item As Object In Me.Items
            MyWrapper = New GroupComboboxPopup.ObjectWrapper

            With MyWrapper
                .CanSelect = True
                .Item = Item
                .Group = GetGroupName(Item)
                .Text = GetDisplayMember(Item)
            End With

            MyPopup.dicItemInfo.Add(Item, MyWrapper)
        Next
    End Sub

    Private Sub RunOnFirstPopup()
        If bPopupShown Then Return 'Only once.
        bPopupShown = True

        SortItems()

        CreateGroupWrappers()
        CreateItemWrappers()

        GetItemImages()
        GetGroupImages()
    End Sub

    Private Sub GetItemImages()
        Dim myImage As Image = Nothing
        Dim wrapper As GroupComboboxPopup.ObjectWrapper
        For Each item As Object In Items
            myImage = Nothing
            wrapper = MyPopup.dicItemInfo(item)

            RaiseEvent GetItemImage(wrapper.Text, myImage)

            If myImage Is Nothing Then myImage = GetItemImageMember(item)
            If myImage Is Nothing Then Continue For

            wrapper.Image = myImage
            If myImage.Width > ItemIndent Then ItemIndent = myImage.Width
        Next

    End Sub

    Private Sub GetGroupImages()
        Dim MyImage As Image = Nothing
        Dim sGroup As String

        For Each sGroup In Me.lstSortedGroups
            MyImage = Nothing
            RaiseEvent GetGroupImage(sGroup, MyImage)

            If Not MyImage Is Nothing Then

                Dim wrapper As GroupComboboxPopup.ObjectWrapper = MyPopup.dicGroupInfo(sGroup)

                wrapper.Image = MyImage

                HasGroupImages = True
                If MyImage.Width > GroupIndent Then GroupIndent = MyImage.Width
                'dicGroupImage.Add(sGroup, MyImage)
            End If
        Next

    End Sub

    Private Sub ShowPopup()
        Dim MyPoint As Point
        Dim MySize As Size

        MySize = New Size(Me.Width, Me.DropdownHeight)

        If MyPopup Is Nothing Then MyPopup = New GroupComboboxPopup(Me)

        RunOnFirstPopup()

        MyPoint = New Point(0, Me.Height)

        With MyPopup
            .Size = MySize
            .Visible = True
        End With

        If MyDropDown Is Nothing Or True Then
            MyControlHost = New ToolStripControlHost(MyPopup)
            MyDropDown = New ToolStripDropDown()
            MyDropDown.Items.Add(MyControlHost)
        End If

        With MyControlHost
            .Margin = Padding.Empty
            .Padding = Padding.Empty
            .AutoSize = False
            .Size = MySize
        End With

        With MyDropDown
            .AutoSize = False
            .Size = MySize
            .Margin = Padding.Empty
            .Padding = Padding.Empty
            .AutoClose = True
            .Show(Me, MyPoint)
            bDropDownOpened = True
        End With

        MyPopup.Select()
        MyPopup.Focus()

        Me.Invalidate()
    End Sub

    Public Function DropdownHandle() As IntPtr
        If MyDropDown Is Nothing Then Return IntPtr.Zero
        Return Me.MyDropDown.Handle
    End Function

    Private bDropDownOpened As Boolean

    Private Sub GroupCombobox_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim G As Graphics
        Dim BorderColor As Color = Color.FromArgb(141, 141, 141)
        Dim MyBorderPen As New Pen(BorderColor, 1)
        Dim BorderRect As Rectangle
        Dim myTextBoxRect As Rectangle
        Dim MyForeColor As Color = Color.Black

        G = e.Graphics
        G.Clear(Color.White)

        BorderRect = New Rectangle(0, 0, Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1)

        'DRAW BORDER AROUND CONTROL
        If bDropDownOpened Then MyBorderPen.Color = Color.FromArgb(0, 120, 212)
        G.DrawRectangle(MyBorderPen, BorderRect)

        MyDropDownButtonRect = New Rectangle((Me.ClientRectangle.Width - iDropDownButtonWidth), 0, iDropDownButtonWidth, 21)

        'DRAW THE BUTTON
        If bMouseOver OrElse bDropDownOpened Then
            ComboBoxRenderer.DrawDropDownButton(G, MyDropDownButtonRect, VisualStyles.ComboBoxState.Hot)
        Else
            ComboBoxRenderer.DrawDropDownButton(G, MyDropDownButtonRect, VisualStyles.ComboBoxState.Normal)
        End If

        If Not bMouseOver AndAlso Not bDropDownOpened Then G.DrawRectangle(MyBorderPen, BorderRect)

        'DRAW STRING
        myTextBoxRect = New Rectangle(2, 0, Me.ClientRectangle.Width - iDropDownButtonWidth, 21)

        If Not mySelectedItem Is Nothing Then
            Dim sText As String
            Dim itemImage As Image = Nothing

            sText = GetDisplayMember(mySelectedItem)

            itemImage = GetImageByItemName(sText)
            If itemImage Is Nothing Then itemImage = GetItemImageMember(mySelectedItem)

            If Not itemImage Is Nothing Then
                Dim y As Integer = (21 - itemImage.Height) / 2

                G.DrawImage(itemImage, New Rectangle(4, y, itemImage.Width, itemImage.Height))

                myTextBoxRect.SetX(myTextBoxRect.X + itemImage.Width + 2)
            End If

            G.DrawString(sText, Font, ForeColor, myTextBoxRect, StringAlignment.Near, StringAlignment.Center)

            itemImage = GetItemImageMember(mySelectedItem)
        End If

        MyBorderPen.Dispose()
    End Sub

    Protected Overrides Sub OnLayout(e As LayoutEventArgs)
        Me.Height = 21
    End Sub

    Private Sub GroupCombobox_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If MyDropDownButtonRect.Contains(e.Location) Then

            If Not bMouseOver Then
                bMouseOver = True
                Me.Invalidate()
            End If
        Else
            If bMouseOver Then
                bMouseOver = False
                Me.Invalidate()
            End If
        End If
    End Sub

    Private Sub GroupCombobox_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        bMouseOver = False
        Me.Invalidate()
    End Sub

    Private Sub GroupCombobox_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        If MyDropDownButtonRect.Contains(e.Location) Or True Then
            ShowPopup()
        End If
    End Sub


    Private Sub MyDropDown_Closing(sender As Object, e As ToolStripDropDownClosingEventArgs) Handles MyDropDown.Closing

        bDropDownOpened = False
        Me.Invalidate()
    End Sub

    Private Sub GroupCombobox_HandleCreated(sender As Object, e As EventArgs) Handles Me.HandleCreated
        If Me.DesignMode Then Return
    End Sub

    Public Function RaiseDrawItem(ByRef G As Graphics, Item As String, ByRef Rect As Rectangle) As Boolean
        Dim bHandled As Boolean

        bHandled = False

        RaiseEvent DrawItem(G, Item, Rect, bHandled)

        Return bHandled
    End Function

    Public Function RaiseDrawGroup(ByRef G As Graphics, Group As String, ByRef Rect As Rectangle) As Boolean
        Dim bHandled As Boolean

        bHandled = False

        RaiseEvent DrawGroup(G, Group, Rect, bHandled)

        Return bHandled
    End Function

    Public HasGroupImages As Boolean

    Public Function GetImageByGroupName(Groupname As String) As Image
        Dim Result As Image = Nothing



        'If dicGroupImage.ContainsKey(Groupname) Then Return dicGroupImage(Groupname)

        Return Result
    End Function

    Public Function GetImageByItemName(ItemName As String) As Image
        Dim result As Image = Nothing

        RaiseEvent GetItemImage(ItemName, result)

        Return result
    End Function
End Class

