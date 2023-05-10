Imports MCS_Interfaces
Imports System.Drawing.Imaging
Imports System.Text
Imports GemBox.Spreadsheet
Imports System.Runtime.InteropServices

<ToolboxBitmap(GetType(System.Windows.Forms.ListView))>
Public Class ListView
    Private WithEvents MyFrameWork As FrameWorkNS.FrameWork = EasyCare.FrameWork
    Private WithEvents tmrCheckSelectedItem As New System.Windows.Forms.Timer

    Public Event SelectedItemChangedAfterInterval(Item As ListViewItem)

    Public Shadows Event SelectedIndexChanged(sender As Object, e As EventArgs)
    Private bResizingColumns As Boolean = False
    Private m_SortingColumn As ColumnHeader
    Private bAutoSort As Boolean = True
    Private _SortingColumnIndex As Integer = 0
    Private sStartSettings As String = String.Empty
    Private Shared MySettings As Dictionary(Of String, String)
    Private WithEvents oComparer As ListViewComparer

    Public Property AutoResizeColumnsDisabled As Boolean = False

    '  Private bColumnsChanged As Boolean
    Public Property AltijdBovenKolom As String = ""
    Public Property AltijdBovenValue As String = ""

    Public Event GetCompareResult(Item1 As ListViewItem, Item2 As ListViewItem, ColumnIndex As Integer, SortOrder As SortOrder, ByRef Result As Integer)
    Public Event SubItemMouseHoverChanged(ListItem As ListViewItem, SubItem As ListViewItem.ListViewSubItem)

    Public Property ScrollWindowUnderMousePointer As Boolean = True

    Public Structure LVBKIMAGE
        Public ulFlags As Int32
        Public hbm As IntPtr
        Public pszImage As String
        Public cchImageMax As Int32
        Public xOffsetPercent As Int32
        Public yOffsetPercent As Int32
    End Structure

    'Constant Declarations
    Private Const LVM_FIRST As Int32 = &H1000
    Private Const LVM_SETBKIMAGEW As Int32 = (LVM_FIRST + 138)
    Private Const LVBKIF_TYPE_WATERMARK As Int32 = &H10000000

    'API Declarations
    Private Declare Sub CoInitialize Lib "ole32.dll" (ByVal pvReserved As Int32)
    Private Declare Sub CoUninitialize Lib "ole32.dll" ()
    Private Declare Function SendMessage Lib "user32.dll" Alias "SendMessageA" (ByVal hwnd As Int32, ByVal wMsg As Int32, ByVal wParam As Int32, ByVal lParam As Int32) As Int32

    Private iFillOutColumnIndex As Integer = -1

    Private dicColumnWidths As Dictionary(Of Integer, Integer)

    Public Sub BackupColumnWidths()
        dicColumnWidths = New Dictionary(Of Integer, Integer)

        For X As Integer = 0 To Me.Columns.Count - 1
            dicColumnWidths.Add(X, Me.Columns.Item(X).Width)
        Next
    End Sub

    Public Sub RestoreColumnWidths()
        If dicColumnWidths Is Nothing Then Return

        For Each columnIndex As Integer In dicColumnWidths.Keys
            Me.Columns.Item(columnIndex).Width = dicColumnWidths(columnIndex)
        Next
    End Sub

    Public Event ItemClick(Sender As MCSCTRLS1.ListView, Item As ListViewItem, e As MouseEventArgs)

    Private ItemAtMouseDown As ListViewItem

    Private Sub ListView_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        ItemAtMouseDown = GetItemAtV2(e.Location)
    End Sub

    Public Property SelectedItemKey As String
        Get
            Return GetItemKey(Me.SelectedItem)
        End Get
        Set(value As String)
            SelectItemByKey(value)
        End Set
    End Property

    Private Function GetItemKey(Item As ListViewItem) As String
        If Item Is Nothing Then Return ""

        Dim sResult As String = ""

        For X As Integer = 0 To Item.SubItems.Count - 1
            If X > 0 Then sResult &= "|"
            sResult &= Item.SubItems(X).Text
        Next

        Return sResult
    End Function

    Private Sub SelectItemByKey(Key As String)
        Me.DeselectAll()

        For Each Item As ListViewItem In Me.Items
            If GetItemKey(Item) = Key Then
                Item.Selected = True
                Return
            End If
        Next
    End Sub

    Private Function GetItemAtV2(Location As Point) As ListViewItem
        For itemIndex As Integer = 0 To Me.Items.Count - 1
            Dim item As ListViewItem = Me.Items(itemIndex)
            Dim itemRect As Rectangle = item.GetBounds(ItemBoundsPortion.Entire)

            If itemRect.Contains(Location) Then
                Return item
                Exit For
            End If
        Next

        Return Nothing
    End Function

    Private Sub listview_MouseClick(sender As Object, e As MouseEventArgs) Handles Me.MouseClick
        Dim MyListViewItem As ListViewItem

        MyListViewItem = GetItemAtV2(e.Location)

        If MyListViewItem Is Nothing Then Return
        If Not MyListViewItem Is ItemAtMouseDown Then Return

        RaiseEvent ItemClick(Me, MyListViewItem, e)
    End Sub

    Public Sub AddListviewItems(ListViewItems As List(Of ListViewItem), Optional bClearFirst As Boolean = True, Optional bResizeColumns As Boolean = True, Optional bSetColors As Boolean = True)

        Me.BeginUpdate()
        If bClearFirst Then Me.Items.Clear()

        If Not ListViewItems Is Nothing Then
            Me.Items.AddRange(ListViewItems.ToArray)
        End If

        If bResizeColumns Then Me.AutoResizeColumnsToFit()
        If bSetColors Then Me.SetColorsListviewRows()
        Me.EndUpdate()

    End Sub

    Public Sub SetColumn(OrgText As String, NewText As String)
        Dim MyColumn As ColumnHeader = Me.GetColumnByText(OrgText)
        If MyColumn Is Nothing Then Return

        With MyColumn
            .Text = NewText
        End With
    End Sub

    Public Sub SetColumn(OrgText As String, NewText As String, Width As Integer)
        Dim MyColumn As ColumnHeader = Me.GetColumnByText(OrgText)
        If MyColumn Is Nothing Then Return

        With MyColumn
            .Text = NewText
            If Width > -1 Then .Width = Width
        End With
    End Sub

    Public Sub SetColumn(OrgText As String, Width As Integer)
        Dim MyColumn As ColumnHeader = Me.GetColumnByText(OrgText)
        If MyColumn Is Nothing Then Return

        With MyColumn
            .Width = Width
        End With
    End Sub

    Public Property FillOutColumnIndex As Integer
        Get
            Return iFillOutColumnIndex
        End Get
        Set(value As Integer)
            iFillOutColumnIndex = value
            FillOutColumn()
        End Set
    End Property

    Public Function SelectFirstItem(Optional DeselectOthers As Boolean = False) As Boolean
        Dim MyItem As ListViewItem = Me.FirstItem
        If MyItem Is Nothing Then Return False

        If DeselectOthers Then Me.DeselectAll()
        MyItem.Selected = True

        Return True
    End Function

    Public Function FirstItem() As ListViewItem
        Dim Y As Integer = Integer.MaxValue
        Dim Result As ListViewItem = Nothing

        For Each Item As ListViewItem In Me.Items
            If Item.Bounds.Y < Y Then
                Result = Item
                Y = Item.Bounds.Y
            End If
        Next

        Return Result
    End Function

    Private Sub FillOutColumn()
        If iFillOutColumnIndex < 0 Then Return

        Dim iColumnsWidth As Integer

        If Me.Columns.Count = 1 Then
            Me.Columns(0).Width = Me.ClientRectangle.Width
            Return
        End If

        For X As Integer = 0 To Me.Columns.Count - 1
            If X <> iFillOutColumnIndex Then
                iColumnsWidth += Me.Columns(X).Width
            End If
        Next

        If iColumnsWidth < Me.ClientRectangle.Width Then
            Me.Columns(iFillOutColumnIndex).Width = (Me.ClientRectangle.Width - iColumnsWidth)
        End If

    End Sub

    Private Function LastColumn() As ColumnHeader
        Return Me.Columns(Me.Columns.Count - 1)
    End Function

    Public Sub AutoResizeColumnsToFit(FillOutColumnText As String)
        Dim iIndex As Integer = Me.GetColumnByText(FillOutColumnText).Index
        AutoResizeColumnsToFit(iIndex)
    End Sub

    Public Sub MakeColumnNecessaryWidth(Column As String, TenKosteVanColumn As String)
        Dim MyColumn As ColumnHeader
        Dim iCurrentWidth As Integer
        Dim iVerschil As Integer
        Dim iWidth As Integer
        Dim iTeller As Integer

        MyColumn = GetColumnByText(Column)

        If MyColumn Is Nothing Then Return

        If dicNecessaryColumnWidth Is Nothing OrElse Not dicNecessaryColumnWidth.ContainsKey(Column) Then Return

        iCurrentWidth = MyColumn.Width

        iVerschil = iCurrentWidth - dicNecessaryColumnWidth(Column)

        If iCurrentWidth >= dicNecessaryColumnWidth(Column) Then Return

        iWidth = dicNecessaryColumnWidth(Column)

        SetColumnWidth(MyColumn.Index, iWidth)
        
        MyColumn = Me.GetColumnByText(TenKosteVanColumn)

        iWidth = MyColumn.Width + iVerschil

        iTeller = 0

        While MyColumn.Width <> iWidth
            iTeller += 1
            MyColumn.Width = iWidth 'Geen idee waarom, maar dit moet meerdere keren...

            If iTeller >= 5 Then Exit While
        End While

    End Sub

    Private dicNecessaryColumnWidth As Dictionary(Of String, Integer)

    Public Sub AutoResizeColumnsToFit(FillOutColumnIndex As Integer)
        AutoResizeColumnsToFit()
        iFillOutColumnIndex = FillOutColumnIndex
        FillOutColumn()
    End Sub

    Public Sub AutoResizeColumnsToFit()
        If Me.IsDisposed Then Return
        If AutoResizeColumnsDisabled Then Return

        dicNecessaryColumnWidth = EasyCare.Functions.Collections.GetCaseInsensitiveDictionary(Of Integer)

        EasyCare.Functions.Debug.StartPerformanceCounter("AutoResizeColumnsToFit")

        Me.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)

        For Each C As ColumnHeader In Me.Columns
            If dicNecessaryColumnWidth.ContainsKey(C.Text) Then Continue For
            dicNecessaryColumnWidth.Add(C.Text, C.Width)
        Next

        'Me.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent)

        'Dim iWidth As Integer

        'For Each C As ColumnHeader In Me.Columns
        '    iWidth = C.Width

        '    iWidth = C.Width.Min(dicNecessaryColumnWidth(C.Text))

        '    If C.Width <> iWidth Then C.Width = iWidth
        'Next

        'Sla nu de uiteindelijke bepaalde benodigde breedte op in de collectie.

        dicNecessaryColumnWidth.Clear()

        For Each C As ColumnHeader In Me.Columns
            If dicNecessaryColumnWidth.ContainsKey(C.Text) Then Continue For
            dicNecessaryColumnWidth.Add(C.Text, C.Width)
        Next

        EasyCare.Functions.Debug.StopPerformanceCounter("AutoResizeColumnsToFit")

        For Each C As ColumnHeader In Me.Columns
            If dicNecessaryColumnWidth.ContainsKey(C.Text) Then Continue For
            dicNecessaryColumnWidth.Add(C.Text, C.Width)
        Next

    End Sub

    Public Sub SelectAll()
        Me.SuspendRedraw ' FrameworkNS.Functions.API.SetRedraw(Me.Handle, False)

        For Each Item As ListViewItem In Me.Items
            Item.Selected = True
        Next

        Me.ResumeRedraw 'FrameworkNS.Functions.API.SetRedraw(Me.Handle, True)
    End Sub

    Public Sub DeselectAll()
        Me.SuspendRedraw 'FrameworkNS.Functions.API.SetRedraw(Me.Handle, False)

        bDontRaiseSelectedIndexChanged = True
        For Each Item As ListViewItem In Me.SelectedItems
            Item.Selected = False

        Next

        bDontRaiseSelectedIndexChanged = False
        Me.ResumeRedraw 'FrameworkNS.Functions.API.SetRedraw(Me.Handle, True)
    End Sub

    Public Function GetColumnIndex(Text As String) As Integer
        For Each C As ColumnHeader In Me.Columns
            If C.Text.ToLower.Trim.Equals(Text.ToLower.Trim) Then
                Return C.Index
            End If
        Next

        Return -1
    End Function

    Public Function GetColumnByText(Text As String) As ColumnHeader
        For Each C As ColumnHeader In Me.Columns
            If C.Text.ToLower.Trim.Equals(Text.ToLower.Trim) Then
                Return C
            End If
        Next

        Return Nothing
    End Function

    Private Sub SetBkImage()
        If Not WaterMark Is Nothing Then
            Dim hBMP As IntPtr = GetBMP(WaterMark)
            If Not hBMP = IntPtr.Zero Then
                Dim lv As New LVBKIMAGE
                lv.hbm = hBMP
                lv.ulFlags = LVBKIF_TYPE_WATERMARK
                Dim lvPTR As IntPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Runtime.InteropServices.Marshal.SizeOf(lv))
                System.Runtime.InteropServices.Marshal.StructureToPtr(lv, lvPTR, False)
                SendMessage(Me.Handle, LVM_SETBKIMAGEW, 0, lvPTR)
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(lvPTR)
            End If
        Else
            Try
                Dim lv As New LVBKIMAGE
                lv.hbm = IntPtr.Zero
                lv.ulFlags = LVBKIF_TYPE_WATERMARK
                Dim lvPTR As IntPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(System.Runtime.InteropServices.Marshal.SizeOf(lv))
                System.Runtime.InteropServices.Marshal.StructureToPtr(lv, lvPTR, False)
                SendMessage(Me.Handle, LVM_SETBKIMAGEW, 0, lvPTR)
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(lvPTR)
            Catch ex As Exception

            End Try

        End If
    End Sub

    Private Function GetBMP(ByVal FromImage As Image) As IntPtr
        Dim bmp As Bitmap = New Bitmap(FromImage.Width, FromImage.Height)
        Dim g As Graphics = Graphics.FromImage(bmp)
        Dim Result As IntPtr

        g.Clear(Me.BackColor)

        Dim MyBrush As New SolidBrush(Me.BackColor)
        g.FillRectangle(MyBrush, New RectangleF(0, 0, bmp.Width, bmp.Height))
        MyBrush.Dispose()

        Dim MyImage As Image

        If Me.WatermarkAlpha < 255 Then
            Dim sngOpacity As Single = (1 / 255) * Me.WatermarkAlpha
            MyImage = FrameworkNS.Functions.GDI.MakeImageTransparent(FromImage, sngOpacity)
        Else
            MyImage = FromImage.Clone
        End If

        g.DrawImage(MyImage, 0, 0, bmp.Width, bmp.Height)
        MyImage.Dispose()
        g.Dispose()

        Result = bmp.GetHbitmap
        bmp.Dispose()

        Return Result
    End Function

    Private MyWatermarkAlpha As Integer = 200

    Public Property WatermarkAlpha() As Integer
        Get
            Return MyWatermarkAlpha
        End Get
        Set(ByVal value As Integer)
            MyWatermarkAlpha = value
            SetBkImage()
        End Set
    End Property

    Private MyWaterMark As Image

    Const SBS_HORZ = 0
    Const SBS_VERT = 1

    Const WM_VSCROLL = &H115
    Const WM_HSCROLL = &H114

    Private MyAllowSorting As Boolean = True
    Private Color1 As Color = Color.White
    Private Color2 As Color = Color.White

    Private MyListviewKolommen As New List(Of ListviewKolom)
    Private MySettingsKey As String = ""

    Public Event AfterItemAdded(Item As ListViewItem)
    Public Event AfterItemDeleted(Item As ListViewItem)

    Private Const LVN_INSERTITEM As Integer = 4173
    Private Const LVM_DELETEITEM As Integer = 4104

    <StructLayout(LayoutKind.Sequential)>
    Public Structure NMHDR
        Public hwndFrom As IntPtr
        Public idFrom As IntPtr
        Public code As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure NMLISTVIEW
        Public hdr As NMHDR
        Public iItem As Integer
        Public iSubItem As Integer
        Public uNewState As Integer
        Public uOldState As Integer
        Public uChanged As Integer
        Public lParam As IntPtr
    End Structure

    Private Const HDN_FIRST As Integer = -300
    Private Const HDM_SETORDERARRAY As Integer = 4626
    Private Const HDN_ENDDRAG As Integer = HDN_FIRST - 11
    Private Const LVN_FIRST As Integer = -100
    Private Const LVN_ITEMCHANGING As Integer = (LVN_FIRST - 0)

    Private Const LVIF_STATE As Integer = &H8
    Private Const LVIS_SELECTED As Integer = &H2

    Private bBusy As Boolean

    '    <DebuggerNonUserCode()>
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Select Case m.Msg
            Case LVN_INSERTITEM
                'Nog uitzoeken hoe we aan het ListViewItem komen...
                RaiseEvent AfterItemAdded(Nothing)

            Case LVM_DELETEITEM
                RaiseEvent AfterItemDeleted(Nothing)

            Case FrameworkNS.Functions.API.Messages.WM_MOUSEWHEEL
                If Me.ScrollWindowUnderMousePointer Then
                    If bBusy Then
                        bBusy = False
                        GoTo DefaultProc
                    End If

                    bBusy = True
                    If FrameworkNS.Functions.API.ScrollWindowUnderMousePointer(m) Then Return
                    bBusy = False
                End If

            'Case &H2000 + &H4E 'Reflected WM_Notify
            ''    GoTo DefaultProc

            '    Dim nmhr As NMHDR = CType(Marshal.PtrToStructure(m.LParam, GetType(NMHDR)), NMHDR)

            '    Select Case nmhr.code
            '        Case LVN_ITEMCHANGING
            '            Dim nmlv As NMLISTVIEW = CType(Marshal.PtrToStructure(m.LParam, GetType(NMLISTVIEW)), NMLISTVIEW)

            '            If (nmlv.uChanged And LVIF_STATE) <> 0 Then
            '                Dim currentSel As Boolean = (nmlv.uOldState And LVIS_SELECTED) = LVIS_SELECTED
            '                Dim newSel As Boolean = (nmlv.uNewState And LVIS_SELECTED) = LVIS_SELECTED

            '                If newSel <> currentSel Then
            '                    Dim bCancel As Boolean = False

            '                    RaiseEvent BeforeSelect(Me.SelectedItem, bCancel)
            '                    m.Result = If(bCancel, New IntPtr(1), IntPtr.Zero)

            '                    If Not bCancel AndAlso newSel Then
            '                        Dim MyItem As ListViewItem = Me.Items(nmlv.iItem)

            '                        RaiseEvent AfterSelect(MyItem)
            '                    End If

            '                    Return
            '                End If
            '            End If
            '    End Select


            Case FrameWorkNS.Functions.API.Messages.WM_NOTIFY
                Dim nmhr As NMHDR = CType(Marshal.PtrToStructure(m.LParam, GetType(NMHDR)), NMHDR)

                Select Case nmhr.code
                    Case HDN_ENDDRAG
                        MyBase.WndProc(m)
                        RaiseEvent AfterColumnReordered()
                        Return

                End Select
        End Select

