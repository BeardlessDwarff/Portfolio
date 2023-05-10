Imports MCS_Interfaces

Public Class frmAutoCorrectie
    Private MyWoord As String = ""
    Private ActiveItem As FrameWorkNS.TextControl.AutoCorrectieItem

    Private Sub btnCompleteLijst_Click(sender As System.Object, e As System.EventArgs) Handles btnCompleteLijst.Click
        Using MyForm As New frmAutoCorrectielijst
            MyForm.StartPosition = FormStartPosition.Manual
            MyForm.Location = Me.Location
            MyForm.Size = Me.Size
            MyForm.ShowDialog()
        End Using
    End Sub

    Public Sub New(Woord As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.cmbWoord.DropDownStyle = ComboBoxStyle.DropDown
        MyWoord = Woord
        VulFontSizeComboBox()
        VulAutocorrecties()

        Me.tsbVerwijderen.Image = FrameWorkNS.Functions.Forms.GetDefaultImage(FrameWorkNS.Functions.Forms.eDefaultImage.Delete)

        Me.cmbWoord.MaxLength = MyFramework.DataManager.MaxTextLength("mcs_autocorrectie", "Woord")
        Me.txtCorrectie.MaxLength = MyFramework.DataManager.MaxTextLength("mcs_autocorrectie", "Correctie")
    End Sub

    Private Sub VulAutocorrecties()
        Dim Item As FrameWorkNS.TextControl.AutoCorrectieItem

        Me.cmbWoord.BeginUpdate()

        For Each Item In FrameWorkNS.TextControl.AutoCorrectieItems.Values
            If Item.Nivo = FrameWorkNS.TextControl.AutoCorrectieItem.eNivo.Ziekenhuisbreed Then Continue For
            Me.cmbWoord.Items.Add(Item)
        Next

        Me.cmbWoord.DisplayMember = "Word"

        If MyWoord <> "" Then
            Item = FrameWorkNS.TextControl.GetAutocorrectieItem(MyWoord)
            If Not Item Is Nothing Then
                Me.cmbWoord.SelectedItem = Item
            Else
                Me.cmbWoord.Text = MyWoord
            End If
        End If

        Me.cmbWoord.EndUpdate()

    End Sub

    Private Sub VulFontSizeComboBox()

        cmbFontSize.Items.Clear()
        cmbFontSize.Items.Add("Normaal")
        cmbFontSize.Items.Add("6")
        cmbFontSize.Items.Add("7")
        cmbFontSize.Items.Add("8")
        cmbFontSize.Items.Add("9")
        cmbFontSize.Items.Add("10")
        cmbFontSize.Items.Add("11")
        cmbFontSize.Items.Add("12")
        cmbFontSize.Items.Add("14")
        cmbFontSize.Items.Add("16")
        cmbFontSize.Items.Add("18")
        cmbFontSize.Items.Add("20")
        cmbFontSize.Items.Add("22")
        cmbFontSize.Items.Add("24")
        cmbFontSize.Items.Add("26")
        cmbFontSize.Items.Add("28")
        cmbFontSize.Items.Add("30")
        cmbFontSize.Items.Add("36")
        cmbFontSize.Items.Add("48")
        cmbFontSize.Items.Add("60")
        cmbFontSize.Items.Add("72")

        cmbFontSize.SelectedItem = "Normaal"
    End Sub

    Private Sub CheckVerwijderen()
        If Not TypeOf Me.cmbWoord.SelectedItem Is FrameWorkNS.TextControl.AutoCorrectieItem Then
            Me.tsbVerwijderen.Enabled = False
        Else
            Me.tsbVerwijderen.Enabled = True
        End If
    End Sub

    Private Sub cmbWoord_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cmbWoord.SelectedIndexChanged
        CheckVerwijderen()

        If Not TypeOf Me.cmbWoord.SelectedItem Is FrameWorkNS.TextControl.AutoCorrectieItem Then
            Return
        End If

        Me.tsbVerwijderen.Enabled = True

        ActiveItem = Me.cmbWoord.SelectedItem
        VulBoxen()
    End Sub

    Private Sub VulBoxen()
        Me.txtCorrectie.Text = Me.ActiveItem.Correction
        Me.tsbBold.Checked = Me.ActiveItem.Bold
        Me.tsbItalic.Checked = Me.ActiveItem.Italic
        Me.tsbUnderline.Checked = Me.ActiveItem.Underlined

        If Me.cmbFontSize.Items.Contains(Me.ActiveItem.FontSize.ToString) Then
            Me.cmbFontSize.SelectedItem = Me.ActiveItem.FontSize.ToString
        Else
            Me.cmbFontSize.SelectedItem = "Normaal"
        End If
    End Sub

    Private Sub NewItem()
        ActiveItem = New FrameWorkNS.TextControl.AutoCorrectieItem(0)
        Me.cmbWoord.SelectedItem = Nothing
        Me.cmbWoord.Text = ""
        VulBoxen()
        Me.cmbWoord.Focus()
    End Sub

    Private Sub tsbNew_Click(sender As System.Object, e As System.EventArgs) Handles tsbNew.Click
        NewItem()
    End Sub

    Public Function MyFramework() As FrameWorkNS.FrameWork
        Return EasyCare.FrameWork
    End Function

    Private Sub btnOk_Click(sender As System.Object, e As System.EventArgs) Handles btnOk.Click, btnOpslaanenSluiten.Click
        If Me.ActiveItem Is Nothing Then Me.ActiveItem = New FrameWorkNS.TextControl.AutoCorrectieItem(0)

        If Me.txtCorrectie.Text = "" Then
            MyFramework.ShowNotification("U dient een correctie op te geven", "Onvoldoende gegevens")
            MCSCTRLS2.Functions.GetAttention(Me.txtCorrectie)
            Me.txtCorrectie.Focus()
            Return
        End If

        If Me.cmbWoord.Text = "" Then
            MyFramework.ShowNotification("U dient een woord op te geven", "Onvoldoende gegevens")
            MCSCTRLS2.Functions.GetAttention(Me.cmbWoord)
            Me.cmbWoord.Focus()
            Return
        End If

        Me.VulActiveItemByControls()
        Me.ActiveItem.Save()

        If sender Is btnOpslaanenSluiten Then
            Me.Close()
        Else
            Me.NewItem()
        End If
    End Sub

    Private Sub VulActiveItemByControls()
        If Me.ActiveItem Is Nothing Then Return

        Me.ActiveItem.Word = Me.cmbWoord.Text
        Me.ActiveItem.Correction = Me.txtCorrectie.Text
        Me.ActiveItem.Bold = Me.tsbBold.Checked
        Me.ActiveItem.Underlined = Me.tsbUnderline.Checked
        Me.ActiveItem.Italic = Me.tsbItalic.Checked

        If IsNumeric(Me.cmbFontSize.SelectedItem) Then
            Me.ActiveItem.FontSize = Me.cmbFontSize.SelectedItem
        Else
            Me.ActiveItem.FontSize = 0
        End If


    End Sub
    Private Sub btnCancel_Click(sender As System.Object, e As System.EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub frmAutoCorrectie_Shown(sender As Object, e As System.EventArgs) Handles Me.Shown
        Me.txtCorrectie.Focus()
        Me.txtCorrectie.SelectAll()
    End Sub

    Private Sub tsbVerwijderen_Click(sender As System.Object, e As System.EventArgs) Handles tsbVerwijderen.Click
        If MCSCTRLS2.Functions.Msgbox("Weet u zeker dat u de autocorrectie voor '" & Me.ActiveItem.Word & "' wilt verwijderen?", MsgBoxStyle.YesNo + MsgBoxStyle.Exclamation, "Verwijderen?") = MsgBoxResult.Yes Then

            If Me.cmbWoord.Items.Contains(Me.ActiveItem) Then
                Me.cmbWoord.Items.Remove(Me.ActiveItem)
            End If

            Me.ActiveItem.Delete()
            Me.NewItem()
        End If
    End Sub

    Private Sub cmbWoord_TextChanged(sender As Object, e As System.EventArgs) Handles cmbWoord.TextChanged
        CheckVerwijderen()
    End Sub


End Class