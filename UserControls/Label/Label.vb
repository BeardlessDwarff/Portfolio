Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.Label))>
Public Class Label
    Private MyFramework As FrameWorkNS.FrameWork = EasyCare.FrameWork

    Public Property AutoShrinkFontSize As Integer = 0

    Public Property NoWrap As Boolean = False

    Public Function MaxLengthText() As String
        Dim G As Graphics = Me.CreateGraphics
        Dim Result As String

        Result = G.GiveTruncatedText(Me.ClientRectangle.Width, Me.Text, Me.Font)

        G.Dispose()

        Return Result
    End Function

    Public Overrides Property Text() As String
        Get
            Return MyBase.Text
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then value = ""

            value = value.CorrigeerSpelfouten



            If Debugger.IsAttached Then
                If value.ToLower.StartsWith("dhr") Then GoTo Doorgaan
                If value.ToLower.StartsWith("mevr") Then GoTo Doorgaan

                If value.ToLower.Contains("allergiën") Then Stop 'allergieën.
                If value.ToLower.Contains("patient") Then Stop 'patiënt.
            End If

Doorgaan:
            Try
                value = MyFramework.GeefVertaling(value)
            Catch ex As Exception

            End Try

            MyBase.Text = value
            If AutoShrinkFontSize > 0 Then SetMaxFontSize()
        End Set
    End Property

    Private Sub Label_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        MyFramework = Nothing
    End Sub

    Public Overrides Property Font As System.Drawing.Font
        Get
            Return MyBase.Font
        End Get
        Set(value As System.Drawing.Font)
            MyBase.Font = value
        End Set
    End Property

    Private Sub SetMaxFontSize()
        If Me.NoWrap Then Return 'Er is al voor gekozen om de tekst af te breken...

        Dim G As Graphics = Me.CreateGraphics

        '   If iDefaultFontsize = 0 Then iDefaultFontsize = Me.Font.Size

        Dim iFontSize As Integer = AutoShrinkFontSize
        Dim MyFont As New Font(Me.Font.FontFamily, iFontSize, Me.Font.Style)

        Do While G.MeasureString(Text, MyFont).Width > Me.ClientRectangle.Width
            iFontSize -= 1
            MyFont = New Font(Me.Font.FontFamily, iFontSize, Me.Font.Style)
            If iFontSize = 1 Then Exit Do
        Loop

        G.Dispose()

        bDontRememberFontSize = True
        Me.Font = MyFont
        bDontRememberFontSize = False
    End Sub

    Private bDontRememberFontSize As Boolean

    Private Sub Label_SizeChanged(sender As Object, e As System.EventArgs) Handles Me.SizeChanged
        If AutoShrinkFontSize > 0 Then SetMaxFontSize()
    End Sub

    Public Property TooltipText As String = ""
    Public Property TooltipTitle As String = ""

    Private Sub Label_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        MCSCTRLS2.EasyTooltip.ShowTooltip(True)
    End Sub

    Private Sub Label_MouseLeave(sender As Object, e As EventArgs) Handles Me.MouseLeave
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        MCSCTRLS2.EasyTooltip.HideTooltip()
    End Sub

    Private Sub Label_MouseEnter(sender As Object, e As EventArgs) Handles Me.MouseEnter
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        Dim sTitle As String

        sTitle = IIf(Me.TooltipTitle.isEmptyOrWhiteSpace, "Info", Me.TooltipTitle)

        MCSCTRLS2.EasyTooltip.SetTooltip(sTitle, Me.TooltipText)
    End Sub

    Private bBusyPainting As Boolean = False

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If Me.NoWrap Then
            Dim G As Graphics = e.Graphics
            Dim S As String

            S = G.GiveTruncatedText(Me.ClientRectangle.Width, Me.Text, Me.Font)

            If S.isEqualTo(Me.Text) Then
                MyBase.OnPaint(e)
            Else
                If Me.TooltipText.isEmptyOrWhiteSpace Then Me.TooltipText = Me.Text

                G.Clear(Me.BackColor)
                Dim MyBrush As New SolidBrush(Me.ForeColor)
                G.DrawString(S, Me.Font, MyBrush, Me.ClientRectangle, Me.TextAlign.HorizontalStringAlignment, Me.TextAlign.VerticalStringAlignment)

                MyBrush.Dispose()
            End If
        Else
            MyBase.OnPaint(e)
        End If
    End Sub

    Public Shadows Event Click(sender As Object, e As EventArgs)

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        RaiseEvent Click(Me, e)
    End Sub


End Class