DefaultProc:
        MyBase.WndProc(m)
    End Sub


    Private iCurrentPrintIndex As Integer = 0

    Public Sub BeginPrint(Optional Font As Font = Nothing)
        iCurrentPrintIndex = 0
        CurrentY = 0
        CurrentX = 0
        sStringPrinting = ""

        Me.PrintFont = Font
    End Sub

    Public Function TotalColumnWidth() As Integer
        Dim iResult As Integer = 0

        For Each C As ColumnHeader In Me.Columns
            iResult += C.Width
        Next

        Return iResult
    End Function

    Private sStringPrinting As String = ""
    Private CurrentX As Integer = 0
    Private CurrentY As Integer = 0

    Dim MyBorderPen As New Pen(Brushes.Gray)

    Private Property PrintFont As Font = Nothing

    Private Function PrintStringAf(G As Graphics, Rect As Rectangle) As Boolean
        Dim MyRect As New Rectangle(CurrentX, Rect.Y, Rect.Right - CurrentX, Rect.Height)

        Dim iCharsFit As Integer = 0
        Dim iLinesPerPage As Integer = 0
        Dim iHeight As Integer = 0
        Dim MyItem As ListViewItem = Me.Items(iCurrentPrintIndex)
        Dim sPrintNU As String = ""

        iHeight = G.MeasureString(sStringPrinting, PrintFont.isNull(MyItem.Font), MyRect.Size, StringFormat.GenericDefault, iCharsFit, iLinesPerPage).Height

        If iCharsFit < sStringPrinting.Length Then
            'PAST NIET!
            sPrintNU = sStringPrinting.Substring(0, iCharsFit)
            sStringPrinting = sStringPrinting.Substring(iCharsFit)
        Else
            sPrintNU = sStringPrinting
            sStringPrinting = ""
        End If

        G.DrawString(sPrintNU, PrintFont.isNull(MyItem.Font), Brushes.Black, MyRect)

        If sStringPrinting = "" Then
            CurrentY = Rect.Top + iHeight
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub PrintHeaders(ByRef G As Graphics, ByRef Rect As Rectangle)
        Dim iColumn As Integer
        Dim iWidth As Integer
        Dim sngScale As Single = (Me.TotalColumnWidth / Rect.Width)
        Dim CurrentX As Integer = Rect.X
        Dim CurrentY As Integer = Rect.Y
        Dim MyFont As Font = New Font(PrintFont.isNull(Me.Font), FontStyle.Italic Or FontStyle.Bold)
        Dim MyRect As Rectangle
        Dim iHeight As Integer
        Dim iMaxHeight As Integer = 0

        For iColumn = 0 To Me.Columns.Count - 1
            iWidth = Me.Columns(iColumn).Width / sngScale
            If iWidth = 0 Then Continue For
            iHeight = G.MeasureString(Me.Columns(iColumn).Text.hardTrim, MyFont, New SizeF).Height
            If iHeight > iMaxHeight Then iMaxHeight = iHeight
            CurrentX += iWidth
        Next

        'iMaxHeight = 50

        CurrentX = Rect.X
        MyRect = New Rectangle(CurrentX, CurrentY, Rect.Width, iMaxHeight)

        G.FillRectangle(Brushes.LightGray, MyRect)

        For iColumn = 0 To Me.Columns.Count - 1
            iWidth = Me.Columns(iColumn).Width / sngScale
            If iWidth = 0 Then Continue For

            MyRect = New Rectangle(CurrentX, CurrentY, iWidth, iMaxHeight)

            If Me.PrintLines Then
                'G.DrawLine(MyBorderPen, CurrentX + iWidth, Rect.Top, CurrentX + iWidth, Rect.Bottom)
            End If

            G.DrawString(Me.Columns(iColumn).Text.HardTrim, MyFont, Brushes.Black, MyRect)

            CurrentX += iWidth
        Next

        If Me.PrintLines Then
            ' G.DrawLine(MyBorderPen, Rect.X, Rect.Y + iMaxHeight, Rect.Right, Rect.Y + iMaxHeight)
        End If

        Rect.Y += iMaxHeight
        Rect.Height -= iMaxHeight
        MyFont.Destroy()
    End Sub

    Public Function Print(G As Graphics, Rect As Rectangle, Optional ByRef Bottom As Integer = 0) As Boolean
        Dim bFirstItemOnPage As Boolean = True

        CurrentY = Rect.Top

        If Me.PrintHeader Then
            PrintHeaders(G, Rect)
        End If

        CurrentY = Rect.Top

        If sStringPrinting <> "" Then
            'De vorige string paste niet op het vel... print deze nu eerst!
            If Not PrintStringAf(G, Rect) Then
                Return True 'HasMorePages 
            Else
                iCurrentPrintIndex += 1
            End If

            bFirstItemOnPage = False
        End If

        Dim sngScale As Single
        Dim iColumn As Integer
        Dim iWidth As Integer
        Dim sPrintNU As String = ""

        'Dim MyText As String
        Dim iMaxHeight As Integer = 0
        Dim iHeight As Integer = 0
        Dim iAvailableHeight As Integer = 0


        Do While iCurrentPrintIndex < Me.Items.Count
            CurrentX = Rect.Left
            iMaxHeight = 0

            Dim MyItem As ListViewItem = Me.Items(iCurrentPrintIndex)
            sngScale = (Me.TotalColumnWidth / Rect.Width)

            For iColumn = 0 To Me.Columns.Count - 1
                iWidth = Me.Columns(iColumn).Width / sngScale

                If iWidth = 0 Then Continue For
                
                Do While MyItem.SubItems.Count < Me.Columns.Count
                    If Debugger.IsAttached Then Stop 'Alle kolommen vullen van het listitem!
                    MyItem.SubItems.Add("")
                Loop

                sStringPrinting = MyItem.SubItems(iColumn).Text
                sStringPrinting = sStringPrinting.HardTrim

                iHeight = G.MeasureString(sStringPrinting, Me.PrintFont.isNull(MyItem.Font), New SizeF).Height
                iAvailableHeight = Rect.Bottom - CurrentY

                Dim MyRect As New Rectangle(CurrentX, CurrentY, iWidth, iAvailableHeight)

                Dim iCharsFit As Integer = 0
                Dim iLinesPerPage As Integer = 0

                iHeight = G.MeasureString(sStringPrinting, Me.PrintFont.isNull(MyItem.Font), MyRect.Size, StringFormat.GenericDefault, iCharsFit, iLinesPerPage).Height

                MyRect.Height = iHeight

                If iCharsFit < sStringPrinting.Length Then
                    If Not bFirstItemOnPage Then
                        MyRect = New Rectangle(Rect.X, CurrentY, Rect.Width, Rect.Bottom - CurrentY)
                        MyRect.Inflate(-1, -1)
                        G.FillRectangle(Brushes.White, MyRect) 'Verberg de al geprinte tekst
                        ' DrawLines(G, Rect)
                        sStringPrinting = ""
                        DrawLines(G, Rect)
                        Return True 'HasMorePages
                    End If

                    'PAST NIET!
                    sPrintNU = sStringPrinting.Substring(0, iCharsFit)
                    sStringPrinting = sStringPrinting.Substring(iCharsFit)
                Else
                    sPrintNU = sStringPrinting
                    sStringPrinting = ""
                End If

                G.DrawString(sPrintNU, Me.PrintFont.isNull(MyItem.Font), Brushes.Black, MyRect)

                If MyRect.Height > iMaxHeight Then iMaxHeight = MyRect.Height

                If MyRect.Bottom >= Rect.Bottom Then
                    MyRect = New Rectangle(Rect.X, CurrentY, Rect.Width, Rect.Bottom - CurrentY)

                    MyRect.Height = Rect.Bottom - MyRect.Y
                    MyRect.Width = (Rect.Right - MyRect.X)
                    G.FillRectangle(Brushes.White, MyRect) 'Verberg de al geprinte tekst

                    DrawLines(G, Rect)
                    Return True 'HasMoreMages 
                End If

                CurrentX += iWidth
            Next

            If Me.PrintLines Then
                'Teken de horizontale lijn onder elk item!
                G.DrawLine(MyBorderPen, Rect.Left, CurrentY + iMaxHeight, Rect.Right, CurrentY + iMaxHeight)
            End If

            CurrentY += iMaxHeight

            iCurrentPrintIndex += 1
            bFirstItemOnPage = False
        Loop

        Rect = New Rectangle(Rect.X, Rect.Y, Rect.Width, CurrentY - Rect.Y)
        DrawLines(G, Rect)

        Bottom = CurrentY

        Return False 'HasMorePages 
    End Function

    Private Sub DrawLines(G As Graphics, Rect As Rectangle)
        Dim iColumn As Integer
        Dim iWidth As Integer
        Dim sngScale As Single = (Me.TotalColumnWidth / Rect.Width)
        Dim CurrentX As Integer

        CurrentX = Rect.X

        For iColumn = 0 To Me.Columns.Count - 1
            iWidth = Me.Columns(iColumn).Width / sngScale

            CurrentX += iWidth

            G.DrawLine(MyBorderPen, CurrentX, Rect.Y, CurrentX, Rect.Bottom)

        Next

        G.DrawRectangle(MyBorderPen, Rect)
    End Sub

    Public Property PrintLines As Boolean = True
    Public Property PrintHeader As Boolean = True

    Public ReadOnly Property SaveColumnState() As Boolean
        Get
            If AutoResizeColumnsDisabled Then Return False
            If Me.Name = "" Then Return False 'Dynamisch toegevoegd. Dan hebben we geen key.
            If bResizingColumns Then Return False
            Return True
        End Get
    End Property

    Private WithEvents MyThreadRunner As New EasyCare.MultipleThreadRunner

    Public Function LoadBySqlAsync(ByVal SQL As String, Optional CompletedCallback As Action = Nothing, Optional ByVal ShowNULL As Boolean = False, Optional ByVal TrimValues As Boolean = True) As Boolean
        Dim Args As Dictionary(Of String, Object) = EasyCare.Functions.Collections.GetCaseInsensitiveDictionary(Of Object)

        MyThreadRunner = New EasyCare.MultipleThreadRunner

        Args.Add("SQL", SQL)
        Args.Add("ShowNULL", ShowNULL)
        Args.Add("TrimValues", TrimValues)
        Args.Add("CompletedCallback", CompletedCallback)

        MyThreadRunner.AddJob("LoadDatatable", AddressOf LoadDataTable,,, Args)
        MyThreadRunner.Run(AddressOf DataTableGeladen)
    End Function


    Private Sub LoadDataTable()
        Dim Args As Dictionary(Of String, Object) = MyThreadRunner.CurrentJob.Args
        Dim SQL As String
        Dim DT As DataTable

        SQL = Args("SQL")

        DT = EasyCare.SQL.GetDataTable(SQL)

        If DT Is Nothing Then
            If Debugger.IsAttached Then Stop
        End If

        MyThreadRunner.CurrentJob.Results.Add("DT", DT)

    End Sub

    Private Sub DataTableGeladen()
        Dim MyJob As EasyCare.MultipleThreadRunner.JobInfo
        Dim DT As DataTable
        Dim DR As IDataReader
        Dim bShowNull As Boolean
        Dim bTrimValues As Boolean
        Dim CompletedCallback As Action

        MyJob = MyThreadRunner.Results.First 'Er is maar 1 job.

        DT = MyJob.Results("DT")

        If Not DT Is Nothing Then
            DR = DT.CreateDataReader
        Else
            DR = Nothing
        End If

        Me.BeginUpdate()
        Me.Items.Clear()

        bShowNull = MyJob.Args("ShowNULL")
        bTrimValues = MyJob.Args("TrimValues")
        CompletedCallback = MyJob.Args("CompletedCallback")


        Me.LoadFromDataReader(DR, bShowNull, bTrimValues, False)

        DR.Destroy
        DT.Destroy

        Me.AutoResizeColumnsToFit()
        Me.SetColorsListviewRows()
        Me.EndUpdate()

        If Not CompletedCallback Is Nothing Then CompletedCallback.Invoke()

        MyThreadRunner.Destroy
    End Sub

    Public Function LoadBySQL(ByVal SQL As String, Optional ByVal ShowNULL As Boolean = False, Optional ByVal TrimValues As Boolean = True) As Boolean
        Dim DR As idatareader = Nothing
        Dim DM As MCS_Interfaces.iDataManager
        Dim bResult As Boolean = True

        DM = EasyCare.SQL.NewDataManager

        Try
            DM.SuspendSQLErrors()
            DR = DM.SQLdrResult(SQL)

            If Not DR Is Nothing Then
                bResult = Me.LoadFromDataReader(DR, ShowNULL, TrimValues, True)
            End If
        Catch ex As Exception
            bResult = False
        Finally
            DM.Destroy(DR)
        End Try

        Return bResult
    End Function

    Public Shared Function GetListItemFromDataReader(ByVal DR As idatareader, Optional ByVal ShowNULL As Boolean = False, Optional ByVal TrimValues As Boolean = True) As ListViewItem
        Dim MyItem As ListViewItem
        Dim X As Integer
        Dim sValue As String

        MyItem = New ListViewItem
        MyItem.Text = GetValue(DR, 0, ShowNULL, TrimValues)

        Dim Tags As New MCS_Interfaces.Dictionary(Of String, Object)(StringComparer.OrdinalIgnoreCase)
        Dim TagAlgevuld As Boolean = False

        For X = 1 To DR.FieldCount - 1
            sValue = GetValue(DR, X, ShowNULL, TrimValues)


            If DR.GetName(X).Trim.ToLower.StartsWith("tag_") Then
                Tags.Add(DR.GetName(X).Trim.Substring(4), DR(X).ToString)

            ElseIf DR.GetName(X).Trim.ToLower = "tag" Then
                MyItem.Tag = DR(X).ToString
                TagAlgevuld = True
            Else
                sValue = sValue.RtfToText
                MyItem.SubItems.Add(sValue)
            End If
        Next

        If Not TagAlgevuld Then
            If Tags.Count = 0 Then
                MyItem.Tag = DR.ToDictionary
            Else
                MyItem.Tag = Tags
            End If

        End If

        Return MyItem
    End Function

    Public Function LoadFromDataReader(ByRef DR As IDataReader, Optional ByVal ShowNULL As Boolean = False, Optional ByVal TrimValues As Boolean = True, Optional ResizeColumns As Boolean = False) As Boolean
        If DR Is Nothing Then
            If Debugger.IsAttached Then Stop
            Return False
        End If

        Dim MyItems As New List(Of ListViewItem)
        Dim MyItem As ListViewItem
        Dim X As Integer
        Dim bError As Boolean
        Dim sColumnName As String

        Me.SuspendLayout()
        Me.BeginUpdate()

        Me.Items.Clear()
        Me.Columns.Clear()

        For X = 0 To DR.FieldCount - 1
            sColumnName = DR.GetName(X).Trim()

            If sColumnName.Trim.ToLower = "tag" Then Continue For
            If sColumnName.Trim.ToLower.StartsWith("tag_") Then Continue For

            Me.Columns.Add(sColumnName)
        Next

        Do While DR.Read
            MyItem = GetListItemFromDataReader(DR, ShowNULL, TrimValues)
            MyItems.Add(MyItem)
        Loop

        If ResizeColumns Then
            Me.AddListviewItems(MyItems)
        Else
            Me.Items.AddRange(MyItems.ToArray)
        End If

        MyItems.Clear()
        MyItems = Nothing

        Me.EndUpdate()
        Me.ResumeLayout()
        Return Not bError

    End Function

    Public Overloads Sub AutoResizeColumns(headerAutoResize As ColumnHeaderAutoResizeStyle)
        If Me.Items.Count > 10 Or True Then
            AutoResizeColumnsFast(headerAutoResize)
        Else
            MyBase.AutoResizeColumns(headerAutoResize)
        End If

    End Sub

    Private Sub AutoResizeColumnsFast(headerAutoResize As ColumnHeaderAutoResizeStyle)
        'Deze functie bepaalt de kolombreedte wat minder accuraat, maar VEEL sneller.

        Dim dicColumns As New Dictionary(Of Integer, Integer)
        Dim G As Graphics = Me.CreateGraphics
        Dim iWidth As Integer
        Dim iTeller As Integer = 0

        For X As Integer = 0 To Me.Columns.Count - 1
            iWidth = TextWidth(G, Me.Columns(X).Text)

            If Debugger.IsAttached Then
                If iWidth > 500 Then Stop
            End If

            dicColumns.Add(X, iWidth)
        Next

        For Each LI As ListViewItem In Me.Items
            iTeller += 1

            If iTeller > 250 Then Exit For 'We analyseren maximaal 250 items. Anders duurt het te lang.

            For X As Integer = 0 To LI.SubItems.Count - 1
                iWidth = TextWidth(G, LI.SubItems(X).Text)

                If LI.ImageIndex >= 0 OrElse Not LI.ImageKey.isEmptyOrWhiteSpace Then
                    If Not LI.ListView.SmallImageList Is Nothing Then
                        iWidth += LI.ListView.SmallImageList.ImageSize.Width
                    End If
                End If

                If Not dicColumns.ContainsKey(X) Then Exit For
                If dicColumns(X) < iWidth Then dicColumns(X) = iWidth
            Next
        Next

        If Not Me.SmallImageList Is Nothing Then
            If dicColumns.ContainsKey(0) Then
                dicColumns(0) += Me.SmallImageList.ImageSize.Width
            End If
        End If

        For Each iIndex As Integer In dicColumns.Keys
            SetColumnWidth(iIndex, dicColumns(iIndex))
        Next

        G.Dispose()
    End Sub

    Private Sub SetColumnWidth(Index As Integer, Width As Integer)
        'Listview pakt vaak de width van een kolom niet in 1x.
        For X As Integer = 1 To 5
            If Me.Columns(Index).Width = Width Then Return
            Me.Columns(Index).Width = Width
        Next
    End Sub

    Private Function TextWidth(G As Graphics, Text As String) As Integer
        Return G.MeasureString(Text, Me.Font).Width + 20
    End Function

    Private Shared Function GetValue(ByVal DR As IDataReader, ByVal Index As Integer, ByVal ShowNull As Boolean, ByVal TrimValues As Boolean) As String
        Dim sResult As String
        Dim sType As String

        If IsDBNull(DR(Index)) Then
            If ShowNull Then
                Return "NULL"
            Else
                Return ""
            End If
        End If

        sType = DR.GetDataTypeName(Index).Trim.ToUpper

        Select Case sType
            Case "TEXT"
                sResult = DR(Index).ToString

                If sResult.Length > 250 Then
                    sResult = "LONG TEXT"
                End If

            Case "DATETIME", "SMALLDATETIME"
                sResult = DirectCast(DR(Index), DateTime).ToDutchDateTime   'DirectCast(DR(Index), DateTime).ToString("yyyy-MM-dd HH:mm")

            Case "BIT", "BOOLEAN"
                sResult = IIf(Convert.ToBoolean(DR(Index)), "Ja", "Nee")

            Case Else
                sResult = DR(Index).ToString
        End Select

        '  If sResult.isRTF Then dit niet hier doen.. Veel te langzaam.
        'sResult = FrameworkNS.Functions.Richtext.RtfToText(sResult)
        ' End If 

        If TrimValues Then
            sResult = sResult.Trim
        End If

        Return sResult.TrimCommasAndSpaces

    End Function

    Public Function LoadFromCSV(ByVal Filename As String, Optional ByVal Delimiter As String = ";", Optional ByVal FirstRowContainsColumnHeaders As Boolean = True) As Boolean
        Dim bError As Boolean = False
        Dim bFirstRow As Boolean = True
        Dim iColumn As Integer
        Dim sValue As String = ""
        Dim MyItems As New List(Of ListViewItem)
        Dim MyItem As ListViewItem

        Me.SuspendLayout()
        Me.BeginUpdate()
        Me.Items.Clear()
        Me.Columns.Clear()

        If Not FirstRowContainsColumnHeaders Then
            bFirstRow = False
        End If

        Try
            Using MyReader As New Microsoft.VisualBasic.FileIO.TextFieldParser(Filename, System.Text.Encoding.Default)

                MyReader.TextFieldType = FileIO.FieldType.Delimited
                MyReader.SetDelimiters(Delimiter)

                Dim currentRow As String()

                While Not MyReader.EndOfData
                    Try
                        currentRow = MyReader.ReadFields()

                        If bFirstRow Then
                            'Voeg de columnheaders toe.
                            For iColumn = 0 To currentRow.GetUpperBound(0)
                                sValue = currentRow(iColumn).Trim
                                Me.Columns.Add(sValue)
                            Next

                            bFirstRow = False
                        Else
                            MyItem = New ListViewItem

                            MyItem.Text = currentRow(0)

                            For iColumn = 1 To currentRow.GetUpperBound(0)
                                sValue = currentRow(iColumn)
                                MyItem.SubItems.Add(sValue)
                            Next

                            MyItems.Add(MyItem)
                        End If

                    Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                        bError = True
                    End Try

                End While
            End Using
        Catch ex As Exception
            bError = True
        End Try

        Me.Items.AddRange(MyItems.ToArray)
        MyItems.Clear()
        MyItems = Nothing

        Me.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize)

        Me.EndUpdate()
        Me.ResumeLayout()
        Return Not bError
    End Function

    Public Function ExportToExcel(ByVal WorksheetName As String, Optional ByVal Filename As String = "") As String
        GemBox.Spreadsheet.SpreadsheetInfo.SetLicense(FrameworkNS.Functions.Serials.GemBox())

        Dim MyFile As New GemBox.Spreadsheet.ExcelFile
        Dim MyWS As GemBox.Spreadsheet.ExcelWorksheet
        Dim MyRow As GemBox.Spreadsheet.ExcelRow
        Dim MyCell As GemBox.Spreadsheet.ExcelCell
        Dim iColumnTeller As Integer = 0
        Dim iRowTeller As Integer = 0
        Dim sFileName As String = WorksheetName

        WorksheetName = RemoveIllegalWorksheetChars(WorksheetName) 'Worksheetnaam mag niet langer zijn dan 31 karakters.

        MyWS = MyFile.Worksheets.Add(WorksheetName)

        If Filename = "" Then
            Dim MyDialog As New SaveFileDialog

            With MyDialog
                .AddExtension = True
                .SupportMultiDottedExtensions = True
                .Filter = "Excel (*.xlsx)|*.xlsx"
                .FileName = sFileName & ".xlsx"
                If .ShowDialog <> Windows.Forms.DialogResult.OK Then Return ""

                Filename = .FileName
            End With

            MyDialog.Dispose()
        End If

        'Maak hier het excel document.
        MyRow = MyWS.Rows(iRowTeller)
        MyRow.Style.Font.Weight = GemBox.Spreadsheet.ExcelFont.BoldWeight
        MyRow.Style.FillPattern.SetSolid(Color.FromArgb(34, 110, 208))
        MyRow.Style.Font.Color = Color.White

        For Each Column As ColumnHeader In Me.Columns
            MyRow.Cells(iColumnTeller).Value = Column.Text
            MyRow.Cells(iColumnTeller).Style.Borders.SetBorders(MultipleBorders.Outside, Color.Black, LineStyle.Thick)
            iColumnTeller += 1
        Next

        Dim Color1 As Color = Color.FromArgb(206, 227, 255)
        Dim COlor2 As Color = Color.White

        Dim sText As String

        For Each Item As ListViewItem In Me.Items
            iRowTeller += 1
            MyRow = MyWS.Rows(iRowTeller)

            For iColumnTeller = 0 To Me.Columns.Count - 1
                MyCell = MyRow.Cells(iColumnTeller)
                sText = Item.SubItems(iColumnTeller).Text
                MyCell.Value = sText
            Next

            If iRowTeller Mod 2 = 0 Then
                MyRow.Style.FillPattern.SetSolid(Color1)
            Else
                MyRow.Style.FillPattern.SetSolid(COlor2)
            End If

            MyRow.AutoFit()
        Next

        For iColumnTeller = 0 To Me.Columns.Count - 1
            MyWS.Columns(iColumnTeller).AutoFit()
        Next

        MyWS.Panes = New WorksheetPanes(PanesState.Frozen, 0, 1, "A2", PanePosition.BottomLeft)

        MyFile.DocumentProperties.BuiltIn(BuiltInDocumentProperties.Author) = "EasyCare"
        MyFile.DocumentProperties.BuiltIn(BuiltInDocumentProperties.Title) = WorksheetName

        Try
            MyFile.SaveXlsx(Filename)
        Catch ex As Exception
            Throw ex
        End Try


        Return Filename
    End Function

    Private Function RemoveIllegalWorksheetChars(ByVal WorksheetName As String) As String
        Dim sClearChars As String
        Dim i As Integer
        Dim Result As String = WorksheetName

        sClearChars = "*." & Chr(34) & "//\[]:;|=,"

        For i = 1 To Len(sClearChars) - 1
            Result = Microsoft.VisualBasic.Replace(Result, Mid(sClearChars, i, 1), "")
        Next

        Result = Result.Left(31)

        Return Result
    End Function

    Public Function Export(ByVal WorkSheetName As String) As String
        Dim MyDialog As New SaveFileDialog
        Dim FileName As String = ""

        With MyDialog
            .AddExtension = True
            .SupportMultiDottedExtensions = True
            .Filter = "Excel (*.xls)|*.xls|Gescheiden door lijstscheidingsteken (*.csv)|*.csv"
            .FileName = WorkSheetName
            If .ShowDialog <> Windows.Forms.DialogResult.OK Then Return ""

            FileName = .FileName
        End With

        MyDialog.Dispose()

        Select Case FileName.Substring(FileName.LastIndexOf(".")).Trim.ToUpper
            Case ".XLS"
                FileName = ExportToExcel(WorkSheetName, FileName)

            Case ".CSV"
                FileName = ExportToCSV(FileName)

            Case Else
                FileName = ""
        End Select

        Return FileName
    End Function

    Public Function ExportToCSV(Optional ByVal Filename As String = "") As String
        Dim SB As New StringBuilder
        Dim sLine As String = ""

        If Filename = "" Then
            Dim MyDialog As New SaveFileDialog

            With MyDialog
                .AddExtension = True
                .SupportMultiDottedExtensions = True
                .Filter = "Gescheiden door lijstscheidingsteken (*.csv)|*.csv"
                If .ShowDialog <> Windows.Forms.DialogResult.OK Then Return ""

                Filename = .FileName
            End With

            MyDialog.Dispose()
        End If

        For Each Column As ColumnHeader In Me.Columns
            sLine &= Chr(34) & Column.Text.Trim.Replace(Chr(34), Chr(34) & Chr(34)) & Chr(34) & ";"
        Next

        sLine = sLine.Substring(0, sLine.Length - 1)
        SB.AppendLine(sLine)

        For Each Item As ListViewItem In Me.Items
            sLine = ""

            For X As Integer = 0 To Me.Columns.Count - 1
                sLine &= Chr(34) & Item.SubItems(X).Text.Replace(Chr(34), Chr(34) & Chr(34)) & Chr(34) & ";"
            Next

            sLine = sLine.Substring(0, sLine.Length - 1)
            SB.AppendLine(sLine)
        Next

        Try
            My.Computer.FileSystem.WriteAllText(Filename, SB.ToString, False, System.Text.Encoding.Default)
        Catch ex As Exception
            MCSCTRLS2.Functions.Msgbox(ex.Message, MsgBoxStyle.Exclamation, "Fout bij exporteren.")
            Return ""
        End Try

        Return Filename
    End Function

    Public Sub SetColorsListviewRows()
        Me.SetColorsListviewRows(Color.White, Color.FromArgb(238, 246, 255))
    End Sub

    Public Sub SetColorsListviewRows(ByVal Color1 As Color, ByVal Color2 As Color)
        Dim iTeller As Integer

        Me.Color1 = Color1
        Me.Color2 = Color2

        If Me.Color1 = Color.White And Me.Color2 = Color.White Then Return

        For Each Item As ListViewItem In Me.Items
            If Not Item.BackColor.Equals(SystemColors.Window) Then 
                if Not Item.BackColor.Equals(Me.color1) andalso Not Item.BackColor.Equals(Me.color2) then Continue For
            End If 'Kleur is expliciet gezet.
            Item.BackColor = IIf(iTeller Mod 2 = 0, Color1, Color2)
            iTeller += 1
        Next

    End Sub

    Public Function CellText(ByVal Item As ListViewItem, ByVal ColumnText As String) As String
        Dim sText As String

        For Each Column As ColumnHeader In Me.Columns
            If Column.Text Is Nothing Then
                sText = ""
            Else
                sText = Column.Text.Trim.ToUpper
            End If
            If sText = ColumnText.Trim.ToUpper Then
                Return Item.SubItems(Column.Index).Text
            End If
        Next

        Return Nothing
    End Function

    Public Property AllowSorting() As Boolean
        Get
            Return MyAllowSorting
        End Get
        Set(ByVal value As Boolean)
            MyAllowSorting = value
        End Set
    End Property

    Public Overrides Property BackColor() As System.Drawing.Color
        Get
            Return MyBase.BackColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            Try
                If value.Equals(Color.Transparent) Then Return
                MyBase.BackColor = value
            Catch e As Exception

            Finally
                If Not Me.WaterMark Is Nothing Then
                    SetBkImage()
                End If
            End Try

        End Set
    End Property

    Public Property WaterMark() As Image
        Get
            Return MyWaterMark
        End Get
        Set(ByVal value As Image)
            If MyFrameWork.LowMemoryMode Then Return
            If value Is MyWaterMark Then Return
            MyWaterMark = value
            SetBkImage()
        End Set
    End Property

    Public Event AfterColumnReordered()

    Private Sub ListView_ColumnChanged() Handles Me.ColumnWidthChanged, Me.AfterColumnReordered
        If Not bColumnSettingsLoaded Then Return
        '  SaveColumnSettings()
    End Sub

    Private Sub ListView_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        MyFrameWork = Nothing
    End Sub

    Private Function DefaultKey() As String
        Dim sKey As String = ""
        Dim C As Control
        Dim sControlKey As String

        C = Me

        Do While Not C Is Nothing
            If TypeOf C Is MCSCTRLS2.ExpandingPanel Then
                sControlKey = C.Name
                If Not TypeOf DirectCast(C, MCSCTRLS2.ExpandingPanel).Parent Is MCSCTRLS2.ExpandingPanels Then
                    sControlKey &= "_EXPANDED"
                Else
                    sControlKey &= "_COLLAPSED"
                End If
            Else
                sControlKey = C.Name
            End If
            sKey = sControlKey & "_" & sKey
            C = C.Parent
        Loop

        sKey = sKey.TrimEnd("_")

        Return sKey
    End Function

    Private sLastColumnSettingsKey As String = String.Empty
    Private bColumnSettingsLoaded As Boolean = False

    Public Function LoadColumnSettings(Optional bLoadEvenIfbDoneIsTrue As Boolean = False) As Boolean
        If AutoResizeColumnsDisabled Then Return False

        bColumnSettingsLoaded = False
        Dim bResult As Boolean = False

        If Not Me.SaveColumnState Then Return False

        Me.SuspendRedraw

        Static sLastColumnSettingsKey As String = ""
        'Static bColumnSettingsLoaded As Boolean

        Dim Kolom As ColumnHeader

        If Me.Columns.Count = 0 Then GoTo Einde

        Dim Key As String = DefaultKey()

        If sLastColumnSettingsKey <> Key Then bColumnSettingsLoaded = False

        sLastColumnSettingsKey = Key

        If bColumnSettingsLoaded AndAlso bLoadEvenIfbDoneIsTrue = False Then
            bResult = True
            GoTo Einde
        End If

        bColumnSettingsLoaded = True

        If MySettings Is Nothing Then
            If Not Me.DesignMode Then
                LoadSettings()
            End If
        End If

        If Me.DesignMode Then GoTo Einde

        If MySettings Is Nothing OrElse MySettings.Count = 0 OrElse Not MySettings.ContainsKey(Key) OrElse Not ListView.ListViewSettings(Me.MyFrameWork).ContainsKey(Key) Then
            GoTo Einde
        End If

        Dim XML As String = MySettings.Item(Key)

        XML = FrameworkNS.Functions.IO.DecompressString(XML)

        FrameworkNS.Functions.XML.LoadClassFromXML(XML, MyListviewKolommen)

        For Each Item As ListviewKolom In Me.MyListviewKolommen
            Kolom = Me.Columns(Item.Name)

            If Kolom Is Nothing Then
                Kolom = Me.GetKolomByTekst(Item.Name)
            End If

            If Kolom Is Nothing Then Continue For

            Kolom.Width = Item.Width

            Try
                Kolom.DisplayIndex = Item.Index
            Catch ex As Exception

            End Try
        Next

        bResult = True


