Imports System.ComponentModel
Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.TextBox))>
Public Class Textbox
    Private bRaiseChangeCompleted As Boolean = False
    Private WithEvents tmrDoneTyping As New System.Windows.Forms.Timer
    Private MyAlleenNumeriek As Boolean
    Private sDescription As String = ""
    Private MyStringFormat As New StringFormat
    Private bNumLockOnGotFocus As Boolean
    Private MyMinValue As Single = Single.MinValue
    Private MyMaxValue As Single = Single.MaxValue
    Private bAutoCompleteListview As Boolean = False
    Private MySearchThread As Threading.Thread
    Private bShowDefaultEasyCareMenu As Boolean

    Private WithEvents MyAutoCompleteListview As MCSCTRLS1.ListView
    Private WithEvents MyAutoCompleteListviewPopup As MCSCTRLS2.EasyControlPopup

    Public Property DecimalenAltijdTonen As Boolean = False
    Public Property MedihulpKey As String = ""
    Public Property VoorloopnullenLength As Integer = 0

    Public Property ShowDefaultEasyCareMenu As Boolean
        Get
            Return bShowDefaultEasyCareMenu
        End Get
        Set(value As Boolean)
            bShowDefaultEasyCareMenu = value

            If value Then
                Me.ContextMenu = New ContextMenu 'Door een nieuw contextmenu zonder items te koppelen, wordt er geen contextmenu getoond door Windows zelf.
            Else
                Me.ContextMenu = Nothing
            End If
        End Set
    End Property

    Public Property AutoCompleteListView As Boolean
        Get
            Return bAutoCompleteListview
        End Get
        Set(value As Boolean)
            bAutoCompleteListview = value

            If value Then
                If MyAutoCompleteListview Is Nothing OrElse MyAutoCompleteListview.IsDisposed Then MyAutoCompleteListview = New MCSCTRLS1.ListView
                If MyAutoCompleteListviewPopup Is Nothing Then MyAutoCompleteListviewPopup = New MCSCTRLS2.EasyControlPopup(MyAutoCompleteListview)
            End If

            If Not value Then
                EasyCare.Functions.MemoryManagement.DisposeObject(MyAutoCompleteListview)
                MyAutoCompleteListview = Nothing
            End If
        End Set
    End Property

    Public Property MaxValue As Single
        Get
            Return MyMaxValue
        End Get
        Set(value As Single)
            MyMaxValue = value
            CheckMaxNumeriekLength()
        End Set
    End Property

    Public Property MinValue As Single
        Get
            Return MyMinValue
        End Get
        Set(value As Single)
            MyMinValue = value
            CheckMaxNumeriekLength()
        End Set
    End Property

    Public Function FocusAndGotoEndOfText() As Boolean
        Try
            Me.Select()
            Me.Select(Me.Text.Length, 0)
            Me.ScrollToCaret()
        Catch ex As Exception

        End Try
    End Function

    Public Function FocusAndSelectAllText() As Boolean
        Try
            Me.Focus()
            Me.Select()
            Me.SelectAll()
        Catch ex As Exception
            Return False
        End Try


        Return True
    End Function

    Private Sub CheckMaxNumeriekLength()
        If Not Me.AlleenNumeriek Then Return

        Dim iMaxLengthMinValue As String = ""
        Dim iMaxLengthMaxValue As String = ""
        Dim iMin As Integer
        Dim iMax As Integer

        If Me.MyMinValue.ToString = MyMaxValue.ToString Then GoTo Skip
        If Me.MinValue.ToString = Single.MinValue.ToString Then GoTo Skip
        If Me.MaxValue.ToString = Single.MaxValue.ToString Then GoTo Skip

        iMaxLengthMinValue = Me.MinValue.ToString.Trim
        If iMaxLengthMinValue.Contains(",") Then iMaxLengthMinValue = iMaxLengthMinValue.Substring(0, iMaxLengthMinValue.IndexOf(","))

        iMaxLengthMaxValue = Me.MaxValue.ToString.Trim
        If iMaxLengthMaxValue.Contains(",") Then iMaxLengthMaxValue = iMaxLengthMaxValue.Substring(0, iMaxLengthMaxValue.IndexOf(","))

        iMin = iMaxLengthMinValue.Length
        iMax = iMaxLengthMaxValue.Length

        If Me.AantalDecimalen > 0 Then
            iMin += (Me.AantalDecimalen + 1)
            iMax += (Me.AantalDecimalen + 1)
        End If

        If iMin > iMax Then
            Me.MaxLength = iMin
        Else
            Me.MaxLength = iMax
        End If

        Return

