Imports System.Drawing
Imports System.Windows.Forms
Imports System.Runtime.InteropServices

Public Class TextBoxAPIHelper
    Private Const anInch As Double = 14.4

    Public Shared Function GetBaselineOffsetAtCharIndex(tb As System.Windows.Forms.TextBoxBase, index As Integer) As Integer
        Dim rtb As System.Windows.Forms.RichTextBox = TryCast(tb, RichTextBox)
        If rtb Is Nothing Then
            Return tb.Font.Height
        End If

        Dim lineNumber As Integer = rtb.GetLineFromCharIndex(index)
        Dim lineIndex As Integer = NativeMethods.SendMessageInt(rtb.Handle, NativeMethods.EM_LINEINDEX, New IntPtr(lineNumber), IntPtr.Zero).ToInt32()
        Dim lineLength As Integer = NativeMethods.SendMessageInt(rtb.Handle, NativeMethods.EM_LINELENGTH, New IntPtr(lineNumber), IntPtr.Zero).ToInt32()


        Dim charRange As NativeMethods.CHARRANGE
        charRange.cpMin = lineIndex
        charRange.cpMax = lineIndex + lineLength
        '			charRange.cpMin = index; 
        '			charRange.cpMax = index + 1; 


        Dim rect As NativeMethods.RECT
        rect.Top = 0
        rect.Bottom = CInt(Math.Truncate(anInch))
        rect.Left = 0
        rect.Right = 10000000
        '(int)(rtb.Width * anInch + 20); 

        Dim rectPage As NativeMethods.RECT
        rectPage.Top = 0
        rectPage.Bottom = CInt(Math.Truncate(anInch))
        rectPage.Left = 0
        rectPage.Right = 10000000
        '(int)(rtb.Width * anInch + 20); 
        Dim canvas As Graphics = Graphics.FromHwnd(rtb.Handle)
        Dim canvasHdc As IntPtr = canvas.GetHdc()

        Dim formatRange As NativeMethods.FORMATRANGE
        formatRange.chrg = charRange
        formatRange.hdc = canvasHdc
        formatRange.hdcTarget = canvasHdc
        formatRange.rc = rect
        formatRange.rcPage = rectPage

        NativeMethods.SendMessage(rtb.Handle, NativeMethods.EM_FORMATRANGE, IntPtr.Zero, formatRange).ToInt32()

        canvas.ReleaseHdc(canvasHdc)
        canvas.Dispose()

        'return (int) ((formatRange.rc.Bottom - formatRange.rc.Top)/anInch);
        Return CInt(Math.Truncate(((formatRange.rc.Bottom - formatRange.rc.Top) / anInch) * rtb.ZoomFactor))
    End Function

    ''' <summary>
    ''' Returns the index of the character under the specified 
    ''' point in the control, or the nearest character if there
    ''' is no character under the point.
    ''' </summary>
    ''' <param name="txt">The text box control to check.</param>
    ''' <param name="pt">The point to find the character for, 
    ''' specified relative to the client area of the text box.</param>
    ''' <returns></returns>
    Friend Shared Function CharFromPos(txt As System.Windows.Forms.TextBoxBase, pt As Point) As Integer
        ' Convert the point into a DWord with horizontal position
        ' in the loword and vertical position in the hiword:
        Dim xy As Integer = (pt.X And &HFFFF) + ((pt.Y And &HFFFF) << 16)
        ' Get the position from the text box.
        Dim res As Integer = NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_CHARFROMPOS, IntPtr.Zero, New IntPtr(xy)).ToInt32()
        ' the Platform SDK appears to be incorrect on this matter.
        ' the hiword is the line number and the loword is the index
        ' of the character on this line
        Dim lineNumber As Integer = ((res And &HFFFF) >> 16)
        Dim charIndex As Integer = (res And &HFFFF)

        ' Find the index of the first character on the line within 
        ' the control:
        Dim lineStartIndex As Integer = NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_LINEINDEX, New IntPtr(lineNumber), IntPtr.Zero).ToInt32()
        ' Return the combined index:
        Return lineStartIndex + charIndex

    End Function

    ''' <summary>
    ''' Returns the position of the specified character
    ''' </summary>
    ''' <param name="txt">The text box to find the character in.</param>
    ''' <param name="charIndex">The index of the character whose
    ''' position needs to be found.</param>
    ''' <returns>The position of the character relative to the client
    ''' area of the control.</returns>
    Friend Shared Function PosFromChar(txt As System.Windows.Forms.TextBoxBase, charIndex As Integer) As Point
        Dim xy As Integer = NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_POSFROMCHAR, New IntPtr(charIndex), IntPtr.Zero).ToInt32()
        Return New Point(xy)

    End Function

    Friend Shared Function GetFirstVisibleLine(txt As System.Windows.Forms.TextBoxBase) As Integer
        Return NativeMethods.SendMessageInt(txt.Handle, NativeMethods.EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32()
    End Function

    Private Sub New()
    End Sub
End Class

Friend Class NativeMethods
#Region "Unmanaged Code"

    <StructLayout(LayoutKind.Sequential)> _
    Friend Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Friend Structure CHARRANGE
        Public cpMin As Integer
        'First character of range (0 for start of doc)
        Public cpMax As Integer
        'Last character of range (-1 for end of doc)
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Friend Structure FORMATRANGE
        Public hdc As IntPtr
        'Actual DC to draw on
        Public hdcTarget As IntPtr
        'Target DC for determining text formatting
        Public rc As RECT
        'Region of the DC to draw to (in twips)
        Public rcPage As RECT
        'Region of the whole DC (page size) (in twips)
        Public chrg As CHARRANGE
        'Range of text to draw (see earlier declaration)
    End Structure

    Friend Const WM_USER As Integer = &H400
    Friend Const EM_FORMATRANGE As Integer = WM_USER + 57

    <DllImport("user32")> _
    Friend Shared Function SendMessage(hWnd As IntPtr, msg As UInteger, wp As IntPtr, ByRef lp As FORMATRANGE) As IntPtr
    End Function

    <DllImport("user32", CharSet:=CharSet.Auto, EntryPoint:="SendMessage")> _
    Friend Shared Function SendMessageInt(handle As IntPtr, msg As UInteger, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    Friend Const EM_LINEINDEX As Integer = &HBB
    Friend Const EM_LINELENGTH As Integer = &HC1
    Friend Const EM_POSFROMCHAR As Integer = &HD6
    Friend Const EM_CHARFROMPOS As Integer = &HD7
    Friend Const EM_GETFIRSTVISIBLELINE As Integer = &HCE

    <DllImport("user32", EntryPoint:="ShowCaret")> _
    Friend Shared Function ShowCaretAPI(hwnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

#End Region

    Private Sub New()
    End Sub
End Class
