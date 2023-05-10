Imports MCS_Interfaces
Imports System.Windows.Forms

<ToolboxBitmap(GetType(System.Windows.Forms.CheckBox))>
Public Class CheckBox

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    Private WithEvents MyLinkLabel As MCSCTRLS1.LinkLabel
    Private MyPaddingPanel As Panel
    Private MyLink As String = ""

    <System.ComponentModel.Description("Giving a checkbox a group will let it act like a radiobutton, disabling other checkboxes with in the same group.")>
    Public Property Group As String = ""
    Public Property TooltipText As String = ""
    Public Property TooltipTitle As String = "Info"
    Public Property TooltipShowImmediately As Boolean = False

    Public Event OnCheck(Sender As CheckBox)
    Public Event OnUnCheck(Sender As CheckBox)
    Public Event CheckedChangedByUserClick(Sender As MCSCTRLS1.CheckBox, Checked As Boolean)
    Public Event LinkClicked(URL As String)

    Private Sub Checkbox_MouseEnter(sender As Object, e As System.EventArgs) Handles Me.MouseEnter
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return

        MCSCTRLS2.EasyTooltip.SetTooltip(TooltipTitle, Me.TooltipText)
    End Sub

    Private sRememberUserStateKey As String = ""

    Public Property RememberUserStateKey As String
        Get
            Return sRememberUserStateKey
        End Get
        Set(value As String)
            If sRememberUserStateKey.isEqualTo(value) Then Return
            sRememberUserStateKey = value
            LoadUserState()
        End Set
    End Property

    Private Sub LoadUserState()
        Dim sValue As String = EasyCare.FrameWork.GetUserProp("CHEKBOX_" & Me.RememberUserStateKey, "NOTSET")
        Dim bNewValue As Boolean

        Select Case sValue
            Case "NOTSET"
                Return

            Case "0"
                bNewValue = False

            Case "1"
                bNewValue = True

        End Select

        If Me.Checked = bNewValue Then Return

        Me.Checked = bNewValue

    End Sub

    Private Sub Checkbox_MouseLeave(sender As Object, e As System.EventArgs) Handles Me.MouseLeave
        MCSCTRLS2.EasyTooltip.HideTooltip()
    End Sub

    Private Sub CheckBox_MouseHover(sender As Object, e As System.EventArgs) Handles Me.MouseHover
        If Me.TooltipText.isEmptyOrWhiteSpace Then Return
        MCSCTRLS2.EasyTooltip.ShowTooltip(TooltipShowImmediately)
    End Sub


    Public Property Link As String
        Get
            Return MyLink
        End Get
        Set(value As String)
            MyLink = value
            CheckLink()
        End Set
    End Property

    Public Sub SetCheckedWithoutRaisingEvents(Value As Boolean)
        bDontRaiseEvent = True
        Me.Checked = Value
        bDontRaiseEvent = False
    End Sub

    Private bDontRaiseEvent As Boolean

    Public Shadows Event CheckedChanged(sender As Object, e As EventArgs)

    Private Sub CheckBox_CheckedChanged(sender As Object, e As EventArgs) Handles MyBase.CheckedChanged
        If Me.Checked AndAlso Me.Group <> "" Then UncheckOtherGroupMembers()

        If bDontRaiseEvent Then Return

        RaiseEvent CheckedChanged(sender, e)

        If Me.Checked Then
            RaiseEvent OnCheck(Me)
        Else
            RaiseEvent OnUnCheck(Me)
        End If

    End Sub

    <System.ComponentModel.Description("Returns true if one of the members of the group is checked. Otherwise false.")>
    Public Function ItemCheckedInGroup() As Boolean
        If Me.Checked = True Then Return True

        Dim MyForm As Form
        Dim MyCheckboxes As List(Of MCSCTRLS1.CheckBox)

        MyForm = FrameworkNS.Functions.Forms.FindMostParentForm(Me)
        MyCheckboxes = FrameworkNS.Functions.Forms.AllControls(Of MCSCTRLS1.CheckBox)(MyForm)

        For Each MyCheckbox As MCSCTRLS1.CheckBox In MyCheckboxes
            If Not MyCheckbox.Group.ToLower.Trim = Me.Group.ToLower.Trim Then Continue For
            If MyCheckbox.Checked Then Return True
        Next

        Return False
    End Function

    Private Sub UncheckOtherGroupMembers()
        If Me.Group.Trim = "" Then Return

        Dim MyForm As Form
        Dim MyCheckboxes As List(Of MCSCTRLS1.CheckBox)

        MyForm = FrameworkNS.Functions.Forms.FindMostParentForm(Me)
        MyCheckboxes = FrameWorkNS.Functions.Forms.AllControls(Of MCSCTRLS1.CheckBox)(MyForm)

        For Each MyCheckbox As MCSCTRLS1.CheckBox In MyCheckboxes
            If MyCheckbox Is Me Then Continue For

            If MyCheckbox.Group.ToLower.Trim = Me.Group.ToLower.Trim Then
                MyCheckbox.Checked = False
            End If
        Next
    End Sub

    Private Sub CheckLink()
        If Link = "" Then
            If Not MyLinkLabel Is Nothing Then
                Me.Controls.Clear()
                MyLinkLabel.Dispose()
                MyPaddingPanel.Dispose()
                MyLinkLabel = Nothing
                MyPaddingPanel = Nothing
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
            .TextAlign = ContentAlignment.BottomCenter
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

    Private iMarginRight As Integer = 20

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
        Return FrameworkNS.Functions.GDI.TextSize(Me.Text, Me.Font).Width + 20
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
        If Me.Link.isEmptyOrWhiteSpace Then Return

        Dim bOrgAutoSize As Boolean = Me.AutoSize
        Me.AutoSize = False
        Me.Width = AutoSizeWidth()
        Me.AutoSize = bOrgAutoSize
    End Sub

    Private Sub MyLinkLabel_Click(sender As Object, e As System.EventArgs) Handles MyLinkLabel.Click
        Dim MyURL As String = MyLinkLabel.URL
        If MyURL = "" Then Return
        RaiseEvent LinkClicked(MyURL)
    End Sub

    Private Sub CheckBox_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        If MyLinkLabel Is Nothing Then Return

        MyLinkLabel.Height = Me.Height

    End Sub

    Public Event BeforeCheckedChangedByUserClick(Sender As MCSCTRLS1.CheckBox, NewValue As Boolean, ByRef Cancel As Boolean)

    Private bRestoreAutoCheck As Boolean = False

    Protected Overrides Sub OnClick(e As EventArgs)
        Dim bCancel As Boolean = False
        RaiseEvent BeforeCheckedChangedByUserClick(Me, Not Me.Checked, bCancel)

        If bCancel Then Return

        MyBase.OnClick(e)
        RaiseEvent Click(Me, e)
    End Sub

    Public Shadows Event Click(sender As Object, e As EventArgs)

    Private Sub CheckBox_Click(sender As Object, e As EventArgs) Handles Me.Click
        RaiseEvent CheckedChangedByUserClick(Me, Me.Checked)

        Me.NotifyValueChangedReceiver("CHECKEDCHANGED_BY_USER")

        If Not Me.RememberUserStateKey.isEmptyOrWhiteSpace Then
            EasyCare.FrameWork.SaveUserProp("CHEKBOX_" & Me.RememberUserStateKey, If(Me.Checked, "1", "0"))
        End If

    End Sub

End Class
