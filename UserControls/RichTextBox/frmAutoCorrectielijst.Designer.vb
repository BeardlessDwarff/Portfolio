<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAutoCorrectielijst
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAutoCorrectielijst))
        Me.btnOk = New MCSCTRLS2.AquaButton()
        Me.ToolStrip1 = New MCSCTRLS1.ToolStrip()
        Me.tsbPrintPreview = New System.Windows.Forms.ToolStripButton()
        Me.tsbPrint = New System.Windows.Forms.ToolStripButton()
        Me.toolStripSeparator = New System.Windows.Forms.ToolStripSeparator()
        Me.ListView1 = New MCSCTRLS1.ListView()
        Me.Woord = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Correctie = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Nivo = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnOk
        '
        Me.btnOk.AlleenImageTonen = False
        Me.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.btnOk.BackColor = System.Drawing.Color.Transparent
        Me.btnOk.Image = Nothing
        Me.btnOk.ImageFormaat = MCSCTRLS2.AquaButton.eFormaat.Groot_48x48
        Me.btnOk.Location = New System.Drawing.Point(329, 490)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(117, 30)
        Me.btnOk.TabIndex = 71
        Me.btnOk.Text = "Ok"
        Me.btnOk.Transparent = False
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Dock = System.Windows.Forms.DockStyle.None
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsbPrintPreview, Me.tsbPrint, Me.toolStripSeparator})
        Me.ToolStrip1.Location = New System.Drawing.Point(27, 37)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(95, 25)
        Me.ToolStrip1.TabIndex = 73
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'tsbPrintPreview
        '
        Me.tsbPrintPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbPrintPreview.Image = CType(resources.GetObject("tsbPrintPreview.Image"), System.Drawing.Image)
        Me.tsbPrintPreview.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbPrintPreview.Name = "tsbPrintPreview"
        Me.tsbPrintPreview.Size = New System.Drawing.Size(23, 22)
        Me.tsbPrintPreview.Text = "Afdrukvoorbeeld"
        Me.tsbPrintPreview.ToolTipText = "Afdrukvoorbeeld"
        '
        'tsbPrint
        '
        Me.tsbPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsbPrint.Image = CType(resources.GetObject("tsbPrint.Image"), System.Drawing.Image)
        Me.tsbPrint.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsbPrint.Name = "tsbPrint"
        Me.tsbPrint.Size = New System.Drawing.Size(23, 22)
        Me.tsbPrint.Text = "Afdrukken"
        Me.tsbPrint.ToolTipText = "Afdrukken"
        '
        'toolStripSeparator
        '
        Me.toolStripSeparator.Name = "toolStripSeparator"
        Me.toolStripSeparator.Size = New System.Drawing.Size(6, 25)
        '
        'ListView1
        '
        Me.ListView1.AllowColumnReorder = True
        Me.ListView1.AllowSorting = True
        Me.ListView1.AltijdBovenKolom = ""
        Me.ListView1.AltijdBovenValue = ""
        Me.ListView1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ListView1.AutoSort = True
        Me.ListView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Woord, Me.Correctie, Me.Nivo})
        Me.ListView1.FullRowSelect = True
        Me.ListView1.Location = New System.Drawing.Point(27, 65)
        Me.ListView1.Name = "ListView1"
        Me.ListView1.ShowItemToolTips = True
        Me.ListView1.Size = New System.Drawing.Size(725, 407)
        Me.ListView1.SortingColumnIndex = 0
        Me.ListView1.TabIndex = 72
        Me.ListView1.UseCompatibleStateImageBehavior = False
        Me.ListView1.View = System.Windows.Forms.View.Details
        Me.ListView1.WaterMark = Nothing
        Me.ListView1.WatermarkAlpha = 200
        '
        'Woord
        '
        Me.Woord.Text = "Woord"
        Me.Woord.Width = 183
        '
        'Correctie
        '
        Me.Correctie.Text = "Correctie"
        Me.Correctie.Width = 227
        '
        'Nivo
        '
        Me.Nivo.Text = "Nivo"
        Me.Nivo.Width = 128
        '
        'frmAutoCorrectielijst
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(793, 541)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Controls.Add(Me.ListView1)
        Me.Controls.Add(Me.btnOk)
        Me.Name = "frmAutoCorrectielijst"
        Me.ShowCloseButton = True
        Me.Text = "AutoCorrectielijst"
        Me.Title = "AutoCorrectielijst"
        Me.Controls.SetChildIndex(Me.btnOk, 0)
        Me.Controls.SetChildIndex(Me.ListView1, 0)
        Me.Controls.SetChildIndex(Me.ToolStrip1, 0)
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnOk As MCSCTRLS2.AquaButton
    Friend WithEvents ListView1 As MCSCTRLS1.ListView
    Friend WithEvents Woord As System.Windows.Forms.ColumnHeader
    Friend WithEvents Correctie As System.Windows.Forms.ColumnHeader
    Friend WithEvents ToolStrip1 As MCSCTRLS1.ToolStrip
    Friend WithEvents tsbPrintPreview As System.Windows.Forms.ToolStripButton
    Friend WithEvents tsbPrint As System.Windows.Forms.ToolStripButton
    Friend WithEvents toolStripSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents Nivo As System.Windows.Forms.ColumnHeader
End Class
