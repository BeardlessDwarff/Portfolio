Imports System.Runtime.InteropServices
Imports MCS_Interfaces
<ToolboxBitmap(GetType(System.Windows.Forms.TreeView))>
Public Class TreeView

    Public Event CreateItemContextMenu(ClickedItem As TreeNode, ByRef Menu As MCSCTRLS2.ContextMenu)
    Public Event CreateTreeviewContextMenu(ByRef Menu As MCSCTRLS2.ContextMenu)
    Public Event ContextMenuClick(Key As String, ClickedItem As TreeNode)
    Public Property ScrollWindowUnderMousePointer As Boolean = True
    Public Property AllowTreenodeReorder As Boolean = False

    Private Const TV_FIRST As Integer = &H1100
    Private Const TVM_GETIMAGELIST As Integer = (TV_FIRST + 8)
    Private Const TVSIL_STATE As Integer = 2

    Public Function AddImageToImageList(Key As String, Image As Image) As Boolean
        Dim iIndex As Integer

        If Me.ImageList Is Nothing Then Return False
        If Image Is Nothing Then Return False

        iIndex = FrameWorkNS.Functions.GDI.AddImageToImageList(Me.ImageList, Image, Key)

        Return iIndex >= 0
    End Function

    <DllImport("comctl32")>
    Private Shared Function ImageList_Destroy(ByVal himl As IntPtr) As Boolean
    End Function

    Protected Overrides Sub OnHandleDestroyed(ByVal e As EventArgs)
        If Me.CheckBoxes Then
            Dim hStateImageList As IntPtr = FrameWorkNS.Functions.API.SendMessageIntPtr(Me.Handle, TVM_GETIMAGELIST, CType(TVSIL_STATE, IntPtr), IntPtr.Zero)

            If Not hStateImageList.Equals(IntPtr.Zero) Then ImageList_Destroy(hStateImageList)
        End If

        MyBase.OnHandleDestroyed(e)
    End Sub

    Public ReadOnly Property AllNodes(Node As TreeNode) As List(Of TreeNode)
        Get
            Dim MyList As New List(Of TreeNode)
            VulNodes(Node.Nodes, MyList)
            Return MyList
        End Get
    End Property

    Public Shadows Sub ExpandALL()
        Me.BeginUpdate()
        Me.SuspendRedraw
        MyBase.ExpandAll()
        Me.EndUpdate()
        Me.ResumeRedraw
    End Sub

    Public Overrides Property Font As System.Drawing.Font
        Get
            Return MyBase.Font
        End Get
        Set(value As System.Drawing.Font)
            MyBase.Font = value
        End Set
    End Property

    Public Shadows Sub CollapseALL()
        If Not bHasExpandedNodes Then Return

        Me.BeginUpdate()
        Me.SuspendRedraw
        MyBase.CollapseAll()
        Me.EndUpdate()
        Me.ResumeRedraw
        bHasExpandedNodes = False
    End Sub


    Public ReadOnly Property AllNodes() As List(Of TreeNode)
        Get
            Dim MyList As New List(Of TreeNode)
            VulNodes(Me.Nodes, MyList)
            Return MyList
        End Get
    End Property

    Private Sub VulNodes(ByVal Nodes As TreeNodeCollection, ByRef Collection As List(Of TreeNode))
        For Each Node As TreeNode In Nodes
            Collection.Add(Node)
            VulNodes(Node.Nodes, Collection)
        Next
    End Sub

    Public Function AddNodeWithGroups(FullPath As String, GroupImageKey As String, ItemImageKey As String) As TreeNode
        Dim arrPath() As String = FullPath.Split("\")
        Return AddNodeWithGroups(arrPath, GroupImageKey, ItemImageKey)
    End Function

    Public Function AddNodeWithGroups(arrPath As String(), GroupImageKey As String, ItemImageKey As String) As TreeNode
        Dim Col As TreeNodeCollection = Me.Nodes
        Dim NodeText As String
        Dim MyNode As TreeNode = Nothing

        For X As Integer = 0 To arrPath.Length - 1
            NodeText = arrPath(X)
            MyNode = GetNodeByText(Col, NodeText)

            If MyNode Is Nothing OrElse X = arrPath.Length - 1 Then
                MyNode = Col.Add(NodeText)

                MyNode.ImageKey = GroupImageKey
                MyNode.SelectedImageKey = GroupImageKey
            End If

            Col = MyNode.Nodes
        Next

        MyNode.ImageKey = ItemImageKey
        MyNode.SelectedImageKey = ItemImageKey

        Return MyNode
    End Function

    Private Function GetNodeByText(Col As TreeNodeCollection, Text As String) As TreeNode
        For Each N As TreeNode In Col
            If N.Text.ToLower.Trim = Text.ToLower.Trim Then Return N
        Next

        Return Nothing
    End Function

    'Protected Overrides ReadOnly Property CreateParams() As System.Windows.Forms.CreateParams
    '    Get
    '        Dim cp As System.Windows.Forms.CreateParams = MyBase.CreateParams
    '        cp.ExStyle = cp.ExStyle Or &H2000000 'Zou flikkeren moeten tegengaan...
    '        Return cp
    '    End Get
    'End Property


    Public Function GetNodeByKey(ByVal Key As String) As TreeNode
        If Me.Nodes.ContainsKey(Key) Then Return Me.Nodes.Item(Key)

        For Each node As TreeNode In Me.AllNodes
            If node.Nodes.ContainsKey(Key) Then Return node.Nodes.Item(Key)
        Next

        Return Nothing
    End Function

    Public Sub BeginPrint()
        NodeToPrint = Nothing

        Me.ExpandALL()
    End Sub

    Private NodeToPrint As TreeNode
    Private iCurrentY As Integer
    Private iCurrentX As Integer

    Private Function GetIndent(Node As TreeNode) As Integer
        If Node.Parent Is Nothing Then Return 0

        Dim iResult As Integer = 0
        Dim TempNode As TreeNode = Node

        Do While Not TempNode.Parent Is Nothing
            iResult += Me.Indent
            TempNode = TempNode.Parent
        Loop

        Return iResult
    End Function

    Private PrintFont As New Font("Arial", 10)

    Public Function Print(G As Graphics, Rect As Rectangle) As Boolean
        If G Is Nothing Then Return False
        If Rect = Nothing Then Return False

        If Me.Nodes.Count = 0 Then Return False 'HasMorePages 

        Rect.Inflate(-5, -5)

        If Not Me.Font Is Nothing Then PrintFont = Me.Font

        If NodeToPrint Is Nothing Then
            NodeToPrint = Me.Nodes(0)
        End If

        iCurrentY = Rect.Top

        Do While PrintNode(G, Rect)
            If Not NodeToPrint.NextNode Is Nothing Then
                NodeToPrint = NodeToPrint.NextNode
            Else
                If NodeToPrint.Parent Is Nothing Then
                    NodeToPrint = Nothing
                    GoTo Einde
                End If

                Do While NodeToPrint.Parent.NextNode Is Nothing
                    NodeToPrint = NodeToPrint.Parent
                    If NodeToPrint Is Nothing Then GoTo Einde
                    If NodeToPrint.Parent Is Nothing Then
                        NodeToPrint = Nothing
                        GoTo Einde
                    End If

                Loop

                NodeToPrint = NodeToPrint.Parent.NextNode
            End If

        Loop

Einde:
        If NodeToPrint Is Nothing Then
            Return False
        Else
            Return True 'HasMorePages
        End If
    End Function

    Private Function PrintNode(G As Graphics, Rect As Rectangle) As Boolean
        If NodeToPrint Is Nothing Then Return False
        iCurrentX = Rect.Left + GetIndent(NodeToPrint)
        Dim MyImage As Image = Nothing
        Dim iHeight As Integer

        iHeight = G.MeasureString(NodeToPrint.Text, PrintFont, Rect.Right - iCurrentX).Height

        If (Rect.Bottom - iCurrentY) < iHeight Then Return False

        If Not Me.ImageList Is Nothing Then
            If NodeToPrint.ImageIndex > -1 Then
                MyImage = Me.ImageList.Images(NodeToPrint.ImageIndex)
            ElseIf Me.ImageIndex > -1 Then
                MyImage = Me.ImageList.Images(Me.ImageIndex)
            End If

        End If

        If Not MyImage Is Nothing Then
            Dim Myrect As New Rectangle(iCurrentX, iCurrentY, 16, 16)
            G.DrawImage(MyImage, Myrect)
        End If

        iCurrentX += 20

        G.DrawString(NodeToPrint.Text, PrintFont, Brushes.Black, iCurrentX, iCurrentY)
        iCurrentY += iHeight

        If NodeToPrint.Nodes.Count > 0 Then
            NodeToPrint = NodeToPrint.Nodes(0)
            PrintNode(G, Rect)
        End If

        Return True
    End Function

    Public ReadOnly Property FirstNode() As TreeNode
        Get
            For Each Node As TreeNode In Me.Nodes
                Return Node
            Next

            Return Nothing
        End Get
    End Property

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.HideSelection = False

        SetStyle(ControlStyles.OptimizedDoubleBuffer Or ControlStyles.AllPaintingInWmPaint, True)
    End Sub

    Private MyNodeSelecterenMousedown As Boolean = True

    Public Property NodeSelecterenMousedown() As Boolean
        Get
            Return MyNodeSelecterenMousedown
        End Get
        Set(ByVal value As Boolean)
            MyNodeSelecterenMousedown = value
        End Set
    End Property

    Private Sub TreeView_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If Not NodeSelecterenMousedown Then Return
        Dim MyNode As TreeNode

        MyNode = Me.GetNodeAt(e.X, e.Y)

        If MyNode Is Nothing Then Return

        Me.SelectedNode = MyNode
    End Sub

    <DebuggerNonUserCode()>
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Select Case m.Msg
            Case FrameWorkNS.Functions.API.Messages.WM_ERASEBKGND
                m.Result = IntPtr.Zero
                Return

            Case FrameWorkNS.Functions.API.Messages.WM_MOUSEWHEEL
                If Me.ScrollWindowUnderMousePointer Then
                    If FrameWorkNS.Functions.API.ScrollWindowUnderMousePointer(m) Then Return
                End If

        End Select

DefaultProc:
        MyBase.WndProc(m)
    End Sub

    Public Sub SortChildNodes(ByVal Node As TreeNode, Order As SortOrder, Optional ByVal TopLevelOnly As Boolean = True)
        If Node Is Nothing Then Return
        Dim Sorter As New DefaultNodeComparer

        Sorter.SortOrder = Order

        Me.BeginUpdate()
        Dim MyArray(Node.Nodes.Count - 1) As TreeNode
        Node.Nodes.CopyTo(MyArray, 0)
        Array.Sort(MyArray, Sorter)
        Node.Nodes.Clear()
        Node.Nodes.AddRange(MyArray)

        If Not TopLevelOnly Then
            For Each ChildNode As TreeNode In Node.Nodes
                If ChildNode.Nodes.Count > 0 Then
                    SortChildNodes(Node, Sorter, False)
                End If
            Next
        End If

        Sorter = Nothing

        Me.EndUpdate()
    End Sub

    Private bBusySorting As Boolean
    Private DicChildNodes As Dictionary(Of TreeNode, TreeNodeCollection)

    Public Sub AddNodes(Nodes As List(Of TreeNode))
        'Deze sub gaan we gebruiken voor lazy loading.
        Dim Node As TreeNode
        Dim NewNode As TreeNode

        Dim MyNodes As New List(Of TreeNode)

        DicChildNodes = New Dictionary(Of TreeNode, TreeNodeCollection)

        For Each Node In Nodes
            NewNode = CloneNode(Node) ' New tr Me.Nodes.Add(Node.Name, Node.Text, Node.ImageKey) 'Clone de Node

            If Node.Nodes.Count > 0 Then
                DicChildNodes.Add(NewNode, Node.Nodes)
                NewNode.Nodes.Add("EasyCareDummy")
            End If

            MyNodes.Add(NewNode)
        Next

        Me.Nodes.AddRange(MyNodes.ToArray)

    End Sub

    Private Function CloneNode(N As TreeNode) As TreeNode
        Dim Result As New TreeNode

        With Result
            .Text = N.Text
            .ImageKey = N.ImageKey
            .SelectedImageKey = N.SelectedImageKey
            .ImageIndex = N.ImageIndex
            .SelectedImageIndex = N.SelectedImageIndex

            .Tag = N.Tag

        End With

        Return Result

    End Function

    Public Sub Clear()
        DicChildNodes = Nothing

        Me.BeginUpdate() 'Lijkt onlogisch, maar is sneller.
        Me.SuspendRedraw
        Me.SelectedNode = Nothing

        Me.Nodes.Clear()
        Me.ResumeRedraw
    End Sub


    Public Sub SortChildNodes(ByVal Node As TreeNode, Optional ByVal Sorter As IComparer = Nothing, Optional ByVal TopLevelOnly As Boolean = True)

        'Return


        If Node Is Nothing Then Return
        If Sorter Is Nothing Then Sorter = New DefaultNodeComparer

        If bBusySorting Then Return 'Als er een expanded node wordt toegevoegd aan een treeview, gaat het event 'AfterExpand' opnieuw af.




        bBusySorting = True

        Me.BeginUpdate()
        Dim MyArray(Node.Nodes.Count - 1) As TreeNode
        Node.Nodes.CopyTo(MyArray, 0)
        Array.Sort(MyArray, Sorter)

        'For Each N As TreeNode In MyArray
        '    If N.IsExpanded Then N.Collapse()
        'Next

        Node.Nodes.Clear()
        Node.Nodes.AddRange(MyArray)


        If Not TopLevelOnly Then
            For Each ChildNode As TreeNode In Node.Nodes
                If ChildNode.Nodes.Count > 0 Then
                    SortChildNodes(Node, Sorter, False)
                End If
            Next
        End If

        Me.EndUpdate()

        bBusySorting = False
    End Sub

    Public Sub SortParentNodes(Order As SortOrder)
        Dim sSelectedPath As String = ""
        Dim MyNode As TreeNode

        Dim Sorter As New DefaultNodeComparer
        Sorter.SortOrder = Order

        If Not Me.SelectedNode Is Nothing Then
            sSelectedPath = Me.SelectedNode.FullPath
        End If

        If Sorter Is Nothing Then Sorter = New DefaultNodeComparer
        Me.BeginUpdate()
        Dim MyArray(Me.Nodes.Count - 1) As TreeNode
        Me.Nodes.CopyTo(MyArray, 0)
        Array.Sort(MyArray, Sorter)
        Me.Nodes.Clear()
        Me.Nodes.AddRange(MyArray)

        If sSelectedPath <> "" Then
            MyNode = Me.GetNodeByFullPath(sSelectedPath)
            If Not MyNode Is Nothing Then
                Me.SelectedNode = MyNode
            End If
        End If

        Sorter = Nothing

        Me.EndUpdate()
    End Sub

    Public Sub SortParentNodes(Optional ByVal Sorter As IComparer = Nothing)
        Dim sSelectedPath As String = ""
        Dim MyNode As TreeNode

        If Not Me.SelectedNode Is Nothing Then
            sSelectedPath = Me.SelectedNode.FullPath
        End If

        If Sorter Is Nothing Then Sorter = New DefaultNodeComparer
        Me.BeginUpdate()
        Dim MyArray(Me.Nodes.Count - 1) As TreeNode
        Me.Nodes.CopyTo(MyArray, 0)
        Array.Sort(MyArray, Sorter)
        Me.Nodes.Clear()
        Me.Nodes.AddRange(MyArray)

        If sSelectedPath <> "" Then
            MyNode = Me.GetNodeByFullPath(sSelectedPath)
            If Not MyNode Is Nothing Then
                Me.SelectedNode = MyNode
            End If
        End If

        Me.EndUpdate()
    End Sub

    Public Function GetNodeByFullPath(ByVal FullPath As String) As TreeNode
        Dim MyNode As TreeNode = Nothing

        For Each MyNode In Me.AllNodes
            If MyNode.FullPath = FullPath Then
                Return MyNode
            End If
        Next

        Return Nothing
    End Function

    Private bMouseUpBusy As Boolean = False

    Private Sub TreeView_MouseUp(sender As Object, e As MouseEventArgs) Handles Me.MouseUp
        If e.Button <> Windows.Forms.MouseButtons.Right Then Return

        'Static bBusy As Boolean = False
        If bMouseUpBusy Then Return
        bMouseUpBusy = True

        Dim MyItem As TreeNode = Me.HitTest(e.Location.X, e.Location.Y).Node

        Me.SelectedNode = MyItem

        Dim MyMenu As New MCSCTRLS2.ContextMenu

        If MyItem Is Nothing Then
            RaiseEvent CreateTreeviewContextMenu(MyMenu)
        Else
            RaiseEvent CreateItemContextMenu(MyItem, MyMenu)
        End If

        If MyMenu.MenuItems.Count > 0 Then
            Dim sResult As String
            sResult = MyMenu.Show(Me)
            If sResult <> "" Then
                RaiseEvent ContextMenuClick(sResult, MyItem)
            End If
        End If

        MyMenu.Dispose()
        bMouseUpBusy = False
    End Sub

    Private bHasExpandedNodes As Boolean

    Private Sub TreeView_AfterExpand(sender As Object, e As TreeViewEventArgs) Handles Me.AfterExpand
        bHasExpandedNodes = True
    End Sub

    Private Sub TreeView_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs) Handles Me.BeforeExpand
        EasyCare.Hourglass = True
        CheckLazyLoadingNodes(e.Node)
        EasyCare.Hourglass = False
    End Sub

    Private Sub CheckLazyLoadingNodes(N As TreeNode)
        If N.Nodes.Count <> 1 Then Return
        If N.Nodes(0).Text <> "EasyCareDummy" Then Return
        If DicChildNodes Is Nothing Then Return
        If Not DicChildNodes.ContainsKey(N) Then Return

        Dim ChildNodes As TreeNodeCollection
        Dim MyClone As TreeNode
        Dim MyNodes As New List(Of TreeNode)

        ChildNodes = DicChildNodes(N)

        DicChildNodes.Remove(N)

        For Each ChildNode As TreeNode In ChildNodes
            MyClone = CloneNode(ChildNode)

            If ChildNode.Nodes.Count > 0 Then
                DicChildNodes.Add(MyClone, ChildNode.Nodes)
                MyClone.Nodes.Add("EasyCareDummy")
            End If

            MyNodes.Add(MyClone)
        Next


        N.Nodes.Clear()

        MyNodes.Sort(AddressOf GetNodeCompareResult_Text)

        N.Nodes.AddRange(MyNodes.ToArray)

    End Sub

    Private Function GetNodeCompareResult_Text(x As TreeNode, y As TreeNode) As Integer
        Return x.Text.CompareTo(y.Text)
    End Function

    '#Region "Probeer flikkeren tegen te gaan..."

    '    Public Const PRF_CLIENT As Integer = &H4

    '    Protected Overrides Sub OnPaint(e As PaintEventArgs)
    '        If GetStyle(ControlStyles.UserPaint) Then
    '            Dim m As New Message()
    '            m.HWnd = Handle

    '            m.Msg = FrameworkNS.Functions.API.Messages.WM_PRINTCLIENT
    '            m.WParam = e.Graphics.GetHdc()
    '            m.LParam = New IntPtr(PRF_CLIENT)
    '            DefWndProc(m)
    '            e.Graphics.ReleaseHdc(m.WParam)
    '        End If
    '        MyBase.OnPaint(e)
    '    End Sub
    '#End Region
End Class

Public Class DefaultNodeComparer
    Implements IComparer

    Public Property SortOrder As SortOrder = SortOrder.None

    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
        Dim sValue As String = Split(x.text, " ")(0)
        If Me.SortOrder = SortOrder.None Then
            If BegintMetDatum(sValue) Then  'Het is een datum
                Me.SortOrder = SortOrder.Descending
            Else
                Me.SortOrder = SortOrder.Ascending
            End If
        End If
        Return FrameWorkNS.Functions.Algemeen.DoDefaultSort(x.Text, y.Text, Me.SortOrder)
    End Function
    Private Function BegintMetDatum(ByVal S As String) As Boolean
        If Microsoft.VisualBasic.IsDate(S) Then Return True
        If S.Length > 9 Then If Microsoft.VisualBasic.IsDate(S.Substring(0, 10)) Then Return True
        Return False
    End Function
End Class