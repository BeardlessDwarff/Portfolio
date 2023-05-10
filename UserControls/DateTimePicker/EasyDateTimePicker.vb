Imports System.Runtime.InteropServices
Imports FrameworkNS.Functions
Imports System.Windows.Forms.VisualStyles
Imports System.ComponentModel
Imports MCS_Interfaces

<DefaultEvent("ValueChangedAfterInterval")>
Public Class DateTimePicker
    Private DayNameRect As New Rectangle(0, 0, 58, 20)
    Private DayRect As New Rectangle(54, 0, 18, 20)
    Private MonthRect As New Rectangle(70, 0, 58, 20)
    Private YearRect As New Rectangle(120, 0, 40, 20)
    Private CheckBoxRect As New Rectangle(2, 3, 16, 16)
    Private MySelectedElement As eElement = eElement.None
    Private sInput As String = ""
    Private bButtonMouseDown As Boolean
    Private ButtonRect As Rectangle
    Private MyUnCheckedString As String = ""
    Private MyShowCheckBox As Boolean = False
    Private MyChecked As Boolean = True
    Private WithEvents MyToolstripDropDown As New EasyToolstripDropDown
    Private WithEvents MyMonthCalendar As New MCSCTRLS1.MonthCalendar
    Private WithEvents tmrCheckPopup As New System.Timers.Timer(300)
    Public Event BeforePopup(ByVal sender As Object)
    Public Event CloseUp(ByVal Sender As Object, ByVal e As System.EventArgs)
    Public Shadows Event Validating(ByVal Sender As Object, ByVal E As System.ComponentModel.CancelEventArgs)
    Public Event ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Public Event ValueChangedAfterInterval(ByVal sender As System.Object, ByVal e As System.EventArgs)
    Private MyDisplayStyle As eDisplayStyle = eDisplayStyle.AutoSize

    Enum eDisplayStyle
        DagUitgebreid_Dag_MaandUitgebreid_Jaar = 0
        Dag_MaandUitgebreid_Jaar = 1
        Dag_Maand_Jaar = 2
        DagUitgebreid_Dag_Maand_Jaar = 3
        AutoSize = 4
    End Enum

    Public Property DisplayStyle() As eDisplayStyle
        Get
            Return MyDisplayStyle
        End Get
        Set(ByVal value As eDisplayStyle)
            MyDisplayStyle = value
            Me.Invalidate()
        End Set
    End Property

    Public Property CalendarForeColor() As Color
        Get
            Return MyMonthCalendar.ForeColor
        End Get
        Set(ByVal value As Color)
            MyMonthCalendar.ForeColor = value
        End Set
    End Property

    Public Property CustomFormat() As String
        Get
            Return ""
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Public Property Format() As String
        Get
            Return ""
        End Get
        Set(ByVal value As String)
        End Set
    End Property

    Public Property MinDate() As Date
        Get
            If MyMonthCalendar Is Nothing Then MyMonthCalendar = New MCSCTRLS1.MonthCalendar
            Return MyMonthCalendar.MinDate
        End Get
        Set(ByVal value As Date)
            'If value < Me.MyMonthCalendar.MinDate Then value = Me.MyMonthCalendar.MinDate 
            If value > Me.Value Then
                Me.Value = value
            End If
            If value > Me.MaxDate Then
                Me.MaxDate = value
            End If
            If value < CDate("1-1-1753") Then value = CDate("1-1-1753")
            If value = New Date(Date.MaxValue.Year, Date.MaxValue.Month, Date.MaxValue.Day) Then value = CDate("1-1-1753")
            Me.MyMonthCalendar.MinDate = value
        End Set
    End Property

    Public Property MaxDate() As Date
        Get
            If MyMonthCalendar Is Nothing Then MyMonthCalendar = New MCSCTRLS1.MonthCalendar
            Return Me.MyMonthCalendar.MaxDate
        End Get
        Set(ByVal value As Date)
            value = value.EndOfDay

            If value < Me.Value Then Me.Value = value
            If value < Me.MinDate Then Me.MinDate = value

            Me.MyMonthCalendar.MaxDate = value
        End Set
    End Property

    Public Property UnCheckedString() As String
        Get
            Return MyUnCheckedString
        End Get
        Set(ByVal value As String)
            MyUnCheckedString = value
            Me.Invalidate()
        End Set
    End Property

    Public Property SelectedElement() As eElement
        Get
            Return MySelectedElement
        End Get
        Set(ByVal value As eElement)
            MySelectedElement = value
            sInput = ""
            Me.Invalidate()
        End Set
    End Property

    Public Property ShowCheckBox() As Boolean
        Get
            Return MyShowCheckBox
        End Get
        Set(ByVal value As Boolean)
            MyShowCheckBox = value
            Me.Invalidate()
        End Set
    End Property

    Public Event CheckedChanged(Checked As Boolean)

    Public Property Checked() As Boolean
        Get
            If Not Me.ShowCheckBox Then Return True
            Return MyChecked
        End Get
        Set(ByVal value As Boolean)
            If MyChecked <> value Then
            MyChecked = value
                RaiseEvent CheckedChanged(MyChecked)
            End If
            MyChecked = value
            If Me.ShowCheckBox Then
                Dim MyEventArgs As New EventArgs
                RaiseEvent ValueChanged(Me, MyEventArgs)
            End If
            Me.Invalidate()
        End Set
    End Property

    Private Sub CheckMinMax(Value As Date)
        If Value < Me.MinDate Then Me.MinDate = Value.RemoveTime
        If Value > Me.MaxDate Then Me.MaxDate = Value.EndOfDay

    End Sub
    Public Sub SetValueWithoutRaisingEvents(Value As Date)
        DontRaiseEvents = True
        CheckMinMax(Value)
        MyMonthCalendar.SelectionStart = Value
        Me.Invalidate()

        LastValue = Value

        DontRaiseEvents = False
    End Sub

    Private DontRaiseEvents As Boolean = False

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property Value() As Date
        Get
            If MyMonthCalendar Is Nothing Then MyMonthCalendar = New MCSCTRLS1.MonthCalendar
            Return MyMonthCalendar.SelectionStart
        End Get
        Set(ByVal value As Date)
            If value = MyMonthCalendar.SelectionStart Then
                Me.Invalidate()
                RaiseValueChanged()
                Return
            End If

            'MyMonthCalendar.SelectionStart = value

            Try
                If value > MyMonthCalendar.MinDate AndAlso value < MyMonthCalendar.MaxDate Then MyMonthCalendar.SelectionStart = value
            Catch ex As Exception

            End Try

            Me.Invalidate()
            RaiseValueChanged()
        End Set
    End Property

    Private WithEvents tmrRaiseValueChangedAfterInterval As New System.Windows.Forms.Timer

    Private Sub tmrRaiseValueChangedAfterInterval_Tick(sender As Object, e As System.EventArgs) Handles tmrRaiseValueChangedAfterInterval.Tick
        Me.tmrRaiseValueChangedAfterInterval.Stop()

        Dim MyEventArgs As New EventArgs
        RaiseEvent ValueChangedAfterInterval(Me, MyEventArgs)

    End Sub

    Private LastValue As Date = Date.MinValue

    Private Sub RaiseValueChanged()
        If DontRaiseEvents Then Return

        If Me.Value.RemoveTime.Equals(LastValue.RemoveTime) Then Return
        LastValue = Me.Value

        Me.tmrRaiseValueChangedAfterInterval.Stop()
        Me.tmrRaiseValueChangedAfterInterval.Start()

        Dim MyEventArgs As New EventArgs
        RaiseEvent ValueChanged(Me, MyEventArgs)
    End Sub

    Private Sub RaiseValidating()
        Static LastValue As Date = Date.MinValue

        If Me.Value = LastValue Then Return
        LastValue = Me.Value

        Dim MyEventArgs As New System.ComponentModel.CancelEventArgs
        RaiseEvent Validating(Me, MyEventArgs)

    End Sub

    Enum eElement
        None
        Day
        Month
        Year
    End Enum

    Protected Overrides Sub OnLayout(ByVal e As System.Windows.Forms.LayoutEventArgs)
        MyBase.OnLayout(e)
        e.AffectedControl.Height = 20
    End Sub

    Public Overloads ReadOnly Property CanFocus() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overloads ReadOnly Property CanSelect() As Boolean
        Get
            Return True
        End Get
    End Property

    Private Function DropDownControl() As Control
        Return MyMonthCalendar
    End Function

    Private Sub ShowDateTimePicker()
        If Me.tmrCheckPopup.Enabled Then Return
        If Not Me.Checked Then Me.Checked = True
        RaiseEvent BeforePopup(Me)
        MyToolstripDropDown.Items.Clear()
        MyToolstripDropDown.Items.Add(New ToolStripControlHost(DropDownControl))
        MyToolstripDropDown.Show(GeefPopupPositie)

    End Sub

    Private Sub ShowDateTimePickerInThread()

    End Sub

    Private Function GeefPopupPositie() As Point
        Dim MyPoint As Point = Me.PointToScreen(New Point(0, 0))
        MyPoint.Y += Me.Height

        MyPoint.Y -= 4

        If MyPoint.Y + Me.DropDownControl.Height > My.Computer.Screen.WorkingArea.Height Then
            MyPoint.Y = Me.PointToScreen(New Point(0, 0)).Y - Me.DropDownControl.Height - 3
        End If

        MyPoint.X -= 1

        Return MyPoint

    End Function

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        SetStyle(ControlStyles.Selectable, True)

        Me.Height = 20

        With MyMonthCalendar
            .MaxSelectionCount = 1 'Gewoon 1 dag.
        End With


        With MyToolstripDropDown
            .Margin = New System.Windows.Forms.Padding(0)
            .AutoSize = True
            .Opacity = 1
        End With

        Me.tmrRaiseValueChangedAfterInterval.Interval = 500
    End Sub

    Private Function HighlightBrush() As SolidBrush
        Return New SolidBrush(SystemColors.Highlight)
    End Function

    Private Sub DateTimePicker_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        If Not Me.MyToolstripDropDown Is Nothing Then
        Me.MyToolstripDropDown.Dispose()
        Me.MyToolstripDropDown = Nothing
        End If

        If Not MyMonthCalendar Is Nothing Then
        MyMonthCalendar.Dispose()
        MyMonthCalendar = Nothing
        End If

        If Not tmrCheckPopup Is Nothing Then
        tmrCheckPopup.Dispose()
        tmrCheckPopup = Nothing
        End If
        
    End Sub


    Private Sub EasyDateTimePicker_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.GotFocus
        Me.Invalidate()
    End Sub

    Private Sub EasyDateTimePicker_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LostFocus
        Me.Invalidate()
        RaiseValidating()
    End Sub

    Private Sub EasyDateTimePicker_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If e.Button <> Windows.Forms.MouseButtons.Left Then Return

        If ButtonRect.Contains(e.Location) Then
            bButtonMouseDown = True
            Me.Invalidate()
            ShowDateTimePicker()
            Return
        End If

        If Me.ShowCheckBox AndAlso CheckBoxRect.Contains(e.Location) Then
            Me.Checked = Not Me.Checked
            Me.Invalidate()
            Return
        End If

        If DayRect.Contains(e.Location) Then
            Me.SelectedElement = eElement.Day
            Me.Invalidate()
            Return
        End If

        If MonthRect.Contains(e.Location) Then
            Me.SelectedElement = eElement.Month
            Me.Invalidate()
            Return
        End If

        If YearRect.Contains(e.Location) Then
            Me.SelectedElement = eElement.Year
            Me.Invalidate()
            Return
        End If
    End Sub

    Public Overrides Property Text() As String
        Get
            If Not Me.Checked Then Return ""
            Return Me.Value.ToLongDateString
        End Get
        Set(ByVal value As String)
            Me.Value = Date.Parse(value)
        End Set
    End Property

    Private Function GetPaintingDisplayStyle()

        Dim iWidth As Integer = Me.Width

        If Me.ShowCheckBox Then iWidth -= 20 'Reserveer 20 pixels voor de checkbox

        Select Case Me.DisplayStyle
            Case eDisplayStyle.AutoSize
                Select Case iWidth

                    Case Is < 120
                        Return eDisplayStyle.Dag_Maand_Jaar

                    Case Is < 180
                        Return eDisplayStyle.Dag_MaandUitgebreid_Jaar

                    Case Else
                        Return eDisplayStyle.DagUitgebreid_Dag_MaandUitgebreid_Jaar
                End Select
            Case Else
                Return Me.DisplayStyle
        End Select
    End Function

    Private Sub EasyDateTimePicker_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        Dim BorderColor As Color = Color.FromArgb(127, 157, 185)
        Dim BorderRect As Rectangle = Me.ClientRectangle
        Dim G As Graphics = e.Graphics
        Dim MySF As New StringFormat

        DayNameRect = New Rectangle(0, 0, 58, 20)
        DayRect = New Rectangle(56, 0, 18, 20)
        MonthRect = New Rectangle(70, 0, 58, 20)
        YearRect = New Rectangle(120, 0, 40, 20)
        ButtonRect = New Rectangle(Me.ClientRectangle.Width - 19, 2, 17, 16)

        Dim ixOffset As Integer = 0

        If ShowCheckBox Then
            ixOffset = 15
            DrawCheckBox(G)
        End If

        Dim sDagNaam As String = Dag()

        If sDagNaam.isEmptyOrWhiteSpace Then ixOffset -= 58

        DayNameRect.X += ixOffset
        DayRect.X += ixOffset
        MonthRect.X += ixOffset
        YearRect.X += ixOffset

        MySF.Alignment = StringAlignment.Center
        MySF.LineAlignment = StringAlignment.Center

        BorderRect.Width -= 1
        BorderRect.Height -= 1

        If Me.Focused Then
            If Me.SelectedElement = eElement.None Then
                Me.SelectedElement = eElement.Day
            End If
        End If

        Select Case Me.GetPaintingDisplayStyle
            Case eDisplayStyle.Dag_Maand_Jaar
                ixOffset = IIf(Me.ShowCheckBox, 15, 0)
                Dim X As Integer
                Dim sDay As String = Me.Value.Day.ToString("00")
                Dim sMaand As String = Me.Value.Month.ToString("00")
                Dim sYear As String = Me.Value.Year.ToString("0000")

                Dim iDayWidth As Integer = G.MeasureString(sDay, Me.Font).Width
                Dim iMonthWidth As Integer = G.MeasureString(sMaand, Me.Font).Width
                Dim iStreepjeWidth As Integer = G.MeasureString("-", Me.Font).Width

                X = ixOffset
                DayRect = New Rectangle(X, 0, 18, 20)
                X += iDayWidth
                X += iStreepjeWidth

                MonthRect = New Rectangle(X, 0, G.MeasureString(sMaand, Me.Font).Width, 20)
                X += iMonthWidth
                X += iStreepjeWidth

                YearRect = New Rectangle(X, 0, G.MeasureString(sYear, Me.Font).Width, 20)

                G.DrawString("-", Me.Font, Brushes.Black, New Point(DayRect.Right, 4))
                G.DrawString("-", Me.Font, Brushes.Black, New Point(MonthRect.Right, 4))
        End Select


        'G.DrawRectangle(Pens.Red, DayRect)
        'G.DrawRectangle(Pens.Red, MonthRect)
        'G.DrawRectangle(Pens.Blue, YearRect)

        If Me.ShowCheckBox And Not Me.Checked Then
            Dim TempSF As New StringFormat
            TempSF.LineAlignment = StringAlignment.Center
            G.DrawString(Me.UnCheckedString, Me.Font, New SolidBrush(SystemColors.GrayText), New Rectangle(DayNameRect.X, DayNameRect.Y, 1000, DayNameRect.Height), TempSF)
        Else
            DayNameRect.Y += 1
            If Not sDagNaam.isEmptyOrWhiteSpace Then G.DrawString(Dag, Me.Font, New SolidBrush(SystemColors.WindowText), DayNameRect, MySF)

            DayRect.Y += 1
            MonthRect.Y += 1
            YearRect.Y += 1

            DrawDay(G, Me.SelectedElement = eElement.Day)
            DrawMonth(G, Me.SelectedElement = eElement.Month)
            DrawYear(G, Me.SelectedElement = eElement.Year)
        End If

        G.DrawRectangle(New Pen(BorderColor), BorderRect)

        DrawComboButton(G, ButtonRect, bButtonMouseDown)

        'G.FillRectangle(Brushes.White, New Rectangle(0, 0, 70, 20))
        'G.DrawString(Me.ClientRectangle.Width.ToString, SystemInformation.MenuFont, Brushes.Blue, 2, 2)

    End Sub


    Private Sub DrawComboButtonNoStyle(ByVal G As Graphics, ByVal Rect As Rectangle, ByVal MouseDown As Boolean)
        If MouseDown Then
            ControlPaint.DrawComboButton(G, Rect, ButtonState.Pushed)
        Else
            ControlPaint.DrawComboButton(G, Rect, ButtonState.Normal)
        End If
    End Sub

    Private Sub DrawComboButton(ByVal G As Graphics, ByVal Rect As Rectangle, ByVal MouseDown As Boolean)
        Dim Renderer As VisualStyleRenderer

        If Not GDI.IsVisualStylesEnabled Then
            DrawComboButtonNoStyle(G, Rect, MouseDown)
            Return
        End If

        If MouseDown Then
            Renderer = New VisualStyleRenderer(VisualStyleElement.ComboBox.DropDownButton.Pressed)
        Else
            Renderer = New VisualStyleRenderer(VisualStyleElement.ComboBox.DropDownButton.Normal)
        End If

        Renderer.DrawBackground(G, Rect)

    End Sub

    Private Sub DrawCheckboxNoStyle(ByVal G As Graphics, ByVal Checked As Boolean)
        If Checked Then
            ControlPaint.DrawCheckBox(G, CheckBoxRect, ButtonState.Checked)
        Else
            ControlPaint.DrawCheckBox(G, CheckBoxRect, ButtonState.Normal)
        End If
    End Sub

    Private Sub DrawCheckBox(ByVal G As Graphics)
        Dim Renderer As VisualStyleRenderer

        If Not GDI.IsVisualStylesEnabled Then
            DrawCheckboxNoStyle(G, Me.Checked)
            Return
        End If

        If Me.Checked Then
            If Me.Focused Then
                Renderer = New VisualStyleRenderer(VisualStyleElement.Button.CheckBox.CheckedHot)
            Else
                Renderer = New VisualStyleRenderer(VisualStyleElement.Button.CheckBox.CheckedNormal)
            End If
        Else
            If Me.Focused Then
                Renderer = New VisualStyleRenderer(VisualStyleElement.Button.CheckBox.UncheckedHot)
            Else
                Renderer = New VisualStyleRenderer(VisualStyleElement.Button.CheckBox.UncheckedNormal)
            End If

        End If

        Renderer.DrawBackground(G, CheckBoxRect)
    End Sub

    Private Sub DrawYear(ByVal G As Graphics, Optional ByVal Highlight As Boolean = False)
        Dim MyWidth As Single
        Dim MyRect As Rectangle

        MyWidth = G.MeasureString(Value.Year, Me.Font, 1000).Width

        Dim X As Integer

        YearRect.X += 2 'iets ruimte geven
        X = YearRect.X + (YearRect.Width / 2) - (MyWidth / 2)

        MyRect = New Rectangle(X, 3, MyWidth, 14)

        If Highlight And Me.Focused Then G.FillRectangle(HighlightBrush, MyRect)

        Dim MySF As New StringFormat
        MySF.LineAlignment = StringAlignment.Center
        MySF.Alignment = StringAlignment.Center

        Dim MyBrush As SolidBrush

        If Highlight And Me.Focused Then
            MyBrush = New SolidBrush(SystemColors.HighlightText)
        Else
            MyBrush = New SolidBrush(SystemColors.WindowText)
        End If

        G.DrawString(Value.Year, Me.Font, MyBrush, YearRect, MySF)

        MySF.Dispose()
    End Sub

    Private Sub DrawMonth(ByVal G As Graphics, Optional ByVal Highlight As Boolean = False)
        Dim MyWidth As Single
        Dim MyRect As Rectangle
        Dim sText As String = Me.Maand

        If Me.GetPaintingDisplayStyle = eDisplayStyle.Dag_Maand_Jaar Then sText = Me.Value.Month.ToString("00")

        MyWidth = G.MeasureString(sText, Me.Font).Width
        'MyWidth -= 5

        Dim X As Integer

        X = MonthRect.X
        X += 2 'iets ruimte geven
        If Me.GetPaintingDisplayStyle <> eDisplayStyle.Dag_Maand_Jaar Then
            X += (MonthRect.Width / 2) - (MyWidth / 2)
        End If

        MyRect = New Rectangle(X, MonthRect.Y, MyWidth + 1, MonthRect.Height)

        If Highlight And Me.Focused Then G.FillRectangle(HighlightBrush, MyRect)

        Dim MySF As New StringFormat
        MySF.LineAlignment = StringAlignment.Center
        MySF.Alignment = StringAlignment.Center

        Dim MyBrush As SolidBrush

        If Highlight And Me.Focused Then
            MyBrush = New SolidBrush(SystemColors.HighlightText)
        Else
            MyBrush = New SolidBrush(SystemColors.WindowText)
        End If

        G.DrawString(sText, Me.Font, MyBrush, MyRect, MySF)

        MySF.Dispose()
    End Sub

    Private Sub DrawDay(ByVal G As Graphics, Optional ByVal HighLight As Boolean = False)
        Dim MyWidth As Single
        Dim MyRect As Rectangle
        Dim sText As String = Value.Day.ToString

        If Me.GetPaintingDisplayStyle = eDisplayStyle.Dag_Maand_Jaar Then sText = Value.Day.ToString("00")

        MyWidth = G.MeasureString(sText, Me.Font, 100).Width
        MyRect = New Rectangle(DayRect.X + DayRect.Width - MyWidth, 3, MyWidth, 14)
        MyRect.X += 2

        If HighLight And Me.Focused Then G.FillRectangle(HighlightBrush, MyRect)

        Dim MySF As New StringFormat
        MySF.LineAlignment = StringAlignment.Center
        MySF.Alignment = StringAlignment.Far

        Dim MyBrush As SolidBrush

        If HighLight And Me.Focused Then
            MyBrush = New SolidBrush(SystemColors.HighlightText)
        Else
            MyBrush = New SolidBrush(SystemColors.WindowText)
        End If

        G.DrawString(sText, Me.Font, MyBrush, DayRect, MySF)

        MySF.Dispose()
    End Sub

    Public Function Maand() As String
        Select Case Me.GetPaintingDisplayStyle
            Case eDisplayStyle.Dag_MaandUitgebreid_Jaar, eDisplayStyle.DagUitgebreid_Dag_MaandUitgebreid_Jaar
                Select Case Value.Month
                    Case 1
                        Return "januari"

                    Case 2
                        Return "februari"
                    Case 3
                        Return "maart"

                    Case 4
                        Return "april"

                    Case 5
                        Return "mei"

                    Case 6
                        Return "juni"

                    Case 7
                        Return "juli"

                    Case 8
                        Return "augustus"

                    Case 9
                        Return "september"

                    Case 10
                        Return "oktober"

                    Case 11
                        Return "november"

                    Case Else
                        Return "december"

                End Select
            Case Else
                Return Value.Month
        End Select
    End Function

    Public Function Dag() As String
        Select Case Me.GetPaintingDisplayStyle
            Case eDisplayStyle.DagUitgebreid_Dag_Maand_Jaar, eDisplayStyle.DagUitgebreid_Dag_MaandUitgebreid_Jaar
                Select Case Value.DayOfWeek
                    Case DayOfWeek.Monday
                        Return "maandag"

                    Case DayOfWeek.Tuesday
                        Return "dinsdag"

                    Case DayOfWeek.Wednesday
                        Return "woensdag"

                    Case DayOfWeek.Thursday
                        Return "donderdag"

                    Case DayOfWeek.Friday
                        Return "vrijdag"

                    Case DayOfWeek.Saturday
                        Return "zaterdag"

                    Case Else
                        Return "zondag"

                End Select
            Case Else
                Return ""
        End Select
    End Function

    Private Sub DateTimePicker1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        Select Case e.KeyCode
            Case Keys.Down
                Select Case Me.SelectedElement
                    Case eElement.Day
                        Me.Value = Me.Value.AddDays(-1)

                    Case eElement.Month
                        Me.Value = Me.Value.AddMonths(-1)

                    Case eElement.Year
                        Me.Value = Me.Value.AddYears(-1)

                End Select

            Case Keys.Up

                Select Case Me.SelectedElement
                    Case eElement.Day
                        Me.Value = Me.Value.AddDays(1)

                    Case eElement.Month
                        Me.Value = Me.Value.AddMonths(1)

                    Case eElement.Year
                        Me.Value = Me.Value.AddYears(1)
                End Select

            Case Keys.Left
                Select Case Me.SelectedElement
                    Case eElement.Day
                        Me.SelectedElement = eElement.Year
                    Case eElement.Month
                        Me.SelectedElement = eElement.Day
                    Case eElement.Year
                        Me.SelectedElement = eElement.Month
                End Select


            Case Keys.Right
                Select Case Me.SelectedElement
                    Case eElement.Day
                        Me.SelectedElement = eElement.Month
                    Case eElement.Month
                        Me.SelectedElement = eElement.Year
                    Case eElement.Year
                        Me.SelectedElement = eElement.Day
                End Select

            Case Keys.Subtract, Keys.OemMinus, Keys.Enter, Keys.Decimal, Keys.Divide, Keys.Space  '-
                If Me.SelectedElement <> eElement.Year Then
                    If sInput <> "" Then Me.SelectedElement += 1
                End If

            Case Keys.NumPad0, Keys.D0
                HandelInputAf(0)

            Case Keys.NumPad1, Keys.D1
                HandelInputAf(1)

            Case Keys.NumPad2, Keys.D2
                HandelInputAf(2)

            Case Keys.NumPad3, Keys.D3
                HandelInputAf(3)

            Case Keys.NumPad4, Keys.D4
                HandelInputAf(4)

            Case Keys.NumPad5, Keys.D5
                HandelInputAf(5)

            Case Keys.NumPad6, Keys.D6
                HandelInputAf(6)

            Case Keys.NumPad7, Keys.D7
                HandelInputAf(7)

            Case Keys.NumPad8, Keys.D8
                HandelInputAf(8)

            Case Keys.NumPad9, Keys.D9
                HandelInputAf(9)

        End Select
    End Sub

    Private Sub HandelInputAf(ByVal Getal As Integer)
        sInput &= Getal
        Dim NewDate As Date

        Select Case Me.SelectedElement
            Case eElement.Day
                If Val(sInput) > 31 Then
                    Beep()
                    sInput = ""
                    Return
                End If

                Dim iJaar As Integer
                Dim iMaand As Integer

                If Val(sInput) > 0 Then
                    If AantalDagen(Value.Month, Value.Year) < sInput Then
                        iJaar = Value.Year
                        iMaand = Value.Month

                        Do While AantalDagen(iMaand, iJaar) < sInput
                            iMaand += 1

                            If iMaand > 12 Then
                                iMaand = iMaand Mod 12
                            End If

                        Loop
                        NewDate = New Date(iJaar, iMaand, sInput)
                    Else
                        NewDate = New Date(Value.Year, Value.Month, sInput)
                    End If

                    Me.Value = NewDate
                End If

                If sInput.Length = 2 OrElse (Val(sInput) > 3) Then
                    Me.SelectedElement = eElement.Month
                End If

            Case eElement.Month
                If Val(sInput) > 12 Then
                    Beep()
                    sInput = ""
                    Return
                End If

                If Val(sInput) > 0 Then
                    If Not IsValidDate(Value.Year, sInput, Value.Day) Then
                        If CInt(sInput) = 2 AndAlso Value.Day = 29 Then 'Kan alleen in een schikkeljaar.
                            Dim iYear As Integer = Value.Year

                            Do While Not IsValidDate(iYear, 2, 29)
                                iYear += 1
                            Loop

                            NewDate = New Date(iYear, 2, 29)
                            Me.Value = NewDate
                        End If

                        Beep()
                        sInput = ""
                        Return
                    End If

                    NewDate = New Date(Value.Year, sInput, Value.Day)
                    Me.Value = NewDate
                End If

                If (sInput.Length = 2) OrElse (Val(sInput) > 1) Then
                    Me.SelectedElement = eElement.Year
                End If

            Case eElement.Year
                Dim Jaar As Integer
                Jaar = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.ToFourDigitYear(sInput)

                If Not IsValidDate(Jaar, Value.Month, Value.Day) Then
                    If Value.Month = 2 AndAlso Value.Day = 29 Then 'Kan alleen in een schikkeljaar.
                        '  Dim iYear As Integer = Value.Year

                        Do While Not IsValidDate(Jaar, 2, 29)
                            Jaar += 1
                        Loop

                        NewDate = New Date(Jaar, 2, 29)
                        Me.Value = NewDate
                    End If

                    Beep()
                    sInput = ""
                    Return
                End If

                NewDate = New Date(Jaar, Value.Month, Value.Day)
                Me.Value = NewDate

                If sInput.Length > 3 Then
                    sInput = ""
                End If
        End Select


    End Sub

    Private Function IsValidDate(ByVal Year As Integer, ByVal Month As Integer, ByVal Day As Integer) As Boolean
        Dim MyDate As Date

        Try
            MyDate = New Date(Year, Month, Day)
        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

    Private Function AantalDagen(ByVal Maand As Integer, ByVal Jaar As Integer) As Integer
        Dim iDag As Integer = 27
        Dim MyDate As New Date(Jaar, Maand, iDag)

