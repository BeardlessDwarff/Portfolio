Imports System
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Drawing.Printing
Imports MCSCTRLS2
Imports FrameWorkNS.Functions.API
Imports System.ComponentModel
Imports MCS_Interfaces
Imports System.Security.Permissions

<ToolboxBitmap(GetType(System.Windows.Forms.RichTextBox))>
Public Class RichTextBox

    Private Shared bShowMedihulpen As Boolean = False
    Private MyAutoFixFont As Boolean = True
    Private MyAutoCorrectCrLF As Boolean = True
    Private MyMask As eDataMask = eDataMask.None
    Private iPosOnKeyDown As Integer = 0

    Public Shadows Event ReadOnlyChanged(Value As Boolean)

    Public Enum eDataMask
        [None]
        [Time]
    End Enum

    Protected Overrides Sub OnReadOnlyChanged(e As EventArgs)
        MyBase.OnReadOnlyChanged(e)

        RaiseEvent ReadOnlyChanged(Me.ReadOnly)
    End Sub

    Public Sub FocusAndGotoEndOfText()
        Me.Focus()
        Me.Select()
        Me.GotoEndOfText()
    End Sub

    Public Sub FocusAndSelectAllText()
        Me.Focus()
        Me.Select()
        Me.SelectAll()
    End Sub

    Public Property Mask As eDataMask
        Get
            Return MyMask
        End Get
        Set(value As eDataMask)
            If value = MyMask Then Return
            MyMask = value
            SetMask()
        End Set
    End Property

    Private Sub SetMask()
        Select Case MyMask
            Case eDataMask.None
                Me.MaxLength = Integer.MaxValue
                Me.Text = ""

            Case eDataMask.Time
                Me.MaxLength = 5
                Me.Text = "__:__"

        End Select
    End Sub

    Public Sub AddText(Text As String)
        Me.GotoEndOfText()
        Me.SelectedText = Text
        Me.GotoEndOfText()
    End Sub

    Private Sub CheckTijdMask()
        Dim sTest As String

        Dim sNewText As String = Me.Text

        Do While sNewText.Length < 5
            sNewText &= "_"
            Me.Text = sNewText
        Loop

        Try
            sTest = Me.Text.Substring(2, 1)
        Catch ex As Exception
            sTest = ""
        End Try

        If Not isNumeric(Mid(sNewText, 1, 1)) Then Mid(sNewText, 1, 1) = "_"
        If Not isNumeric(Mid(sNewText, 2, 1)) Then Mid(sNewText, 2, 1) = "_"
        If Mid(sNewText, 3, 1) <> ":" Then Mid(sNewText, 3, 1) = ":"
        If Not isNumeric(Mid(sNewText, 4, 1)) Then Mid(sNewText, 4, 1) = "_"
        If Not isNumeric(Mid(sNewText, 5, 1)) Then Mid(sNewText, 5, 1) = "_"

        If Me.Text <> sNewText Then Me.Text = sNewText
    End Sub

    Public Property AutoFixFont As Boolean
        Get
            Return MyAutoFixFont
        End Get
        Set(value As Boolean)
            If Not value Then MyAutoFixFont = False
            MyAutoFixFont = value
        End Set
    End Property

    Public Event OnBeforePaste(ByRef Richtextbox As RichTextBox, ByRef Cancel As Boolean)
    Public Event OnAfterPaste(ByRef Richtextbox As RichTextBox)

#Region "Declarations"
    Shared MyWoordenBoek As Dictionary(Of String, String)
    Shared WithEvents MyListBox As New ListBox

    Private Shared MyPopuphelper As New FrameWorkNS.vbAccelerator.Components.Controls.PopupWindowHelper
    Private MyFrameWork As FrameWorkNS.FrameWork = EasyCare.FrameWork
    Private bAutoCorrectieActief As Boolean = True
    Private bAutoAanvullen As Boolean = True
    Private WithEvents tmrMouseOnWord As New Windows.Forms.Timer
    Private MousePos As New Point(0, 0)
    Private Shared sLastWord As String = ""
    Public Shadows Event KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property DontLooseFocus As Boolean = False
#End Region


    <DebuggerNonUserCode()>
    Public Property AutoCorrectCrLF() As Boolean
        Get
            Return MyAutoCorrectCrLF
        End Get
        Set(ByVal value As Boolean)
            MyAutoCorrectCrLF = value
        End Set
    End Property

    <DllImport("USER32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wp As IntPtr, ByVal lp As IntPtr) As IntPtr
    End Function

    Private Const anInch As Double = 14.4

    Public Sub GotoNewLine()
        Me.GotoEndOfText()
        Me.SelectedText = vbCrLf
        Me.GotoEndOfText()
    End Sub

    Public Sub GotoEndOfText()
        Me.Select(Me.Text.Length, 0)
    End Sub

    Public Property LockKey As String = ""

    Public Sub TrimLines()
        Dim sFind As String

        For X As Integer = 1 To 50
            sFind = vbCr & Space(X) & vbCr

            Do While Me.FindAndSelect(sFind)
                Me.SelectedText = vbCr & vbCr
            Loop
        Next
    End Sub

    Public Sub RemoveDoubleEmptyLines()
        Return 'Blijft problemen geven. Zie Jira AC-50

        'If Debugger.IsAttached Then Return

        If Not Me.Text.ContainsDoubleEmptyLines Then Return

        If Me.RTF.Contains("\line") Then Me.RTF = Me.RTF.Replace("\line", "\par")

        Dim iIndex As Integer = 1
        Dim sOrgRTF As String = Me.RTF

        Me.SuspendLayout()

        Me.TrimLines()

        Dim iSelStart As Integer = Me.SelectionStart
        Dim iSelLength As Integer = Me.SelectionLength
        Dim iTeller As Integer = 0

        Do While iIndex > -1
            iTeller += 1

            If iTeller > 200 Then
                'Dit gaat fout. Gebeurt in ieder geval soms als er tabellen in de rtf zitten.
                'We gaan terug naar de originele rtf.
                Me.RTF = sOrgRTF
                Exit Do
            End If


            Try
                iIndex = Me.Find(vbCr & vbCr & vbCr, 0, RichTextBoxFinds.None)
            Catch ex As Exception
                iIndex = -1
            End Try

            If iIndex > -1 Then
                Me.SelectedText = vbCr & vbCr
            End If
        Loop

        Me.Select(iSelStart, iSelLength)
        Me.ResumeLayout()
    End Sub

    Public Sub ReplaceColor(FromColor As Color, ToColor As Color)
        Me.RTF = Me.RTF.Replace("\red" & FromColor.R & "\green" & FromColor.G & "\blue" & FromColor.B & ";", "\red" & ToColor.R & "\green" & ToColor.G & "\blue" & ToColor.B & ";")
    End Sub

    Public Function CurrentLine() As Integer
        Dim iResult As Integer
        iResult = Me.GetLineFromCharIndex(Me.SelectionStart) + 1
        Return iResult
    End Function

    Public Function CurrentColumn() As Integer
        Dim lCurLine As Long
        lCurLine = 1 + Me.GetLineFromCharIndex(Me.SelectionStart)
        CurrentColumn = SendMessage(Me.Handle, CInt(&HBB), CInt(lCurLine - 1), IntPtr.Zero)
        CurrentColumn = ((Me.SelectionStart) - CurrentColumn) + 1
    End Function

    Public Property AlleenNumeriek As Boolean = False

    Public Overloads Property SelectedRTF As String
        Get
            Return MyBase.SelectedRtf
        End Get
        Set(value As String)
            If Not FrameWorkNS.Functions.Richtext.IsRTF(value) Then
                value = FrameWorkNS.Functions.Richtext.TextToRTF(value)
            End If

            MyBase.SelectedRtf = value
        End Set
    End Property

    Public Overloads Property SelectedText As String
        Get
            Return MyBase.SelectedText
        End Get
        Set(value As String)
            If value.isRTF Then
                MyBase.SelectedRtf = value
            Else
                MyBase.SelectedText = value
            End If
        End Set
    End Property

    Public Function ReplaceAll(sFind As String, Replacement As String, Optional Options As RichTextBoxFinds = RichTextBoxFinds.None) As Integer

        Dim iStart As Integer
        Dim iTeller As Integer = 0

        Try
            iStart = Me.Find(sFind, Options) 'Bij het zoeken naar chr(13) gaat deze functie fout als deze niet in de tekst zit.
        Catch ex As Exception
            Return 0
        End Try

        Do While iStart > -1
            iTeller += 1
            Me.Select(iStart, sFind.Length)
            Me.SelectedText = Replacement

            Try
                iStart = Me.Find(sFind, Options) 'Bij het zoeken naar chr(13) gaat deze functie fout als deze niet in de tekst zit.
            Catch ex As Exception
                Return iTeller
            End Try
        Loop

        Return iTeller
    End Function

    Public Function LineHeight(Optional CharIndex As Integer = -1) As Integer
        If CharIndex = -1 Then CharIndex = Me.SelectionStart
        Return TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(Me, CharIndex)
    End Function

    <DllImport("gdi32.dll", SetLastError:=True)>
    Public Shared Function SetBkMode(hdc As IntPtr, <MarshalAs(UnmanagedType.I4)> iBkMode As gdiBkMode) As Integer
    End Function
    Public Enum gdiBkMode As Integer
        TRANSPARENT = 1
        OPAQUE = 2
    End Enum

    Public Function Print(ByVal charFrom As Integer, ByVal charTo As Integer, ByVal Rectangle As Rectangle, ByVal G As Graphics, Optional ByRef RectBottom As Integer = 0) As Integer
        'Calculate the area to render and print
        Dim rectToPrint As RECT

        If Not G.Transform Is Nothing Then
            Rectangle.X += G.Transform.OffsetX
            Rectangle.Y += G.Transform.OffsetY
        End If

        rectToPrint.Top = CInt((Rectangle.Top * anInch))
        rectToPrint.Bottom = CInt((Rectangle.Bottom * anInch))
        rectToPrint.Left = CInt((Rectangle.Left * anInch))
        rectToPrint.Right = CInt((Rectangle.Right * anInch))

        'Calculate the size of the page
        Dim rectPage As RECT
        rectPage.Top = CInt((Rectangle.Top * anInch))
        rectPage.Bottom = CInt((Rectangle.Bottom * anInch))
        rectPage.Left = CInt((Rectangle.Left * anInch))
        rectPage.Right = CInt((Rectangle.Right * anInch))

        Dim hdc As IntPtr = G.GetHdc()

        Dim fmtRange As FORMATRANGE
        fmtRange.chrg.cpMax = charTo
        'Indicate character from to character to 
        fmtRange.chrg.cpMin = charFrom
        fmtRange.hdc = hdc
        'Use the same DC for measuring and rendering
        fmtRange.hdcTarget = hdc
        'Point at printer hDC
        fmtRange.rc = rectToPrint
        'Indicate the area on page to print
        fmtRange.rcPage = rectPage
        'Indicate size of page

        Dim res As IntPtr = IntPtr.Zero

        Dim wparam As IntPtr = New IntPtr(1) 'IntPtr.Zero

        'Get the pointer to the FORMATRANGE structure in memory
        Dim lparam As IntPtr = IntPtr.Zero
        lparam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange))
        Marshal.StructureToPtr(fmtRange, lparam, False)

        'Send the rendered data for printing 
        'SetBkMode(hdc, gdiBkMode.TRANSPARENT)
        res = SendMessage(Handle, Messages.EM_FORMATRANGE, wparam, lparam)

        Dim rc As FORMATRANGE = Marshal.PtrToStructure(lparam, GetType(FORMATRANGE))
        RectBottom = rc.rc.Bottom / anInch

        If Me.Text = "" Then
            RectBottom = Rectangle.Y
        End If

        'Free the block of memory allocated
        Marshal.FreeCoTaskMem(lparam)

        'Release the device context handle obtained by a previous call
        G.ReleaseHdc(hdc)

        'Return last + 1 character printer
        Return res.ToInt32()
    End Function

    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function LoadLibrary(path As String) As IntPtr
    End Function

    Private Shared moduleHandle As IntPtr

    Private iSelStart As Integer
    Private iSelLength As Integer

    Public Sub StoreSelection()
        iSelLength = Me.SelectionStart
        iSelLength = Me.SelectionLength
    End Sub

    Public Sub RestoreSelection()
        Try
            Me.Select(iSelStart, iSelLength)
        Catch ex As Exception

        End Try
    End Sub

    Public Enum eTrimSide
        BeginOfText
        EndOfText
        Both
    End Enum

    Public Sub RemoveCrLf(Side As eTrimSide)
        Dim bReadonly As Boolean = Me.ReadOnly
        Me.ReadOnly = False
        StoreSelection()

        If Side = eTrimSide.BeginOfText Or Side = eTrimSide.Both Then RemoveCrLfAtBegin()
        If Side = eTrimSide.EndOfText Or Side = eTrimSide.Both Then RemoveCrLfAtEnd()

        Me.ReadOnly = bReadonly
        RestoreSelection()
    End Sub

    Private Sub RemoveCrLfAtBegin()
        On Error Resume Next
        Dim iTeller As Integer