Skip:
        ' Me.MaxLength = 0

    End Sub

    Public Function NumericValue(Optional ErrorValue As Single = Single.MinValue) As Single
        If Me.Text.isEmptyOrWhiteSpace Then Return ErrorValue

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

    Public Property AantalDecimalen As Integer = 2

    Public Property DoneTypingInterval() As Integer
        Get
            Return tmrDoneTyping.Interval
        End Get
        Set(ByVal value As Integer)
            tmrDoneTyping.Interval = value
        End Set
    End Property

    Public Property Description() As String
        Get
            Return sDescription
        End Get
        Set(ByVal value As String)
            sDescription = value.TrimEnd({" "c, "."c})
            Me.Invalidate()
        End Set
    End Property

    <Runtime.InteropServices.DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As Integer, <Runtime.InteropServices.MarshalAs(Runtime.InteropServices.UnmanagedType.LPWStr)> ByVal lParam As String) As IntPtr
    End Function

    Private Sub CreateDescription()
        If Not Me.IsHandleCreated Then Return

        If Not Me.Description.isEmptyOrWhiteSpace Then
            SendMessage(Me.Handle, &H1501, 1, Me.Description)
        Else
            SendMessage(Me.Handle, &H1501, 1, "")
        End If

    End Sub

    Public Property AlleenNumeriek() As Boolean
        Get
            Return MyAlleenNumeriek
        End Get
        Set(ByVal value As Boolean)
            MyAlleenNumeriek = value
            If value Then
                CheckMaxNumeriekLength()
            End If
        End Set
    End Property

    Public Property RaiseChangeCompleted() As Boolean
        Get
            Return bRaiseChangeCompleted
        End Get
        Set(ByVal value As Boolean)
            bRaiseChangeCompleted = value
        End Set
    End Property

    Public Event DoneTyping(ByVal Sender As Textbox, ByVal e As EventArgs)

    Public Function AllTextSelected() As Boolean
        Return (Me.SelectedText = Me.Text)
    End Function

    Public Property SelectNearestTextboxWithArrowKeys As Boolean = False
    Public Property SelectNearestControlWithArrowKeys As Boolean = False

    Protected Overrides Function ProcessCmdKey(ByRef msg As System.Windows.Forms.Message, keyData As System.Windows.Forms.Keys) As Boolean
        'CTRL-A werkte niet bij een multiline textbox, vandaar deze code toegevoegd.
        If keyData = (Keys.Control Or Keys.A) Then
            Me.SelectAll()
            Return True
        End If

        If keyData = Keys.Tab Then
            HideAutoCompleteListview()
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Public Property PlusToestaanInNumeriekeTextbox As Boolean = False

    Private Sub Textbox_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        If e.KeyChar = ChrW(27) Then
            e.Handled = True
            Return
        End If

        If Me.AlleenNumeriek Then
            If Not isNumeric(e.KeyChar) Then e.Handled = True 'We gaan er vanuit dat het dan niet mag.
            If e.KeyChar.ToString.ToUpper = "" Then e.Handled = False

            If e.KeyChar.ToString.ToUpper = "," Or e.KeyChar.ToString.ToUpper = "." Then
                If sender.Text.Contains(",") Or sender.Text.Contains(".") Or Me.AantalDecimalen = 0 Then
                    e.Handled = True
                Else
                    e.Handled = False
                End If
            End If

            If e.KeyChar.ToString.ToUpper = "-" Then
                'If Me.MinValue <> 0 And Me.MaxValue <> 0 Then
                If Me.MinValue >= 0 Then e.Handled = True : Return
                'End If

                If (sender.Text.trim <> "") And Not AllTextSelected() Then
                    If Me.SelectionStart > 0 Then
                        e.Handled = True : Return
                    Else
                        If Me.Text.Contains("-") Then e.Handled = True : Return
                    End If
                End If

                e.Handled = False
            End If
            If e.KeyChar = "+" Then
                If Me.PlusToestaanInNumeriekeTextbox Then
                    If Me.AllTextSelected OrElse Me.SelectionStart = 0 Then
                        e.Handled = False
                    End If
                End If
            End If
        End If
    End Sub

    Public Function RTF() As String
        Return FrameWorkNS.Functions.Richtext.TextToRTF(MyBase.Text)
    End Function

    Private Sub Textbox_GotFocus(sender As Object, e As EventArgs) Handles Me.GotFocus
        If Me.AlleenNumeriek AndAlso EasyCare.FrameWork.PropertyManager.GetBoolean("EnableNumlockOnNumericTextboxes") Then
            bNumLockOnGotFocus = FrameWorkNS.Functions.API.GetNumLock
            FrameWorkNS.Functions.API.SetNumLock(True)
        End If
    End Sub

    Public Event BeforeNumericValueValidation(Sender As MCSCTRLS1.Textbox, ByRef Cancel As Boolean)

    Public Property AutoCorrectMinMaxValue As Boolean = True

    Private Sub ShowMinMaxAndDontLoseFocus()
        Dim sMessage As String
        Dim sMin As String
        Dim sMax As String

        sMin = Me.MinValue.ToString
        sMax = Me.MaxValue.ToString

        sMessage = $"U dient een waarde tussen de {sMin} en {sMax} op te geven."

        Me.GetAttention(sMessage, True, "Ongeldige waarde", "INVALID_VALUE")

        Me.FocusAndSelectAllText()
    End Sub

    Public Function CheckMinMaxOk(Optional ShowMelding As Boolean = False) As Boolean
        If Not Me.AlleenNumeriek Then Return True
        If Me.MinValue = Me.MaxValue Then Return True

        Dim sText As String
        Dim sngResult As Single
        Dim bOngeldig As Boolean
        Dim bResult As Boolean

        bResult = True 'Gaan we vanuit

        sText = Me.Text.Replace(".", ",")

        If Not Single.TryParse(sText, sngResult) Then
            If Me.ReadOnly Then Return True
            If Not Me.Enabled Then Return True

            Me.Text = ""
            Return True
        End If

        If Not Me.MinValue.isMinOrMaxValue Then
            If (sngResult < MinValue) Then bOngeldig = True
        End If

        If Not Me.MaxValue.isMinOrMaxValue Then
            If (sngResult > MaxValue) Then bOngeldig = True
        End If

        If bOngeldig Then
            bResult = False

            If ShowMelding Then
                ShowMinMaxAndDontLoseFocus()
            End If
        End If

        Return bResult
    End Function

    Private Sub Textbox_LostFocus(sender As Object, e As System.EventArgs) Handles Me.LostFocus
        If Me.AlleenNumeriek Then

            Dim bCancel As Boolean = False
            RaiseEvent BeforeNumericValueValidation(Me, bCancel)
            If bCancel Then Return

            Dim sngResult As Single
            Dim sText As String = Me.Text.Replace(".", ",")

            If Not Single.TryParse(sText, sngResult) Then
                If Me.ReadOnly Then Return
                If Not Me.Enabled Then Return

                Me.Text = ""
                Return
            End If

            Dim bOngeldig As Boolean

            If Not AutoCorrectMinMaxValue Then Return

            If Not Me.AutoCorrectMinMaxValue And false Then 'Voorlopig nog niet.
                If Me.MinValue <> Me.MaxValue Then
                    If Me.MinValue > Single.MinValue Then
                        If (sngResult < MinValue) Then bOngeldig = True
                    End If

                    If Me.MaxValue < Single.MaxValue Then
                        If (sngResult > MaxValue) Then bOngeldig = True
                    End If

                    If bOngeldig Then
                        ShowMinMaxAndDontLoseFocus()
                        Return
                    End If
                End If
            End If

            If Me.MinValue <> Me.MaxValue Then
                If Me.MinValue > Single.MinValue Then
                    If (sngResult < MinValue) Then sngResult = MinValue
                End If

                If Me.MaxValue < Single.MaxValue Then
                    If (sngResult > MaxValue) Then sngResult = MaxValue
                End If
            End If

            sngResult = Math.Round(sngResult, Me.AantalDecimalen)

            Dim sNumeriekeWaardeToString As String = sngResult.ToString.Replace(".", ",")

            If DecimalenAltijdTonen Then
                If sNumeriekeWaardeToString.Contains(",") Then
                    Dim arrItems() As String = Split(sNumeriekeWaardeToString, ",")
                    If arrItems(1).Length < Me.AantalDecimalen Then
                        sNumeriekeWaardeToString = arrItems(0) & "," & arrItems(1) & FrameWorkNS.Functions.Rekenfuncties.AddVoorloopNullen("", Me.AantalDecimalen - arrItems(1).Length)
                    End If
                Else
                    sNumeriekeWaardeToString &= "," & FrameWorkNS.Functions.Rekenfuncties.AddVoorloopNullen("", Me.AantalDecimalen)
                End If
            End If

            If Me.PlusToestaanInNumeriekeTextbox AndAlso Me.Text.Trim.StartsWith("+") Then
                sNumeriekeWaardeToString = "+" & sNumeriekeWaardeToString
            End If

            sText = sNumeriekeWaardeToString.TrimEnd({","c})

            If sText.Length < Me.VoorloopnullenLength Then sText = sText.AddVoorloopNullen(Me.VoorloopnullenLength)

            Me.Text = sText

            If EasyCare.FrameWork.PropertyManager.GetBoolean("EnableNumlockOnNumericTextboxes") Then
                FrameWorkNS.Functions.API.SetNumLock(bNumLockOnGotFocus)
            End If

        End If
    End Sub

    Private Sub Textbox_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.TextChanged
        If Me.Text.isEmptyOrWhiteSpace Then
            Me.Invalidate() 'Aangepast 15-01-2016. 'Alleen nodig voor description lijkt mij. Die wordt alleen getoond als de tekst leeg is.
        End If

        If LastTextChangeWasByKeyboard AndAlso Me.RaiseChangeCompleted Then
            tmrDoneTyping.Stop()
            tmrDoneTyping.Start()
        End If

        CheckAutoComplete()

        Me.NotifyValueChangedReceiver
    End Sub

    Private sSearchText As String = ""
    Private iLastSearchActionThreadID As Integer

    Public Event AddAutoCompleteItemsToListview(SearchText As String, ByRef Listview As ListView)

    Public Sub RaiseGetAutoCompleteListviewItems()
        Dim MyItems As New List(Of ListViewItem)
        Dim MyColumns As New List(Of ColumnHeader)

        Try
            RaiseEvent AddAutoCompleteItemsToListview(sSearchText, MyAutoCompleteListview)
        Catch TAE As System.Threading.ThreadAbortException
            'Niet stoppen! 
            Threading.Thread.ResetAbort()
        Catch ex As Exception

        End Try

        If Not iLastSearchActionThreadID.Equals(Threading.Thread.CurrentThread.ManagedThreadId) Then Return 'Dit was een vorige zoekactie.

        If MyAutoCompleteListview.Items.Count = 0 Then
            HideAutoCompleteListview()
        Else
            ShowAutoCompleteListviewItems(MyColumns, MyItems)
        End If
    End Sub

    Private Sub HideAutoCompleteListview()
        If Me.InvokeRequired Then
            Me.Invoke(New MethodInvoker(AddressOf HideAutoCompleteListview))
            Return
        End If

        If MyAutoCompleteListviewPopup Is Nothing Then Return
        If MyAutoCompleteListviewPopup.Visible Then MyAutoCompleteListviewPopup.Close()
    End Sub

    Private Delegate Sub Delegate_ShowAutoCompleteListviewItems(Columns As List(Of ColumnHeader), Items As List(Of ListViewItem))

    Public Event GetAutoCompleteListviewOffset(ByRef Result As Point)

    Private Sub ShowAutoCompleteListviewItems(Columns As List(Of ColumnHeader), Items As List(Of ListViewItem))
        If Me.InvokeRequired Then
            Dim Args(1) As Object
            Args(0) = Columns
            Args(1) = Items
            Dim X As New Delegate_ShowAutoCompleteListviewItems(AddressOf ShowAutoCompleteListviewItems)
            Me.Invoke(X, Args)
            Return
        End If

        If MyAutoCompleteListview.Items.Count = 0 Then
            HideAutoCompleteListview()
            Return
        End If

        Dim MyPoint As New Point(0, 0)

        RaiseEvent GetAutoCompleteListviewOffset(MyPoint)

        MyPoint = New Point(0 + MyPoint.X, Me.Height + MyPoint.Y)

        MyAutoCompleteListview.Width = MyAutoCompleteListview.TotalColumnWidth.Min(Me.Width)
        MyAutoCompleteListview.Height = 300
        MyAutoCompleteListview.Scrollable = True
        MyAutoCompleteListview.BorderStyle = BorderStyle.None

        With MyAutoCompleteListviewPopup
            .AutoClose = False
            .Show(Me, MyPoint)
        End With


    End Sub

    Private Sub CheckAutoComplete()
        If Not Me.AutoCompleteListView Then Return
        If Not Me.LastTextChangeWasByKeyboard Then Return

        Dim MyHandle As IntPtr = Me.MyAutoCompleteListview.Handle 'Zorg dat de handle wordt gecreeerd op de goede thread.

        Me.MyAutoCompleteListview.MultiSelect = False

        sSearchText = Me.Text.Trim

        If sSearchText.Length < 2 Then
            'Verberg de listview
            HideAutoCompleteListview()
            Return
        End If

        If Not MySearchThread Is Nothing Then MySearchThread.Abort()

        MySearchThread = New Threading.Thread(AddressOf RaiseGetAutoCompleteListviewItems)

        iLastSearchActionThreadID = MySearchThread.ManagedThreadId

        MySearchThread.Start()
    End Sub

    Public Function TextWithCrlfCorrected() As String
        Dim sResult As String = MyBase.Text
        sResult = sResult.Replace(Chr(10), "")
        sResult = sResult.Replace(Chr(13), vbCrLf)
        Return sResult
    End Function

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property LastTextChangeWasByKeyboard As Boolean = False

    Public Overrides Property Text As String
        Get
            Return MyBase.Text
        End Get
        Set(value As String)
            Me.LastTextChangeWasByKeyboard = False
            If value.isRTF Then value = FrameWorkNS.Functions.Richtext.RtfToText(value)
            MyBase.Text = value
            MyBase.Select(0, 0) 'Zet de cursor naar het begin van de tekst. Zorgt ervoor dat niet alle tekst standaard geselecteerd wordt.
        End Set
    End Property

    Public Sub GotoEndOfText()
        MyBase.Select(MyBase.TextLength, 0)
    End Sub

    Private Sub MyTimer_Elapsed(ByVal sender As Object, ByVal e As EventArgs) Handles tmrDoneTyping.Tick
        tmrDoneTyping.Stop()

        If bRaiseChangeCompleted Then
            RaiseEvent DoneTyping(Me, New EventArgs)
        End If

    End Sub

    '<DebuggerNonUserCode()> _
    'Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
    '    Select Case m.Msg
    '        Case FrameWorkNS.Functions.API.Messages.WM_PAINT
    '            MyBase.WndProc(m)
    '            DrawDescription()
    '            Return

    '        Case Else
    '            MyBase.WndProc(m)
    '    End Select
    'End Sub

    'Private Sub DrawDescription()
    '    If Not Me.Text.isEmptyOrWhiteSpace Then Return
    '    If Me.Description.isEmptyOrWhiteSpace Then Return
    '    If Me.Focused Then Return

    '    Dim iOffsetX As Integer = 0

    '    If Me.Multiline Then
    '        MyStringFormat.Alignment = StringAlignment.Center
    '        MyStringFormat.LineAlignment = StringAlignment.Center
    '    Else
    '        MyStringFormat.Alignment = StringAlignment.Near
    '        MyStringFormat.LineAlignment = StringAlignment.Far
    '        iOffsetX = 1
    '    End If

    '    'If Me.Width < 30 Then iOffsetX = 0

    '    Dim G As Graphics = Me.CreateGraphics 'Graphics.FromHwnd(Me.Handle)
    '    Dim MyRect As New Rectangle(iOffsetX, 0, Me.ClientRectangle.Width - iOffsetX, Me.ClientRectangle.Height)
    '    Dim MyFont As New Font("Courier new", 8, FontStyle.Italic) 'New Font(Me.Font, FontStyle.Italic)

    '    MyRect.Width += 10

    '    G.Clear(Me.BackColor)
    '    G.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
    '    G.DrawString(Me.Description, MyFont, Brushes.Gray, MyRect, MyStringFormat)

    '    MyFont.Destroy()
    'End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        MyStringFormat.Alignment = StringAlignment.Near
        MyStringFormat.LineAlignment = StringAlignment.Center

        Me.tmrDoneTyping.Interval = 250
    End Sub



    Private Sub Textbox_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If Me.SelectNearestControlWithArrowKeys OrElse SelectNearestTextboxWithArrowKeys Then
            If My.Computer.Keyboard.ShiftKeyDown Then Return

            Select Case e.KeyCode
                Case Keys.Left
                    If Me.SelectionStart = 0 Then e.Handled = CheckNearestControl(FrameWorkNS.Functions.Forms.eSide.Left)

                Case Keys.Right
                    If Me.SelectionStart = Me.TextLength Then e.Handled = CheckNearestControl(FrameWorkNS.Functions.Forms.eSide.Right)

                Case Keys.Up
                    e.Handled = CheckNearestControl(FrameWorkNS.Functions.Forms.eSide.Up)

                Case Keys.Down
                    e.Handled = CheckNearestControl(FrameWorkNS.Functions.Forms.eSide.Down)
            End Select
        End If

        Select Case e.KeyCode
            Case Keys.Escape, Keys.Return, Keys.Tab
                HideAutoCompleteListview()
                e.Handled = True
                Return
        End Select

        If Not MyAutoCompleteListview Is Nothing AndAlso Me.MyAutoCompleteListview.Visible Then
            Select Case e.KeyCode
                Case Keys.Down

                    Me.MyAutoCompleteListview.SelectFirstItem(True)
                    Me.MyAutoCompleteListview.Select()
                    Me.MyAutoCompleteListview.Focus()
            End Select
        End If

        Me.LastTextChangeWasByKeyboard = True
    End Sub

    Private Function CheckNearestControl(Side As FrameWorkNS.Functions.Forms.eSide) As Boolean
        Dim NearestControl As Control = Nothing

        If Me.SelectNearestControlWithArrowKeys Then
            NearestControl = FrameWorkNS.Functions.Forms.GetNearestControl(Of Textbox)(Me, Side)
        ElseIf Me.SelectNearestTextboxWithArrowKeys Then
            NearestControl = FrameWorkNS.Functions.Forms.GetNearestControl(Of Textbox)(Me, Side)
        End If

        If NearestControl Is Nothing Then Return False

        NearestControl.Focus()

        If Side = FrameWorkNS.Functions.Forms.eSide.Left Then DirectCast(NearestControl, Textbox).GotoEndOfText()

        Return True
    End Function

    Private Sub SelectSelectedItemFromListview()
        RaiseEvent BeforeItemChosenFromAutoCompleteListview(Me.Text, MyAutoCompleteListview.SelectedItem.Tag)

        Me.Text = MyAutoCompleteListview.SelectedItem.Text
        MyAutoCompleteListviewPopup.Close()
        Me.Focus()
        Me.GotoEndOfText()
        RaiseEvent ItemChosenFromAutoCompleteListview(MyAutoCompleteListview.SelectedItem.Tag)
    End Sub

    Public Event BeforeItemChosenFromAutoCompleteListview(SearchText As String, Item As iSelectableObject)
    Public Event ItemChosenFromAutoCompleteListview(Item As iSelectableObject)

    Private Sub MyAutoCompleteListview_KeyDown(sender As Object, e As KeyEventArgs) Handles MyAutoCompleteListview.KeyDown
        Select Case e.KeyCode
            Case Keys.Enter

                SelectSelectedItemFromListview()


            Case Keys.Up
                If MyAutoCompleteListview.SelectedItem Is MyAutoCompleteListview.FirstItem Then
                    Me.Focus()
                    Me.GotoEndOfText()

                End If
        End Select
    End Sub

    Private Sub MyAutoCompleteListview_MouseClick(sender As Object, e As MouseEventArgs) Handles MyAutoCompleteListview.MouseClick
        If MyAutoCompleteListview.SelectedItem Is Nothing Then Return

        If Settings.SelectAutocompleteOnSingleClick Then SelectSelectedItemFromListview()
    End Sub

    Private Sub MyAutoCompleteListview_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles MyAutoCompleteListview.MouseDoubleClick
        If MyAutoCompleteListview.SelectedItem Is Nothing Then Return
        If Not Settings.SelectAutocompleteOnSingleClick Then SelectSelectedItemFromListview()
    End Sub

    Private Sub Textbox_HandleCreated(sender As Object, e As EventArgs) Handles Me.HandleCreated
        CreateDescription()
    End Sub

    Private Function ShowFavoriteMedihulpenAtToplevel() As Boolean
        Return True
    End Function

    Public Event MedihulpCompleted(Medihulp As MCSCTRLS2.Medihulp)

    Public Event BeforeShowContextMenu(Sender As MCSCTRLS1.Textbox, Menu As MCSCTRLS2.ContextMenu, ByRef bCancel As Boolean)
    Public Event ContextMenuClick(Sender As MCSCTRLS1.Textbox, Key As String, ClickedItem As MCSCTRLS2.EasyMenuItem)

    Private Sub Textbox_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        If e.Button <> MouseButtons.Right Then Return
        If Not Me.Enabled Then Return
        If Not Me.ShowDefaultEasyCareMenu Then Return

        Dim bEnabled As Boolean = True
        Dim MyMenu As New MCSCTRLS2.ContextMenu
        Dim sKey As String
        Dim mnuMedihulpen As MCSCTRLS2.EasyMenuItem
        Dim MyItem As MCSCTRLS2.EasyMenuItem

        If Me.ReadOnly Then bEnabled = False

        MyMenu.AddDefaultItem(MCSCTRLS2.ContextMenu.eDefaultContextMenuItem.Copy,,, Me.SelectedText.Length > 0)
        MyMenu.AddDefaultItem(MCSCTRLS2.ContextMenu.eDefaultContextMenuItem.Paste,,, bEnabled AndAlso My.Computer.Clipboard.ContainsText(TextDataFormat.Text))
        MyMenu.AddDefaultItem(MCSCTRLS2.ContextMenu.eDefaultContextMenuItem.Cut,,, bEnabled And Me.SelectedText.Length > 0)

        If Not Me.MedihulpKey.isEmptyOrWhiteSpace Then
            MyMenu.AddDefaultItem(MCSCTRLS2.ContextMenu.eDefaultContextMenuItem.Seperator)
            mnuMedihulpen = MyMenu.AddItem("MEDIHULPEN", "Medihulpen", Nothing)

            MCSCTRLS2.MedihulpManager.VulContextMenu(Me.MedihulpKey, mnuMedihulpen)
        End If

        If ShowFavoriteMedihulpenAtToplevel() AndAlso Not mnuMedihulpen Is Nothing Then
            Dim lstMove As New List(Of MCSCTRLS2.EasyMenuItem)
            Dim lstAllItems As New List(Of MCSCTRLS2.EasyMenuItem)

            For Each MyItem In mnuMedihulpen.MenuItems
                If MyItem.Key.ToUpper.StartsWith("MEDIHULP_") Then
                    lstMove.Add(MyItem)
                End If
            Next