AddDay:
        iDag += 1

        Try
            MyDate = New Date(Jaar, Maand, iDag)
        Catch ex As Exception
            Return iDag - 1
        End Try

        GoTo AddDay

    End Function

    Private Sub DateTimePicker1_TriedToSetFocus()
        Me.Focus()
        Me.Select()
    End Sub

    Private Sub DateTimePicker1_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        RaiseValidating()
    End Sub

    Private Sub EasyDateTimePicker_PreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.PreviewKeyDownEventArgs) Handles Me.PreviewKeyDown
        Select Case e.KeyCode
            Case Keys.Tab
                e.IsInputKey = False

            Case Else
                e.IsInputKey = True
        End Select

    End Sub

    Private bHideBorder As Boolean = False

    Public Property HideBorder As Boolean
        Get
            Return bHideBorder
        End Get
        Set(value As Boolean)
            bHideBorder = value

            Select Case value
                Case True
                    SetHideBorderRegion()
                Case False
                    Me.Region = Nothing
            End Select


        End Set
    End Property

    Private Sub SetHideBorderRegion()
        Dim iWidth As Integer = Me.ClientRectangle.Width
        Dim iHeight As Integer = Me.ClientRectangle.Height

        Dim MyRect As New Rectangle(1, 1, iWidth - 2, iHeight - 2)

        Me.Region = New Region(MyRect)
    End Sub
    Private Sub EasyDateTimePicker_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        bButtonMouseDown = False
        Me.Invalidate()
    End Sub

    Private Sub MyToolstripDropDown_Closed(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripDropDownClosedEventArgs) Handles MyToolstripDropDown.Closed
        tmrCheckPopup.Stop()
        tmrCheckPopup.Start()
        Me.Invalidate()
        RaiseValueChanged()
    End Sub

    Private Sub MyToolstripDropDown_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyToolstripDropDown.Opening
        Dim Myrect As New Rectangle(0, 0, MyToolstripDropDown.Size.Width, MyToolstripDropDown.Size.Height)
        Myrect.Y += 3
        Myrect.X += 1
        Myrect.Width -= 2
        Myrect.Height -= 7
        MyToolstripDropDown.Region = New Region(Myrect)
    End Sub

    Private Sub MyMonthCalendar_DateChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DateRangeEventArgs) Handles MyMonthCalendar.DateSelected
        MyToolstripDropDown.Close()
        'Me.Invalidate()
        'RaiseValueChanged()
    End Sub

    Private Sub tmrCheckPopup_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles tmrCheckPopup.Elapsed
        tmrCheckPopup.Stop()
    End Sub

    Private Sub DateTimePicker_ParentChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ParentChanged

    End Sub

    Private Sub DateTimePicker_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.HideBorder Then SetHideBorderRegion()
        Me.Refresh()

    End Sub

    Private Sub DateTimePicker_Layout(sender As Object, e As LayoutEventArgs) Handles Me.Layout
        Dim iMinWidth As Integer = 96

        If Me.ShowCheckBox Then iMinWidth = 112

        If Me.Width < iMinWidth Then Me.Width = iMinWidth

    End Sub

    Private Sub DateTimePicker_ValueChanged(sender As Object, e As EventArgs) Handles Me.ValueChanged

    End Sub
End Class
