Imports System.Windows.Controls.Primitives
Imports System.Windows.Controls.Canvas
Imports System.Collections.Specialized
Imports System.Globalization
Imports System.ComponentModel

Public Class ExThumb
    Inherits Thumb
    Private Main As MainWindow
    Public Sub SetMain(owner As MainWindow)
        Main = owner
        'Dim img As ExImage = Me.Template.FindName("image1", Me)
        'TemplateImage = img
    End Sub

    Public Sub New()
        VisualBitmapScalingMode = BitmapScalingMode.Fant

    End Sub

    Public Property TemplateImage As ExImage
    'Public TemplateImage As ExImage
    'Public Property FileName As String

    '座標、親コンテナのCanvasに対する自身の左上の座標
    'Canvas.GetLeft,Canvas.GetTopで得られる値
    '内部座標(Canvasにセットする方)
    Private _LocationInside As Point
    Public Property LocationInside As Point
        Get
            Return _LocationInside
        End Get
        Set(value As Point)
            _LocationInside = value
            '値が変更されたら、表示位置も変更してステータスバーの表示も更新
            'SetLeft(Me, value.X)
            'SetTop(Me, value.Y)
            LocateX = value.X
            LocateY = value.Y
            '再描画！！！！
            Main.ReRender(Me)
            'Main.StatusBarDisplayUpdate(Me)
            Main.RefreshStatusBarLocate(Me)
        End Set
    End Property

    Public Shared ReadOnly Property LocatePropertyX As DependencyProperty =
        DependencyProperty.Register("LocateX", GetType(Double), GetType(ExThumb), New PropertyMetadata(0R))
    Public Property LocateX As Double
        Get
            Return GetValue(LocatePropertyX)
        End Get
        Set(value As Double)
            SetValue(LocatePropertyX, value)
            SetLeft(Me, value)
            SetTop(Me, LocateY)
        End Set
    End Property

    Public Shared ReadOnly Property LocatePropertyY As DependencyProperty =
        DependencyProperty.Register("LocateY", GetType(Double), GetType(ExThumb), New PropertyMetadata(0R))
    Public Property LocateY As Double
        Get
            Return GetValue(LocatePropertyY)
        End Get
        Set(value As Double)
            SetValue(LocatePropertyY, value)
            SetLeft(Me, LocateX)
            SetTop(Me, value)
        End Set
    End Property

    '見た目上の座標(グリッドに合わせる方)
    Public Property LocationSurface As Point

    'バックアップ画像
    Public Property BackupBitmap As BitmapSource

    ''SaveData
    'Public Property ExSaveData As SaveData

    'マウスクリックダウン
    '右でも左でも選択画像を変更するのでステータスバーも更新
    Protected Overrides Sub OnMouseDown(e As MouseButtonEventArgs)
        MyBase.OnMouseDown(e)
        Main.FocusExThumb = Me
        'Call Main.AjustLocation()
        Call Main.AdjustGrid(Me)
        'ステータスバー更新
        Call Main.RefreshStatusBarAll(Me)
        Call Main.RefreshDisplayZIndex()
        'フォーカスをtbxDummyに移す、必要ないかも
        Main.tbxDummy.Focus()
    End Sub


End Class


'汎用ジェネリックコレクション その2 ObservableCollection/ReadOnlyObservableCollection (System.Collections.ObjectModel) - Programming/.NET Framework/コレクション - 総武ソフトウェア推進所
'http://smdn.jp/programming/netfx/collections/3_objectmodel_2_observablecollection/
'ObservableCollectionはItemの移動ができる、追加、削除、移動した時のメソッドをOverridesできる
Public Class ObservableCollectionExThumb
    Inherits ObjectModel.ObservableCollection(Of ExThumb)
    Private Main As MainWindow
    Public Sub New(owner As MainWindow)
        Main = owner
    End Sub

    'Item移動の時
    Protected Overrides Sub MoveItem(oldIndex As Integer, newIndex As Integer)
        '移動先のIndexが全画像数より大きいか0未満ならなにもしないで終了
        If newIndex >= Count OrElse newIndex < 0 Then Return
        MyBase.MoveItem(oldIndex, newIndex)
        SetZIndex(Item(oldIndex), oldIndex)
        SetZIndex(Item(newIndex), newIndex)
    End Sub

    'Insertの他にAddでも発生する、発生しないのはRemoveを確認
    Protected Overrides Sub InsertItem(index As Integer, item As ExThumb)
        MyBase.InsertItem(index, item)
        SetZIndex(item, index)
        For i As Integer = index To Count - 1
            SetZIndex(Me.Item(i), i)

        Next
    End Sub

    '削除された時
    Protected Overrides Sub RemoveItem(index As Integer)
        MyBase.RemoveItem(index)
        '削除対象より上にあるItemのZIndexを変更する
        For i As Integer = index To Count - 1
            SetZIndex(Me.Item(i), i)
        Next
    End Sub

    ''Itemに変更があった時に発生？タイミングはRemoveItemより先
    'Protected Overrides Sub OnCollectionChanged(e As NotifyCollectionChangedEventArgs)
    '    MyBase.OnCollectionChanged(e)
    '    'ZIndexの表示更新
    '    Main.UpdateDisplayZIndex()
    'End Sub