Again:
            For Each MyItem In lstMove
                If Not mnuMedihulpen.MenuItems.Contains(MyItem) Then Continue For

                mnuMedihulpen.MenuItems.Remove(MyItem)
                GoTo Again
            Next

            If lstMove.Count > 0 Then
                lstAllItems.AddRange(lstMove.ToArray)
                lstAllItems.Add(New MCSCTRLS2.EasyMenuItem("-", Nothing))
            End If


            For Each MyItem In MyMenu.MenuItems
                lstAllItems.Add(MyItem)
            Next

            MyMenu.MenuItems.Clear()
            MyMenu.MenuItems.AddRange(lstAllItems.ToArray)


        End If

        Dim bCancel As Boolean = False

        RaiseEvent BeforeShowContextMenu(Me, MyMenu, bCancel)

        If bCancel Then Return

        Dim MyMenuItem As MCSCTRLS2.EasyMenuItem

        MyMenuItem = MyMenu.ShowMenu(Me)
        sKey = MyMenuItem.Key

        MyMenu.Dispose()

        If sKey.isEmptyOrWhiteSpace Then Return

        Select Case sKey
            Case "COPY"
                Me.SelectedText.CopyToClipboard

            Case "PASTE"
                Dim sText As String = My.Computer.Clipboard.GetText

                If sText.isEmptyOrWhiteSpace Then Return

                Me.SelectedText = sText

            Case "CUT"
                Dim sText As String = Me.SelectedText

                If sText.Length = 0 Then Return

                Me.SelectedText = ""

                My.Computer.Clipboard.SetText(sText)

            Case Else
                If sKey.StartsWith("EDITMEDIHULP_") Then
                    sKey = sKey.Substring(13)

                    MCSCTRLS2.MedihulpManager.EditMedihulp(sKey, Me.FindForm)
                ElseIf sKey.ToUpper.StartsWith("MEDIHULP_") Then
                    Dim MyMedihulp As MCSCTRLS2.Medihulp = MyMenu.ClickedItem.Tag

                    MCSCTRLS2.MedihulpManager.DoeMedihulp(MyMedihulp, Me, Me.EasyCareApplication, Me.FindForm)

                    Me.GotoEndOfText()
                    RaiseEvent MedihulpCompleted(MyMedihulp)
                Else
                    RaiseEvent ContextMenuClick(Me, MyMenuItem.Key, MyMenuItem)
                End If

        End Select


    End Sub


End Class
