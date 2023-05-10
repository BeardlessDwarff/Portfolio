Imports System.Reflection

<ToolboxBitmap(GetType(System.Windows.Forms.SplitContainer))>
Public Class SplitContainer
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        Return

        ' Add any initialization after the InitializeComponent() call.
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)

        Dim objMethodInfo As MethodInfo = GetType(Control).GetMethod("SetStyle", BindingFlags.NonPublic Or BindingFlags.Instance)
        Dim objArgs As Object() = New Object() {ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True}
        objMethodInfo.Invoke(Me.Panel1, objArgs)
        objMethodInfo.Invoke(Me.Panel2, objArgs)
    End Sub


End Class
