<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAutoCorrectie
    Inherits MCSCTRLS2.McsRoundedForm


    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAutoCorrectie))
        Me.glbBelangrijkeInfo = New MCSCTRLS2.GradientLabel()
        Me.GradientLabel1 = New MCSCTRLS2.GradientLabel()
        Me.GradientLabel2 = New MCSCTRLS2.GradientLabel()
        Me.btnOk = New MCSCTRLS2.AquaButton()
        Me.btnCancel = New MCSCTRLS2.AquaButton()
        Me.btnCompleteLijst = New MCSCTRLS2.AquaButton()
        Me.ToolStrip2 = New MCSCTRLS1.ToolStrip()
        Me.tsbNew = New System.Windows.Forms.ToolStripButton()
        Me.tsbVerwijderen = New System.Windows.Forms.ToolStripButton()
        Me.ToolStrip1 = New MCSCTRLS1.ToolStrip()
        Me.tsbBold = New System.Windows.Forms.ToolStripButton()
        Me.tsbItalic = New System.Windows.Forms.ToolStripButton()
        Me.tsbUnderline = New System.Windows.Forms.ToolStripButton()
        Me.cmbFontSize = New MCSCTRLS1.ComboBox()
        Me.cmbWoord = New MCSCTRLS1.ComboBox()
        Me.txtCorrectie = New MCSCTRLS1.Textbox()
        Me.btnOpslaanenSluiten = New MCSCTRLS2.AquaButton()
        Me.ToolStrip2.SuspendLayout()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'glbBelangrijkeInfo
        '
        Me.glbBelangrijkeInfo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.glbBelangrijkeInfo.Clickable = False
        Me.glbBelangrijkeInfo.Cursor = System.Windows.Forms.Cursors.Default
        Me.glbBelangrijkeInfo.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.glbBelangrijkeInfo.ForeColor = System.Drawing.Color.White
        Me.glbBelangrijkeInfo.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical
        Me.glbBelangrijkeInfo.Image = Nothing
        Me.glbBelangrijkeInfo.ImageAlignMent = System.Drawing.ContentAlignment.MiddleCenter
        Me.glbBelangrijkeInfo.ImageSize = New System.Drawing.Size(0, 0)
        Me.glbBelangrijkeInfo.Location = New System.Drawing.Point(36, 99)
        Me.glbBelangrijkeInfo.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.glbBelangrijkeInfo.MouseOverColor1 = System.Drawing.Color.FromArgb(CType(CType(89, Byte), Integer), CType(CType(135, Byte), Integer), CType(CType(214, Byte), Integer))
        Me.glbBelangrijkeInfo.MouseOverColor2 = System.Drawing.Color.FromArgb(CType(CType(3, Byte), Integer), CType(CType(56, Byte), Integer), CType(CType(147, Byte), Integer))
        Me.glbBelangrijkeInfo.Name = "glbBelangrijkeInfo"
        Me.glbBelangrijkeInfo.Size = New System.Drawing.Size(665, 16)
        Me.glbBelangrijkeInfo.TabIndex = 67
        Me.glbBelangrijkeInfo.TabStop = False
        Me.glbBelangrijkeInfo.Text = "Origineel"
        Me.glbBelangrijkeInfo.TextAlignMent = System.Drawing.ContentAlignment.MiddleCenter
        Me.glbBelangrijkeInfo.TextFont = New System.Drawing.Font("Comic Sans MS", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.glbBelangrijkeInfo.ToolTipText = Nothing
        '
        'GradientLabel1
        '
        Me.GradientLabel1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GradientLabel1.Clickable = False
        Me.GradientLabel1.Cursor = System.Windows.Forms.Cursors.Default
        Me.GradientLabel1.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GradientLabel1.ForeColor = System.Drawing.Color.White
        Me.GradientLabel1.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical
        Me.GradientLabel1.Image = Nothing
        Me.GradientLabel1.ImageAlignMent = System.Drawing.ContentAlignment.MiddleCenter
        Me.GradientLabel1.ImageSize = New System.Drawing.Size(0, 0)
        Me.GradientLabel1.Location = New System.Drawing.Point(36, 140)
        Me.GradientLabel1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GradientLabel1.MouseOverColor1 = System.Drawing.Color.FromArgb(CType(CType(89, Byte), Integer), CType(CType(135, Byte), Integer), CType(CType(214, Byte), Integer))
        Me.GradientLabel1.MouseOverColor2 = System.Drawing.Color.FromArgb(CType(CType(3, Byte), Integer), CType(CType(56, Byte), Integer), CType(CType(147, Byte), Integer))
        Me.GradientLabel1.Name = "GradientLabel1"
        Me.GradientLabel1.Size = New System.Drawing.Size(665, 16)
        Me.GradientLabel1.TabIndex = 68
        Me.GradientLabel1.TabStop = False
        Me.GradientLabel1.Text = "Vervanging"
        Me.GradientLabel1.TextAlignMent = System.Drawing.ContentAlignment.MiddleCenter
        Me.GradientLabel1.TextFont = New System.Drawing.Font("Comic Sans MS", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GradientLabel1.ToolTipText = Nothing
        '
        'GradientLabel2
        '
        Me.GradientLabel2.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GradientLabel2.Clickable = False
        Me.GradientLabel2.Cursor = System.Windows.Forms.Cursors.Default
        Me.GradientLabel2.Font = New System.Drawing.Font("Comic Sans MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GradientLabel2.ForeColor = System.Drawing.Color.White
        Me.GradientLabel2.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical
        Me.GradientLabel2.Image = Nothing
        Me.GradientLabel2.ImageAlignMent = System.Drawing.ContentAlignment.MiddleCenter
        Me.GradientLabel2.ImageSize = New System.Drawing.Size(0, 0)
        Me.GradientLabel2.Location = New System.Drawing.Point(36, 185)
        Me.GradientLabel2.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.GradientLabel2.MouseOverColor1 = System.Drawing.Color.FromArgb(CType(CType(89, Byte), Integer), CType(CType(135, Byte), Integer), CType(CType(214, Byte), Integer))
        Me.GradientLabel2.MouseOverColor2 = System.Drawing.Color.FromArgb(CType(CType(3, Byte), Integer), CType(CType(56, Byte), Integer), CType(CType(147, Byte), Integer))
        Me.GradientLabel2.Name = "GradientLabel2"
        Me.GradientLabel2.Size = New System.Drawing.Size(665, 16)
        Me.GradientLabel2.TabIndex = 69
        Me.GradientLabel2.TabStop = False
        Me.GradientLabel2.Text = "Opmaak"
        Me.GradientLabel2.TextAlignMent = System.Drawing.ContentAlignment.MiddleCenter
        Me.GradientLabel2.TextFont = New System.Drawing.Font("Comic Sans MS", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.GradientLabel2.ToolTipText = Nothing
        '
        'btnOk
        '
        Me.btnOk.AlleenImageTonen = False
        Me.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.btnOk.BackColor = System.Drawing.Color.Transparent
        Me.btnOk.Image = Nothing
        Me.btnOk.ImageFormaat = MCSCTRLS2.AquaButton.eFormaat.Groot_48x48
        Me.btnOk.Location = New System.Drawing.Point(288, 393)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(151, 30)
        Me.btnOk.TabIndex = 70
        Me.btnOk.Text = "Opslaan"
        Me.btnOk.ToolTipText = ""
        Me.btnOk.Transparent = False
        '
        'btnCancel
        '
        Me.btnCancel.AlleenImageTonen = False
        Me.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.btnCancel.BackColor = System.Drawing.Color.Transparent
        Me.btnCancel.Image = Nothing
        Me.btnCancel.ImageFormaat = MCSCTRLS2.AquaButton.eFormaat.Groot_48x48
        Me.btnCancel.Location = New System.Drawing.Point(134, 393)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(151, 30)
        Me.btnCancel.TabIndex = 71
        Me.btnCancel.Text = "Annuleren"
        Me.btnCancel.ToolTipText = ""
        Me.btnCancel.Transparent = False
        '
        'btnCompleteLijst
        '
        Me.btnCompleteLijst.AlleenImageTonen = False
        Me.btnCompleteLijst.BackColor = System.Drawing.Color.Transparent
        Me.btnCompleteLijst.Image = Nothing
        Me.btnCompleteLijst.ImageFormaat = MCSCTRLS2.AquaButton.eFormaat.Groot_48x48
        Me.btnCompleteLijst.Location = New System.Drawing.Point(36, 236)
        Me.btnCompleteLijst.Name = "btnCompleteLijst"
        Me.btnCompleteLijst.Size = New System.Drawing.Size(201, 30)
        Me.btnCompleteLijst.TabIndex = 72
        Me.btnCompleteLijst.Text = "Complete lijst weergeven"
        Me.btnCompleteLijst.ToolTipText = ""
        Me.btnCompleteLijst.Transparent = False
        '
        'ToolStrip2
        '
        Me.ToolStrip2.Dock = System.Windows.Forms.DockStyle.None
        Me.ToolStrip2.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbNew, Me.tsbVerwijderen})
        Me.ToolStrip2.Location = New System.Drawing.Point(36, 70)
        Me.ToolStrip2.Name = "ToolStrip2"
        Me.ToolStrip2.Size = New System.Drawing.Size(58, 25)
        Me.ToolStrip2.TabIndex = 73
        Me.ToolStrip2.Text = "ToolStrip2"
        '
        'tsbNew
        '
        Me.tsbNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbNew.Image = CType(resources.GetObject("tsbNew.Image"), System.Drawing.Image)
        Me.tsbNew.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbNew.Name = "tsbNew"
        Me.tsbNew.Size = New System.Drawing.Size(23, 22)
        Me.tsbNew.Text = "Nieuw"
        Me.tsbNew.ToolTipText = "Nieuw"
        '
        'tsbVerwijderen
        '
        Me.tsbVerwijderen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbVerwijderen.Image = CType(resources.GetObject("tsbVerwijderen.Image"), System.Drawing.Image)
        Me.tsbVerwijderen.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbVerwijderen.Name = "tsbVerwijderen"
        Me.tsbVerwijderen.Size = New System.Drawing.Size(23, 22)
        Me.tsbVerwijderen.Text = "Verwijderen"
        Me.tsbVerwijderen.ToolTipText = "Verwijderen"
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Dock = System.Windows.Forms.DockStyle.None
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbBold, Me.tsbItalic, Me.tsbUnderline})
        Me.ToolStrip1.Location = New System.Drawing.Point(157, 204)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(81, 25)
        Me.ToolStrip1.TabIndex = 4
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'tsbBold
        '
        Me.tsbBold.CheckOnClick = True
        Me.tsbBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbBold.Image = CType(resources.GetObject("tsbBold.Image"), System.Drawing.Image)
        Me.tsbBold.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbBold.Name = "tsbBold"
        Me.tsbBold.Size = New System.Drawing.Size(23, 22)
        Me.tsbBold.Text = "Vet"
        Me.tsbBold.ToolTipText = "Vet"
        '
        'tsbItalic
        '
        Me.tsbItalic.CheckOnClick = True
        Me.tsbItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbItalic.Image = CType(resources.GetObject("tsbItalic.Image"), System.Drawing.Image)
        Me.tsbItalic.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbItalic.Name = "tsbItalic"
        Me.tsbItalic.Size = New System.Drawing.Size(23, 22)
        Me.tsbItalic.Text = "Cursief"
        Me.tsbItalic.ToolTipText = "Cursief"
        '
        'tsbUnderline
        '
        Me.tsbUnderline.CheckOnClick = True
        Me.tsbUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbUnderline.Image = CType(resources.GetObject("tsbUnderline.Image"), System.Drawing.Image)
        Me.tsbUnderline.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbUnderline.Name = "tsbUnderline"
        Me.tsbUnderline.Size = New System.Drawing.Size(23, 22)
        Me.tsbUnderline.Text = "Onderstreept"
        Me.tsbUnderline.ToolTipText = "Onderstreept"
        '
        'cmbFontSize
        '
        Me.cmbFontSize.AantalDecimalen = 2
        Me.cmbFontSize.Description = ""
        Me.cmbFontSize.FormattingEnabled = True
        Me.cmbFontSize.IgnoreMouseWheel = True
        Me.cmbFontSize.Location = New System.Drawing.Point(36, 207)
        Me.cmbFontSize.Name = "cmbFontSize"
        Me.cmbFontSize.Size = New System.Drawing.Size(118, 21)
        Me.cmbFontSize.TabIndex = 3
        '
        'cmbWoord
        '
        Me.cmbWoord.AantalDecimalen = 2
        Me.cmbWoord.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmbWoord.Description = ""
        Me.cmbWoord.FormattingEnabled = True
        Me.cmbWoord.IgnoreMouseWheel = True
        Me.cmbWoord.Location = New System.Drawing.Point(36, 113)
        Me.cmbWoord.Name = "cmbWoord"
        Me.cmbWoord.Size = New System.Drawing.Size(665, 21)
        Me.cmbWoord.TabIndex = 1
        '
        'txtCorrectie
        '
        Me.txtCorrectie.AantalDecimalen = 2
        Me.txtCorrectie.AlleenNumeriek = False
        Me.txtCorrectie.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCorrectie.DecimalenAltijdTonen = False
        Me.txtCorrectie.Description = ""
        Me.txtCorrectie.DoneTypingInterval = 250
        Me.txtCorrectie.Location = New System.Drawing.Point(36, 158)
        Me.txtCorrectie.MaxValue = 3.402823E+38!
        Me.txtCorrectie.MinValue = -3.402823E+38!
        Me.txtCorrectie.Name = "txtCorrectie"
        Me.txtCorrectie.PlusToestaanInNumeriekeTextbox = False
        Me.txtCorrectie.RaiseChangeCompleted = False
        Me.txtCorrectie.SelectNearestControlWithArrowKeys = False
        Me.txtCorrectie.SelectNearestTextboxWithArrowKeys = False
        Me.txtCorrectie.Size = New System.Drawing.Size(665, 20)
        Me.txtCorrectie.TabIndex = 2
        '
        'btnOpslaanenSluiten
        '
        Me.btnOpslaanenSluiten.AlleenImageTonen = False
        Me.btnOpslaanenSluiten.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.btnOpslaanenSluiten.BackColor = System.Drawing.Color.Transparent
        Me.btnOpslaanenSluiten.Image = Nothing
        Me.btnOpslaanenSluiten.ImageFormaat = MCSCTRLS2.AquaButton.eFormaat.Groot_48x48
        Me.btnOpslaanenSluiten.Location = New System.Drawing.Point(442, 393)
        Me.btnOpslaanenSluiten.Name = "btnOpslaanenSluiten"
        Me.btnOpslaanenSluiten.Size = New System.Drawing.Size(151, 30)
        Me.btnOpslaanenSluiten.TabIndex = 74
        Me.btnOpslaanenSluiten.Text = "Opslaan en sluiten"
        Me.btnOpslaanenSluiten.ToolTipText = ""
        Me.btnOpslaanenSluiten.Transparent = False
        '
        'frmAutoCorrectie
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(726, 435)
        Me.Controls.Add(Me.btnOpslaanenSluiten)
        Me.Controls.Add(Me.ToolStrip2)
        Me.Controls.Add(Me.btnCompleteLijst)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.GradientLabel2)
        Me.Controls.Add(Me.GradientLabel1)
        Me.Controls.Add(Me.glbBelangrijkeInfo)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.cmbFontSize)
        Me.Controls.Add(Me.cmbWoord)
        Me.Controls.Add(Me.txtCorrectie)
        Me.Name = "frmAutoCorrectie"
        Me.Text = "AutoCorrectie"
        Me.Title = "AutoCorrectie"
        Me.Controls.SetChildIndex(Me.txtCorrectie, 0)
        Me.Controls.SetChildIndex(Me.cmbWoord, 0)
        Me.Controls.SetChildIndex(Me.cmbFontSize, 0)
        Me.Controls.SetChildIndex(Me.ToolStrip1, 0)
        Me.Controls.SetChildIndex(Me.glbBelangrijkeInfo, 0)
        Me.Controls.SetChildIndex(Me.GradientLabel1, 0)
        Me.Controls.SetChildIndex(Me.GradientLabel2, 0)
        Me.Controls.SetChildIndex(Me.btnOk, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.btnCompleteLijst, 0)
        Me.Controls.SetChildIndex(Me.ToolStrip2, 0)
        Me.Controls.SetChildIndex(Me.btnOpslaanenSluiten, 0)
        Me.ToolStrip2.ResumeLayout(False)
        Me.ToolStrip2.PerformLayout()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmbWoord As MCSCTRLS1.ComboBox
    Friend WithEvents txtCorrectie As MCSCTRLS1.Textbox
    Friend WithEvents cmbFontSize As MCSCTRLS1.ComboBox
    Friend WithEvents ToolStrip1 As MCSCTRLS1.ToolStrip
    Friend WithEvents tsbBold As System.Windows.Forms.ToolStripButton
    Friend WithEvents tsbItalic As System.Windows.Forms.ToolStripButton
    Friend WithEvents tsbUnderline As System.Windows.Forms.ToolStripButton
    Friend WithEvents glbBelangrijkeInfo As MCSCTRLS2.GradientLabel
    Friend WithEvents GradientLabel1 As MCSCTRLS2.GradientLabel
    Friend WithEvents GradientLabel2 As MCSCTRLS2.GradientLabel
    Friend WithEvents btnOk As MCSCTRLS2.AquaButton
    Friend WithEvents btnCancel As MCSCTRLS2.AquaButton
    Friend WithEvents btnCompleteLijst As MCSCTRLS2.AquaButton
    Friend WithEvents ToolStrip2 As MCSCTRLS1.ToolStrip
    Friend WithEvents tsbNew As System.Windows.Forms.ToolStripButton
    Friend WithEvents tsbVerwijderen As System.Windows.Forms.ToolStripButton
    Friend WithEvents btnOpslaanenSluiten As MCSCTRLS2.AquaButton
End Class
