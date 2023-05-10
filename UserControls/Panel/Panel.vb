Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.Panel))> ', DebuggerNonUserCode()>
Public Class Panel

    Private iCornerRadius As Integer = 20
    Private MyImage As Image
    Private MyStyle As eStyle = eStyle.Normal
    Private MyStartColor As Color = Color.Blue
    Private MyEndColor As Color = Color.WhiteSmoke
    Private myRoundCorners As Corner = Corner.TRBL
    Private MyImagePlace As eImagePlace = eImagePlace.RechtOnder
    Private MyAlpha As Integer = 200

    Public Event FormShownInPanel(Form As Form)
    Public Event ScrolledToBottom()

    Public Property ShowAnimation As ePanelAnimation = ePanelAnimation.None
    Public Property HideAnimation As ePanelAnimation = ePanelAnimation.None
    Public Property AutoScrollToActivatedControl As Boolean = True

    Public Property AnimationDuration As Integer = 150

    Public Sub RaiseFormShownInPanel(Form As Form)
        RaiseEvent FormShownInPanel(Form)
    End Sub

    Public Enum ePanelAnimation
        None
        SlideUP
        SlideLeft
        SlideRight
        SlideDown
        Center
    End Enum

    Private iLastScrollValue As Integer = 0

    Public Function ContentHeight() As Integer
        Dim iResult As Integer = 0

        For Each C As Control In Me.Controls
            iResult += C.Height
        Next

        Return iResult
    End Function

    'Protected Overrides Sub OnResize(eventargs As EventArgs)
    '    Me.SuspendRedraw
    '    Me.Visible = False
    '    MyBase.OnResize(eventargs)
    '    Me.Visible = True
    '    Me.ResumeRedraw
    'End Sub

    Private Sub CheckScrolledToBottom() Handles Me.Scroll, Me.MouseWheel
        If Not Me.VerticalScroll.Visible Then Return

        If Me.VerticalScroll.Value = iLastScrollValue Then Return
        iLastScrollValue = Me.VerticalScroll.Value

        If iLastScrollValue = (Me.VerticalScroll.Maximum - Me.VerticalScroll.LargeChange + 1) Then
            RaiseEvent ScrolledToBottom()
        End If
    End Sub

    Public Shadows Property Visible As Boolean
        Get
            Return MyBase.Visible
        End Get
        Set(value As Boolean)
            If MyBase.Visible = value Then
                MyBase.Visible = value
                Return 'Opmerking van Remco: aangepast met Erik op 31-5-2018. Rare code, maar anders worden panel niet meer zichtbaar.
            End If

            Dim MyLabeledPanel As MCSCTRLS2.LabeledPanel = Nothing

            If TypeOf Me.Parent Is System.Windows.Forms.Panel Then
                If Not Parent.Parent Is Nothing Then
                    If Not Parent.Parent.Parent Is Nothing Then
                        If TypeOf Parent.Parent.Parent Is MCSCTRLS2.LabeledPanel Then
                            MyLabeledPanel = DirectCast(Parent.Parent.Parent, MCSCTRLS2.LabeledPanel)
                        End If
                    End If
                End If
            End If

            If MyLabeledPanel Is Nothing OrElse Not MyLabeledPanel.AutoSize OrElse Not MyLabeledPanel.Expanded Then
                MyBase.Visible = value
                Return
            End If

            Dim iHeight As Integer = Me.Height 'Onthoud de oorspronkelijke hoogte.

            If value Then
                Me.Height = 0
                MyBase.Visible = True
                Me.AnimateToHeight(MyLabeledPanel, iHeight)
            Else
                Me.AnimateToHeight(MyLabeledPanel, 0)
                MyBase.Visible = False
            End If

            Me.Height = iHeight
        End Set
    End Property

    Private Sub AnimateToHeight(LabeledPanel As MCSCTRLS2.LabeledPanel, Height As Integer)
        Dim MyAnimationTimer As New MCSCTRLS2.AnimationTimer
        Dim iStartHeight As Integer = Me.Height
        Dim iCurrentHeight As Integer
        Dim IlabeledPanelStartHeight As Integer = LabeledPanel.Height
        Dim iVerschil As Integer
        MyAnimationTimer.AlwaysAnimate = True
        MyAnimationTimer.Duration = 250
        MyAnimationTimer.AddAnimationValue("Height", Me.Height, Height)
        MyAnimationTimer.RunAsync()
        Do While MyAnimationTimer.isRunning
            iCurrentHeight = MyAnimationTimer.GetValue("Height")
            iVerschil = iStartHeight - iCurrentHeight
            If iVerschil <> 0 Then
                LabeledPanel.Height = IlabeledPanelStartHeight - iVerschil
                'Stop
            End If
            Me.Height = iCurrentHeight
            If Me.Height = Height Then Exit Do
            Me.Refresh()
            Application.DoEvents()
        Loop
        If Me.Height <> Height Then
            iVerschil = Me.Height - Height
            LabeledPanel.Height -= iVerschil
            Me.Height = Height
        End If
    End Sub
    Protected Overrides Function ScrollToControl(activeControl As System.Windows.Forms.Control) As System.Drawing.Point
        If AutoScrollToActivatedControl Then
            Return MyBase.ScrollToControl(activeControl)
        Else
            Dim MyPoint As New Point(-Me.HorizontalScroll.Value, -Me.VerticalScroll.Value)
            Return MyPoint
        End If
    End Function

    Private MyGradientMode As LinearGradientMode = LinearGradientMode.Vertical

    Public Sub AnimateTo(X As Integer, Y As Integer, Width As Integer, Height As Integer, Optional Duration As Integer = 250)
        FrameworkNS.Functions.Forms.AnimateTo(Me, X, Y, Width, Height, Duration)
    End Sub

    Public Sub MovePanel(X As Integer, Y As Integer, Width As Integer, Height As Integer)
        FrameworkNS.Functions.Forms.Move(Me.Handle, X, Y, Width, Height)
    End Sub

    Public Sub Sort(Sorter As IComparer(Of Control))
        FrameworkNS.Functions.API.LockWindowUpdate(Me.Handle)
        Dim MyControls(Me.Controls.Count - 1) As Control
        Me.Controls.CopyTo(MyControls, 0)
        Me.Controls.Clear()
        Array.Sort(MyControls, Sorter)
        Me.Controls.AddRange(MyControls)
        FrameworkNS.Functions.API.LockWindowUpdate(IntPtr.Zero)
    End Sub

    Public Function ToBitmap() As Bitmap
        Return FrameworkNS.Functions.GDI.PanelToImage(Me)
    End Function

    Public Sub BeginPrint()
        Bmp = Me.ToBitmap
        BitmapY = 0
    End Sub

    Private Bmp As Bitmap
    Private BitmapY As Integer

    Public Function Print(G As Graphics, Rect As Rectangle, Optional ByRef CurrentY As Integer = 0) As Boolean

        If Bmp.Width > Rect.Width Then
            Dim iNewHeight As Integer
            iNewHeight = (Rect.Width * Bmp.Height) / Bmp.Width
            Bmp = New Bitmap(Bmp, Rect.Width, iNewHeight)
        End If

        Dim srcRect As New Rectangle(0, BitmapY, Rect.Width, Rect.Height)
        Dim iWitteLijn As Integer

        If (Bmp.Height - BitmapY) <= srcRect.Height Then
            srcRect.Height = Bmp.Height - BitmapY
        End If

        If Rect.Height > srcRect.Height Then
            Rect.Height = srcRect.Height
        Else
            'Het past dus niet...
            'Zoek naar een goed punt om de bitmap te knippen (witte lijn).
            iWitteLijn = ZoekWitteLijnInBitmap(Bmp, srcRect.Y + srcRect.Height)

            If iWitteLijn > 0 Then
                srcRect.Height = (iWitteLijn - 2) - BitmapY
            End If
        End If

        G.DrawImage(Bmp, Rect, srcRect, GraphicsUnit.Pixel)

        BitmapY = srcRect.Y + srcRect.Height

        CurrentY = BitmapY

        If BitmapY >= Bmp.Height Then Return False

        Return True

    End Function

    Public Sub EndPrint()
        Bmp.Dispose()
        Bmp = Nothing
        BitmapY = 0
    End Sub

    Private Function ZoekWitteLijnInBitmap(bmp As Bitmap, BovenY As Integer) As Integer
        For LijnIndex As Integer = BovenY To 0 Step -1
            For X As Integer = 0 To bmp.Width - 1
                With bmp.GetPixel(X, LijnIndex)
                    If .R <> 255 Then GoTo NextLine
                    If .G <> 255 Then GoTo NextLine
                    If .B <> 255 Then GoTo NextLine
                End With
            Next

            Return LijnIndex