Einde:
        Me.Refresh()
        bColumnSettingsLoaded = True

        Me.ResumeRedraw

        Return bResult
    End Function

    'Private bColumnSettingsLoaded As Boolean = False

    Private Function GetKolomByTekst(ByVal Tekst As String) As ColumnHeader
        For Each Kolom As ColumnHeader In Me.Columns
            If Kolom.Text.Trim = Tekst.Trim Then Return Kolom
        Next
        Return Nothing
    End Function

    Private Sub VertaalColumnHeaders()
        For Each Header As ColumnHeader In Me.Columns
            Dim NewString As String = FrameworkNS.Functions.Language.GeefVertaling(Header.Text)
            If Header.Text <> NewString Then
                Try
                    Header.Text = NewString
                Catch

                End Try
            End If
        Next
    End Sub

    Public Shared Function GeefListViewSettings(ByVal ListView As ListView, ByVal Framework As FrameWorkNS.FrameWork) As String
        Dim sKey As String

        If ListView.FindForm Is Nothing Then Return ""

        sKey = ListView.FindForm.Name.ToUpper & "_" & ListView.Name

        Dim MySettings As Dictionary(Of String, String) = ListViewSettings(Framework)

        If MySettings Is Nothing Then Return ""

        If MySettings.ContainsKey(sKey) Then
            Return MySettings.Item(sKey)
        Else
            Return ""
        End If
    End Function

    Private Shared Function ListViewSettings(ByVal FrameWork As FrameWorkNS.FrameWork) As Dictionary(Of String, String)
        If MySettings Is Nothing Then LoadSettings()
        Return MySettings
    End Function

    Private Shared Sub LoadSettings()
        If Not Threading.Thread.CurrentThread.isMainThread Then
            If Debugger.IsAttached Then Stop
            Return
        End If

        Dim MyFramework As FrameWorkNS.FrameWork = EasyCare.FrameWork

        If MyFramework Is Nothing Then Return
        If MyFramework.DataManager Is Nothing Then Return

        If Not MyFramework.DataManager.TableExists("mcs_easycare_listview_Settings_EasyCare") Then Return

        Dim SQL As String = ""
        Dim DR As IDataReader
        Dim sKey As String
        Dim sValue As String

        If MyFramework.Customer Is Nothing Then Return
        If MyFramework.Customer.UserManager Is Nothing Then Return
        If MyFramework.Customer.UserManager.SelectedUser Is Nothing Then Return

        Static bDone As Boolean

        If MySettings Is Nothing Then bDone = False
        If bDone Then Return

        MySettings = New Dictionary(Of String, String)

        Dim DM as iDataManager = EasyCare.SQL.NewDataManager()
        SQL = "Select ID, ListViewKey, Settings from mcs_easycare_listview_Settings_EasyCare where UserID=" & MyFramework.Customer.UserManager.SelectedUser.ID

        DM.SuspendSQLErrors()
        DR = DM.SQLdrResult(SQL)
        DM.ResumeSQLErrors()

        If DR Is Nothing Then
            bDone = False
            DM.Dispose()
            Return
        End If

        Do While DR.Read
            sKey = DR!ListviewKey
            sValue = DR!Settings.ToString

            If Not MySettings.ContainsKey(sKey) Then
                MySettings.Add(sKey, sValue)
            End If
        Loop

        DM.Destroy(DR)

        bDone = True
    End Sub

    Public Overloads ReadOnly Property Columns() As ColumnHeaderCollection
        Get
            Return MyBase.Columns
        End Get
    End Property


    Public Property AutoSort() As Boolean
        Get
            Return bAutoSort
        End Get
        Set(ByVal value As Boolean)
            bAutoSort = value
        End Set
    End Property

    Public Property SortingColumnIndex() As Integer
        Get
            Return _SortingColumnIndex
        End Get
        Set(ByVal value As Integer)
            _SortingColumnIndex = value
            Me.Sorteren(_SortingColumnIndex)
        End Set
    End Property

    Public Property SortOnColumnClick As Boolean = True

    Private Sub uc_sorted_listview_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles Me.ColumnClick
        If Not SortOnColumnClick Then Return

        Dim PrevCursor As Cursor = Me.Cursor

        Me.Cursor = Cursors.WaitCursor
        Me.Sorteren(e.Column)
        Me.Cursor = PrevCursor
    End Sub

    Public Sub Sorteren(ByVal Kolom As String, Optional ByVal Mysort_order As System.Windows.Forms.SortOrder = SortOrder.None)
        Dim iColumIndex As Integer = Me.GetColumnIndex(Kolom)
        If iColumIndex > -1 Then Sorteren(iColumIndex, Mysort_order)
    End Sub

    Public Property SortOrder As System.Windows.Forms.SortOrder
        Get
            Return sort_order
        End Get
        Set(value As System.Windows.Forms.SortOrder)
            sort_order = value
        End Set
    End Property

    Private sort_order As System.Windows.Forms.SortOrder

    Public Sub Sorteren(ByVal Kolom As Integer, Optional ByVal Mysort_order As System.Windows.Forms.SortOrder = SortOrder.None)
        If Me.Columns.Count = 0 Then Return
        If Me.Columns.Count <= Kolom Then Return
        If Not AutoSort Then Return
        If Not AllowSorting Then Return

        Dim new_sorting_column As ColumnHeader = Me.Columns.Item(Kolom)

        If Mysort_order = SortOrder.None Then
            If m_SortingColumn Is Nothing Then
                sort_order = SortOrder.Ascending
            Else
                If new_sorting_column.Equals(m_SortingColumn) Then
                    If sort_order = SortOrder.Ascending Then
                        sort_order = SortOrder.Descending
                    Else
                        sort_order = SortOrder.Ascending
                    End If
                Else
                    sort_order = SortOrder.Ascending
                End If

            End If
        Else
            sort_order = Mysort_order
        End If

        m_SortingColumn = new_sorting_column

        oComparer = New ListViewComparer(Kolom, sort_order, Me.Columns(Kolom).Text)

        Try
            If new_sorting_column.Tag = "int" Then
                oComparer.Type = "integer"
            ElseIf new_sorting_column.Tag = "date" Then
                oComparer.Type = "date"
            End If
        Catch ex As Exception

        End Try

        oComparer.AltijdBovenKolom = Me.AltijdBovenKolom
        oComparer.AltijdBovenValue = Me.AltijdBovenValue

        Try
            Me.ListViewItemSorter = oComparer
        Catch ex As Exception
            If Debugger.IsAttached Then Stop 'Netjes oplossen.
        End Try

        Me.ListViewItemSorter = Nothing

        If Not Me.Color1.Equals(Me.Color2) Then
            Me.SetColorsListviewRows(Me.Color1, Me.Color2)
        End If

        Me.BeginUpdate() 'Dit zit erin omdat bij een groep de sortering niet getoond wordt onder citrix (Opmerking Remco!)
        Me.EndUpdate()

        oComparer = Nothing
    End Sub

    Public Sub SaveColumnSettings()
        If Not Me.SaveColumnState Then Return
        If Not bColumnSettingsLoaded Then Return
        If MyFrameWork Is Nothing Then Return
        If MyFrameWork.DataManager Is Nothing Then Return
        If Not EasyCare.SQL.TableExists("mcs_easycare_listview_Settings") And Not MyFrameWork.DataManager.TableExists("mcs_easycare_listview_Settings_EasyCare") Then Return
        If MyFrameWork.Customer.UserManager.SelectedUser Is Nothing Then Return

        Dim Key As String = DefaultKey()

        MyListviewKolommen.Clear()

        Dim Kolom As ListviewKolom

        For Each MyKolom As ColumnHeader In Me.Columns
            Kolom = New ListviewKolom
            With Kolom
                If MyKolom.Name <> "" Then
                    .Name = MyKolom.Name
                Else
                    .Name = MyKolom.Text
                End If
                .Visible = True

                .Index = MyKolom.DisplayIndex

                If .Index < 0 Then Return

                .Width = MyKolom.Width
            End With
            MyListviewKolommen.Add(Kolom)
        Next

        Dim XML As String = FrameworkNS.Functions.XML.SaveClassAsXML(MyListviewKolommen)
        Dim SQL As String = ""

        SQL = "Declare @XML varchar(max) = '<XML>'" & vbCrLf
        SQL &= "if not exists(Select '' from mcs_easycare_listview_Settings_EasyCare where ListviewKey='<LISTVIEWKEY>' and Userid=<USERID>)" & vbCrLf
        SQL &= "Begin" & vbCrLf
        SQL &= "insert into mcs_easycare_listview_Settings_EasyCare (UserID, ListviewKey, Settings) values (<USERID>,'<LISTVIEWKEY>', @XML)" & vbCrLf
        SQL &= "End" & vbCrLf
        SQL &= "Else" & vbCrLf
        SQL &= "Begin" & vbCrLf
        SQL &= "Update mcs_easycare_listview_Settings_EasyCare set Settings=@XML where Listviewkey='<LISTVIEWKEY>' and UserID=<USERID> " & vbCrLf
        SQL &= "End"

        XML = FrameworkNS.Functions.IO.CompressString(XML)

        SQL = SQL.Replace("<USERID>", MyFrameWork.Customer.UserManager.SelectedUser.ID)
        SQL = SQL.Replace("<LISTVIEWKEY>", Key.SqlWaarde)
        SQL = SQL.Replace("<XML>", XML.SqlWaarde)

        EasyCare.SQL.ExecuteSQLinSeperateThread(SQL)

        If MySettings Is Nothing Then MySettings = New Dictionary(Of String, String)

        If Not MySettings.ContainsKey(Key) Then
            MySettings.Add(Key, XML)
        Else
            MySettings(Key) = XML
        End If

        MySettings.Item(Key) = XML