Again:
        iTeller += 1
        Me.Select(0, 1)

        Select Case Asc(Me.SelectedText)
            Case 10, 13
                Me.SelectedText = ""
                If iTeller < 1000 Then GoTo Again
        End Select
    End Sub

    Private Sub RemoveCrLfAtEnd()
        On Error Resume Next
        Dim iTeller As Integer

        Me.Select(Integer.MaxValue, 1)
        Dim iStart As Integer

Again:
        iTeller += 1
        iStart = Me.SelectionStart
        Me.Select(iStart - 1, 1)

        Select Case Asc(Me.SelectedText)
            Case 10, 13
                Me.SelectedText = ""
                If iTeller < 1000 Then GoTo Again
        End Select

    End Sub

    Private Function RichEdit20Params() As CreateParams
        If moduleHandle = IntPtr.Zero Then
            moduleHandle = LoadLibrary("RICHED20.DLL")
            If CLng(moduleHandle) < &H20 Then
                Return MyBase.CreateParams
                'Throw New Win32Exception(Marshal.GetLastWin32Error(), "Could not load Msftedit.dll")
            End If
        End If

        Dim createParams__1 As CreateParams = MyBase.CreateParams
        createParams__1.ClassName = "RichEdit20W"

        If Me.Multiline Then
            If ((Me.ScrollBars And RichTextBoxScrollBars.Horizontal) <> RichTextBoxScrollBars.None) AndAlso Not MyBase.WordWrap Then
                createParams__1.Style = createParams__1.Style Or &H100000
                If (Me.ScrollBars And CType(&H10, RichTextBoxScrollBars)) <> RichTextBoxScrollBars.None Then
                    createParams__1.Style = createParams__1.Style Or &H2000
                End If
            End If
            If (Me.ScrollBars And RichTextBoxScrollBars.Vertical) <> RichTextBoxScrollBars.None Then
                createParams__1.Style = createParams__1.Style Or &H200000
                If (Me.ScrollBars And CType(&H10, RichTextBoxScrollBars)) <> RichTextBoxScrollBars.None Then
                    createParams__1.Style = createParams__1.Style Or &H2000
                End If
            End If
        End If

        If (BorderStyle.FixedSingle = MyBase.BorderStyle) AndAlso ((createParams__1.Style And &H800000) <> 0) Then
            createParams__1.Style = createParams__1.Style And -8388609
            createParams__1.ExStyle = createParams__1.ExStyle Or &H200
        End If

        Return createParams__1
    End Function

    Private Function RichEdit50Params() As CreateParams
        If moduleHandle = IntPtr.Zero Then
            moduleHandle = LoadLibrary("msftedit.dll")
            If CLng(moduleHandle) < &H20 Then
                Return MyBase.CreateParams
                'Throw New Win32Exception(Marshal.GetLastWin32Error(), "Could not load Msftedit.dll")
            End If
        End If

        Dim createParams__1 As CreateParams = MyBase.CreateParams
        createParams__1.ClassName = "RichEdit50W"

        If Me.Multiline Then
            If ((Me.ScrollBars And RichTextBoxScrollBars.Horizontal) <> RichTextBoxScrollBars.None) AndAlso Not MyBase.WordWrap Then
                createParams__1.Style = createParams__1.Style Or &H100000
                If (Me.ScrollBars And CType(&H10, RichTextBoxScrollBars)) <> RichTextBoxScrollBars.None Then
                    createParams__1.Style = createParams__1.Style Or &H2000
                End If
            End If
            If (Me.ScrollBars And RichTextBoxScrollBars.Vertical) <> RichTextBoxScrollBars.None Then
                createParams__1.Style = createParams__1.Style Or &H200000
                If (Me.ScrollBars And CType(&H10, RichTextBoxScrollBars)) <> RichTextBoxScrollBars.None Then
                    createParams__1.Style = createParams__1.Style Or &H2000
                End If
            End If
        End If

        If (BorderStyle.FixedSingle = MyBase.BorderStyle) AndAlso ((createParams__1.Style And &H800000) <> 0) Then
            createParams__1.Style = createParams__1.Style And -8388609
            createParams__1.ExStyle = createParams__1.ExStyle Or &H200
        End If

        Return createParams__1
    End Function

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get

            Return RichEdit20Params() 'Is de enige die goed gaat.

            Dim Result As CreateParams = Nothing

            If LicenseManager.UsageMode = LicenseUsageMode.Designtime Then Return MyBase.CreateParams 'DesignTime

            'Return RichEdit50Params() 'OZR klaagde over tabellen die niet meer goed werkten. 
            'Weer gedisabled. Geeft fouten.
            Return RichEdit20Params()
        End Get
    End Property

    Public Property AutoCorrect() As Boolean
        Get
            Return bAutoCorrectieActief
        End Get
        Set(ByVal value As Boolean)
            bAutoCorrectieActief = value
        End Set
    End Property

    Public Function RtfWithoutCrlfsAtEnd() As String
        If MyBase.Rtf = Nothing Then Return ""

        Dim RTF As String = MyBase.Rtf.TrimEnd

        RTF = RTF.Substring(0, RTF.Length - 1).TrimEnd

        Do While RTF.ToUpper.EndsWith("\PAR")
            RTF = RTF.Substring(0, RTF.Length - 4).TrimEnd
        Loop

        RTF &= "}"

        Return RTF
    End Function

    Private ptCursor As Point = Point.Empty

    Private Sub StoreCursor()
        'De richtextbox gedraagt zich af en toe vreemd als de cursor zich op een beeldscherm beving, waarvan de x van rechterkant < 0 is.
        'Daarom verplaatsen we de cursor maar.

        If Cursor.Position.X < 0 Then
            ptCursor = Cursor.Position
            Cursor.Position = New Point(5, Cursor.Position.Y)
        End If
    End Sub

    Private Sub RestoreCursor()
        If ptCursor.IsEmpty Then Return

        Cursor.Position = ptCursor

        ptCursor = Point.Empty
    End Sub
    Public Function FindAndSelect(ByVal Text As String, Optional IgnoreCase As Boolean = True, Optional ByVal StartPosition As Integer = 0) As Boolean
        Dim iPos As Integer



        iPos = Me.Find(Text, StartPosition, If(IgnoreCase, RichTextBoxFinds.None, RichTextBoxFinds.MatchCase))

        If iPos < 0 Then Return False

        Try
            Me.Select(iPos, Text.Length)
        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

    Public ReadOnly Property LineCount() As Integer
        Get
            Const EM_GETLINECOUNT = &HBA
            Return FrameWorkNS.Functions.API.SendMessage(Me.Handle.ToInt32, EM_GETLINECOUNT, 0&, 0&)
        End Get
    End Property

    Public Property AutoAanvullen() As Boolean
        Get
            Return bAutoAanvullen
        End Get
        Set(ByVal value As Boolean)
            bAutoAanvullen = value
        End Set
    End Property

    <Runtime.InteropServices.DllImport("user32.dll")>
    Public Shared Function LockWindowUpdate(ByVal handle As IntPtr) As Boolean
    End Function

    Private Sub VulWoordenBoek()
        If MyFrameWork Is Nothing Then Return
        If MyFrameWork.DataManager Is Nothing Then Return
        If Not MyWoordenBoek Is Nothing Then Return
        Dim DM As MCS_Interfaces.iDataManager
        If Not DM.IsConnected Then Return
        If Not DM.TableExists("mcs_woordenboek") Then Return

        MyWoordenBoek = New Dictionary(Of String, String)

        Dim SQL As String
        Dim DR As IDataReader

        SQL = "Select woord, betekenis from mcs_woordenboek"

        DM = EasyCare.SQL.NewDataManager
        DR = DM.SQLdrResult(SQL)

        'MyFrameWork.SetStatus("Laden woordenboek")
        Do While DR.Read
            Dim MyKey As String = DR!Woord.ToString.Trim.ToUpper

            Do While MyKey.EndsWith(",")
                MyKey = MyKey.Remove(MyKey.Length - 1)
            Loop

            If Not MyWoordenBoek.ContainsKey(MyKey) Then
                MyWoordenBoek.Add(MyKey, DR!betekenis.ToString)
            End If
        Loop

        DM.Destroy(DR)

    End Sub

    Private Function GiveLastWord(Optional ByVal Position As Integer = 0) As LastWord
        Dim MyLastWord As New LastWord

        If Position = 0 Then Position = Me.SelectionStart - 1
        If Position >= Me.Text.Length Then Position = Me.Text.Length - 1
        If Position < 0 Then Return Nothing

        Select Case Me.Text.Substring(Position, 1)
            Case " ", ",", "."
                MyLastWord.FollowedBySpace = True
                Position -= 1
        End Select

        Dim iStartPosition As Integer = Position

        If iStartPosition < 0 Then Return Nothing

        Do While True
            Select Case Me.Text.Substring(iStartPosition, 1)
                Case " "
                    MyLastWord.Start = iStartPosition
                    MyLastWord.Text = Me.Text.Substring(iStartPosition + 1, Position - iStartPosition)
                    Exit Do
                Case Chr(13), Chr(10)
                    MyLastWord.Start = iStartPosition
                    MyLastWord.Text = Me.Text.Substring(iStartPosition + 1, Position - iStartPosition)
                    Exit Do
            End Select

            If iStartPosition = 0 Then
                MyLastWord.IsFirstWordOfText = True
                Dim PosSpace As Integer = Me.Text.IndexOf(" ")
                If PosSpace > -1 Then
                    MyLastWord.Text = Me.Text.Substring(0, PosSpace)
                Else
                    MyLastWord.Text = Me.Text
                End If
                MyLastWord.Start = iStartPosition
                Exit Do
            End If

            iStartPosition -= 1
        Loop

        If Not MyLastWord.IsFirstWordOfText Then
            MyLastWord.Start += 1
        End If

        Return MyLastWord
    End Function

    Private Sub DoeAutoCorrectie(Optional ByVal iSelectionStart As Integer = 0)
        If Not bAutoCorrectieActief Then Return 'Hoeft niet voor deze richtextbox!

        If iSelectionStart = 0 Then iSelectionStart = Me.SelectionStart

        Dim MyWord As LastWord = Me.GiveLastWord(iSelectionStart)
        If MyWord Is Nothing Then Return

        Dim sCorrectie As String = ""
        Dim iVerschil As Integer
        Dim Item As FrameWorkNS.TextControl.AutoCorrectieItem

        Item = FrameWorkNS.TextControl.GetAutocorrectieItem(MyWord.ToString)
        If Item Is Nothing Then Return

        Dim CurrentPosition As Integer = Me.SelectionStart
        sCorrectie = Item.Correction
        iVerschil = Len(MyWord.Text.Trim) - sCorrectie.Trim.Length

        With Me
            Dim bBold As Boolean = SelBold
            Dim bItalic As Boolean = SelItalic
            Dim bUnderlined As Boolean = SelUnderlined
            Dim iFontSize As Integer = SelFontSize

            .SuspendRedraw 'FrameworkNS.Functions.API.SetRedraw(.Handle, False, False)
            .SelectionStart = MyWord.Start
            .SelectionLength = MyWord.Text.Length

            .SelectedText = sCorrectie
            .SelectionStart = MyWord.Start
            .SelectionLength = sCorrectie.Length

            .SelBold = Item.Bold OrElse bBold
            .SelItalic = Item.Italic OrElse bItalic
            .SelUnderlined = Item.Underlined OrElse bUnderlined

            If Item.FontSize > 0 Then
                .SelFontSize = Item.FontSize
            End If

            .SelectionStart = CurrentPosition - iVerschil
            .SelectionLength = 0

            .SelBold = bBold
            .SelItalic = bItalic
            .SelUnderlined = bUnderlined
            .SelFontSize = iFontSize


            DoeHoofdletterCorrectie()

            .SelectionStart = CurrentPosition - iVerschil
            .SelectionLength = 0

            .SelBold = bBold
            .SelItalic = bItalic
            .SelUnderlined = bUnderlined
            .SelFontSize = iFontSize

            .ResumeRedraw 'FrameworkNS.Functions.API.SetRedraw(.Handle, True, False)
        End With

    End Sub

    Private Sub DoeAutoAanvullen()
        If Not Me.AutoAanvullen Then Return
        If Me.ReadOnly Then Return

        Try
            Dim LastWord As LastWord = Me.GiveLastWord

            If LastWord Is Nothing OrElse (LastWord.Text.Length < 4) Then
                RichTextBox.HideListBox()
                Return
            End If

            Dim sTest As String = ""
            Dim MyParentForm As Form = Me.FindForm

            If RichTextBox.MyListBox Is Nothing Then Return
            If RichTextBox.MyListBox.IsDisposed Then Return 'Zou niet moeten kunnen...

            With RichTextBox.MyListBox
                .Tag = Me

                If Not .Parent Is MyParentForm Then
                    .Visible = False
                    .Parent = MyParentForm
                End If

                .Items.Clear()

                For Each Item As String In FrameWorkNS.TextControl.GetAutoAanvulItems(LastWord.ToString).Values
                    'Laatste controle...
                    If Item.Length <= LastWord.Text.Length + 1 Then Continue For

                    If LastWord.Text.Substring(0, 1) = LastWord.Text.Substring(0, 1).ToUpper Then
                        Item = Item.Substring(0, 1).ToUpper & Item.Substring(1)
                    End If
                    .Items.Add(Item)
                Next
            End With

            If MyListBox.Items.Count > 0 Then
                If Not Me.SelectionFont Is Nothing Then MyListBox.Font = Me.SelectionFont.Clone

                If Not MyListBox.Visible Then MyListBox.Visible = True
                Dim MyLocation As Point = GeefCursorPosition()
                MyListBox.BringToFront()
                MyLocation.X -= (FrameWorkNS.Functions.GDI.TextSize(LastWord.ToString, MyListBox.Font).Width)
                MyLocation.X += 3
                MyListBox.Location = MyLocation

                If MyListBox.Items.Count < 15 Then
                    MyListBox.Height = MyListBox.GetItemHeight(0) * (MyListBox.Items.Count + 1)
                Else
                    MyListBox.Size = New Size(160, 200)
                End If
            Else
                RichTextBox.HideListBox()
            End If

        Catch ex As Exception

        End Try
    End Sub

    'Public Event OnScrollSelectionIntoView(ByRef Handled As Boolean)

    'Public Sub ScrollSelectionIntoView()
    '    Dim bHandled As Boolean = False
    '    RaiseEvent OnScrollSelectionIntoView(bHandled)
    '    If bHandled Then Return
    '    'TODO: Hier kan de richtextbox eventueel zelf proberen om de geselecteerde text in beeld te scrollen...
    '    'Nu niet nodig, dus later...
    'End Sub

    Private Function GeefCursorPosition() As Point
        Dim MyForm As Form = Me.FindForm
        Dim MyPoint As Point
        Dim FormPointToScreen As Point = MyForm.PointToScreen(New Point(0, 0))
        Dim RichtextboxPointToScreen As Point = Me.PointToScreen(New Point(0, 0))

        MyPoint = New Point(RichtextboxPointToScreen.X - FormPointToScreen.X, RichtextboxPointToScreen.Y - FormPointToScreen.Y)
        MyPoint.Y += 15

        Dim CursorPos As Point = Me.GetPositionFromCharIndex(Me.SelectionStart)

        MyPoint.X += CursorPos.X
        MyPoint.Y += CursorPos.Y

        Return MyPoint

    End Function

    Private Sub VerwerkSnelToetsen(ByVal e As System.Windows.Forms.KeyEventArgs)
        Dim bSomethingChanged As Boolean

        If Me.ReadOnly Then Return

        If e.Control And Not e.Alt Then
            Select Case e.KeyCode
                Case Keys.B
                    Me.SelBold = Not Me.SelBold
                    bSomethingChanged = True

                Case Keys.I
                    Me.SelItalic = Not Me.SelItalic
                    bSomethingChanged = True

                Case Keys.U
                    Me.SelUnderlined = Not Me.SelUnderlined
                    bSomethingChanged = True
            End Select
        End If

        If bSomethingChanged Then
            'raise event
            e.Handled = True
            e.SuppressKeyPress = True

            SetStyleToolstrip(True)
            Return
        End If
    End Sub

    Private Sub RichTextBox_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        If Me.ReadOnly Then Return

        iPosOnKeyDown = Me.SelectionStart
        If Me.Mask = eDataMask.Time Then Return

        CreateUndoInfo(sender, e)

        VerwerkSnelToetsen(e)

        If e.Handled Then Return

        Select Case e.KeyCode
            Case Keys.Enter, Keys.Space
                Me.DoeHoofdletterCorrectie()
                HideListBox()
        End Select

        Select Case e.KeyCode
            Case Keys.Down
                If Not MyListBox Is Nothing Then
                    If Not MyListBox.IsDisposed Then
                        If MyListBox.Visible Then
                            MyListBox.Select()
                            If MyListBox.Items.Count > 0 Then
                                MyListBox.SelectedItem = MyListBox.Items.Item(0)
                            End If
                            e.Handled = True
                        End If
                    End If
                End If
        End Select

        If Not e.Handled Then RaiseEvent KeyDown(sender, e)
    End Sub

    Private iContentHeight As Integer

    Private Sub RichTextBox_ContentsResized(sender As Object, e As System.Windows.Forms.ContentsResizedEventArgs) Handles Me.ContentsResized
        iContentHeight = e.NewRectangle.Height
    End Sub

    Public Function IsInputValidForMask() As Boolean
        Select Case Me.Mask
            Case eDataMask.Time
                Try
                    Dim iHour As Integer = Split(Me.Text, ":")(0)
                    Dim iMinutes As Integer = Split(Me.Text, ":")(1)

                    If iHour < 0 Then Return False
                    If iHour > 24 Then Return False
                    If iMinutes < 0 Then Return False
                    If iMinutes > 59 Then Return False

                Catch ex As Exception
                    Return False
                End Try

        End Select

        Return True

    End Function

    Private Sub RichTextBox_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyUp
        If Me.ReadOnly Then Return

        If Me.Mask = eDataMask.Time Then
            CheckKeyUpMask_Time(e)
            Return
        End If

        If e.Control And Not e.Alt And Not e.Shift Then
            Select Case e.KeyCode
                Case Keys.D 'Datum
                    Me.SelBold = False
                    Me.SelectedText = Now.ToDutchDateTime & " "
                    e.Handled = True

                Case Keys.G 'Gebruiker
                    Me.SelBold = False
                    Me.SelectedText = EasyCare.CurrentUser.FullName
                    e.Handled = True

                Case Keys.L
                    Me.SelBold = True
                    Me.SelectedText = Now.ToDutchDateTime & " - " & EasyCare.CurrentUser.FullName & ":" & vbCrLf
                    Me.SelBold = False
                    e.Handled = True
            End Select

            If e.Handled Then Return
        End If

        Select Case e.KeyCode
            Case Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.Back
                DoeAutoAanvullen()

            Case Keys.Escape
                HideListBox()

            Case Keys.F1
                'ShowOrHideMedihulp()
                'e.Handled = True
        End Select

    End Sub

    Public Function ContentHeightWithoutAPI() As Integer
        Return iContentHeight
    End Function

    Public Function ContentHeight() As Integer
        Dim iResult As Integer = ContentHeight(Me.ClientRectangle.Width)

        If iContentHeight > iResult Then iResult = iContentHeight

        Return iResult + 5
    End Function

    Public Function ContentHeight(Width As Integer) As Integer
        Dim MyRect As RECT

        MyRect.Left = 0
        MyRect.Top = 0
        MyRect.Bottom = 655360
        MyRect.Right = Width * anInch

        If Me.Text = "" Then Return 17

        Dim hdc As IntPtr = FrameWorkNS.Functions.API.GetDC(Me.Handle)
        Dim fmtRange As FORMATRANGE

        fmtRange.chrg.cpMax = Me.Text.Length
        fmtRange.chrg.cpMin = 0
        fmtRange.hdc = hdc
        fmtRange.hdcTarget = hdc
        fmtRange.rc = MyRect
        fmtRange.rcPage = MyRect

        Dim res As IntPtr = IntPtr.Zero
        Dim Result As Integer

        Dim wparam As IntPtr = IntPtr.Zero 'Alleen measuren. Niet echt printen.

        Dim lparam As IntPtr = IntPtr.Zero
        lparam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange))
        Marshal.StructureToPtr(fmtRange, lparam, False)

        res = SendMessage(Handle, Messages.EM_FORMATRANGE, wparam, lparam)
        Dim rc As FORMATRANGE = Marshal.PtrToStructure(lparam, GetType(FORMATRANGE))

        Result = rc.rc.Bottom / anInch

        Marshal.FreeCoTaskMem(lparam)

        Return Result * Me.ZoomFactor

    End Function

    Private Function HoofdletterCorrectieUitzonderingen() As Dictionary(Of String, String)
        Dim MyDic As New Dictionary(Of String, String)

        MyDic.Add("bijv.", "")
        MyDic.Add("bv.", "")
        MyDic.Add("afd.", "")
        MyDic.Add("afb.", "")
        MyDic.Add("afk.", "")

        Return MyDic
    End Function

    Private Function isHoofdletterCorrectieUitzondering(Woord As String) As Boolean
        Woord = Woord.ToLower.Trim
        If Not Woord.EndsWith(".") Then Woord &= "."
        Return HoofdletterCorrectieUitzonderingen.ContainsKey(Woord)
    End Function

    Private Sub DoeHoofdletterCorrectie()
        Dim Lastword As LastWord = Me.GiveLastWord
        Dim iStart As Integer = iPosOnKeyDown  ' Me.SelectionStart
        Dim bCorrectie As Boolean = False

        If Lastword Is Nothing Then Return
        If Lastword.Length < 1 Then Return
        If Lastword.Text.Substring(0, 1) = Lastword.Text.Substring(0, 1).ToUpper Then Return

        Try
            SetRedraw(Me.Handle, False)

            If Lastword.IsFirstWordOfText Then
                bCorrectie = True
            Else
                For X As Integer = Lastword.Start - 1 To 0 Step -1
                    Dim MyChar As String = Me.Text.Substring(X, 1)

                    Select Case MyChar
                        Case " "
                            'Doe niets bij een spatie..

                        Case vbCr, vbLf, "!", "?"
                            bCorrectie = True
                            Exit For

                        Case "."
                            If X > 2 Then
                                If Me.Text.Substring(X - 2, 1) = "." Then Return 'Een afkorting!
                            End If

                            Dim EenNaLaatsteWoord As LastWord = GiveLastWord(X)
                            If isHoofdletterCorrectieUitzondering(EenNaLaatsteWoord.Text) Then Return

                            bCorrectie = True
                            Exit For

                        Case Else
                            Return

                    End Select
                Next
            End If

            If bCorrectie Then
                Me.Select(Lastword.Start, 1)
                Me.SelectedText = Lastword.FirstLetter.ToUpper
            End If

        Catch ex As Exception
            If Debugger.IsAttached Then Stop
        Finally
            Me.Select(iStart, 0)
            SetRedraw(Me.Handle, True)
        End Try
    End Sub

    Public Event MouseWheelProc(M As System.Windows.Forms.Message, ByRef CancelDefProc As Boolean)

    Public Sub RaiseMouseWheelEvent(M As System.Windows.Forms.Message, ByRef CancelDefProc As Boolean)
        RaiseEvent MouseWheelProc(M, CancelDefProc)
    End Sub

    Private bSpellCheckingEnabled As Boolean = True

    Public Property SpellCheckingEnabled As Boolean
        Get
            If Me.ReadOnly Then Return False
            If MyFrameWork.SpellingChecker Is Nothing Then Return False
            If Me.IsDisposed Then Return False
            If Not Me.Visible Then Return False
            If Me.Parent Is Nothing Then Return False
            If Me.ReadOnly Then Return False

            Return bSpellCheckingEnabled
        End Get
        Set(value As Boolean)
            bSpellCheckingEnabled = value
        End Set
    End Property

    Protected Overrides Function ProcessDialogKey(keyData As System.Windows.Forms.Keys) As Boolean
        If Me.IsDisposed Then Return True

        If keyData = Keys.Tab And Not Me.AcceptsTab Then
            RaiseEvent KeyDown(Me, New KeyEventArgs(keyData))
            Return True 'Voorkom verdere afhandeling...
        End If

        Return MyBase.ProcessDialogKey(keyData)
    End Function

    Private Const EM_GETSCROLLPOS = (WM_USER + 221)
    Private Const EM_SETSCROLLPOS = (WM_USER + 222)

    Private Declare Auto Function RtfScroll Lib "user32.dll" Alias "SendMessage" (ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByRef lParam As System.Drawing.Point) As Integer

    Public Sub ScrollToTop()
        Dim pt As New Point

        pt.X = GetScrollPos.X
        pt.Y = 0

        RtfScroll(Me.Handle, EM_SETSCROLLPOS, 0, pt)
    End Sub

    Private Function GetScrollPos() As Point
        Dim pt As New System.Drawing.Point()
        RtfScroll(Me.Handle, EM_GETSCROLLPOS, 0, pt)
        Return pt
    End Function

    Public Property SendMouseWheelToFirstScrollableControl As Boolean = False

    <DebuggerNonUserCode()>
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Dim bCancel As Boolean = False

        Select Case m.Msg
            Case FrameWorkNS.Functions.API.Messages.WM_MOUSEWHEEL
                If SendMouseWheelToFirstScrollableControl Then
                    Dim MyParent As ScrollableControl = Me.FirstParent(Of ScrollableControl)
                    SendMessage(MyParent.Handle, m.Msg, m.WParam, m.LParam)
                    Return
                End If

                RaiseMouseWheelEvent(m, bCancel)
                If bCancel Then Return

            Case FrameWorkNS.Functions.API.Messages.WM_PAINT
                If Me.SpellCheckingEnabled Then
                    MyBase.WndProc(m)
                    DrawSpellCheckingWaves()
                    Return
                End If

            Case FrameWorkNS.Functions.API.Messages.WM_PASTE
                If CheckCancelPaste() Then Return
                MyBase.WndProc(m)
                RaiseEvent OnAfterPaste(Me)
                Return

        End Select

        MyBase.WndProc(m)
    End Sub

    Private UnderlinedSections As New Dictionary(Of Integer, Integer) 'Start + Length

    Public Sub ClearWaves()
        UnderlinedSections.Clear()
    End Sub

    Public Sub AddWave(iStart As Integer, iLength As Integer)
        UnderlinedSections.Add(iStart, iLength)
    End Sub

    Private Sub DrawSpellCheckingWaves()

        If UnderlinedSections Is Nothing OrElse UnderlinedSections.Count = 0 Then Return

        Dim tmpBitmap As Bitmap = Nothing
        Dim textBoxGraphics As Graphics = Nothing
        Dim bufferGraphics As Graphics = Nothing

        If Me.Width = 0 OrElse Me.Height = 0 Then Return

        Try
            'Create a bitmap with the same dimensions as the textbox
            tmpBitmap = New Bitmap(Me.Width, Me.Height)

            'Create the graphics object from this bitmap...this is where we will draw the lines to start with
            bufferGraphics = Graphics.FromImage(tmpBitmap)
            bufferGraphics.Clip = New Region(Me.ClientRectangle)

            'Get the graphics object for the textbox.  We use this to draw the bufferGraphics
            textBoxGraphics = Graphics.FromHwnd(Me.Handle)

            ' clear the graphics buffer
            ' bufferGraphics.Clear(Color.Transparent)

            For Each wordStart As Integer In UnderlinedSections.Keys
                'If ignoredSections IsNot Nothing AndAlso ignoredSections.ContainsKey(wordStart) Then
                '    Continue For
                'End If

                Dim wordEndIndex As Integer = wordStart + UnderlinedSections(wordStart) - 1
                Dim start As Point = Me.GetPositionFromCharIndex(wordStart)
                Dim [end] As Point = Me.GetPositionFromCharIndex(wordEndIndex)

                Dim curIndex As Integer = wordStart
                Dim safetyDrewOnce As Integer = -1
                If curIndex < Text.Length Then
                    Do
                        start = Me.GetPositionFromCharIndex(curIndex)
                        'Determine the first line of waves to draw
                        While curIndex <= wordEndIndex
                            If curIndex < Text.Length AndAlso Me.GetPositionFromCharIndex(curIndex).Y = start.Y Then
                                curIndex += 1
                            Else
                                curIndex -= 1
                                Exit While
                            End If
                        End While

                        [end] = Me.GetPositionFromCharIndex(curIndex)

                        ' The position above now points to the top left corner of the character.
                        ' We need to account for the character height so the underlines go
                        ' to the right place.
                        [end].X += 1
                        Dim yOffset As Integer = TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(Me, wordStart)
                        start.Y += yOffset
                        [end].Y += yOffset

                        'Add a new wavy line using the starting and ending point
                        DrawWave(bufferGraphics, start, [end], Color.Red)

                        If safetyDrewOnce <> curIndex Then
                            safetyDrewOnce = curIndex
                        Else
                            Exit Do
                        End If

                        curIndex += 1
                        'TODO: something with indeces
                        'Replace words in text with empty words.
                    Loop While curIndex <= wordEndIndex
                End If
            Next

            ''Nu de WIKI woorden.
            'For Each wordStart As Integer In WikiWords.Keys        
            '    Dim wordEndIndex As Integer = wordStart + WikiWords(wordStart) - 1
            '    Dim start As Point = Me.GetPositionFromCharIndex(wordStart)
            '    Dim [end] As Point = Me.GetPositionFromCharIndex(wordEndIndex)
            '    Dim curIndex As Integer = wordStart
            '    Dim safetyDrewOnce As Integer = -1
            '    If curIndex < Text.Length Then
            '        Do
            '            start = Me.GetPositionFromCharIndex(curIndex)
            '            'Determine the first line of waves to draw
            '            While curIndex <= wordEndIndex
            '                If curIndex < Text.Length AndAlso Me.GetPositionFromCharIndex(curIndex).Y = start.Y Then
            '                    curIndex += 1
            '                Else
            '                    curIndex -= 1
            '                    Exit While
            '                End If
            '            End While
            '            [end] = Me.GetPositionFromCharIndex(curIndex)
            '            ' The position above now points to the top left corner of the character.
            '            ' We need to account for the character height so the underlines go
            '            ' to the right place.
            '            [end].X += 1
            '            Dim yOffset As Integer = TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(Me, wordStart)
            '            start.Y += yOffset
            '            [end].Y += yOffset
            '            'Add a new wavy line using the starting and ending point
            '            DrawWave(bufferGraphics, start, [end], Color.Blue)
            '            If safetyDrewOnce <> curIndex Then
            '                safetyDrewOnce = curIndex
            '            Else
            '                Exit Do
            '            End If
            '            curIndex += 1
            '            'TODO: something with indeces
            '            'Replace words in text with empty words.
            '        Loop While curIndex <= wordEndIndex
            '    End If
            'Next

            ' Now we just draw our internal buffer on top of the TextBox.
            ' Everything should be at the right place.
            textBoxGraphics.DrawImageUnscaled(tmpBitmap, 0, 0)
        Catch ex As Exception
            If Debugger.IsAttached Then Stop
        Finally
            If Not tmpBitmap Is Nothing Then tmpBitmap.Dispose()
            If Not textBoxGraphics Is Nothing Then textBoxGraphics.Dispose()
            If Not bufferGraphics Is Nothing Then bufferGraphics.Dispose()

            tmpBitmap = Nothing
            textBoxGraphics = Nothing
            bufferGraphics = Nothing
        End Try
    End Sub

    Private Sub DrawWave(graphics As Graphics, StartOfLine As Point, EndOfLine As Point, Color As Color)
        'correction to draw line closer to text
        StartOfLine.Y -= 1
        EndOfLine.Y -= 1

        Dim newPen As Pen = New Pen(Color)

        If (EndOfLine.X - StartOfLine.X) > 4 Then
            Dim pl As New ArrayList()
            For i As Integer = StartOfLine.X To (EndOfLine.X - 2) Step 4
                pl.Add(New Point(i, StartOfLine.Y))
                pl.Add(New Point(i + 2, StartOfLine.Y + 2))
            Next

            Dim p As Point() = DirectCast(pl.ToArray(GetType(Point)), Point())
            graphics.DrawLines(newPen, p)
        Else
            graphics.DrawLine(newPen, StartOfLine, EndOfLine)
        End If
        newPen.Dispose()
    End Sub

    Private Sub RichTextBox_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        If Me.ReadOnly Then Return

        If Me.Mask = eDataMask.Time Then
            CheckTijdMask()
            Return
        End If

        If Me.AlleenNumeriek Then
            If Not FrameWorkNS.Functions.StringFunctions.IsNumericChar(e.KeyChar, True, True) Then
                e.Handled = True
                Return
            End If
        End If

        Select Case e.KeyChar.ToString
            Case " ", ",", "."
                HideListBox()
                DoeAutoCorrectie(Me.SelectionStart - 1)

        End Select

    End Sub

    Private Sub CheckKeyUpMask_Time(ByRef e As System.Windows.Forms.KeyEventArgs)
        If Me.Mask <> eDataMask.Time Then Return

        Dim iPos As Integer = Me.SelectionStart
        Dim sChar As String = ChrW(e.KeyCode) 'e.KeyChar.ToString

        If e.KeyCode = Keys.Delete Then
            CheckTijdMask()
        End If

        If e.KeyCode = Keys.Back Then
            Try
                Dim iNewSelStart As Integer = iPosOnKeyDown - 1
                If iNewSelStart = 3 Then iNewSelStart = 2
                Me.SelectionStart = iNewSelStart
            Catch ex As Exception
                Me.SelectionStart = 0
            End Try

            Return
        End If

        If Not isNumeric(sChar) Then e.Handled = True : Return

        If iPos > 4 Then Return

        Dim sNewString As String = Me.Text

        Try
            Mid(sNewString, iPos + 1, 1) = sChar
        Catch ex As Exception
            Me.Text = "__:__"

            Try
                Mid(sNewString, iPos + 1, 1) = sChar
            Catch ex2 As Exception
                Me.Text = "__:__"
            End Try
        End Try

        Me.Text = sNewString

        Dim iNewStart As Integer = iPos + 1

        If iNewStart = 2 Then iNewStart = 3
        Me.SelectionStart = iNewStart

    End Sub

    Shared Sub HideListBox()
        If MyListBox Is Nothing Then Return
        If MyListBox.IsDisposed Then Return
        If Not MyListBox.Visible Then Return

        MyListBox.Visible = False
        MyListBox.Items.Clear()
    End Sub

    Private Sub SetDefaultFont()
        Dim MyFont As Font

        Try
            If EasyCare.FrameWork.IsFrameWorkReady Then
                MyFont = MyFrameWork.PropertyManager.GetFont("DEFAULT_FONT").Clone
            Else
                MyFont = New Font("Microsoft Sans Serif", 10)
            End If

        Catch ex As Exception
            MyFont = New Font("Microsoft Sans Serif", 10)
        End Try

        Try
            Me.Font = MyFont
        Catch ex As Exception
            Me.Font = New Font("Microsoft Sans Serif", 10)
        End Try

    End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        '   if Debugger.IsAttached Then Return

        Me.DetectUrls = False
        SetDefaultFont()


        If MyListBox Is Nothing Then MyListBox = New ListBox

        Try
            With MyListBox
                .Size = New Size(160, 200)
                .BackColor = Color.FromArgb(255, 255, 230)
            End With
        Catch ex As Exception

        End Try

        tmrMouseOnWord.Interval = 800


        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        SetStyle(ControlStyles.AllPaintingInWmPaint, True)

        If Me.ZoomFactor <> 1 Then
            If Debugger.IsAttached Then Stop
        End If

        Me.ZoomFactor = 1

        Me.RaisePasteEvent = True
    End Sub

    Private Class LastWord
        Public Text As String = ""
        Public Start As Integer
        Public FollowedBySpace As Boolean
        Public IsFirstWordOfText As Boolean

        Public Function FirstLetter() As String
            If Text = "" Then Return ""
            Return Text.Substring(0, 1)
        End Function

        Public Function Length() As Integer
            Return Text.Length
        End Function

        Public Overrides Function ToString() As String
            Return Me.Text
        End Function
    End Class

    Private Shared Sub MyListBox_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyListBox.DoubleClick
        VerwerkWoord(MyListBox.Tag, MyListBox.SelectedItem)
    End Sub

    Private Shared Sub MyListBox_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyListBox.KeyDown
        Dim MyRichtextbox As MCSCTRLS1.RichTextBox = MyListBox.Tag

        Select Case e.KeyCode
            Case Keys.Enter, Keys.Space
                If MyListBox.SelectedItem Is Nothing Then Return
                VerwerkWoord(MyRichtextbox, MyListBox.SelectedItem)

            Case Keys.Up
                If MyListBox.SelectedItem Is MyListBox.Items(0) Then
                    MyRichtextbox.Select()
                End If
        End Select
    End Sub

    Private Shared Sub VerwerkWoord(ByVal Rtb As MCSCTRLS1.RichTextBox, ByVal Woord As String)
        Dim MyLastWord As LastWord = Rtb.GiveLastWord

        With Rtb
            LockWindowUpdate(.Handle.ToInt32)
            .SelectionStart = MyLastWord.Start
            .SelectionLength = MyLastWord.Text.Length
            .SelectedText = Woord & " "
            .Select()
            LockWindowUpdate(System.IntPtr.Zero)
        End With

        HideListBox()
    End Sub

    Private Shared Sub MyListBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyListBox.LostFocus
        HideListBox()
    End Sub

    Private Sub RichTextBox_LocationChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LocationChanged
        HideListBox()
    End Sub

    Private Sub RichTextBox_SizeChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.SizeChanged
        HideListBox()
    End Sub

    Private Sub RichTextBox_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return

        MCSCTRLS2.EasyTooltip.SetTooltip(Me.TooltipTitle, Me.TooltipText)
    End Sub

    Private Sub RichTextBox_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return

        MCSCTRLS2.EasyTooltip.ShowTooltip(False)
    End Sub

    Private Sub RichTextBox_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.MouseLeave
        MCSCTRLS2.EasyTooltip.HideTooltip()
        sLastWord = ""
        Me.tmrMouseOnWord.Stop()
    End Sub

    Public Property TooltipText As String = ""
    Public Property TooltipTitle As String = ""

    Public Function WordAtPostion(ByVal Position As Point) As String
        Dim pos As Integer
        Dim Txt As String
        Dim Start_pos As Integer
        Dim CH As String
        Dim TxtLen As Integer
        Dim End_pos As Integer
        Dim GetWordUnderMousePointer As String = ""

        pos = Me.GetCharIndexFromPosition(Position)

        If pos <= 0 Then Return ""

        Txt = Me.Text

        For Start_pos = pos To 1 Step -1
            CH = Mid$(Me.Text, Start_pos, 1)
            If CH = Chr(32) Or CH = Chr(10) Or CH = Chr(11) Or CH = Chr(13) Then Exit For
        Next Start_pos

        Start_pos = Start_pos + 1

        TxtLen = Len(Txt)

        For End_pos = pos To TxtLen
            CH = Mid$(Txt, End_pos, 1)
            If CH = Chr(32) Or CH = Chr(10) Or CH = Chr(11) Or CH = Chr(13) Or CH = "." Or CH = "," Then Exit For
        Next End_pos

        End_pos = End_pos - 1

        If Start_pos <= End_pos Then GetWordUnderMousePointer = Mid$(Txt, Start_pos, End_pos - Start_pos + 1)

        Dim lstTrim As New List(Of Char)

        lstTrim.Add(".")
        lstTrim.Add("!")
        lstTrim.Add("?")
        lstTrim.Add("/")
        lstTrim.Add("\")
        lstTrim.Add("{")
        lstTrim.Add("}")
        lstTrim.Add("[")
        lstTrim.Add("]")
        lstTrim.Add("^")
        lstTrim.Add(" ")
        lstTrim.Add(":")
        lstTrim.Add(",")
        lstTrim.Add("(")
        lstTrim.Add(")")

        GetWordUnderMousePointer = GetWordUnderMousePointer.Trim(lstTrim.ToArray)
        Return GetWordUnderMousePointer
    End Function

    Public Shared Sub ShowAutoCorrectieWindow(Woord As String, ParentForm As IWin32Window)
        Dim MyForm As New frmAutoCorrectie(Woord)
        FrameWorkNS.Functions.Forms.ShowPopup(MyForm, ParentForm)
        MyForm.Dispose()
        MyForm = Nothing
    End Sub

    Private Sub tmrMouseOnWord_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrMouseOnWord.Tick
        tmrMouseOnWord.Stop()

        Dim GetWordUnderMousePointer As String = WordAtPostion(MousePos)

        GetWordUnderMousePointer = GetWordUnderMousePointer.Trim.ToUpper

        Dim Betekenis As String = ""

        If MyWoordenBoek.ContainsKey(GetWordUnderMousePointer) Then
            Betekenis = MyWoordenBoek.Item(GetWordUnderMousePointer)

            Do While Betekenis.StartsWith(" ") Or Betekenis.StartsWith(",")
                Betekenis = Betekenis.Trim(" ")
                Betekenis = Betekenis.Trim(",")
            Loop

            MyFrameWork.Talk(StrConv(GetWordUnderMousePointer, VbStrConv.ProperCase), Betekenis, 0)

        End If
    End Sub

    ' Private Const SCF_SELECTION As Integer = &H1
    Private Const CFM_UNDERLINETYPE As Integer = 8388608
    'Private Const EM_SETCHARFORMAT As Integer = 1092
    '  Private Const EM_GETCHARFORMAT As Integer = 1082

    <StructLayout(LayoutKind.Sequential)>
    Private Structure CHARFORMAT
        Public cbSize As Integer
        Public dwMask As UInteger
        Public dwEffects As UInteger
        Public yHeight As Integer
        Public yOffset As Integer
        Public crTextColor As Integer
        Public bCharSet As Byte
        Public bPitchAndFamily As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)>
        Public szFaceName As Char()

        ' CHARFORMAT2 from here onwards. 
        Public wWeight As Short
        Public sSpacing As Short
        Public crBackColor As Integer
        Public LCID As Integer
        Public dwReserved As UInteger
        Public sStyle As Short
        Public wKerning As Short
        Public bUnderlineType As Byte
        Public bAnimation As Byte
        Public bRevAuthor As Byte
    End Structure

    <DllImport("user32", CharSet:=CharSet.Auto)>
    Private Shared Function SendMessage(ByVal hWnd As HandleRef, ByVal msg As Integer, ByVal wParam As Integer, ByRef lp As CHARFORMAT) As Integer
    End Function


    Private Property SelectionUnderlineStyle() As UnderlineStyle
        Get
            Dim fmt As CHARFORMAT = New CHARFORMAT()
            fmt.cbSize = Marshal.SizeOf(fmt)

            ' Get the underline style. 
            SendMessage(New HandleRef(Me, Handle), EM_GETCHARFORMAT, SCF_SELECTION, fmt)

            ' Default to no underline. 
            If (fmt.dwMask And CFM_UNDERLINETYPE) = 0 Then
                Return UnderlineStyle.None
            End If

            Dim style As Byte = CByte(fmt.bUnderlineType And &HF)

            Return CType(style, UnderlineStyle)
        End Get

        Set(ByVal value As UnderlineStyle)
            ' Ensure we don't alter the color by accident. 
            Dim color As UnderlineColor = SelectionUnderlineColor

            ' Ensure we don't show it if it shouldn't be shown. 
            If value = UnderlineStyle.None Then
                color = UnderlineColor.Black
            End If

            Dim fmt As CHARFORMAT = New CHARFORMAT()
            fmt.cbSize = Marshal.SizeOf(fmt)
            fmt.dwMask = CFM_UNDERLINETYPE

            fmt.bUnderlineType = CByte(CByte(value) Or CByte(color))

            ' Set the underline type. 
            SendMessage(New HandleRef(Me, Handle), EM_SETCHARFORMAT, SCF_SELECTION, fmt)
        End Set
    End Property

    Private Enum UnderlineStyle
        None = 0
        Normal = 1
        Word = 2
        [Double] = 3
        Dotted = 4
        Dash = 5
        DashDot = 6
        DashDotDot = 7
        Wave = 8
        Thick = 9
        HairLine = 10
        DoubleWave = 11
        HeavyWave = 12
        LongDash = 13
    End Enum

    Private Property SelectionUnderlineColor() As UnderlineColor
        Get
            Dim fmt As CHARFORMAT = New CHARFORMAT()
            fmt.cbSize = Marshal.SizeOf(fmt)

            ' Get the underline color. 
            SendMessage(New HandleRef(Me, Handle), EM_GETCHARFORMAT, SCF_SELECTION, fmt)

            ' Default to black. 
            If (fmt.dwMask And CFM_UNDERLINETYPE) = 0 Then
                Return UnderlineColor.Black
            End If

            Dim style As Byte = CByte(fmt.bUnderlineType And &HF0)

            Return CType(style, UnderlineColor)
        End Get

        Set(ByVal value As UnderlineColor)
            ' Ensure we don't alter the style. 
            Dim style As UnderlineStyle = SelectionUnderlineStyle

            ' Ensure we don't show it if it shouldn't be shown. 
            If style = UnderlineStyle.None Then
                value = UnderlineColor.Black
            End If

            Dim fmt As CHARFORMAT = New CHARFORMAT()
            fmt.cbSize = Marshal.SizeOf(fmt)
            fmt.dwMask = CFM_UNDERLINETYPE

            fmt.bUnderlineType = CByte(CByte(style) Or CByte(value))


            ' Set the underline color. 
            SendMessage(New HandleRef(Me, Handle), EM_SETCHARFORMAT, SCF_SELECTION, fmt)
        End Set
    End Property

    Public Enum UnderlineColor
        Black = &H0
        Blue = &H10
        Cyan = &H20
        LimeGreen = &H30
        Magenta = &H40
        Red = &H50
        Yellow = &H60
        White = &H70
        DarkBlue = &H80
        DarkCyan = &H90
        Green = &HA0
        DarkMagenta = &HB0
        Brown = &HC0
        OliveGreen = &HD0
        DarkGray = &HE0
        Gray = &HF0
    End Enum

    Public Function TrimmedRTF(Optional Side As eTrimSide = eTrimSide.Both) As String
        Select Case Side
            Case eTrimSide.BeginOfText, eTrimSide.Both
                Me.RemoveCrLfAtBegin()

        End Select

        Return RtfWithoutCrlfsAtEnd()
    End Function

    Public Overloads Property RTF() As String
        Get
            If AutoCorrectCrLF Then
                Return RtfWithoutCrlfsAtEnd()
            Else
                If MyBase.Rtf Is Nothing Then Return ""
                Return MyBase.Rtf
            End If
        End Get
        Set(ByVal value As String)
            If Me.IsDisposed Then
                If Debugger.IsAttached Then Stop
                Return
            End If

            'FrameWorkNS.Functions.API.SetRedraw(Me.Handle, False)
            Me.SuspendRedraw
            Dim sZoomFactor As Single = Me.ZoomFactor

            If FrameWorkNS.Functions.Richtext.IsRTF(value) Then
                MyBase.Rtf = value
            Else
                MyBase.Text = value
            End If

            If Me.ZoomFactor <> sZoomFactor Then
                Me.ZoomFactor = sZoomFactor
            End If

            Me.ResumeRedraw
        End Set
    End Property

    Public Sub ReplaceWhiteToBlack()
        If MyBase.Rtf.Contains("\red255\green255\blue255") Then
            Dim sRTF As String = MyBase.Rtf
            sRTF = sRTF.Replace("\red255\green255\blue255", "\red0\green0\blue0")
            Me.RTF = sRTF
        End If
    End Sub
    Public Overrides Property Text As String
        Get
            Dim sResult As String

            sResult = MyBase.Text

            If sResult = "" Then
                'Er zit een vreemde bug in de Richtextbox. De Text Property klopt niet altijd. In ieder geval als alle text is geselecteerd.
                'Via Win32 API krijgen we wel de juiste text terug.

                sResult = FrameWorkNS.Functions.API.GetWindowText(Me.Handle)  'Me.Rtf.RtfToText

                If Not sResult = "" Then MyBase.Text = sResult
            End If

            Return sResult
        End Get
        Set(value As String)
            If value = "RichtextBox" Then Return
            If FrameWorkNS.Functions.Richtext.IsRTF(value) Then
                Me.RTF = value
            Else
                MyBase.Text = value
            End If
        End Set
    End Property

    Public Function TextWithCrlfCorrected() As String
        Dim sResult As String = MyBase.Text

        sResult = sResult.Replace(Chr(13), "")
        sResult = sResult.Replace(Chr(10), vbCrLf)

        Return sResult
    End Function

    Public Property MedihulpKey As String = ""
    Public Property MedihulpNiveau As MCS_Interfaces.eMedihulpNiveau = MCS_Interfaces.eMedihulpNiveau.System_Defined
    Public Property ShowDefaultMenu As Boolean = False

    Public Event BeforeDefaultMenuPopup(ByRef Menu As MCSCTRLS2.ContextMenu, ByRef Cancel As Boolean)
    Public Event BeforeMedihulp(Value As Medihulp)
    Public Event AfterMedihulp(Value As Medihulp)

    Private Sub ShowDefaultContextMenu()
        Dim bCancel As Boolean = False
        Dim sResult As String = ""
        Dim MyMenu As New MCSCTRLS2.ContextMenu
        Dim miMedihulpen As EasyMenuItem

        miMedihulpen = MyMenu.AddItem("Medihulpen", "Medihulpen", Nothing)
        MyMenu.AddDefaultItem(MCSCTRLS2.ContextMenu.eDefaultContextMenuItem.Seperator)
        MyMenu.AddItem("SELECT_ALL", "Alles selecteren", Nothing)
        MyMenu.AddDefaultItem(MCSCTRLS2.ContextMenu.eDefaultContextMenuItem.Seperator)
        MyMenu.AddItem("COPY", "Kopiëren", EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Copy))
        MyMenu.AddItem("CUT", "Knippen", EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Cut))
        MyMenu.AddItem("PASTE", "Plakken", EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Paste))

        RaiseEvent BeforeDefaultMenuPopup(MyMenu, bCancel)

        If bCancel Then Return

        If Me.MedihulpKey = "" Or Me.ReadOnly Then
            miMedihulpen.Visible = False
        Else
            miMedihulpen.Visible = True

            MCSCTRLS2.MedihulpManager.VulContextMenu(Me.MedihulpKey, DirectCast(miMedihulpen.Parent, MCSCTRLS2.ContextMenu), MCS_Interfaces.eMedihulpNiveau.System_Defined)
            'MCSCTRLS2.MedihulpManager.VulContextMenu(Me.MedihulpKey, miMedihulpen, MCS_Interfaces.eMedihulpNiveau.System_Defined)
        End If

        sResult = MyMenu.Show(Me)

        Select Case sResult
            Case ""

            Case "COPY"
                Me.Copy()

            Case "CUT"
                Me.Cut()

            Case "PASTE"
                Me.Paste()

            Case "SELECT_ALL"
                Me.SelectAll()

            Case Else
                'If Debugger.IsAttached Then Stop
        End Select

        If sResult.StartsWith("Medihulp_") Then
            Dim MyMedihulp As Medihulp = MyMenu.ClickedItem.Tag

            RaiseEvent BeforeMedihulp(MyMedihulp)
            MCSCTRLS2.MedihulpManager.DoeMedihulp(MyMedihulp, Me, Me.GetApplication, Me.FindForm)
            RaiseEvent AfterMedihulp(MyMedihulp)

        ElseIf sResult.StartsWith("EDITMEDIHULP_") Then
            Dim sKey As String = sResult.Substring(13)

            MCSCTRLS2.MedihulpManager.EditMedihulp(sKey, Me.FindForm)
        End If

        MyMenu.Dispose()

    End Sub

    Private Sub RichTextBox_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp

        If Me.Mask = eDataMask.Time AndAlso Me.Text = "__:__" Then
            Me.Select(0, 0)
            Return
        End If

        If Not e.Button = Windows.Forms.MouseButtons.Right Then Return

        If ShowDefaultMenu Then ShowDefaultContextMenu()

    End Sub

    Private Sub DoeMedihulp(Item As ToolStripItem)
        If Not TypeOf Item.Tag Is MCSCTRLS2.Medihulp Then Return

        'Dim MyControl As Control
        Dim MyApplication As MCS_Interfaces.iApplication = GetApplication()

        'MyControl = Me

        'Do While Not MyControl Is Nothing
        '    If TypeOf MyControl Is MCS_Interfaces.IEpdPartForm Then
        '        MyApplication = DirectCast(MyControl, MCS_Interfaces.IEpdPartForm).Application
        '        Exit Do
        '    End If

        '    MyControl = MyControl.Parent
        'Loop

        MCSCTRLS2.MedihulpManager.DoeMedihulp(Item.Tag, Me, MyApplication, Me.FindForm)
    End Sub

    Private Function GetApplication() As iApplication
        Dim MyControl As Control = Me
        Dim MyApplication As iApplication = Nothing

        Do While Not MyControl Is Nothing
            If TypeOf MyControl Is MCS_Interfaces.iEpdPartForm Then
                MyApplication = DirectCast(MyControl, MCS_Interfaces.iEpdPartForm).Application
                Exit Do
            End If

            MyControl = MyControl.Parent
        Loop

        Return MyApplication
    End Function

    Public Sub FixFont()
        Try
            FontAfdwingen()
        Catch ex As Exception

        End Try
    End Sub

    Public Sub FontAfdwingen()
        If Me.DesignMode Then Return
        If Not Me.AutoFixFont Then Return
        If MyFrameWork Is Nothing Then Return
        If MyFrameWork.PropertyManager Is Nothing Then Return
        If Not MyFrameWork.PropertyManager.GetBoolean("AUTO_FIX_FONT") Then Return

        Dim MyFont As Font

        MyFont = MyFrameWork.PropertyManager.GetFont("DEFAULT_FONT").Clone

        If MyFont Is Nothing Then MyFont = New Font("Arial", 10)

        Me.SuspendRedraw
        SetEntireFontName(MyFont.FontFamily.Name)
        SetEntireFontSize(MyFont.Size)
        Me.ResumeRedraw

        MyFont.Destroy

    End Sub

    Public Function SetEntireFontName(ByVal face As String) As Boolean
        Dim cf As New STRUCT_CHARFORMAT
        cf.cbSize = Marshal.SizeOf(cf)
        cf.dwMask = Convert.ToUInt32(CFM_FACE)
        ' ReDim face name to relevant size
        ReDim cf.szFaceName(32)
        face.CopyTo(0, cf.szFaceName, 0, Math.Min(31, face.Length))

        Dim lParam As IntPtr
        lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
        Marshal.StructureToPtr(cf, lParam, False)

        Dim res As Integer
        res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_ALL, lParam)
        Return (res = 0)

    End Function


    Public Function SetEntireFontSize(ByVal size As Integer) As Boolean
        Dim cf As New STRUCT_CHARFORMAT
        cf.cbSize = Marshal.SizeOf(cf)
        cf.dwMask = Convert.ToUInt32(CFM_SIZE)
        ' yHeight is in 1/20 pt
        cf.yHeight = size * 20

        Dim lParam As IntPtr
        lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
        Marshal.StructureToPtr(cf, lParam, False)

        Dim res As Integer
        res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_ALL, lParam)
        Return (res = 0)
    End Function

