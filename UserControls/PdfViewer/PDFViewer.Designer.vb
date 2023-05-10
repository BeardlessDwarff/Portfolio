<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PDFViewer
    Inherits UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PDFViewer))
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.tsPrint = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator4 = New System.Windows.Forms.ToolStripSeparator()
        Me.tsPageLabel = New System.Windows.Forms.ToolStripLabel()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.tsPrevious = New System.Windows.Forms.ToolStripButton()
        Me.tsNext = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripLabel2 = New System.Windows.Forms.ToolStripLabel()
        Me.tsPageNum = New System.Windows.Forms.ToolStripTextBox()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.tsZoomOut = New System.Windows.Forms.ToolStripButton()
        Me.tsZoomIn = New System.Windows.Forms.ToolStripButton()
        Me.tsRotateCC = New System.Windows.Forms.ToolStripButton()
        Me.tsRotateC = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripLabel3 = New System.Windows.Forms.ToolStripLabel()
        Me.tscbZoom = New System.Windows.Forms.ToolStripComboBox()
        Me.tsWidth = New System.Windows.Forms.ToolStripButton()
        Me.tsHeight = New System.Windows.Forms.ToolStripButton()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.TableLayoutPanel1 = New MCSCTRLS1.TableLayoutPanel()
        Me.TreeView1 = New MCSCTRLS1.TreeView()
        Me.Panel1 = New MCSCTRLS1.Panel()
        Me.ToolStrip1.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolStrip1
        '
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.tsPrint, Me.ToolStripSeparator4, Me.tsPageLabel, Me.ToolStripSeparator1, Me.tsPrevious, Me.tsNext, Me.ToolStripSeparator2, Me.ToolStripLabel2, Me.tsPageNum, Me.ToolStripSeparator3, Me.tsZoomOut, Me.tsZoomIn, Me.tsRotateCC, Me.tsRotateC, Me.ToolStripLabel3, Me.tscbZoom, Me.tsWidth, Me.tsHeight})
        Me.ToolStrip1.Location = New System.Drawing.Point(0, 0)
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.Size = New System.Drawing.Size(755, 25)
        Me.ToolStrip1.TabIndex = 0
        Me.ToolStrip1.Text = "ToolStrip1"
        '
        'tsPrint
        '
        Me.tsPrint.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsPrint.Image = CType(resources.GetObject("tsPrint.Image"), System.Drawing.Image)
        Me.tsPrint.ImageTransparentColor = System.Drawing.Color.Black
        Me.tsPrint.Name = "tsPrint"
        Me.tsPrint.Size = New System.Drawing.Size(23, 22)
        Me.tsPrint.Text = "ToolStripButton1"
        Me.tsPrint.ToolTipText = "Print"
        Me.tsPrint.Visible = False
        '
        'ToolStripSeparator4
        '
        Me.ToolStripSeparator4.Name = "ToolStripSeparator4"
        Me.ToolStripSeparator4.Size = New System.Drawing.Size(6, 25)
        Me.ToolStripSeparator4.Visible = False
        '
        'tsPageLabel
        '
        Me.tsPageLabel.AutoSize = False
        Me.tsPageLabel.Name = "tsPageLabel"
        Me.tsPageLabel.Size = New System.Drawing.Size(120, 22)
        Me.tsPageLabel.Text = "Pagina 1 van 1"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 25)
        '
        'tsPrevious
        '
        Me.tsPrevious.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsPrevious.Image = CType(resources.GetObject("tsPrevious.Image"), System.Drawing.Image)
        Me.tsPrevious.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsPrevious.Name = "tsPrevious"
        Me.tsPrevious.Size = New System.Drawing.Size(23, 22)
        Me.tsPrevious.Text = "ToolStripButton1"
        Me.tsPrevious.ToolTipText = "Vorige pagina"
        '
        'tsNext
        '
        Me.tsNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsNext.Image = CType(resources.GetObject("tsNext.Image"), System.Drawing.Image)
        Me.tsNext.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsNext.Name = "tsNext"
        Me.tsNext.Size = New System.Drawing.Size(23, 22)
        Me.tsNext.Text = "ToolStripButton2"
        Me.tsNext.ToolTipText = "Volgende pagina"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(6, 25)
        '
        'ToolStripLabel2
        '
        Me.ToolStripLabel2.Name = "ToolStripLabel2"
        Me.ToolStripLabel2.Size = New System.Drawing.Size(89, 22)
        Me.ToolStripLabel2.Text = "Ga naar pagina:"
        '
        'tsPageNum
        '
        Me.tsPageNum.Name = "tsPageNum"
        Me.tsPageNum.Size = New System.Drawing.Size(40, 25)
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(6, 25)
        '
        'tsZoomOut
        '
        Me.tsZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsZoomOut.Image = CType(resources.GetObject("tsZoomOut.Image"), System.Drawing.Image)
        Me.tsZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsZoomOut.Name = "tsZoomOut"
        Me.tsZoomOut.Size = New System.Drawing.Size(23, 22)
        Me.tsZoomOut.Text = "ToolStripButton3"
        Me.tsZoomOut.ToolTipText = "Zoom Uit"
        '
        'tsZoomIn
        '
        Me.tsZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsZoomIn.Image = CType(resources.GetObject("tsZoomIn.Image"), System.Drawing.Image)
        Me.tsZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsZoomIn.Name = "tsZoomIn"
        Me.tsZoomIn.Size = New System.Drawing.Size(23, 22)
        Me.tsZoomIn.Text = "ToolStripButton4"
        Me.tsZoomIn.ToolTipText = "Zoom In"
        '
        'tsRotateCC
        '
        Me.tsRotateCC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsRotateCC.Image = Global.MCSCTRLS1.My.Resources.Resources.Rotate_90
        Me.tsRotateCC.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsRotateCC.Name = "tsRotateCC"
        Me.tsRotateCC.Size = New System.Drawing.Size(23, 22)
        Me.tsRotateCC.Text = "Linksom draaien"
        '
        'tsRotateC
        '
        Me.tsRotateC.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsRotateC.Image = Global.MCSCTRLS1.My.Resources.Resources.Rotate90
        Me.tsRotateC.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsRotateC.Name = "tsRotateC"
        Me.tsRotateC.Size = New System.Drawing.Size(23, 22)
        Me.tsRotateC.Text = "Rechtsom draaien"
        '
        'ToolStripLabel3
        '
        Me.ToolStripLabel3.Name = "ToolStripLabel3"
        Me.ToolStripLabel3.Size = New System.Drawing.Size(10, 22)
        Me.ToolStripLabel3.Text = " "
        '
        'tscbZoom
        '
        Me.tscbZoom.AutoSize = False
        Me.tscbZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.tscbZoom.Name = "tscbZoom"
        Me.tscbZoom.Size = New System.Drawing.Size(130, 23)
        Me.tscbZoom.Visible = False
        '
        'tsWidth
        '
        Me.tsWidth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsWidth.Image = Global.MCSCTRLS1.My.Resources.Resources.Width
        Me.tsWidth.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsWidth.Name = "tsWidth"
        Me.tsWidth.Size = New System.Drawing.Size(23, 22)
        Me.tsWidth.ToolTipText = "Hele breedte"
        '
        'tsHeight
        '
        Me.tsHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.tsHeight.Image = Global.MCSCTRLS1.My.Resources.Resources.Height
        Me.tsHeight.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.tsHeight.Name = "tsHeight"
        Me.tsHeight.Size = New System.Drawing.Size(23, 22)
        Me.tsHeight.ToolTipText = "Hele hoogte"
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.FileName = "OpenFileDialog1"
        '
        'Timer1
        '
        Me.Timer1.Interval = 250
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 2
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.0!))
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.TreeView1, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel1, 1, 0)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 25)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 1
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(755, 401)
        Me.TableLayoutPanel1.TabIndex = 2
        '
        'TreeView1
        '
        Me.TreeView1.AllowTreenodeReorder = False
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.HideSelection = False
        Me.TreeView1.Location = New System.Drawing.Point(3, 3)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.NodeSelecterenMousedown = True
        Me.TreeView1.ScrollWindowUnderMousePointer = True
        Me.TreeView1.Size = New System.Drawing.Size(145, 395)
        Me.TreeView1.TabIndex = 2
        '
        'Panel1
        '
        Me.Panel1.Alpha = 200
        Me.Panel1.AnimationDuration = 150
        Me.Panel1.AutoScroll = True
        Me.Panel1.AutoScrollToActivatedControl = True
        Me.Panel1.BackgroundText = ""
        Me.Panel1.ColorEnd = System.Drawing.Color.WhiteSmoke
        Me.Panel1.ColorStart = System.Drawing.Color.Blue
        Me.Panel1.CornerRadius = 20
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel1.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical
        Me.Panel1.HideAnimation = MCSCTRLS1.Panel.ePanelAnimation.None
        Me.Panel1.Image = Nothing
        Me.Panel1.ImagePlace = MCSCTRLS1.Panel.eImagePlace.RechtOnder
        Me.Panel1.Location = New System.Drawing.Point(154, 3)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.RoundCorners = CType((MCSCTRLS1.Corner.TopRight Or MCSCTRLS1.Corner.BottomLeft), MCSCTRLS1.Corner)
        Me.Panel1.ShowAnimation = MCSCTRLS1.Panel.ePanelAnimation.None
        Me.Panel1.Size = New System.Drawing.Size(598, 395)
        Me.Panel1.Style = MCSCTRLS1.Panel.eStyle.Normal
        Me.Panel1.TabIndex = 8
        '
        'PDFViewer
        '
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Controls.Add(Me.ToolStrip1)
        Me.Name = "PDFViewer"
        Me.Size = New System.Drawing.Size(755, 426)
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents tsPageLabel As ToolStripLabel
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents tsPrevious As ToolStripButton
    Friend WithEvents tsNext As ToolStripButton
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents ToolStripLabel2 As ToolStripLabel
    Friend WithEvents tsPageNum As ToolStripTextBox
    Friend WithEvents ToolStripSeparator3 As ToolStripSeparator
    Friend WithEvents tsZoomOut As ToolStripButton
    Friend WithEvents tsZoomIn As ToolStripButton
    Friend WithEvents tscbZoom As ToolStripComboBox
    Friend WithEvents ToolStripLabel3 As ToolStripLabel
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents TreeView1 As MCSCTRLS1.TreeView
    Friend WithEvents tsPrint As ToolStripButton
    Friend WithEvents ToolStripSeparator4 As ToolStripSeparator
    Friend WithEvents tsRotateCC As ToolStripButton
    Friend WithEvents tsRotateC As ToolStripButton
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents Panel1 As MCSCTRLS1.Panel
    Friend WithEvents tsWidth As System.Windows.Forms.ToolStripButton
    Friend WithEvents tsHeight As System.Windows.Forms.ToolStripButton

End Class
