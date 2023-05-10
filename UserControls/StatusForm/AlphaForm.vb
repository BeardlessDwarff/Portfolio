Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging
Imports System.Drawing
Imports System.Windows.Forms

Public Class AlphaForm
    Public Sub SetBitmap(ByVal bitmap As Bitmap)
        If Me.IsDisposed Then Return
        SetBitmap(bitmap, 255)
    End Sub

    Private Delegate Sub Delegate_SetBitmap(ByRef bitmap As Bitmap, ByVal opacity As Byte)

    <DebuggerNonUserCode()>
    Public Sub SetBitmap(ByRef bitmap As Bitmap, ByVal opacity As Byte)

        If Me.InvokeRequired Then
            Dim Args(1) As Object
            Args(0) = bitmap
            Args(1) = opacity
            Dim x As New Delegate_SetBitmap(AddressOf SetBitmap)
            Me.Invoke(x, Args)
            Return
        End If

        If bitmap.PixelFormat <> PixelFormat.Format32bppArgb Then
            bitmap = Functions.GetImageAs32BppArgb(bitmap)
        End If

        ' The idea of this is very simple, 
        ' 1. Create a compatible DC with screen; 
        ' 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC; 
        ' 3. Call the UpdateLayeredWindow. 

        Dim screenDc As IntPtr = Win32.GetDC(IntPtr.Zero)
        Dim memDc As IntPtr = Win32.CreateCompatibleDC(screenDc)
        Dim hBitmap As IntPtr = IntPtr.Zero
        Dim oldBitmap As IntPtr = IntPtr.Zero

        Try
            hBitmap = bitmap.GetHbitmap(Color.FromArgb(0))
            ' grab a GDI handle from this GDI+ bitmap 
            oldBitmap = Win32.SelectObject(memDc, hBitmap)

            Dim size As New Win32.Size(bitmap.Width, bitmap.Height)
            Dim pointSource As New Win32.Point(0, 0)
            Dim topPos As New Win32.Point(Left, Top)
            Dim blend As New Win32.BLENDFUNCTION With {
                .BlendOp = Win32.AC_SRC_OVER,
                .BlendFlags = 0,
                .SourceConstantAlpha = opacity,
                .AlphaFormat = Win32.AC_SRC_ALPHA
            }

            Win32.UpdateLayeredWindow(Handle, screenDc, topPos, size, memDc, pointSource,
            0, blend, Win32.ULW_ALPHA)
        Finally
            Win32.ReleaseDC(IntPtr.Zero, screenDc)
            If hBitmap <> IntPtr.Zero Then
                Win32.SelectObject(memDc, oldBitmap)
                Win32.DeleteObject(hBitmap)
            End If
            Win32.DeleteDC(memDc)
        End Try
        bitmap.Dispose()
    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation() As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overloads Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim WS_EX_TOPMOST As Integer = &H8&
            Dim WS_EX_NOACTIVATE As Integer = &H8000000

            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or 524288 'Or WS_EX_NOACTIVATE Or WS_EX_TOPMOST
            Return cp
        End Get
    End Property

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        Me.ShowInTaskbar = False
        Me.ShowIcon = False
    End Sub
End Class

' class that exposes needed win32 gdi functions. 
Class Win32
    Public Enum Bool
        [False] = 0
        [True]
    End Enum

    <StructLayout(LayoutKind.Sequential)>
    Public Structure Point
        Public x As Int32
        Public y As Int32

        Public Sub New(ByVal x As Int32, ByVal y As Int32)
            Me.x = x
            Me.y = y
        End Sub
    End Structure


    <StructLayout(LayoutKind.Sequential)>
    Public Structure Size
        Public cx As Int32
        Public cy As Int32

        Public Sub New(ByVal cx As Int32, ByVal cy As Int32)
            Me.cx = cx
            Me.cy = cy
        End Sub
    End Structure


    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Private Structure ARGB
        Public Blue As Byte
        Public Green As Byte
        Public Red As Byte
        Public Alpha As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Public Structure BLENDFUNCTION
        Public BlendOp As Byte
        Public BlendFlags As Byte
        Public SourceConstantAlpha As Byte
        Public AlphaFormat As Byte
    End Structure


    Public Const ULW_COLORKEY As Int32 = 1
    Public Const ULW_ALPHA As Int32 = 2
    Public Const ULW_OPAQUE As Int32 = 4

    Public Const AC_SRC_OVER As Byte = 0
    Public Const AC_SRC_ALPHA As Byte = 1


    Public Declare Auto Function UpdateLayeredWindow Lib "user32.dll" (ByVal hwnd As IntPtr, ByVal hdcDst As IntPtr, ByRef pptDst As Point, ByRef psize As Size, ByVal hdcSrc As IntPtr, ByRef pprSrc As Point,
    ByVal crKey As Int32, ByRef pblend As BLENDFUNCTION, ByVal dwFlags As Int32) As Bool

    Public Declare Auto Function GetDC Lib "user32.dll" (ByVal hWnd As IntPtr) As IntPtr

    <DllImport("user32.dll", ExactSpelling:=True)>
    Public Shared Function ReleaseDC(ByVal hWnd As IntPtr, ByVal hDC As IntPtr) As Integer
    End Function

    Public Declare Auto Function CreateCompatibleDC Lib "gdi32.dll" (ByVal hDC As IntPtr) As IntPtr

    Public Declare Auto Function DeleteDC Lib "gdi32.dll" (ByVal hdc As IntPtr) As Bool

    <DllImport("gdi32.dll", ExactSpelling:=True)>
    Public Shared Function SelectObject(ByVal hDC As IntPtr, ByVal hObject As IntPtr) As IntPtr
    End Function

    Public Declare Auto Function DeleteObject Lib "gdi32.dll" (ByVal hObject As IntPtr) As Bool
End Class
