Imports MCS_Interfaces

<ToolboxBitmap(GetType(System.Windows.Forms.TabControl))>
Public Class TabControl
    Private bHideHeaders As Boolean = False

    Public Property HideHeaders As Boolean
        Get
            Return bHideHeaders
        End Get
        Set(value As Boolean)
            If bHideHeaders = value Then Return

            bHideHeaders = value
            SetHeadersVisibility()
        End Set
    End Property

    Public Function FirstTabSelected() As Boolean
        Return Me.SelectedIndex = 0
    End Function

    Public Function LastTabSelected() As Boolean
        Return Me.SelectedIndex = Me.TabPages.Count - 1
    End Function

    Public Function SelectNextTabPage(Optional Roundtrip As Boolean = False) As TabPage
        Dim iCurrentIndex As Integer = Me.SelectedIndex

        Me.SelectedIndex += 1

        If Me.SelectedIndex = iCurrentIndex Then
            If Not Roundtrip Then Return Nothing 'Er was geen tabpage meer
            Me.SelectedIndex = 0 'Toon de eerste weer.
        End If

        Return Me.SelectedTab
    End Function

    Public Function SelectPreviousTabPage(Optional Roundtrip As Boolean = False) As TabPage
        Dim iCurrentIndex As Integer = Me.SelectedIndex
        Dim NewTabPage As TabPage = Nothing

        If iCurrentIndex = 0 Then
            If Not Roundtrip Then Return Nothing 'Er was geen tabpage meer

            Me.SelectedIndex = Me.TabPages.Count - 1
        Else
            NewTabPage = Me.TabPages.Item(iCurrentIndex - 1)
            'Me.SelectedIndex -= 1
        End If

        Me.SelectTab(NewTabPage)


        Return NewTabPage
    End Function

    Private Sub SetHeadersVisibility()
    
        If Me.HideHeaders AndAlso Me.RuntimeMode Then
            Me.Appearance = TabAppearance.FlatButtons
            Me.ItemSize = New Size(0, 1)
            Me.SizeMode = TabSizeMode.Fixed
        Else
            Me.Appearance = TabAppearance.Normal
            Me.ItemSize = New Size(0, 0)
            Me.SizeMode = TabSizeMode.Normal
        End If
    End Sub

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
    End Sub

End Class
