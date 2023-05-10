Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.ComboBox))>
Public Class ComboBox
    Private sDescription As String = ""
    Private MyStringFormat As New StringFormat
    Private bDropDownIsOpen As Boolean = False
    Private iVorigeAantalDagen As Integer = 0

    Public Property AutoSizeDropDownList As Boolean = True
    Public Property AantalDecimalen As Integer = 2
    Public Property IgnoreMouseWheel As Boolean = True 'Het scrollen in combobox met het mousewheel gebeurt vaak per ongeluk. Standaard negeren we het mousewheel-bericht.

    Public Sub VulMetPeriodes()
        Me.VulByEnum(Of ePeriode)
    End Sub

    Public Function Data() As Dictionary(Of String, Object)
        If TypeOf Me.SelectedItem Is ComboBoxItemDictionary Then
            Return DirectCast(Me.SelectedItem, ComboBoxItemDictionary).Data
        End If

        Return Nothing
    End Function

    Public Sub VulBySQL(SQL As String, Optional AddEmptyItem As Boolean = True)
        SQL = SQL.HardTrim

        Dim DM As iDataManager
        Dim DR As IDataReader
        Dim MyItem As ComboBoxItemDictionary
        Dim MyItems As New List(Of ComboBoxItemDictionary)

        DM = EasyCare.SQL.NewDataManager

        DR = DM.SQLdrResult(SQL)

        If AddEmptyItem Then
            MyItem = New ComboBoxItemDictionary(New Dictionary(Of String, Object), "")
            MyItems.Add(MyItem)
        End If

        Do While DR.Read
            MyItem = New ComboBoxItemDictionary(DR.ToDictionary)

            MyItems.Add(MyItem)
        Loop

        Me.BeginUpdate()
        Me.Items.Clear()
        Me.Items.AddRange(MyItems.ToArray)
        Me.DisplayMember = "text"
        Me.SelectFirstItem()
        Me.EndUpdate()

    End Sub

    Public Sub VulMetPeriodes(Data As List(Of Date))
        For Each Item As ePeriode In [Enum].GetValues(GetType(ePeriode))
            If Not PeriodeKomtVoorInDatumLijst(Data, Item) Then Continue For
            Me.Items.Add(Item)
        Next
    End Sub

    Public Sub Sort(Comparer As Comparison(Of Object))
        If Me.Items.Count < 2 Then Return

        Dim MyItems As New List(Of Object)
        Dim arrItems(Me.Items.Count - 1) As Object

        Me.Items.CopyTo(arrItems, 0)

        MyItems = arrItems.ToList

        MyItems.Sort(Comparer)

        Me.Items.Clear()
        Me.Items.AddRange(MyItems.ToArray)
    End Sub

    Private Function PeriodeKomtVoorInDatumLijst(lstData As List(Of Date), Periode As ePeriode) As Boolean
        If Periode = ePeriode.Alles Then Return True

        Dim iDagenPeriode As Integer = Periode.ToInteger
        Dim iDagenMeting As Integer

        For Each Datum As Date In lstData
            iDagenMeting = DateDiff(DateInterval.Day, Datum, Date.Now)
            If iDagenMeting < iDagenPeriode And iDagenMeting > iVorigeAantalDagen Then Return True
        Next

        Return False
    End Function

    Public Sub VulMetPeriodes(PatientItems As iEPDPatientItems)
        Dim lstData As New List(Of Date)
        For Each Item As iEPDPatientItem In PatientItems.Items.Values
            lstData.Add(Item.DatumVanaf)
        Next

        lstData.Sort(New DateSorter())

        Me.VulMetPeriodes(lstData)
    End Sub

    <Runtime.InteropServices.DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lParam As String) As IntPtr
    End Function

    Public Property Description() As String
        Get
            Return sDescription
        End Get
        Set(ByVal value As String)
            If sDescription = value Then Return
            If sDescription = value.TrimEnd({" "c, "."c}) Then Return

            sDescription = value.TrimEnd({" "c, "."c})
            CreateDescription()
        End Set
    End Property

    Private Sub CreateDescription()
        If Not Me.IsHandleCreated Then Return

        If Not Me.Description.isEmptyOrWhiteSpace Then
            SendMessage(Me.Handle, &H1703, 1, Me.Description)
        Else
            SendMessage(Me.Handle, &H1703, 1, "")
        End If

    End Sub

    '<DebuggerNonUserCode()> _
    'Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    '    MyBase.WndProc(m)

    '    Select Case m.Msg
    '        Case FrameworkNS.Functions.API.Messages.WM_PAINT
    '            DrawDescription()

    '    End Select
    'End Sub

    Public Function SelectComboboxItem(O As Object) As Boolean
        For Each Item As Object In Me.Items
            If TypeOf Item Is MCS_Interfaces.ComboBoxItem Then
                If DirectCast(Item, MCS_Interfaces.ComboBoxItem).Obj Is O OrElse DirectCast(Item, MCS_Interfaces.ComboBoxItem).Obj.Equals(O) Then
                    Me.SelectedItem = Item
                    Return true
                End If
            End If
        Next

        Return False
    End Function

    Public Function SelectItemByText(Text As String) As Boolean
        Dim sText As String

        For X As Integer = 0 To Me.Items.Count - 1
            sText = GetComboItemText(X)
            If sText.isEqualTo(Text) Then
                Me.SelectedIndex = X
                Return True
            End If
        Next

        Return False
    End Function

    Public Function SelectFirstItem() As Boolean
        If Me.Items.Count = 0 Then Return False
        Me.SelectedIndex = 0
        Return True
    End Function

    Public Function SelectSecondItem() As Boolean
        If Me.Items.Count < 2 Then Return False
        Me.SelectedIndex = 1
        Return True
    End Function

    Private Sub SetDropDownWidth()

        Dim S As String
        Dim G As Graphics = Me.CreateGraphics
        Dim iWidth As Integer = 0
        Dim iMaxWidth As Integer = 0


        For Each Item As Object In Me.Items
            S = Me.GetItemText(Item)
            iWidth = G.MeasureString(S, Me.Font).Width
            If iWidth > iMaxWidth Then iMaxWidth = iWidth
        Next

        G.Dispose()

        iMaxWidth += 10

        Me.DropDownWidth = iMaxWidth
    End Sub

    'Private Sub DrawDescription()
    '    If Me.Description.isEmptyOrWhiteSpace Then Return
    '    If Not Me.Text.isEmptyOrWhiteSpace Then Return
    '    If Not Me.Enabled Then Return

    '    Dim iOffsetX As Integer = 2
    '    Dim G As Graphics = Graphics.FromHwnd(Me.Handle)
    '    Dim MyRect As New Rectangle(iOffsetX, 0, Me.ClientRectangle.Width - iOffsetX, Me.ClientRectangle.Height)

    '    G.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
    '    G.DrawString(Me.Description, New Font(Me.Font, FontStyle.Italic), Brushes.Gray, MyRect, MyStringFormat)
    'End Sub

    Public Function NumericValue(Optional ErrorValue As Single = Single.MinValue) As Single
        Dim sngResult As Single
        Dim MyTekst As String = Me.Text
        MyTekst = MyTekst.Replace(".", ",")
        Try
            sngResult = Convert.ToSingle(MyTekst)
        Catch ex As Exception
            sngResult = ErrorValue
        End Try

        Return Math.Round(sngResult, Me.AantalDecimalen)
    End Function

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        MyStringFormat.Alignment = StringAlignment.Near
        MyStringFormat.LineAlignment = StringAlignment.Center

        Me.DropDownStyle = ComboBoxStyle.DropDownList
        Me.DrawMode = DrawMode.OwnerDrawFixed
    End Sub

    Private Function GetComboItemText(Index As Integer) As String
        Dim MyItem As Object = Me.Items.Item(Index)

        If TypeOf MyItem Is System.Enum Then
            'Het is een enum.
            Return DirectCast(MyItem, System.Enum).GetDescription
        End If

        If TypeOf MyItem Is String Then Return MyItem.ToString
        If DisplayMember = "ComboItem.DisplayName" Then DisplayMember = ""

        If Me.DisplayMember <> "" Then
            Try
                return MCS_Interfaces.extObject.GetPropertyByName(Of String)(MyItem, Me.DisplayMember)
            Catch ex As Exception
                Return ""
            End Try
        End If

        Return MyItem.ToString()

    End Function

    Private Sub exCombobox_DrawItem(sender As Object, e As DrawItemEventArgs) Handles Me.DrawItem
        If Not Me.RuntimeMode Then Return

        Dim bFocus As Boolean = (e.State And DrawItemState.Focus) = DrawItemState.Focus
        Dim bSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim bHotLight As Boolean = (e.State And DrawItemState.HotLight) = DrawItemState.HotLight

        If bFocus Then e.DrawFocusRectangle()

        If (e.Index < 0) Then
            Dim MyColor As Color = Color.FromArgb(87, 87, 87)
            Dim MyBrush As New SolidBrush(MyColor)
            Dim MyRect As Rectangle = e.Bounds.Clone

            MyRect.X -= 3

            If Me.Description Is Nothing Then Me.Description = ""
            If Me.Font.isDisposedOrNothing Then Return
            If e.Graphics Is Nothing Then Return
            If MyBrush Is Nothing Then Return
            If Me.Description Is Nothing Then Return
            If Me.Font Is Nothing Then Return

            e.Graphics.DrawString(Me.Description, Me.Font, MyBrush, MyRect, StringAlignment.Near, StringAlignment.Center)

            MyBrush.Destroy
            Return
        End If



        Dim sText As String = GetComboItemText(e.Index)

        e.DrawBackground()

        If Not Me.Enabled Then
            e.Graphics.Clear(Color.FromArgb(240, 240, 240))
            e.Graphics.DrawString(sText, e.Font, Brushes.Gray, e.Bounds)
        ElseIf bFocus OrElse bSelected OrElse bHotLight Then
            e.Graphics.DrawString(sText, e.Font, Brushes.White, e.Bounds)
        Else
            Dim MyBrush As New SolidBrush(Me.ForeColor)
            e.Graphics.DrawString(sText, e.Font, MyBrush, e.Bounds)
            MyBrush.Dispose()
        End If

        If bFocus Then e.DrawFocusRectangle()
    End Sub

    Private Sub ComboBox_MouseWheel(sender As Object, e As MouseEventArgs) Handles Me.MouseWheel
        'If bDropDownIsOpen Then Return
        'DirectCast(e, HandledMouseEventArgs).Handled = IgnoreMouseWheel
    End Sub

    Private Sub ComboBox_DropDown(sender As Object, e As EventArgs) Handles Me.DropDown
        If AutoSizeDropDownList Then Me.SetDropDownWidth()
        bDropDownIsOpen = True
    End Sub

    Private Sub ComboBox_DropDownClosed(sender As Object, e As EventArgs) Handles Me.DropDownClosed
        bDropDownIsOpen = False
    End Sub

    <DebuggerNonUserCode>
    Protected Overrides Sub DefWndProc(ByRef m As Message)
        If m.Msg = FrameWorkNS.Functions.API.Messages.WM_MOUSEWHEEL Then
            If Me.IgnoreMouseWheel Then
                If Me.bDropDownIsOpen Then Return
                m.HWnd = Me.Parent.Handle
            End If
        End If

        MyBase.DefWndProc(m)
    End Sub

    Private Sub ComboBox_HandleCreated(sender As Object, e As EventArgs) Handles Me.HandleCreated
        CreateDescription()
    End Sub

    Private Sub ComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Me.SelectedIndexChanged
        Me.NotifyValueChangedReceiver
    End Sub
End Class

Public Class DateSorter
    Implements IComparer(Of Date)

    Public Function Compare(x As Date, y As Date) As Integer Implements IComparer(Of Date).Compare
        Date.Compare(x, y)
    End Function
End Class