#Region " Structs "
    <StructLayout(LayoutKind.Sequential)>
    Private Structure STRUCT_RECT
        Public left As Int32
        Public top As Int32
        Public right As Int32
        Public bottom As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure STRUCT_CHARRANGE
        Public cpMin As Int32
        Public cpMax As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure STRUCT_FORMATRANGE
        Public hdc As IntPtr
        Public hdcTarget As IntPtr
        Public rc As STRUCT_RECT
        Public rcPage As STRUCT_RECT
        Public chrg As STRUCT_CHARRANGE
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure STRUCT_CHARFORMAT
        Public cbSize As Integer
        Public dwMask As UInt32
        Public dwEffects As UInt32
        Public yHeight As Int32
        Public yOffset As Int32
        Public crTextColor As Int32
        Public bCharSet As Byte
        Public bPitchAndFamily As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)>
        Public szFaceName() As Char
    End Structure
#End Region
#Region " Win32 Api "
    <DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Int32, ByVal wParam As Int32, ByVal lParam As IntPtr) As Int32
    End Function
#End Region
#Region " Constants "
    Private Const WM_USER As Int32 = &H400&
    Private Const EM_FORMATRANGE As Int32 = WM_USER + 57
    Private Const EM_GETCHARFORMAT As Int32 = WM_USER + 58
    Private Const EM_SETCHARFORMAT As Int32 = WM_USER + 68

    Private SCF_SELECTION As Int32 = &H1&
    Private SCF_WORD As Int32 = &H2&
    Private SCF_ALL As Int32 = &H4&

    Private Const CFM_BOLD As Long = &H1&
    Private Const CFM_ITALIC As Long = &H2&
    Private Const CFM_UNDERLINE As Long = &H4&
    Private Const CFM_STRIKEOUT As Long = &H8&
    Private Const CFM_PROTECTED As Long = &H10&
    Private Const CFM_LINK As Long = &H20&
    Private Const CFM_SIZE As Long = &H80000000&
    Private Const CFM_COLOR As Long = &H40000000&
    Private Const CFM_FACE As Long = &H20000000&
    Private Const CFM_OFFSET As Long = &H10000000&
    Private Const CFM_CHARSET As Long = &H8000000&

    Private Const CFE_BOLD As Long = &H1&
    Private Const CFE_ITALIC As Long = &H2&
    Private Const CFE_UNDERLINE As Long = &H4&
    Private Const CFE_STRIKEOUT As Long = &H8&
    Private Const CFE_PROTECTED As Long = &H10&
    Private Const CFE_LINK As Long = &H20&
    Private Const CFE_AUTOCOLOR As Long = &H40000000&
