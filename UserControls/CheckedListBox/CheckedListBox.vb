Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.CheckedListBox))>
Public Class CheckedListBox
    Public Event ItemCheckedStateChangedAfterInterval()

    Private WithEvents tmrCheckChanged As New System.Windows.Forms.Timer

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        tmrCheckChanged.Interval = 500
    End Sub

    Private sLastState As String = ""
    Private bIgnoreEvent As Boolean

    Public Shadows Event ItemCheck(sender As Object, e As ItemCheckEventArgs)

    Private Sub CheckedListBox_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles MyBase.ItemCheck
        If bIgnoreEvent Then Return

        'Het ItemCheck-event heeft de nare eigenschap dat het item dan nog niet is bijgewerkt in Checkedlistbox.CheckedItems. 
        'Onderstaande code lost dat op.

        Try
            bIgnoreEvent = True
            Dim bChecked As Boolean

            bChecked = IIf(e.NewValue = CheckState.Checked, True, False)

            Me.SetItemChecked(e.Index, bChecked)
            bIgnoreEvent = False

            RaiseEvent ItemCheck(sender, e)

        Catch ex As Exception
            If Debugger.IsAttached Then Stop
        End Try

        Me.tmrCheckChanged.Stop()
        Me.tmrCheckChanged.Start()
    End Sub

    Private Function CurrentState() As String
        Dim sResult As String = ""

        For X As Integer = 0 To Me.Items.Count - 1
            sResult &= Me.GetItemChecked(X).ToBit.ToString
        Next

        Return sResult
    End Function

    Private Sub tmrCheckChanged_Tick(sender As Object, e As EventArgs) Handles tmrCheckChanged.Tick
        tmrCheckChanged.Stop()

        If sLastState = CurrentState() Then Return
        sLastState = CurrentState()

        RaiseEvent ItemCheckedStateChangedAfterInterval()

        tmrCheckChanged.Stop()
    End Sub
End Class
