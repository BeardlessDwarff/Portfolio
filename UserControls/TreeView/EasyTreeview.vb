Public Class Treeview
    Public Event CreateItemContextMenu(ClickedItem As TreeNode, ByRef Menu As MCSCTRLS2.ContextMenu)
    Public Event CreateTreeviewContextMenu(ByRef Menu As MCSCTRLS2.ContextMenu)
    Public Event ContextMenuClick(Key As String, ClickedItem As TreeNode)
    Public Property ScrollWindowUnderMousePointer As Boolean = True
    Public Property AllowTreenodeReorder As Boolean = False

    Private T As System.Windows.Forms.TreeView

    Public ReadOnly Property Nodes As New EasyTreenodeCollection
    Public Property ImageList As ImageList

    Public Function AddImageToImageList(Key As String, Image As Image) As Boolean
        Dim iIndex As Integer

        If Me.ImageList Is Nothing Then Return False
        If Image Is Nothing Then Return False

        iIndex = FrameWorkNS.Functions.GDI.AddImageToImageList(Me.ImageList, Image, Key)

        Return iIndex >= 0
    End Function

    Public Function AddNodeWithGroups(FullPath As String, GroupImageKey As String, ItemImageKey As String) As TreeNode
        Dim arrPath() As String = FullPath.Split("\")
        Return AddNodeWithGroups(arrPath, GroupImageKey, ItemImageKey)
    End Function

    Public Function AddNodeWithGroups(arrPath As String(), GroupImageKey As String, ItemImageKey As String) As TreeNode
        Stop
    End Function

    Public Sub BeginUpdate()

    End Sub

    Public Sub EndUpdate()

    End Sub

    Public Sub AddRange()

    End Sub
    Public Function AllNodes() As List(Of TreeNode)

    End Function

    Public Property CheckBoxes As Boolean
    Public Sub CollapseAll()

    End Sub

    Public Function GetNodeAt(P As Point) As TreeNode

    End Function

    Public Property LabelEdit As Boolean

    Public Function GetNodeByKey(ByVal Key As String) As TreeNode

    End Function

    Public Property ImageIndex As Integer
    Public Property LineColor As Color = Color.Black
    Public Property SelectedImageIndex As Integer
    Public Property ShowLines As Boolean
    Public Property ShowPlusMinus As Boolean
    Public Property ShowRootLines As Boolean

    Public Sub SortChildNodes(ByVal Node As TreeNode, Order As SortOrder, Optional ByVal TopLevelOnly As Boolean = True)

    End Sub
    Public Sub SortChildNodes(ByVal Node As TreeNode, Optional ByVal Sorter As IComparer = Nothing, Optional ByVal TopLevelOnly As Boolean = True)

    End Sub

    Public Property Sorted As Boolean

    Public Sub Sort()

    End Sub
    Private Sub test()

    End Sub

    Public Sub ExpandAll()

    End Sub

    Public Property HideSelection As Boolean
    Private MyNodeSelecterenMousedown As Boolean = True

    Public Property DrawMode As TreeViewDrawMode
    Public Sub BeginPrint()

    End Sub
    Public Property NodeSelecterenMousedown() As Boolean
        Get
            Return MyNodeSelecterenMousedown
        End Get
        Set(ByVal value As Boolean)
            MyNodeSelecterenMousedown = value
        End Set
    End Property

    Public Event NodeMouseClick(ByVal sender As Object, ByVal e As TreeNodeMouseClickEventArgs)
    Public Event NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs)
    Public Property SelectedNode As TreeNode

End Class

Public Class EasyTreenodeCollection
    Implements IList(Of TreeNode)

    Public Sub AddRange(Nodes() As TreeNode)

    End Sub
    Default Public Property Item(index As Integer) As TreeNode Implements IList(Of TreeNode).Item
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As TreeNode)
            Throw New NotImplementedException()
        End Set
    End Property

    Public ReadOnly Property Count As Integer Implements ICollection(Of TreeNode).Count
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of TreeNode).IsReadOnly
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Sub Insert(index As Integer, item As TreeNode) Implements IList(Of TreeNode).Insert
        Throw New NotImplementedException()
    End Sub

    Public Sub RemoveAt(index As Integer) Implements IList(Of TreeNode).RemoveAt
        Throw New NotImplementedException()
    End Sub

    Public Function Add(Text As String) As TreeNode

    End Function
    Public Sub Add(item As TreeNode) Implements ICollection(Of TreeNode).Add
        Throw New NotImplementedException()
    End Sub

    Public Sub Clear() Implements ICollection(Of TreeNode).Clear
        Throw New NotImplementedException()
    End Sub

    Public Sub CopyTo(array() As TreeNode, arrayIndex As Integer) Implements ICollection(Of TreeNode).CopyTo
        Throw New NotImplementedException()
    End Sub

    Public Function IndexOf(item As TreeNode) As Integer Implements IList(Of TreeNode).IndexOf
        Throw New NotImplementedException()
    End Function

    Public Function Contains(item As TreeNode) As Boolean Implements ICollection(Of TreeNode).Contains
        Throw New NotImplementedException()
    End Function

    Public Function Remove(item As TreeNode) As Boolean Implements ICollection(Of TreeNode).Remove
        Throw New NotImplementedException()
    End Function

    Public Function GetEnumerator() As IEnumerator(Of TreeNode) Implements IEnumerable(Of TreeNode).GetEnumerator
        Throw New NotImplementedException()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException()
    End Function

    Public Function ContainsKey(Key As String) As Boolean
        If Debugger.IsAttached Then Stop
    End Function

    Public Function All() As List(Of TreeNode)

    End Function
End Class