End Class






'WPF Imageコントロールの拡大・縮小アルゴリズム変更方法 - Qiita
'http://qiita.com/Nuits/items/c23919eb21db3445dadf
'見本画像表示用、縮小表示を綺麗に表示
Public Class HighQualiyImage
    Inherits Image
    Protected Overrides Sub OnRender(dc As DrawingContext)
        'Me.VisualBitmapScalingMode = BitmapScalingMode.Fant'HighQualityと同等らしい
        Me.VisualBitmapScalingMode = BitmapScalingMode.HighQuality
        MyBase.OnRender(dc)
    End Sub

End Class


'Thumbの中に入れるImage、元画像のサイズを入れるだけに作った
Public Class ExImage
    Inherits Image
    Public Sub New()
        VisualBitmapScalingMode = BitmapScalingMode.Fant
    End Sub
    Protected Overrides Sub OnRenderSizeChanged(sizeInfo As SizeChangedInfo)
        MyBase.OnRenderSizeChanged(sizeInfo)
        Dim neko As BitmapSource = Me.Source
        SourceSize = New Size(neko.PixelWidth, neko.PixelHeight)
    End Sub
    Public Property SourceSize As Size

    '拡大縮小などの変形時の描画品質設定
    Protected Overrides Sub OnRender(dc As DrawingContext)
        MyBase.OnRender(dc)
        VisualBitmapScalingMode = BitmapScalingMode.NearestNeighbor 'ニアレストネイバー(最近傍法)
        VisualBitmapScalingMode = BitmapScalingMode.Fant 'HighQualityと同等

    End Sub
End Class


<Serializable>
Public Class SaveData
    Implements INotifyPropertyChanged
    <NonSerialized>
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Private IsChangeX As Boolean = False '縦横同期の時のループ防止用
    Private IsChangeY As Boolean = False

    '回転角度
    Private Property _Angle As Double
    Public Property Angle As Double
        Get
            Return _Angle
        End Get
        Set(value As Double)
            _Angle = value
            Call OnPropertyChanged("Angle")
        End Set
    End Property

    '拡大率横
    Private Property _ScaleX As Double
    Public Property ScaleX As Double
        Get
            Return _ScaleX
        End Get
        Set(value As Double)
            IsChangeX = True '変更中開始
            _ScaleX = value
            Call OnPropertyChanged("ScaleX")
            If ScaleSync And IsChangeY = False Then '縦横同期ならYも変更
                ScaleY = value
            End If
            IsChangeX = False '変更中終了
        End Set
    End Property

    '拡大率縦
    Private Property _ScaleY As Double
    Public Property ScaleY As Double
        Get
            Return _ScaleY
        End Get
        Set(value As Double)
            IsChangeY = True
            _ScaleY = value
            Call OnPropertyChanged("ScaleY")
            If ScaleSync And IsChangeX = False Then
                ScaleX = value
            End If
            IsChangeY = False
        End Set
    End Property

    '拡大率縦横同期
    Private Property _ScaleSync As Boolean
    Public Property ScaleSync As Boolean
        Get
            Return _ScaleSync
        End Get
        Set(value As Boolean)
            _ScaleSync = value
            Call OnPropertyChanged("ScaleSync")
        End Set
    End Property

    '傾斜横
    Private Property _SkewX As Double
    Public Property SkewX As Double
        Get
            Return _SkewX
        End Get
        Set(value As Double)
            _SkewX = value
            Call OnPropertyChanged("SkewX")
        End Set
    End Property

    '傾斜縦
    Private Property _SkewY As Double
    Public Property SkewY As Double
        Get
            Return _SkewY
        End Get
        Set(value As Double)
            _SkewY = value
            Call OnPropertyChanged("SkewY")
        End Set
    End Property

    Public Property Left As Double
    Public Property Top As Double

End Class