Public Class frmStatus
    Private ImageIndex As Integer = 0
    Private MyScreenImages As New List(Of Image)
    Private MyInnerForm As New frmInnerStatus

    Public Sub New(Args As Dictionary(Of String, Object))

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Me.SetBitmap(My.Resources.StatusBackground)

        Me.StartPosition = FormStartPosition.Manual

        With MyInnerForm
            .StartPosition = FormStartPosition.Manual
            .Location = New Point(0, -10000)
            .StatusForm = Me

            If Not Args Is Nothing Then
                If Args.ContainsKey("Title") Then .Header = Args("Title")
                If Args.ContainsKey("Status") Then .Footer = Args("Status")
            End If

        End With

    End Sub

    Public Property Header As String
        Get
            Return Me.MyInnerForm.Header
        End Get
        Set(value As String)
            Me.MyInnerForm.Header = value
        End Set
    End Property

    Public Property Footer As String
        Get
            Return MyInnerForm.Footer
        End Get
        Set(value As String)
            MyInnerForm.Footer = value
        End Set
    End Property


    Public Sub HideStatusForm()
        If Me.InvokeRequired Then
            Me.Invoke(Sub()
                          HideStatusForm()
                      End Sub)
            Return
        End If

        Me.Top = Integer.MinValue

        SetDefaultText()
        System.Windows.Forms.Application.UseWaitCursor = False
    End Sub

    Private Sub SetDefaultText()
        Me.Header = "Een ogenblik geduld"
        Me.Footer = "Bezig met laden"
    End Sub

    Public Sub ShowStatusForm()
        If Me.InvokeRequired Then
            Me.Invoke(Sub()
                          ShowStatusForm()
                      End Sub)
            Return
        End If

        SetLocation()
    End Sub

    Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
        Get
            Dim WS_EX_TOPMOST As Integer = &H8&
            Dim WS_EX_NOACTIVATE As Integer = &H8000000

            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_NOACTIVATE Or WS_EX_TOPMOST

            Return cp
        End Get
    End Property

    Private Sub SetBackgroundBitmap(ImageIndex As Integer)
        Dim MyBitmap As Bitmap

        If ImageIndex > MyScreenImages.Count - 1 Then
            MyBitmap = GetBitmap(Nothing)
        Else
            MyBitmap = GetBitmap(MyScreenImages(ImageIndex))
        End If

        AlphaForm.SetBitmap(MyBitmap)

        MyBitmap.Dispose()
    End Sub

    Private Sub SetLocation()

        If Me.Top <> -32768 Then Return

        Threading.Thread.Sleep(200)

        If Me.Width <> 630 Then Me.Width = 630
        If Me.Height <> 410 Then Me.Height = 410

        Me.StartPosition = FormStartPosition.Manual

        Dim MyRect As Rectangle = Functions.ActiveScreen.Bounds
        Dim X As Integer = MyRect.X + (MyRect.Width / 2) - (Me.Width / 2)
        Dim Y As Integer = MyRect.Y + (MyRect.Height / 2) - (Me.Height / 2)

        Me.Location = New Point(X, Y)

        System.Windows.Forms.Application.UseWaitCursor = True
    End Sub

    Private Sub frmSplash_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try

        Catch ex As Exception

        End Try
    End Sub

    Private Sub SetInnerFormPosition()

        Dim MyLocation As New Point(Me.Location.X, Me.Location.Y)
        Dim MySize As New Size(Me.Width, Me.Height)

        Dim iMargin As Integer = 120

        MySize.Width = 443
        MySize.Height = 288

        MyLocation.X += 38
        MyLocation.Y += 28

        'MyLocation = New Point(450, 250)

        With MyInnerForm
            .BackColor = Color.White
            .Size = MySize
            .Location = MyLocation
        End With
    End Sub

    Private bLoadingCompleted As Boolean

    Public Function isLoadingCompleted() As Boolean
        Return bLoadingCompleted
    End Function

    Public Sub LoadingCompleted()
        If Me.InvokeRequired Then
            Me.Invoke(Sub()
                          Me.LoadingCompleted()
                      End Sub)
            Return
        End If

        SetLocation()

        bLoadingCompleted = True
        'Dim sfooter As String

        'sfooter = "First Line: Some Footer Text <br> SecondLine: Some Footer Text <br>"

        'MyInnerForm.UpdateFooterText(sfooter)
    End Sub

    Private Sub frmStatus_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Location = New Point(0, Integer.MinValue)
    End Sub

    Private Sub frmStatus_LocationChanged(sender As Object, e As EventArgs) Handles Me.LocationChanged
        SetInnerFormPosition()
    End Sub

    Public Function GetBitmap(ScreenImage As Image) As Bitmap
        Return GetBitmapNew(ScreenImage)
    End Function

    Public Function GetBitmapNew(ScreenImage As Image) As Bitmap
        Dim bmpResult As New Bitmap(630, 410, Imaging.PixelFormat.Format32bppArgb)
        Dim MySplashImage As Image
        Dim G As Graphics = Graphics.FromImage(bmpResult)

        G.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
        G.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        MySplashImage = My.Resources.StatusBackground

        If Not ScreenImage Is Nothing Then
            G.DrawImageUnscaled(ScreenImage, 206, 232, ScreenImage.Width, ScreenImage.Height)
        End If

        G.DrawImageUnscaled(MySplashImage, 0, 0, MySplashImage.Width, MySplashImage.Height)

        G.Dispose()

        Return bmpResult
    End Function

    'Protected Overrides Sub WndProc(ByRef m As Message)

    '    Select Case m.Msg
    '        Case Functions.Messages.WM_MOUSEACTIVATE
    '            Return
    '        Case Functions.Messages.WM_LBUTTONDOWN
    '            Return
    '    End Select

    '    MyBase.WndProc(m)
    'End Sub

    Private Sub frmStatus_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        MyInnerForm.Show(Me)
    End Sub
End Class