﻿Imports System.Windows.Controls.Primitives
Imports System.Windows.Controls.Canvas
Imports System.Collections.Specialized
Imports System.Globalization

Public Class ExImage
    Inherits Image
    Private syoki As Point
    Public Event ExDragDelta(sender As Object, e As DragDeltaEventArgs)
    Private Main As MainWindow

    Public Sub New(o As MainWindow)
        Main = o
    End Sub

    'マウス左クリックダウンの時
    Protected Overrides Sub OnMouseLeftButtonDown(e As MouseButtonEventArgs)
        MyBase.OnMouseLeftButtonDown(e)
        syoki = e.GetPosition(Me)
        Me.CaptureMouse()

        Call Main.AjustGrid(Me) 'グリッドに合わせる
        Main.FocusExImage = Me 'クリックしたExImageを記録
        Call Main.UpdateDisplayZIndex() 'textBlockのZIndex表示更新
    End Sub

    'マウスドラッグ
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If e.LeftButton = MouseButtonState.Pressed Then
            Dim p As Point = Point.Subtract(e.GetPosition(Me), syoki)
            'RaiseEvent ExDragDelta(Me, New DragDeltaEventArgs(p.X, p.Y))

            Dim x As Integer = p.X
            Dim y As Integer = p.Y
            RaiseEvent ExDragDelta(Me, New DragDeltaEventArgs(x, y))
        End If
    End Sub
    Protected Overrides Sub OnMouseUp(e As MouseButtonEventArgs)
        MyBase.OnMouseUp(e)
        Me.ReleaseMouseCapture()
    End Sub

    'マウス左クリックアップの時
    Protected Overrides Sub OnPreviewMouseLeftButtonUp(e As MouseButtonEventArgs)
        MyBase.OnPreviewMouseLeftButtonUp(e)
        Main.AjustLocation()
        Main.StatusBarDisplayUpdate(Me)
    End Sub

    ''元の位置と変形後(実際の描画)の位置の差
    'Public Property RenderDiffLocate As Point


    '座標、親コンテナのCanvasに対する自身の左上の座標
    'Canvas.GetLeft,Canvas.GetTopで得られる値
    Private _Location As Point
    Public Property Location As Point
        Get
            Return _Location
        End Get
        Set(value As Point)
            _Location = value
            '値が変更されたら、表示位置も変更してステータスバーの表示も更新
            SetLeft(Me, value.X)
            SetTop(Me, value.Y)
            Main.StatusBarDisplayUpdate(Me)
        End Set
    End Property

    '元の画像サイズ
    Public Property SourceImageSize As Size

    '元の画像ファイル名
    Public Property FileName As String


End Class


'汎用ジェネリックコレクション その2 ObservableCollection/ReadOnlyObservableCollection (System.Collections.ObjectModel) - Programming/.NET Framework/コレクション - 総武ソフトウェア推進所
'http://smdn.jp/programming/netfx/collections/3_objectmodel_2_observablecollection/
'ObservableCollectionはItemの移動ができる、追加、削除、移動した時のメソッドをOverridesできる
Public Class ObservableCollectionExImage
    Inherits ObjectModel.ObservableCollection(Of ExImage)
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
    Protected Overrides Sub InsertItem(index As Integer, item As ExImage)
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

'Public Class cc
'    Implements IValueConverter

'    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
'        Return value * 2
'        Throw New NotImplementedException()
'    End Function

'    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
'        Throw New NotImplementedException()
'    End Function
'End Class

'Public Class kakeru
'    Implements IMultiValueConverter

'    Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
'        Dim d As Integer = values(0)
'        For i As Integer = 0 To values.Length - 1
'            d *= values(i)
'        Next
'        Return d.ToString

'        Throw New NotImplementedException()
'    End Function

'    Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
'        Throw New NotImplementedException()
'    End Function
'End Class