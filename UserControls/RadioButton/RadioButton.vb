Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.RadioButton))>
Public Class RadioButton

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Public Property TooltipText As String = ""
    Private WithEvents MyLinkLabel As MCSCTRLS1.LinkLabel
    Private MyPaddingPanel As Panel
    Private MyLink As String = ""
    Private iMarginRight As Integer = 20
    Private bCheckedOnMouseDown As Boolean

    Public Event CheckedByUser(Sender As MCSCTRLS1.RadioButton)

    Private Sub RadioButton_MouseEnter(sender As Object, e As System.EventArgs) Handles Me.MouseEnter
        If Me.TooltipText.Trim = "" Then Return
        MCSCTRLS2.EasyTooltip.SetTooltip("Info", Me.TooltipText)
    End Sub

    Private Sub RadioButton_MouseLeave(sender As Object, e As System.EventArgs) Handles Me.MouseLeave
        MCSCTRLS2.EasyTooltip.HideTooltip()
    End Sub

    Private Sub RadioButton_MouseHover(sender As Object, e As System.EventArgs) Handles Me.MouseHover
        If Me.TooltipText.Trim = "" Then Return
        MCSCTRLS2.EasyTooltip.ShowTooltip()
    End Sub

    Public Event LinkClicked(URL As String)
    Public Property Deselectable As Boolean = False

    Public Property Link As String
        Get
            Return MyLink
        End Get
        Set(value As String)
            MyLink = value
            CheckLink()
        End Set
    End Property

    Private Sub CheckLink()
        If Link = "" Then
            If Not MyLinkLabel Is Nothing Then
                Me.Controls.Remove(MyLinkLabel)
                MyLinkLabel.Dispose()
                MyLinkLabel = Nothing
            End If
            Return
        End If

        If MyLinkLabel Is Nothing Then
            MyLinkLabel = New MCSCTRLS1.LinkLabel
            Me.Controls.Add(MyLinkLabel)
        End If

        With MyLinkLabel
            MyLinkLabel.Text = "(?)"
            MyLinkLabel.URL = Link
            .AutoSize = True
            .Dock = DockStyle.None
            .Location = New Point(OriginalCheckBoxWidth, 2)
            .Height = Me.Height
            .Anchor = AnchorStyles.Left
            .TextAlign = ContentAlignment.MiddleCenter
        End With

        If MyPaddingPanel Is Nothing Then
            MyPaddingPanel = New Panel
            Me.Controls.Add(MyPaddingPanel)
            MyPaddingPanel.Dock = DockStyle.Right
            MyPaddingPanel.BackColor = Color.Transparent
        End If

        MyPaddingPanel.Width = Me.MarginAfterLink

        CheckSize()
    End Sub

    Public Property MarginAfterLink As Integer
        Get
            Return iMarginRight
        End Get
        Set(value As Integer)
            iMarginRight = value
            CheckSize()
        End Set
    End Property

    Private Function OriginalCheckBoxWidth() As Integer
        Return FrameworkNS.Functions.GDI.TextSize(Me.Text, Me.Font).Width + 16
    End Function

    Public Function AutoSizeWidth() As Integer
        Dim iWidth As Integer

        iWidth = OriginalCheckBoxWidth()

        If Not MyLinkLabel Is Nothing Then
            iWidth += MyLinkLabel.Width
        End If

        If Not MyPaddingPanel Is Nothing Then
            iWidth += MyPaddingPanel.Width
        End If

        Return iWidth
    End Function

    Private Sub CheckSize()
        Dim bOrgAutosize As Boolean = Me.AutoSize
        Me.AutoSize = False
        Me.Width = AutoSizeWidth()
        Me.AutoSize = bOrgAutosize
    End Sub

    Private Sub MyLinkLabel_Click(sender As Object, e As System.EventArgs) Handles MyLinkLabel.Click
        Dim MyURL As String = MyLinkLabel.URL
        If MyURL = "" Then Return
        RaiseEvent LinkClicked(MyURL)
    End Sub

    Private Sub CheckBox_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        If Not MyLinkLabel Is Nothing Then
            MyLinkLabel.Height = Me.Height
        End If
    End Sub

    Private Sub RadioButton_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
        bCheckedOnMouseDown = Me.Checked
    End Sub

    Private Sub RadioButton_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        If bCheckedOnMouseDown AndAlso Me.Deselectable Then Me.Checked = False
    End Sub

    Private Sub RadioButton_CheckedChanged(sender As Object, e As EventArgs) Handles Me.CheckedChanged
        If Me.Checked Then RaiseEvent CheckedByUser(Me)
        Me.NotifyValueChangedReceiver("CheckedChanged")
    End Sub

    Public Shadows Event Click(sender As Object, e As EventArgs)

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        RaiseEvent Click(Me, e)
    End Sub


End Class