#End Region
#Region " Microsoft functions and subs "
    Public Function FormatRange(ByVal measureOnly As Boolean, ByVal e As PrintPageEventArgs, ByVal charFrom As Integer, ByVal charTo As Integer) As Integer
        ' Specify which characters to print
        Dim cr As STRUCT_CHARRANGE
        cr.cpMin = charFrom
        cr.cpMax = charTo

        ' Specify the area inside page margins
        Dim rc As STRUCT_RECT
        rc.top = HundredthInchToTwips(e.MarginBounds.Top)
        rc.bottom = HundredthInchToTwips(e.MarginBounds.Bottom)
        rc.left = HundredthInchToTwips(e.MarginBounds.Left)
        rc.right = HundredthInchToTwips(e.MarginBounds.Right)

        ' Specify the page area
        Dim rcPage As STRUCT_RECT
        rcPage.top = HundredthInchToTwips(e.PageBounds.Top)
        rcPage.bottom = HundredthInchToTwips(e.PageBounds.Bottom)
        rcPage.left = HundredthInchToTwips(e.PageBounds.Left)
        rcPage.right = HundredthInchToTwips(e.PageBounds.Right)

        ' Get device context of output device
        Dim hdc As IntPtr
        hdc = e.Graphics.GetHdc()

        ' Fill in the FORMATRANGE structure
        Dim fr As STRUCT_FORMATRANGE
        fr.chrg = cr
        fr.hdc = hdc
        fr.hdcTarget = hdc
        fr.rc = rc
        fr.rcPage = rcPage

        ' Non-Zero wParam means render, Zero means measure
        Dim wParam As Int32
        If measureOnly Then
            wParam = 0
        Else
            wParam = 1
        End If

        ' Allocate memory for the FORMATRANGE struct and
        ' copy the contents of our struct to this memory
        Dim lParam As IntPtr
        lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr))
        Marshal.StructureToPtr(fr, lParam, False)

        ' Send the actual Win32 message
        Dim res As Integer
        res = SendMessage(Handle, EM_FORMATRANGE, wParam, lParam)

        ' Free allocated memory
        Marshal.FreeCoTaskMem(lParam)

        ' and release the device context
        e.Graphics.ReleaseHdc(hdc)

        Return res
    End Function

    Private Function HundredthInchToTwips(ByVal n As Integer) As Int32
        Return Convert.ToInt32(n * 14.4)
    End Function

    Public Sub FormatRangeDone()
        Dim lParam As New IntPtr(0)
        SendMessage(Handle, EM_FORMATRANGE, 0, lParam)
    End Sub

    Public Function SetSelectionFont(ByVal face As String) As Boolean
        Dim cf As New STRUCT_CHARFORMAT
        cf.cbSize = Marshal.SizeOf(cf)
        cf.dwMask = Convert.ToUInt32(CFM_FACE)

        ' ReDim face name to relevant size
        ReDim cf.szFaceName(32)
        face.CopyTo(0, cf.szFaceName, 0, Math.Min(31, face.Length))

        Dim lParam As IntPtr
        lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
        Marshal.StructureToPtr(cf, lParam, False)

        Dim res As Integer
        res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_SELECTION, lParam)
        Return (res = 0)

    End Function

    Public Function SetSelectionSize(ByVal size As Integer) As Boolean
        Dim cf As New STRUCT_CHARFORMAT
        cf.cbSize = Marshal.SizeOf(cf)
        cf.dwMask = Convert.ToUInt32(CFM_SIZE)
        ' yHeight is in 1/20 pt
        cf.yHeight = size * 20

        Dim lParam As IntPtr
        lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
        Marshal.StructureToPtr(cf, lParam, False)

        Dim res As Integer
        res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_SELECTION, lParam)
        Return (res = 0)

    End Function

    Private Function HeeftSelectionStyle(Style As Integer) As Boolean
        Dim fmt As CHARFORMAT = New CHARFORMAT()
        Dim bResult As Boolean

        fmt.cbSize = Marshal.SizeOf(fmt)
        SendMessage(New HandleRef(Me, Handle), EM_GETCHARFORMAT, SCF_SELECTION, fmt)
        bResult = CBool(fmt.dwMask And Style) And CBool(fmt.dwEffects And Style)
        Return bResult
    End Function

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Property SelBold As Boolean
        Get
            Return HeeftSelectionStyle(CFE_BOLD)
        End Get
        Set(value As Boolean)
            SetSelectionStyle(CFM_BOLD, IIf(value, CFE_BOLD, 0))
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Property SelStrikeOut As Boolean
        Get
            Return HeeftSelectionStyle(CFE_STRIKEOUT)
        End Get
        Set(value As Boolean)
            Dim bResult As Boolean

            bResult = SetSelectionStyle(CFM_STRIKEOUT, IIf(value, CFE_STRIKEOUT, 0))


        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Property SelFontName As String
        Get
            If Me.SelectionFont Is Nothing Then Return 10
            Return Me.SelectionFont.SizeInPoints
        End Get
        Set(value As String)
            Dim cf As New STRUCT_CHARFORMAT
            cf.cbSize = Marshal.SizeOf(cf)
            cf.dwMask = Convert.ToUInt32(CFM_FACE)
            ' ReDim face name to relevant size
            ReDim cf.szFaceName(32)
            value.CopyTo(0, cf.szFaceName, 0, Math.Min(31, value.Length))

            Dim lParam As IntPtr
            lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
            Marshal.StructureToPtr(cf, lParam, False)

            Dim res As Integer
            res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_SELECTION, lParam)
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Property SelFontSize As Integer
        Get
            If Me.SelectionFont Is Nothing Then Return 10
            Return Me.SelectionFont.SizeInPoints
        End Get
        Set(value As Integer)
            Dim cf As New STRUCT_CHARFORMAT
            cf.cbSize = Marshal.SizeOf(cf)
            cf.dwMask = Convert.ToUInt32(CFM_SIZE)
            ' yHeight is in 1/20 pt
            cf.yHeight = value * 20

            Dim lParam As IntPtr
            lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
            Marshal.StructureToPtr(cf, lParam, False)

            Dim res As Integer
            res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_SELECTION, lParam)
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Property SelItalic As Boolean
        Get
            Return Me.HeeftSelectionStyle(CFM_ITALIC)
        End Get
        Set(value As Boolean)
            SetSelectionStyle(CFM_ITALIC, IIf(value, CFE_ITALIC, 0))
        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(False)>
    Public Property SelUnderlined As Boolean
        Get
            Return HeeftSelectionStyle(CFE_UNDERLINE)
        End Get
        Set(value As Boolean)
            SetSelectionStyle(CFM_UNDERLINE, IIf(value, CFE_UNDERLINE, 0))
        End Set
    End Property

    Private Function SetSelectionStyle(ByVal mask As Int32, ByVal effect As Int32) As Boolean
        Dim cf As New STRUCT_CHARFORMAT

        cf.cbSize = Marshal.SizeOf(cf)
        cf.dwMask = Convert.ToUInt32(mask)
        cf.dwEffects = Convert.ToUInt32(effect)

        Dim lParam As IntPtr
        lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf))
        Marshal.StructureToPtr(cf, lParam, False)

        Dim res As Integer
        res = SendMessage(Handle, EM_SETCHARFORMAT, SCF_SELECTION, lParam)
        Return (res = 0)
    End Function
