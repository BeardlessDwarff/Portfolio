Imports MCS_Interfaces
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Windows.Forms.Design

'<Designer("ComponentDesigner"), ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)>
'Public Class PrintToolstripButton
'    Inherits ToolStripButton

'    Public Sub New()
'        MyBase.New()
'        Me.Site = Nothing 'Nodig om een bug uit Visual Studio te omzijlen.
'        Me.Image = My.Resources.Bold 'EasyCare.Images16x16.GetImage(FrameWorkNS.Functions.Forms.eDefaultImage.Print)
'    End Sub
'End Class


<ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip), DebuggerStepThrough(), DefaultEvent("MouseUp")>
Public Class PrintButton
    Inherits ToolStripButton

    Public Sub New()
        MyBase.New()

        Me.DisplayStyle = ToolStripItemDisplayStyle.Image
        Me.Text = "Afdrukken"
        Me.ToolTipText = "Afdrukken"
    End Sub

    Public Property PrintKey As String = ""
    Public Property PrintDisplayName As String = ""

    Public Overrides Property Image As Image
        Get
            Return My.Resources.Print
        End Get
        Set(value As Image)
            MyBase.Image = value
        End Set
    End Property


End Class

<ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip), DebuggerStepThrough()>
Public Class PrintPreviewButton
    Inherits ToolStripButton

    Public Sub New()
        MyBase.New()

        Me.Image = My.Resources.Print
        Me.DisplayStyle = ToolStripItemDisplayStyle.Image
        Me.Text = "Afdrukvoorbeeld"
        Me.ToolTipText = "Afdrukvoorbeeld"
    End Sub

    Public Overrides Property Image As Image
        Get
            Return My.Resources.PrintPreview
        End Get
        Set(value As Image)
            MyBase.Image = value
        End Set
    End Property

End Class