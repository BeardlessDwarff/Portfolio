Imports System.Runtime.InteropServices

Public Class GroupComboboxPopup

    Private MyGroupCombobox As GroupCombobox
    Private MouseOverItem As Object

    Public dicItemInfo As New Dictionary(Of Object, ObjectWrapper)
    Public dicGroupInfo As New Dictionary(Of String, ObjectWrapper)

    Private SelectedGroup As String = ""
    Private bSuspendvScrollbarValueChanged As Boolean

    Private WithEvents MyScrollbarSubclass As McsSubClass

    Public Sub New(Combobox As GroupCombobox)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        MyGroupCombobox = Combobox


        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.Selectable, True)

        MyScrollbarSubclass = New McsSubClass(Me.VScrollBar1.Handle)

    End Sub


    Private Function DrawGroup(GroupData As ObjectWrapper, ByRef G As Graphics, ByRef Y As Integer) As ObjectWrapper
        Dim MyImage As Image
        Dim MyRect As Rectangle
        Dim TextBrush As System.Drawing.Brush = New SolidBrush(Color.Black)
        Dim Pen As System.Drawing.Pen = New Pen(Color.FromArgb(141, 141, 141))
        Dim X As Integer
        Dim MyForeColor As Color
        'Dim groupInfo As New ObjectWrapper
        Dim bHandled As Boolean

        MyRect = New Rectangle(2, Y, Me.Width, MyGroupCombobox.ItemHeight)

        If MyGroupCombobox.RaiseDrawGroup(G, GroupData.Group, MyRect) Then
            bHandled = True
        End If

        X = 2

        If GroupData.Group = Me.SelectedGroup Then
            Dim MyColor As Color = Color.FromArgb(213, 233, 248)
            Dim MyBrush As New SolidBrush(MyColor)

            MyRect.SetX(0)
            G.FillRectangle(MyBrush, MyRect)
            MyRect.SetX(2)

            MyBrush.Destroy
            MyForeColor = Color.White
        End If

        'DRAW IMAGE
        MyImage = GroupData.Image ' MyGroupCombobox.GetImageByGroupName(Text)

        If MyGroupCombobox.HasGroupImages Then MyRect.SetX(MyGroupCombobox.GroupIndent + 4)

        If Not MyImage Is Nothing Then
            Dim iImageTop As Double = MyRect.Y + (MyRect.Height - MyImage.Height) / 2

            G.DrawImage(MyImage, New Rectangle(4, iImageTop, MyImage.Width, MyImage.Height))

            If MyImage.Width > MyGroupCombobox.GroupIndent Then
                MyGroupCombobox.GroupIndent = MyImage.Width
            End If

        End If

        'DRAW TEXT
        If Not bHandled Then
            G.DrawString(GroupData.Group, MyGroupCombobox.GroupFont, Color.Black, MyRect, StringAlignment.Near, StringAlignment.Center)
        End If


        Y += MyRect.Height

        With GroupData
            .CanSelect = False
            .Region = MyRect
        End With

        TextBrush.Dispose()
        Pen.Dispose()

        Return GroupData
    End Function

    Private Sub GroupComboboxPopup_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim G As System.Drawing.Graphics = e.Graphics
        Dim Brush As System.Drawing.Brush = New SolidBrush(Color.Black)
        Dim Y As Integer = 0
        Dim HandledGroups As New List(Of String)
        Dim sGroup As String
        Dim MyItemData As ObjectWrapper
        Dim MyGroupData As ObjectWrapper

        Y -= Me.VScrollBar1.Value

        'DRAW ITEMS
        For Each Item As Object In MyGroupCombobox.Items

            MyItemData = Me.dicItemInfo(Item)

            sGroup = MyItemData.Group 'MyGroupCombobox.GetGroupName(Item)

            If sGroup <> "" AndAlso Not HandledGroups.Contains(sGroup) Then
                MyGroupData = dicGroupInfo(sGroup)
                DrawGroup(MyGroupData, G, Y)
                MyGroupData.Group = sGroup
                MyGroupData.Item = Item
                HandledGroups.Add(sGroup)

                dicGroupInfo.AddOrUpdate(sGroup, MyGroupData)
            End If


            DrawItem(MyItemData, Y, G)
            MyItemData.Item = Item

            dicItemInfo.AddOrUpdate(MyItemData, MyItemData)
        Next

        'DRAW BORDER AROUND DROPDOWN 
        'Dim BorderColor As Color = Color.FromArgb(141, 141, 141)
        Dim BorderColor As Color = Color.FromArgb(0, 120, 212)
        Dim MyBorderPen As New Pen(BorderColor, 1)

        G.DrawRectangle(MyBorderPen, New Rectangle(0, 0, Me.ClientRectangle.Width - 1, Me.ClientRectangle.Height - 1))

    End Sub

    Private Function DrawItem(itemWrapper As ObjectWrapper, ByRef y As Integer, G As System.Drawing.Graphics) As ObjectWrapper
        Dim MyRect As Rectangle
        Dim MyForeColor As Color
        Dim sItem As String
        Dim myImage As Image = Nothing
        Dim iItemIndent As Integer

        MyForeColor = Color.Black

        MyRect = New Rectangle(iItemIndent, y, Me.Width, MyGroupCombobox.ItemHeight)

        'GET ITEM
        sItem = itemWrapper.Text 'MyGroupCombobox.GetDisplayMember(item)

        If MyGroupCombobox.RaiseDrawItem(G, sItem, MyRect) Then
            itemWrapper.Text = sItem
            y += MyRect.Y
            Return itemWrapper
        End If

        'MOUSE OVER
        If itemWrapper.Item Is MouseOverItem OrElse (MouseOverItem Is Nothing AndAlso SelectedGroup = "" AndAlso itemWrapper.Item Is MyGroupCombobox.SelectedItem) Then
            MyRect.SetX(0)
            Dim MyColor As Color = Color.FromArgb(0, 120, 215)
            G.FillRectangle(MyColor, MyRect)
            MyForeColor = Color.White
            MyRect.SetX(iItemIndent)
        End If

        'DRAW IMAGE
        myImage = itemWrapper.Image

        If Not myImage Is Nothing Then
            Dim imageRect As Rectangle
            Dim f As Double = MyRect.Y + (MyRect.Height - myImage.Height) / 2

            imageRect = New Rectangle(4 + MyGroupCombobox.GroupIndent, f, myImage.Width, myImage.Height)
            G.DrawImage(myImage, imageRect)
        End If

        'DRAW TEXT
        If MyGroupCombobox.ItemIndent <> 0 Then
            MyRect.SetX(MyGroupCombobox.ItemIndent + 6)
        End If

        MyRect.SetX(MyRect.X + MyGroupCombobox.GroupIndent)
        G.DrawString(sItem, MyGroupCombobox.Font, MyForeColor, MyRect, StringAlignment.Near, StringAlignment.Center)

        y += MyRect.Height
        itemWrapper.Region = MyRect

        Return itemWrapper

    End Function

    Private Sub GroupComboboxPopup_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp

        Dim item As Object = Nothing

        item = GetItemFromLocation(e.Location)

        If Not item Is Nothing Then

            MyGroupCombobox.SelectedItem = item
            MyGroupCombobox.Invalidate()
            MyGroupCombobox.ClosePopup()
        End If

    End Sub

    Private Function GetItemFromLocation(Location As Point) As Object

        Dim itemInfo As ObjectWrapper

        For Each item As Object In dicItemInfo.Keys
            itemInfo = dicItemInfo(item)

            If itemInfo.CanSelect AndAlso itemInfo.Region.Contains(Location) Then Return itemInfo.Item
        Next

        Return Nothing
    End Function

    Private Sub GroupComboboxPopup_Leave(sender As Object, e As EventArgs) Handles Me.Leave
        Me.Visible = False
    End Sub


    Private Sub vScrollbar_ValueChanged(sender As Object, e As EventArgs)
        If Not bSuspendvScrollbarValueChanged Then
            Me.Invalidate()
        End If
    End Sub

    Private Sub GroupComboboxPopup_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        With Me.VScrollBar1
            .Size = New Size(.Width, Me.ClientRectangle.Height - 2)
            .Location = New Point(Me.ClientRectangle.Width - .Width - 1, 1)
        End With
    End Sub

    Private Sub VScrollBar1_ValueChanged(sender As Object, e As EventArgs) Handles VScrollBar1.ValueChanged
        Me.Invalidate()
    End Sub

    Private Sub CheckScrollbar()
        Dim iContentHeight As Integer

        iContentHeight = (Me.MyGroupCombobox.Groups.Count + Me.MyGroupCombobox.Items.Count) * (Me.MyGroupCombobox.ItemHeight)

        If iContentHeight > Me.Height Then
            With Me.VScrollBar1
                .Maximum = iContentHeight
                .LargeChange = Me.Height
                .SmallChange = MyGroupCombobox.ItemHeight
                If Not MyGroupCombobox.SelectedItem Is Nothing Then

                    Dim x As Integer
                    Dim sGroup = ""

                    For Each item As Object In MyGroupCombobox.Items
                        Dim stemp As String = MyGroupCombobox.GetGroupName(item)

                        If Not stemp = sGroup Then
                            sGroup = stemp
                            x += 1
                        End If

                        If item Is MyGroupCombobox.SelectedItem Then Exit For

                        x += 1
                    Next

                    If x * MyGroupCombobox.ItemHeight + Me.ClientRectangle.Height > .Maximum Then
                        .Value = .Maximum - Me.ClientRectangle.Height
                    Else
                        .Value = x * MyGroupCombobox.ItemHeight
                    End If

                End If
                .Visible = True
            End With


        Else
            Me.VScrollBar1.Visible = False
        End If
    End Sub

    Protected Overrides Sub SetVisibleCore(value As Boolean)
        If value Then
            CheckScrollbar()
            MouseOverItem = Nothing
        End If
        MyBase.SetVisibleCore(value)
    End Sub

    Private Sub GroupComboboxPopup_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        Dim NewValue As Integer

        If e.Delta < 0 Then
            NewValue = Me.VScrollBar1.Value + Me.VScrollBar1.SmallChange
        Else
            NewValue = Me.VScrollBar1.Value - Me.VScrollBar1.SmallChange
        End If

        If NewValue > (Me.VScrollBar1.Maximum - Me.VScrollBar1.LargeChange) Then
            NewValue = (Me.VScrollBar1.Maximum - Me.VScrollBar1.LargeChange) + 1
        End If

        If NewValue < Me.VScrollBar1.Minimum Then NewValue = Me.VScrollBar1.Minimum
        If NewValue > Me.VScrollBar1.Maximum Then NewValue = Me.VScrollBar1.Maximum

        Me.VScrollBar1.Value = NewValue
    End Sub

    Private Sub GroupComboboxPopup_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        'MouseOverItem
        Dim bMouseOverItemAtStart As Boolean
        Dim itemInfo As ObjectWrapper
        Dim bMouseOverItem As Boolean

        bMouseOverItemAtStart = Not (MouseOverItem Is Nothing)

        For Each item As Object In dicItemInfo.Keys
            itemInfo = dicItemInfo(item)

            If New Rectangle(0, itemInfo.Region.Y, itemInfo.Region.Width, itemInfo.Region.Height).Contains(e.Location) Then
                MouseOverItem = itemInfo.Item
                Me.SelectedGroup = ""
                Me.Invalidate()
                bMouseOverItem = True
                Exit For
            End If
        Next

        If Not bMouseOverItem Then
            'Hangt de muis boven een groep?
            For Each sGroep In dicGroupInfo.Keys

                With dicGroupInfo(sGroep)
                    If New Rectangle(0, .Region.Y, .Region.Width, .Region.Height).Contains(e.Location) Then
                        MouseOverItem = Nothing
                        Me.SelectedGroup = sGroep
                        Me.Invalidate()
                        Exit For
                    End If
                End With
            Next
        End If

        'If bMouseOverItemAtStart Then Me.Invalidate()

    End Sub

    Private Sub SelectItemByKeyPress(index As Integer, Up As Boolean)
        index = index + IIf(Up, -1, 1)

        Try
            Me.MouseOverItem = MyGroupCombobox.Items(index)
            MyGroupCombobox.SelectedItem = MyGroupCombobox.Items(index)
            Me.Invalidate()
        Catch ex As Exception

        End Try

        ScrollMouseOverItemIntoView()

        Me.Invalidate()
    End Sub

    Private Sub ScrollMouseOverItemIntoView()
        Dim MyData As ObjectWrapper = Nothing
        Dim MyRect As Rectangle

        If Me.MouseOverItem Is Nothing Then
            If MyGroupCombobox.SelectedItem Is Nothing Then Return
            If Not dicItemInfo.ContainsKey(MyGroupCombobox.SelectedItem) Then Return
            MyData = dicItemInfo(MyGroupCombobox.SelectedItem)
        Else
            If Not dicItemInfo.ContainsKey(Me.MouseOverItem) Then Return
            MyData = dicItemInfo(Me.MouseOverItem)

        End If


        MyRect = MyData.Region

        If MyRect.Y < 0 Then
            Me.VScrollBar1.Value += MyRect.Y

        ElseIf MyRect.Bottom > Me.ClientRectangle.Height Then
            Me.VScrollBar1.Value += (MyRect.Bottom - Me.ClientRectangle.Height)
        Else
            Me.Invalidate()
        End If
    End Sub

    Private Sub GroupComboboxPopup_GotFocus(sender As Object, e As EventArgs) Handles Me.GotFocus

    End Sub

    Private Sub VScrollBar1_GotFocus(sender As Object, e As EventArgs) Handles VScrollBar1.GotFocus

    End Sub

    Private Function GetFirstItem(Group As String) As Object
        For Each Item As Object In MyGroupCombobox.Items
            With dicItemInfo(Item)
                If .Group = Group Then Return Item
            End With
        Next

        Return Nothing
    End Function

    Private Function GetLastItem(Group As String) As Object
        Dim bGroupFound As Boolean
        Dim Result As Object = Nothing

        For Each Item As Object In MyGroupCombobox.Items
            With dicItemInfo(Item)
                If .Group = Group Then
                    Result = Item
                    bGroupFound = True
                Else
                    If bGroupFound Then Return Result
                End If
            End With
        Next

        Return Nothing
    End Function

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Dim index As Integer


        If MouseOverItem Is Nothing Then
            If MyGroupCombobox.SelectedItem Is Nothing Then

                If Me.SelectedGroup <> "" Then
                    Select Case e.KeyCode
                        Case Keys.Up, Keys.W
                            'Pak het laatste item van de bovenliggende groep.
                            Dim iIndex As Integer = MyGroupCombobox.lstSortedGroups.IndexOf(Me.SelectedGroup)

                            If iIndex > 0 Then
                                Dim sGroup As String

                                sGroup = MyGroupCombobox.lstSortedGroups(iIndex - 1)

                                Me.MouseOverItem = GetFirstItem(sGroup)
                                Me.SelectedGroup = ""
                                Me.ScrollMouseOverItemIntoView()

                            End If

                        Case Keys.Down, Keys.S
                            'Pak het eerste item met deze groep
                            Me.MouseOverItem = GetFirstItem(Me.SelectedGroup)
                            Me.SelectedGroup = ""
                            Me.ScrollMouseOverItemIntoView()

                    End Select
                End If

                MyBase.OnKeyDown(e)
                Return
            End If

            Me.SelectedGroup = ""
            index = MyGroupCombobox.Items.IndexOf(MyGroupCombobox.SelectedItem)

        Else
            index = MyGroupCombobox.Items.IndexOf(MouseOverItem)
        End If


        Select Case e.KeyData
            Case Keys.Up, Keys.W
                SelectItemByKeyPress(Index, True)

            Case Keys.Down, Keys.S
                SelectItemByKeyPress(Index, False)

            Case Keys.Enter
                MyGroupCombobox.ClosePopup()
        End Select
    End Sub

    Protected Overrides Function IsInputKey(keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.Down, Keys.Up, Keys.W, Keys.S, Keys.Enter
                Return True
        End Select

        Return MyBase.IsInputKey(keyData)
    End Function

    Private Sub MyScrollbarSubclass_WinProc(ByRef m As Message, ByRef KillMessage As Boolean) Handles MyScrollbarSubclass.WinProc
        Select Case m.Msg
            Case Functions.Messages.WM_KEYDOWN ' 256 'WM_KEYDOWN
                Functions.SendMessage(Me.Handle, Functions.Messages.WM_KEYDOWN, m.WParam, m.LParam)
                KillMessage = True
        End Select
    End Sub

    'Protected Overrides Sub WndProc(ByRef m As Message)
    '    Dim WM_MOUSEWHEEL As Integer = &H20A

    '    Select Case m.Msg
    '        Case WM_MOUSEWHEEL
    '            SendMessage(Me.VScrollBar1.Handle, m.Msg, m.WParam, m.LParam)
    '            Return
    '    End Select

    '    MyBase.WndProc(m)
    'End Sub

    Public Class ObjectWrapper
        Public Sub New()

        End Sub

        Public Property Item As Object
        Public Property Region As Rectangle
        Public Property CanSelect As Boolean
        Public Property Group As String
        Public Property Image As Image
        Public Property Text As String


    End Class
End Class