Einde:


    End Sub

    Private Sub ListView_ParentChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.ParentChanged
        If Me.DesignMode Then Return
        Dim MySettings As String = ListView.GeefListViewSettings(Me, MyFrameWork)
    End Sub

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)

        CoInitialize(IntPtr.Zero) 'Voor het watermerk... niet weghalen!
        Me.ShowItemToolTips = True
        Me.FullRowSelect = True
        Me.AllowColumnReorder = True
        Me.View = Windows.Forms.View.Details
        Me.HideSelection = False

        tmrCheckSelectedItem.Interval = 50
    End Sub

    Public ReadOnly Property SelectedItem() As ListViewItem
        Get
            If Me.SelectedItems.Count = 0 Then Return Nothing
            Return Me.SelectedItems(0)
        End Get
    End Property

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        CoUninitialize()
    End Sub

    Private Sub ListView_HandleCreated(sender As Object, e As System.EventArgs) Handles Me.HandleCreated
        SetBkImage()
    End Sub

    Private Sub oComparer_GetCompareResult(Item1 As System.Windows.Forms.ListViewItem, Item2 As System.Windows.Forms.ListViewItem, ColumnIndex As Integer, Order As SortOrder, ByRef Result As Integer) Handles oComparer.GetCompareResult
        RaiseEvent GetCompareResult(Item1, Item2, ColumnIndex, Order, Result)
    End Sub

    Private Sub ListView_MouseLeave(sender As Object, e As System.EventArgs) Handles Me.MouseLeave
        'Me.SaveColumnSettings()
    End Sub

    Private SelectedSubItem As ListViewItem.ListViewSubItem

    Private Sub ListView_MouseMove(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        Dim MyItem As ListViewItem = Me.GetItemAt(e.X, e.Y)
        If MyItem Is Nothing Then Return
        Dim MySubItem As ListViewItem.ListViewSubItem = MyItem.GetSubItemAt(e.X, e.Y)
        If MySubItem Is Nothing Then Return
        If MySubItem Is SelectedSubItem Then Return
        Me.SelectedSubItem = MySubItem
        RaiseEvent SubItemMouseHoverChanged(MyItem, MySubItem)
    End Sub

    Private Sub ListView_SizeChanged(sender As Object, e As System.EventArgs) Handles Me.SizeChanged
        FillOutColumn()
    End Sub

    Public Event CreateItemContextMenu(ClickedItem As ListViewItem, Menu As MCSCTRLS2.ContextMenu)
    Public Event CreateListviewContextMenu(Menu As MCSCTRLS2.ContextMenu)
    Public Event ContextMenuClick(Key As String, ClickedItem As ListViewItem, MenuItem As MCSCTRLS2.EasyMenuItem)

    Private Sub ListView_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        If Me.IsDisposed Then
            'If Debugger.IsAttached Then Stop
            Return
        End If

        If e.Button <> Windows.Forms.MouseButtons.Right Then Return

        Static bBusy As Boolean = False
        If bBusy Then Return
        bBusy = True

        Dim MyItem As ListViewItem = Me.GetItemAt(e.Location.X, e.Location.Y)

        If Not MyItem Is Nothing Then
            If Not MyItem.Selected Then
                If Me.SelectedItems.Count <> 0 Then Me.DeselectAll()
                MyItem.Selected = True
            End If
        End If

        Dim MyMenu As New MCSCTRLS2.ContextMenu

        If MyItem Is Nothing Then
            RaiseEvent CreateListviewContextMenu(MyMenu)
        Else
            RaiseEvent CreateItemContextMenu(MyItem, MyMenu)
        End If

        If MyMenu.MenuItems.Count > 0 Then
            Dim sResult As String
            Dim MyClickedMenuItem As MCSCTRLS2.EasyMenuItem

            MyClickedMenuItem = MyMenu.ShowMenu(Me)
            sResult = MyClickedMenuItem.Key 'MyMenu.Show(Me)

            If Not sResult.isEmptyOrWhiteSpace Then RaiseEvent ContextMenuClick(sResult, MyItem, MyClickedMenuItem)
        End If

        MyMenu.Dispose()
        bBusy = False
    End Sub

    Private bDontRaiseSelectedIndexChanged As Boolean = False

    Private Sub ListView_SelectedIndexChanged(sender As Object, e As EventArgs) Handles MyBase.SelectedIndexChanged
        If bDontRaiseSelectedIndexChanged Then Return
        If Me.IsDisposed Then Return
        If Me.tmrCheckSelectedItem Is Nothing Then Return

        If Me.SelectedItem Is Nothing Then RaiseEvent AfterDeselectAll(Me)

        Me.tmrCheckSelectedItem.Stop()
        Me.tmrCheckSelectedItem.Start()

        If Not Me.SelectedItem Is Nothing Then
            PreviousSelectedItem = Nothing
            RaiseEvent SelectedIndexChanged(sender, e)
        End If
    End Sub

    Private Sub tmrCheckSelectedItem_Tick(sender As Object, e As EventArgs) Handles tmrCheckSelectedItem.Tick
        Me.tmrCheckSelectedItem.Stop()
        If Me.SelectedItem Is Nothing Then
            'De SelectedIndexChanged gaat ook af op de Deselect van een item. (dus 2x bij een muisklik).
            'Deze raisen we hier, nadat de timer is afgegaan.
            'Als er in de tussentijd dan weer een item is geselecteerd gaat het event niet af.
            RaiseEvent SelectedIndexChanged(Me, Nothing)
        End If
        RaiseEvent SelectedItemChangedAfterInterval(Me.SelectedItem)
    End Sub

    Public Event AfterDeselectAll(Sender As MCSCTRLS1.ListView)

    Private PreviousSelectedItem As ListViewItem
    Private bIgnoreItemSelectionChanged As Boolean = False

    Public Event BeforeSelect(PrevItem As ListViewItem, ByRef Cancel As Boolean)
    Public Event AfterSelect(Item As ListViewItem)
End Class


Class ListViewComparer
    Implements IComparer

    Private m_ColumnNumber As Integer
    Private m_columnName As String
    Private m_SortOrder As SortOrder

    Public Property AltijdBovenKolom As String = ""
    Public Property AltijdBovenValue As String = ""

    Public Event GetCompareResult(Item1 As ListViewItem, Item2 As ListViewItem, ColumnIndex As Integer, Order As SortOrder, ByRef Result As Integer)

    Private sType As String

    Public Property Type() As String
        Get
            Return sType
        End Get
        Set(ByVal value As String)
            sType = value
        End Set
    End Property

    Public Sub New(ByVal column_number As Integer, ByVal sort_order As SortOrder, column_name As String)
        m_ColumnNumber = column_number
        m_columnName = column_name.ToLower.Trim
        m_SortOrder = sort_order
    End Sub

    Private Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
        Dim oItemx, oItemy As ListViewItem

        oItemx = DirectCast(x, ListViewItem)
        oItemy = DirectCast(y, ListViewItem)

        Dim Result As Integer = Integer.MinValue + 1

        RaiseEvent GetCompareResult(oItemx, oItemy, m_ColumnNumber, m_SortOrder, Result)

        If Result <> Integer.MinValue + 1 Then Return Result 'Event is afgevangen.

        Result = 0

        If TypeOf oItemx.Tag Is String AndAlso oItemx.Tag.ToString.ToUpper.Trim = "ERROR" Then Result = -1 : GoTo einde
        If TypeOf oItemy.Tag Is String AndAlso oItemy.Tag.ToString.ToUpper.Trim = "ERROR" Then Result = 1 : GoTo einde

        Dim String1 As String = ""
        Dim String2 As String = ""

        'CONTROLEER EERST ALTIJD BOVEN KOLOM
        If AltijdBovenKolom <> "" AndAlso AltijdBovenValue <> "" Then
            Dim iAltijdBovenIndex As Integer = -1

            For Each C As ColumnHeader In oItemx.ListView.Columns
                If C.Text.ToLower.Trim = AltijdBovenKolom.ToLower.Trim Then
                    iAltijdBovenIndex = C.Index
                    Exit For
                End If
            Next

            If iAltijdBovenIndex > -1 Then
                String1 = oItemx.SubItems(iAltijdBovenIndex).Text
                String2 = oItemy.SubItems(iAltijdBovenIndex).Text

                If Not String1.Equals(String2) Then
                    If String1.ToLower.Trim = AltijdBovenValue.Trim.ToLower Then Result = 0 : GoTo einde
                    If String2.ToLower.Trim = AltijdBovenValue.Trim.ToLower Then Result = 1 : GoTo einde
                End If
            End If
        End If
        'EINDE ALTIJD BOVEN KOLOM

        If oItemx.SubItems.Count > m_ColumnNumber Then String1 = oItemx.SubItems(m_ColumnNumber).Text
        If oItemy.SubItems.Count > m_ColumnNumber Then String2 = oItemy.SubItems(m_ColumnNumber).Text

        If Debugger.IsAttached And False Then
            If m_SortOrder = SortOrder.Ascending Then
                Return String.Compare(String1, String2)
            Else
                Return String.Compare(String2, String1)
            End If
        End If

        Dim iPosVerschil As Integer = 0

        If String1.Trim = "" Then GoTo verder
        If String2.Trim = "" Then GoTo verder

        If TypeOfString(String1) = "Date" And TypeOfString(String2) = "Date" Then GoTo verder
        If TypeOfString(String1) = "Integer" And TypeOfString(String2) = "Integer" Then GoTo verder

        'Knip het gelijke begin eraf...
        Do While String1.Substring(iPosVerschil, 1) = String2.Substring(iPosVerschil, 1)
            iPosVerschil += 1
            If (iPosVerschil = String1.Length) OrElse (iPosVerschil = String2.Length) Then
                If String1.Length = String2.Length Then
                    Result = 0 : GoTo einde
                Else
                    iPosVerschil -= 1
                    Exit Do
                End If
            End If
        Loop

verder:
        String1 = String1.Substring(iPosVerschil).Trim
        String2 = String2.Substring(iPosVerschil).Trim
      
        If TypeOfString(String1) = TypeOfString(String2) Then
            Type = TypeOfString(String1)
        Else
            Type = "String"
        End If

        If Type = "Integer" Then
            If String1.StartsWith(".") Then String1 = 0 & String1
            If String2.StartsWith(".") Then String2 = 0 & String2
            String1 = String1.Replace(".", ",")
            String2 = String2.Replace(".", ",")

            'dit staat erin omdat er ook een '-' in kan voorkomen (opmerking Remco)

            'If Not IsNumeric(String2) Then		
            If Not Double.TryParse(String2, Nothing) Then
                Result = 1 : GoTo einde
            End If

            'If Not IsNumeric(String1) Then		
            If Not Double.TryParse(String1, Nothing) Then
                Result = 1 : GoTo einde
            End If

            If Convert.ToDouble(String1) < Convert.ToDouble(String2) Then
                If m_SortOrder = SortOrder.Ascending Then
                    Result = -1 : GoTo einde
                Else
                    Result = 1 : GoTo einde
                End If

            ElseIf Convert.ToDouble(String1) = Convert.ToDouble(String2) Then
                Result = 0 : GoTo einde
            Else
                If m_SortOrder = SortOrder.Ascending Then
                    Result = 1 : GoTo einde
                Else
                    Result = -1 : GoTo einde
                End If
            End If

        ElseIf Type = "Date" Then
            If Not FrameworkNS.Functions.Rekenfuncties.isDate(String1) Then Return IIf(m_SortOrder = SortOrder.Ascending, -1, 1)
            If Not FrameworkNS.Functions.Rekenfuncties.isDate(String2) Then Return IIf(m_SortOrder = SortOrder.Ascending, 1, -1)

                If Convert.ToDateTime(String1) < Convert.ToDateTime(String2) Then
                    If m_SortOrder = SortOrder.Ascending Then
                        Result = -1 : GoTo Einde
                    Else
                        Result = 1 : GoTo Einde
                    End If
                ElseIf Convert.ToDateTime(String1) = Convert.ToDateTime(String2) Then
                    Result = 0 : GoTo Einde
                Else
                    If m_SortOrder = SortOrder.Ascending Then
                        Result = 1 : GoTo Einde
                    Else
                        Result = -1 : GoTo Einde
                    End If
                End If
        Else
            If String1.ToLower < String2.ToLower Then
                If m_SortOrder = SortOrder.Ascending Then
                    Result = -1 : GoTo einde
                Else
                    Result = 1 : GoTo einde
                End If
            ElseIf String1.ToLower = String2.ToLower Then
                Result = 0 : GoTo einde
            ElseIf String1.ToLower > String2.ToLower Then
                If m_SortOrder = SortOrder.Ascending Then
                    Result = 1 : GoTo einde
                Else
                    Result = -1 : GoTo einde
                End If
            End If
        End If

Einde:
        'Raise nu een event om te kijken of de listview dit wil overrulen...
        'Verplaatst naar boven, voor de default afhandeling.

        'RaiseEvent GetCompareResult(oItemx, oItemy, m_ColumnNumber, m_SortOrder, Result)

        Return Result
    End Function

    Private Function TypeOfString(ByVal Value As String) As String

        Select Case m_columnName 'Is altijd lowercase.
            Case "gebruikersnaam", "naam", "omschrijving", "opmerking", "specialisme", "opmerking"
                Return "String"

            Case "datum", "startdatum", "einddatum", "geboortedatum"
                Return "Date"
        End Select

        If m_columnName.Contains("naam") Then Return "String"

        If (Value.Length = 10 Or Value.Length = 16) AndAlso Value.IndexOf("-") > 0 AndAlso FrameworkNS.Functions.Rekenfuncties.isDate(Value) Then
            'If Value.Length = 10 AndAlso Value.IndexOf("-") > 0 Or FrameworkNS.Functions.Rekenfuncties.isDate(Value) Then 'met erik overleggen gaat fout in sorteren van poplijsten met datum en tijd op bovenstaande manier.
            Try
                Convert.ToDateTime(Value)
            Catch ex As Exception
                Return "Integer"
            End Try

            Return "Date"
        ElseIf FrameworkNS.Functions.Rekenfuncties.IsNumeric(Value) Then
            Return "Integer"
        Else
            Return "String"
        End If
    End Function
End Class

Public Class ListviewKolom
    Private MyName As String
    Private MyVisible As Boolean
    Private Myindex As Integer
    Private MyWidth As Integer

    Public Property Width() As Integer
        Get
            Return MyWidth
        End Get
        Set(ByVal value As Integer)
            MyWidth = value
        End Set
    End Property

    Public Property Index() As Integer
        Get
            Return Myindex
        End Get
        Set(ByVal value As Integer)
            Myindex = value
        End Set
    End Property

    Public Property Visible() As Boolean
        Get
            Return MyVisible
        End Get
        Set(ByVal value As Boolean)
            MyVisible = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return MyName
        End Get
        Set(ByVal value As String)
            MyName = value
        End Set
    End Property

End Class

'Public Class EasyListViewItems
'    Inherits Windows.Forms.ListView.ListViewItemCollection
'    Public Sub New(ByVal Owner As MCSCTRLS1.ListView)
'        MyBase.New(Owner)
'    End Sub

'    '  <Obsolete("AddRange gebruiken")> _
'    Public Overrides Function Add(ByVal text As String) As System.Windows.Forms.ListViewItem
'        Return MyBase.Add(text)
'    End Function

'    <Obsolete("AddRange gebruiken")> _
'    Public Overrides Function Add(ByVal text As String, ByVal imageKey As String) As System.Windows.Forms.ListViewItem
'        Return MyBase.Add(text, imageKey)
'    End Function

'    <Obsolete("AddRange gebruiken")> _
'    Public Overrides Function Add(ByVal text As String, ByVal imageIndex As Integer) As System.Windows.Forms.ListViewItem
'        Return MyBase.Add(text, imageIndex)
'    End Function

'    <Obsolete("AddRange gebruiken")> _
'    Public Overrides Function Add(ByVal key As String, ByVal text As String, ByVal imageIndex As Integer) As System.Windows.Forms.ListViewItem
'        Return MyBase.Add(key, text, imageIndex)
'    End Function

'    <Obsolete("AddRange gebruiken")> _
'    Public Overrides Function Add(ByVal key As String, ByVal text As String, ByVal imageKey As String) As System.Windows.Forms.ListViewItem
'        Return MyBase.Add(key, text, imageKey)
'    End Function

'    <Obsolete("AddRange gebruiken")> _
'    Public Overrides Function Add(ByVal value As System.Windows.Forms.ListViewItem) As System.Windows.Forms.ListViewItem
'        Return MyBase.Add(value)
'    End Function
'End Class