#End Region

    Private MyUndoItems As New List(Of UndoItem)
    Private iCurrentUndoIndex As Integer = -1
    Private bRaiseEvents As Boolean = True

    Private bLastCanUndo As Boolean = False

    Public Event CanRedoChanged(ByVal CanRedo As Boolean)
    Public Event CanUndoChanged(ByVal CanUndo As Boolean)

    Public Property CurrentUndoIndex As Integer
        Get
            If iCurrentUndoIndex < 0 Then iCurrentUndoIndex = 0
            If iCurrentUndoIndex > MyUndoItems.Count Then iCurrentUndoIndex = MyUndoItems.Count
            Return iCurrentUndoIndex
        End Get
        Set(ByVal value As Integer)
            iCurrentUndoIndex = value
        End Set
    End Property

    Public Event UndoStateChanged()

    Public ReadOnly Property UndoActions As List(Of UndoItem)
        Get
            Dim MyList As New List(Of UndoItem)
            MyList.AddRange(MyUndoItems.ToArray)

            If Not MyRedoItem Is Nothing Then
                MyList.Add(MyRedoItem)
            End If

            Return MyList
        End Get
    End Property

    Dim MyRedoItem As New UndoItem

    Private Sub SaveRedoItem()
        MyRedoItem = New UndoItem
        With MyRedoItem
            .Action = UndoItem.eUndoAction.Laatste_Versie
            .RTF = Me.RTF
            .SelectionStart = Me.SelectionStart
            .SelectionLength = Me.SelectionLength
        End With
    End Sub

    Private Sub VulUndoItem(ByRef UndoItem As UndoItem)
        With UndoItem
            .RTF = Me.RTF
            .SelectionStart = Me.SelectionStart
            .SelectionLength = Me.SelectionLength
        End With
    End Sub

    Public Overloads Sub Undo()
        If CurrentUndoIndex = Me.MyUndoItems.Count Then
            'Bewaar de huidige statis voor een REDO.
            VulUndoItem(MyRedoItem)
        End If

        CurrentUndoIndex -= 1
        ShowRtfFromIndex(CurrentUndoIndex)
    End Sub

    Public Overloads Sub Redo()
        CurrentUndoIndex += 1

        If CurrentUndoIndex >= MyUndoItems.Count Then
            CurrentUndoIndex = MyUndoItems.Count
            ShowUndoItem(MyRedoItem)
            Return
        End If

        ShowRtfFromIndex(CurrentUndoIndex)
    End Sub

    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)>
    Public Property RaisePasteEvent As Boolean = False

    Private Function CheckCancelPaste() As Boolean
        'Deze functie geeft TRUE terug als het plakken geannuleerd is.
        If Not RaisePasteEvent Then Return False

        Dim MyRichtTextBox As New RichTextBox

        MyRichtTextBox.RaisePasteEvent = False
        MyRichtTextBox.Paste()

        Dim Cancel As Boolean
        Cancel = False
        RaiseEvent OnBeforePaste(MyRichtTextBox, Cancel)

        MyRichtTextBox.Dispose()
        MyRichtTextBox = Nothing

        Return Cancel
    End Function

    Public Overloads Sub Paste()
        Me.SaveUndoState(UndoItem.eUndoAction.Plakken)

        Try
            MyBase.Paste()
        Catch ex As Exception

        End Try

    End Sub

    Public Overloads Sub Copy()
        Try
            MyBase.Copy()
        Catch ex As Exception

        End Try
    End Sub

    Public Overloads Sub Cut()
        Me.SaveUndoState(UndoItem.eUndoAction.Knippen)

        Try
            MyBase.Cut()
        Catch ex As Exception

        End Try

    End Sub

    Public Overloads ReadOnly Property CanUndo As Boolean
        Get
            Return CurrentUndoIndex > 0
        End Get
    End Property

    Public Overloads ReadOnly Property CanRedo As Boolean
        Get
            Return False
        End Get
    End Property

    Private sLastUndoState As String = "" 'Laatste RTF

    Private Sub GroupUndoItems(ByVal FromAction As UndoItem.eUndoAction, ByVal ToAction As UndoItem.eUndoAction)
        Dim iIndex As Integer = MyUndoItems.Count - 1

        If iIndex < 0 Then Return

        Do While MyUndoItems(iIndex).Action = FromAction
            iIndex -= 1
            If iIndex < 0 Then Exit Do
        Loop

        iIndex += 1

        If iIndex >= MyUndoItems.Count Then Return 'Er valt niets te groeperen...

        MyUndoItems(iIndex).Action = ToAction

        RemoveLastActionsFromUndoBuffer(FromAction)

        RaiseUndoStateChanged()
    End Sub

    Private Sub RemoveLastActionsFromUndoBuffer(ByVal Action As UndoItem.eUndoAction)
        Do While MyUndoItems.Count > 0 AndAlso MyUndoItems(MyUndoItems.Count - 1).Action = Action
            MyUndoItems.RemoveAt(MyUndoItems.Count - 1)
        Loop
    End Sub

    Private Sub CreateUndoInfo(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs)
        If Me.ReadOnly Then Return

        Dim c As Char = Chr(e.KeyCode)

        If e.Control Then Return 'Vangen we af in KeyPress...

        Select Case e.KeyCode
            Case Keys.Shift, Keys.LShiftKey, Keys.RShiftKey, Keys.ShiftKey
                Return

            Case Keys.Control, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey
                Return

            Case Keys.Alt, Keys.LMenu, Keys.Menu, Keys.RMenu
                Return

            Case Keys.Back, Keys.Delete 'Backspace
                GroupUndoItems(UndoItem.eUndoAction.Letter_getypt, UndoItem.eUndoAction.Woord_getypt)
                GroupUndoItems(UndoItem.eUndoAction.Woord_getypt, UndoItem.eUndoAction.Tekst_getypt)

                Me.SaveUndoState(UndoItem.eUndoAction.Tekst_verwijderd)

            Case Keys.Enter
                GroupUndoItems(UndoItem.eUndoAction.Letter_getypt, UndoItem.eUndoAction.Woord_getypt)
                GroupUndoItems(UndoItem.eUndoAction.Woord_getypt, UndoItem.eUndoAction.Tekst_getypt)

            Case Keys.Space, Keys.OemPeriod, Keys.Oemcomma
                GroupUndoItems(UndoItem.eUndoAction.Letter_getypt, UndoItem.eUndoAction.Woord_getypt)

            Case Else
                Me.SaveUndoState(UndoItem.eUndoAction.Letter_getypt)
        End Select

    End Sub

    Private bUndoSuspended As Boolean = False

    Public Sub SuspendUndoRecording()
        bUndoSuspended = True
    End Sub

    Public Overloads Sub ClearUndo()
        MyUndoItems.Clear()
        MyBase.ClearUndo()
        RaiseUndoStateChanged()
    End Sub

    Public Sub ResumeUndoRecording()
        bUndoSuspended = False
    End Sub

    Private Sub ShowUndoItem(ByVal UndoItem As UndoItem)
        FrameWorkNS.Functions.API.SetRedraw(Me, False)

        Dim sngZoom As Single = Me.ZoomFactor

        With UndoItem
            Me.RTF = .RTF
            Me.Select(.SelectionStart, .SelectionLength)
        End With

        Me.ZoomFactor = 1
        Me.ZoomFactor = sngZoom

        FrameWorkNS.Functions.API.SetRedraw(Me, True)
    End Sub

    Private Function ShowRtfFromIndex(ByVal UndoIndex As Integer) As Boolean
        If UndoIndex < 0 Then UndoIndex = 0
        If MyUndoItems.Count < (UndoIndex + 1) Then Return False

        ShowUndoItem(MyUndoItems(UndoIndex))

        Return True
    End Function

    Private Sub SetLastAction(ByVal Action As UndoItem.eUndoAction)
        If LastUndoItem() Is Nothing Then Return
        LastUndoItem.Action = Action
        RaiseUndoStateChanged()
    End Sub

    Private Function LastUndoItem() As UndoItem
        If MyUndoItems.Count = 0 Then Return Nothing
        Return MyUndoItems(MyUndoItems.Count - 1)
    End Function

    Public Sub SaveUndoState(ByVal ActionName As String)
        Dim bOrg As Boolean = bRaiseEvents
        bRaiseEvents = False
        SaveUndoState(UndoItem.eUndoAction.Onbekend)
        MyUndoItems(MyUndoItems.Count - 1).ActionName = ActionName
        bRaiseEvents = bOrg
        RaiseUndoStateChanged()
    End Sub

    Private Sub RaiseUndoStateChanged()
        If Not bRaiseEvents Then Return
        RaiseEvent UndoStateChanged()
    End Sub

    Private Sub ClearUndoItemsNaHuidigItem()
        Do While CurrentUndoIndex <= MyUndoItems.Count - 1
            MyUndoItems.RemoveAt(MyUndoItems.Count - 1)
        Loop
    End Sub

    Public Function SaveUndoState(ByVal Action As UndoItem.eUndoAction) As UndoItem
        If bUndoSuspended Then Return Nothing
        If Me.IsDisposed Then Return Nothing
        ClearUndoItemsNaHuidigItem()

        If Me.RTF.Equals(sLastUndoState) Then
            SetLastAction(Action)
            Return LastUndoItem()
        End If

        Me.sLastUndoState = Me.RTF

        Dim MyUndoItem As New UndoItem

        With MyUndoItem
            .Action = Action
            .RTF = Me.RTF
            .SelectionStart = Me.SelectionStart
            .SelectionLength = Me.SelectionLength
        End With

        Me.MyUndoItems.Add(MyUndoItem)

        CurrentUndoIndex = MyUndoItems.Count

        SaveRedoItem()
        RaiseUndoStateChanged()

        Return LastUndoItem()
    End Function


    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        Select Case (keyData)

            Case (Keys.Control Or Keys.Z)
                Me.Undo()
                Return True

            Case (Keys.Control Or Keys.V)
                Me.Paste()
                Return True

            Case (Keys.Control Or Keys.X) 'Knippen
                Me.Cut()
                Return True
        End Select

        Return MyBase.ProcessCmdKey(msg, keyData)

    End Function

    Public Function IsMisspelledWordAtPosition(CharIndexPosition As Integer) As Boolean
        For Each Item As Integer In UnderlinedSections.Keys
            If Item <= CharIndexPosition And (Item + UnderlinedSections(Item)) >= CharIndexPosition Then Return True
        Next

        Return False
    End Function

    Public Sub ReplaceMisspelledWord(CharIndexPosition As Integer, NewText As String)
        For Each Item As Integer In UnderlinedSections.Keys
            If Item <= CharIndexPosition And (Item + UnderlinedSections(Item)) >= CharIndexPosition Then
                Me.Select(Item, UnderlinedSections(Item))
                Me.SelectedText = NewText
                Return
            End If
        Next

    End Sub

    'Private Sub DoSpellCheckingWithForm()
    '    DoSpellChecking(False) 'Zet de lijntjes goed.

    '    Dim bOrgHideSelection As Boolean = Me.HideSelection

    '    Me.HideSelection = False

    '    MyFrameWork.SpellingChecker.TxSpellingChecker.Check(Me.Text)
    '    If MyFrameWork.SpellingChecker.TxSpellingChecker.IncorrectWords.Count = 0 Then Return

    '    For Each Item As TXTextControl.Proofing.IncorrectWord In MyFrameWork.SpellingChecker.TxSpellingChecker.IncorrectWords
    '        Me.Select(Item.Start, Item.Length)
    '        MsgBox("Fout")
    '    Next

    '    Me.HideSelection = bOrgHideSelection
    'End Sub

    Public Sub DoSpellChecking(Optional ShowForm As Boolean = False, Optional ShowCompletedMessage As Boolean = True)
        If Not Me.SpellCheckingEnabled Then Return
        If Me.FindForm Is Nothing Then Return

        If ShowForm Then
            Me.DontLooseFocus = True
            MyFrameWork.SpellingChecker.CheckRTF(Me, ShowCompletedMessage)
            Me.DontLooseFocus = False
            Return
        End If

        Try
            If MyFrameWork.SpellingChecker.Available Then
                MyFrameWork.SpellingChecker.TxSpellingChecker.Check(Me.Text)

                Me.ClearWaves()

                If MyFrameWork.SpellingChecker.TxSpellingChecker.IncorrectWords.Count > 0 Then
                    For Each Item As TXTextControl.Proofing.IncorrectWord In MyFrameWork.SpellingChecker.TxSpellingChecker.IncorrectWords
                        If Item.Start <= Me.SelectionStart And (Item.Start + Item.Length) >= Me.SelectionStart Then Continue For 'Het woord wat nu getypt wordt.
                        Me.AddWave(Item.Start, Item.Length)
                    Next
                End If

                Me.Invalidate()
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub RichTextBox_TextChanged(sender As Object, e As System.EventArgs) Handles Me.TextChanged
        If Me.RedrawSuspended Then Return

        Me.SuspendRedraw
        Me.FontAfdwingen()
        DoSpellChecking()
        Me.ResumeRedraw
    End Sub

    Private bUnlockOnLostFocus As Boolean = False

    Private Sub SetStyleToolstrip(Enabled As Boolean)
        Dim MyForm As Form = Me.FindForm

        If MyForm Is Nothing Then Return

        Dim MyControls As List(Of MCSCTRLS2.RichtextBoxFontToolstrip) = MyForm.AllChildControls(Of MCSCTRLS2.RichtextBoxFontToolstrip)

        If MyControls Is Nothing Then Return
        If MyControls.Count = 0 Then Return

        If MyControls.Count <> 1 AndAlso Debugger.IsAttached Then Stop

        Dim MyRichtextBoxFontToolstrip As MCSCTRLS2.RichtextBoxFontToolstrip = MyControls.First

        If Enabled Then
            MyRichtextBoxFontToolstrip.RichtextBox = Me
        Else
            MyRichtextBoxFontToolstrip.RichtextBox = Nothing
        End If
    End Sub

    Public Overloads Property ZoomFactor() As Single
        Get
            Return MyBase.ZoomFactor
        End Get
        Set(value As Single)
            If value = Me.ZoomFactor Then Return

            Dim objType As Type = GetType(System.Windows.Forms.RichTextBox) '  MyBase.GetType
            Dim method1 As Reflection.MethodInfo = objType.GetMethod("SendZoomFactor", Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.NonPublic)
            Dim Params(0) As Object

            Params(0) = value

            method1.Invoke(Me, Params)
            'MyBase.SendZoomFactor(value)
        End Set
    End Property


    Private Sub RichTextBox_GotFocus(sender As Object, e As System.EventArgs) Handles Me.GotFocus
        bUnlockOnLostFocus = False

        If Me.Mask = eDataMask.Time Then
            Me.Select(0, 0)
            Return
        End If

        If Me.ReadOnly Then Return

        Dim MyApp As MCS_Interfaces.iApplication = FrameWorkNS.Functions.Forms.GetApplication(Me)

        MyFrameWork.RaiseEasyProc(MCS_Interfaces.eEasyProcEvent.SPELLINGCONTROLE_MOGELIJK_CHANGED, Me, MyApp, True)

        SetStyleToolstrip(True)
    End Sub

    Private Sub RichTextBox_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LostFocus
        Try
            DontLooseFocus = False

            If Not MyListBox Is Nothing AndAlso MyListBox.Visible Then
                If Not MyListBox.Focused Then
                    HideListBox()
                    DontLooseFocus = False
                Else
                    DontLooseFocus = True
                End If
            End If

            Dim MyApp As MCS_Interfaces.iApplication = FrameWorkNS.Functions.Forms.GetApplication(Me)

            If Me.ReadOnly Then Return

            If Not MyFrameWork Is Nothing Then MyFrameWork.RaiseEasyProc(MCS_Interfaces.eEasyProcEvent.SPELLINGCONTROLE_MOGELIJK_CHANGED, Me, MyApp, False)
            SetStyleToolstrip(False)
        Catch ex As Exception
        End Try

    End Sub

    Private SelectieIndex As Integer = 0

    Private Sub RichTextBox_SelectionChanged(sender As Object, e As System.EventArgs) Handles Me.SelectionChanged

        If Me.SelectionStart + 2 < SelectieIndex Then
            HideListBox()
        Else
            If SelectieIndex + 2 < Me.SelectionStart Then
                HideListBox()
            End If
        End If

        SelectieIndex = Me.SelectionStart
    End Sub


    Private Sub RichTextBox_LinkClicked(sender As Object, e As System.Windows.Forms.LinkClickedEventArgs) Handles Me.LinkClicked
        If Not My.Computer.Keyboard.CtrlKeyDown Then Return 'Alleen links starten als CTRL is ingedrukt.

        Try
            Process.Start(e.LinkText)
        Catch ex As Exception

        End Try
    End Sub


End Class

Public Class UndoItem
    Public Enum eUndoAction
        Onbekend
        Letter_getypt
        Woord_getypt
        Tekst_verwijderd
        Tekst_getypt
        Nieuwe_Regel
        Plakken
        Knippen
        Laatste_Versie
    End Enum

    Public Property Action As eUndoAction = eUndoAction.Onbekend
    Public Property RTF As String = ""

    Public Property SelectionStart As Integer
    Public Property SelectionLength As Integer

    Private MyActionName As String = ""

    Public Property ActionName As String
        Get
            If MyActionName <> "" Then Return MyActionName
            Return Action.ToString.Replace("_", " ")
        End Get
        Set(ByVal value As String)
            MyActionName = value
        End Set
    End Property
End Class