NextLine:
        Next
    End Function

    <DebuggerNonUserCode()>
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Select Case m.Msg
            Case FrameworkNS.Functions.API.Messages.WM_ERASEBKGND
                Return 'Misschien wat sneller...
        End Select

        MyBase.WndProc(m)
    End Sub

    Public Property GradientMode() As LinearGradientMode
        Get
            Return MyGradientMode
        End Get
        Set(ByVal value As LinearGradientMode)
            MyGradientMode = value
            Me.Invalidate()
        End Set
    End Property

    Protected Overrides Sub SetVisibleCore(value As Boolean)
        If value Then
            Select Case ShowAnimation
                Case ePanelAnimation.None

                Case ePanelAnimation.SlideUP
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.VerticalNegative, AnimationDuration)

                Case ePanelAnimation.SlideLeft
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.HorizontalNegative, AnimationDuration)

                Case ePanelAnimation.SlideDown
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.VerticalPositive, AnimationDuration)

                Case ePanelAnimation.SlideRight
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.HorizontalPositive, AnimationDuration)

                Case ePanelAnimation.Center
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Center, AnimationDuration)

            End Select
        Else
            Select Case HideAnimation
                Case ePanelAnimation.None

                Case ePanelAnimation.SlideLeft
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.HorizontalNegative Or FrameworkNS.Functions.API.AnimationFlags.Hide, AnimationDuration)

                Case ePanelAnimation.SlideRight
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.HorizontalPositive Or FrameworkNS.Functions.API.AnimationFlags.Hide, AnimationDuration)

                Case ePanelAnimation.SlideDown
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.VerticalPositive Or FrameworkNS.Functions.API.AnimationFlags.Hide, AnimationDuration)

                Case ePanelAnimation.SlideUP
                    FrameworkNS.Functions.API.AnimateWindow(Me.Handle, FrameworkNS.Functions.API.AnimationFlags.Slide Or FrameworkNS.Functions.API.AnimationFlags.VerticalNegative Or FrameworkNS.Functions.API.AnimationFlags.Hide, AnimationDuration)
            End Select
        End If

        MyBase.SetVisibleCore(value)
    End Sub

    '"Windows Form Designer generated code removed here"

    '===================================================================
    ' for NativeWindow and PostMessageA
    '===================================================================
    Private Const WM_HSCROLL = &H114
    Private Const WM_VSCROLL = &H115
    Private Const WM_MOUSEWHEEL = &H20A
    Private Const WM_COMMAND = &H111
    Private Const WM_USER = &H400
    '===================================================================
    ' for GetScroll and PostMessageA
    '===================================================================
    Private Const SBS_HORZ = 0
    Private Const SBS_VERT = 1
    Private Const SB_THUMBPOSITION = 4
    '===================================================================
    ' for SubClassing
    '===================================================================

    Public Property Alpha() As Integer
        Get
            Return MyAlpha
        End Get
        Set(ByVal value As Integer)
            MyAlpha = value
            Me.Invalidate()
        End Set
    End Property

    Public Overloads Property BorderStyle() As System.Windows.Forms.BorderStyle
        Get
            Return MyBase.BorderStyle
        End Get
        Set(ByVal value As BorderStyle)
            MyBase.BorderStyle = value
            Me.Invalidate()
        End Set
    End Property

    <DebuggerNonUserCode>
    Public Property Style() As eStyle
        Get
            Return MyStyle
        End Get
        Set(ByVal value As eStyle)
            MyStyle = value
            Me.SetStyleGoed()
        End Set
    End Property

    Public Property CornerRadius() As Integer
        Get
            Return iCornerRadius
        End Get
        Set(ByVal value As Integer)
            iCornerRadius = value
            Me.Invalidate()
        End Set
    End Property

    Enum eStyle
        Normal
        Graphical
    End Enum

    Enum eImagePlace
        LinksBoven
        RechtsBoven
        LinksOnder
        RechtOnder
    End Enum

    Public Property RoundCorners() As Corner
        Get
            Return myRoundCorners
        End Get
        Set(ByVal value As Corner)
            myRoundCorners = value
            Me.Invalidate()
        End Set
    End Property

    Public Property ImagePlace() As eImagePlace
        Get
            Return MyImagePlace
        End Get
        Set(ByVal value As eImagePlace)
            MyImagePlace = value
            Me.Invalidate()
        End Set
    End Property

    Public Property ColorStart() As Color
        Get
            Return MyStartColor
        End Get
        Set(ByVal value As Color)
            MyStartColor = value
            Me.Invalidate()
        End Set
    End Property

    Public Property ColorEnd() As Color
        Get
            Return MyEndColor
        End Get
        Set(ByVal value As Color)
            MyEndColor = value
            Me.Invalidate()
        End Set
    End Property


    Private Sub SetStyleGoed()
        Return

        Select Case MyStyle
            Case eStyle.Normal
                ''Me.SetStyle(ControlStyles.SupportsTransparentBackColor Or ControlStyles.DoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.Opaque, True)

                SetStyle(ControlStyles.AllPaintingInWmPaint, True)
                SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
                SetStyle(ControlStyles.SupportsTransparentBackColor, True)
                SetStyle(ControlStyles.ResizeRedraw, False)
                'If Not Me.Parent Is Nothing And Me.BackColor = Color.Transparent Then Me.BackColor = Me.Parent.BackColor
                'If Not Me.DesignMode Then Me.BackColor = Color.Transparent

            Case eStyle.Graphical
                SetStyle(ControlStyles.AllPaintingInWmPaint, True)
                SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
                SetStyle(ControlStyles.SupportsTransparentBackColor, True)
                SetStyle(ControlStyles.ResizeRedraw, True)
                If Not Me.DesignMode Then Me.BackColor = Color.Transparent

        End Select

        Me.Invalidate()

    End Sub

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        'SetStyleGoed()
        ''Me.SetStyle(ControlStyles.DoubleBuffer Or ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.Opaque, True)
        Me.SetStyle(ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)

    End Sub

    Public Property Image() As Image
        Get
            Return MyImage
        End Get
        Set(ByVal value As Image)
            MyImage = value
            Me.Invalidate()
        End Set
    End Property

    Public ReadOnly Property Path() As GraphicsPath
        Get
            Return DrawingHelper.RoundedRect(Me.Rect, Me.CornerRadius, Me.RoundCorners)
        End Get
    End Property

    Private ReadOnly Property Rect() As Rectangle
        Get
            Dim iMargin As Integer = 2
            Dim MyRect As New Rectangle(iMargin, iMargin, Me.Width - (iMargin * 2), Me.Height - (iMargin * 2))

            Return MyRect
        End Get
    End Property

    Private MyBackgroundText As String = ""

    Public Property BackgroundText As String
        Get
            Return MyBackgroundText
        End Get
        Set(value As String)
            MyBackgroundText = value

            If value.Trim <> "" Then
                SetStyle(ControlStyles.ResizeRedraw, True)
            Else
                If Me.Style = eStyle.Normal Then SetStyle(ControlStyles.ResizeRedraw, False)
            End If
            Me.Invalidate()
        End Set
    End Property

    <DebuggerNonUserCode>
    Private Sub TekenBackGroundText(G As Graphics)

        If Me.BackgroundText.Trim = "" Then Return
        If G Is Nothing Then Return

        Dim MyRect As Rectangle = Me.ClientRectangle
        Dim String1 As String = Me.BackgroundText
        Dim MyFont As New Font("Arial", 18)
        Dim X As Integer = 0

        G.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
        G.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        Dim iHeight1 As Integer
        Dim iWidth As Integer
        Dim Y As Integer

        iHeight1 = G.MeasureString(String1, MyFont, Me.ClientRectangle.Width).Height

        iWidth = G.MeasureString(String1, MyFont, Me.ClientRectangle.Width).Width

        X = (Me.ClientRectangle.Width / 2) - (iWidth / 2)
        Y = (Me.ClientRectangle.Height / 2) - ((iHeight1) / 2)

        G.DrawString(String1, MyFont, Brushes.Black, X, Y)

        MyFont.Destroy()
    End Sub

    <DebuggerNonUserCode>
    Private Sub mcsPanel_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If Me.Style = eStyle.Normal Then GoTo einde

        Dim MyPath As GraphicsPath
        Dim g As Graphics = e.Graphics
        Dim KLeur1 As Color
        Dim KLeur2 As Color
        Dim MyBrush As LinearGradientBrush

        If e.ClipRectangle.Width < 1 Then Return
        If e.ClipRectangle.Height < 1 Then Return
        'If Me.Style = eStyle.Normal Then
        'MyRect = New Rectangle(0, 0, Me.Width, Me.Height)
        'KLeur1 = Color.FromArgb(255, ColorStart.R, ColorStart.G, ColorStart.B)
        'KLeur2 = Color.FromArgb(255, ColorEnd.R, ColorEnd.G, ColorEnd.B)
        'MyBrush = New LinearGradientBrush(Me.Rect, KLeur1, KLeur2, LinearGradientMode.Vertical)
        'g.FillRectangle(MyBrush, MyRect)
        'Exit Sub
        'End If

        e.Graphics.CompositingQuality = CompositingQuality.HighQuality
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality
        e.Graphics.CompositingMode = CompositingMode.SourceOver
        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic
        e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit

        Dim MyColor As Color = Color.FromArgb(100, 0, 0, 255)

        KLeur1 = Color.FromArgb(Alpha, ColorStart.R, ColorStart.G, ColorStart.B)
        KLeur2 = Color.FromArgb(Alpha, ColorEnd.R, ColorEnd.G, ColorEnd.B)

        MyBrush = New LinearGradientBrush(Me.Rect, KLeur1, KLeur2, GradientMode)
        MyPath = Me.Path

        g.FillPath(MyBrush, MyPath)

        If Me.BorderStyle <> Windows.Forms.BorderStyle.None Then
            g.DrawPath(Pens.Black, MyPath)
        End If

        If Not Me.Image Is Nothing Then
            Dim X As Integer = Me.Width - 58
            Dim Y As Integer = Me.Height - 58
            Select Case Me.ImagePlace
                Case eImagePlace.LinksBoven
                    X = 0
                    Y = 0
                Case eImagePlace.LinksOnder
                    X = 0
                    Y = Me.Height - 58
                Case eImagePlace.RechtOnder
                    X = Me.Width - 58
                    Y = Me.Height - 58
                Case eImagePlace.RechtsBoven
                    X = Me.Width - 58
                    Y = 0
            End Select
            Dim imgRect As New Rectangle(X, Y, 48, 48)
            g.DrawImage(Me.Image, imgRect, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, Me.MyImageAttributes)
        End If

einde:
        TekenBackGroundText(e.Graphics)
    End Sub



    Private ReadOnly Property MyImageAttributes() As ImageAttributes
        Get
            Dim ia As New ImageAttributes
            Dim cmNormal As ColorMatrix

            cmNormal = New ColorMatrix

            cmNormal.Matrix33 = 0.9


            ia.SetColorMatrix(cmNormal)

            Return ia

        End Get
    End Property

End Class

