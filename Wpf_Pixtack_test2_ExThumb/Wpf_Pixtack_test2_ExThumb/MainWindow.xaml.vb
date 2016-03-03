Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Controls.Panel
Imports System.Windows.Controls.Canvas

Class MainWindow
    'すべてのExThumbを入れておくリストコレスション
    Private OCollectionExThumb As New ObservableCollectionExThumb(Me)
    Public IsGetColor As Boolean 'クリックで色取得フラグ




    Private WithEvents _FocusExThumb As ExThumb
    Public Property FocusExThumb As ExThumb
        Get
            Return _FocusExThumb
        End Get
        Set(value As ExThumb)
            _FocusExThumb = value

            '見本画像更新とか
            If value Is Nothing Then
                mihon.Source = Nothing
            Else
                mihon.Source = value.TemplateImage.Source ' value.Source
                '表示位置とCanvasサイズ調整、これは実行しなくてもいいかなあ
                Call AdjustLocation()
            End If

            ''FocusExThumbの中身が入れ替わった時実行
            'Call StatusBarDisplayUpdate(value)
        End Set
    End Property


    Public Sub RefreshStatusBar(ex As ExThumb)
        If ex Is Nothing Then

        Else
            Dim allR As Rect = GetUnion()
            Dim l As Point = ex.LocationInside
            stSaveSize.Content = $"保存サイズ({allR.Width}x{allR.Height})"

            stLocate.Content = $"座標({ex.LocationInside.ToString})"

            stImaSize.Content = $"サイズ({GetRect(ex).Size})"
            Dim bmp As ImageSource = ex.TemplateImage.Source

            stMotoSize.Content = $"{ex.TemplateImage.SourceSize.ToString()}"
        End If
    End Sub

    'テンプレートの中のコントロールを取得するTemplate.FindName
    Private Function GetTemplateImage(t As ExThumb) As ExImage
        'Dim neko = t.Template.FindName("image1", t)
        Return t.Template.FindName("image1", t)
    End Function


    'ExThumbを左上のグリッドに移動
    '画像を回転したり拡大表示などの変形すると見た目の大きさが変わるだけで
    '位置を指定するCanvas.SetLeftとかで使われるのは変形する前の元の大きさになる
    'でもグリッドに合わせることになるのは変形後の今表示している形の位置なので
    '変形後の位置がグリッドかどれだけずれているかを求めて
    'そのズレの分を元の画像に適用すればいい
    Public Sub AdjustGrid(ex As ExThumb)
        Dim p As Point = GetRect(ex).Location '変形位置取得、今表示している形の位置
        'Dim p As Point = ex.LocationInside '変形位置取得、

        Dim g As Integer = gridSdr.Value '指定グリッド数値取得

        'ズレの取得、今表示している形の位置がグリッドからどれだけずれているか
        Dim xm As Double = p.X Mod g '横位置をグリッドで割った余り
        Dim ym As Double = p.Y Mod g '縦位置を～

        Dim np As New Point(ex.LocationInside.X - xm, ex.LocationInside.Y - ym)
        ex.LocationInside = np



    End Sub

    '位置調整、移動後の座標がマイナスだった時に全部の画像をマイナス分をプラスにして調整する
    '、左上の画像を削除して空白ができた時に
    Public Sub AdjustLocation()
        'すべての画像がピッタリ収まるRect
        Dim r As Rect = GetUnion()
        'Canvasサイズを変更
        canvas1.Width = r.Width
        canvas1.Height = r.Height

        ''移動した画像がマイナス座標じゃなければ終了
        'If FocusExThumb.Location.X > 0 AndAlso FocusExThumb.Location.Y > 0 Then Return

        'すべての画像の位置を調節
        If r.X <> 0 OrElse r.Y <> 0 Then
            For Each ex As ExThumb In OCollectionExThumb
                '座標をセット
                ex.LocationInside -= r.Location
            Next
        End If
    End Sub






    'マウスドラッグ移動、グリッドに合わせた移動
    Private Sub ExThumb_DragDelta(sender As Object, e As DragDeltaEventArgs)
        Dim g As Integer = gridSdr.Value
        Dim ex As ExThumb = DirectCast(sender, ExThumb)
        '元の形の位置取得、
        'これは左クリックダウンイベントの時にAdjustGridメソッドでグリッドに合わせた位置になっているはずなので
        'あとはマウスの移動距離がグリッド値を超えたら隣のグリッドへ移動させる感じ

        ''Pixtack紫陽花と同じ方式
        'Dim xChange As Double = e.HorizontalChange
        'Dim yChange As Double = e.VerticalChange
        'Dim mx As Double = xChange Mod g '横移動距離をグリッドで割った余り
        'Dim my As Double = yChange Mod g
        ''移動先座標
        'Dim x As Double = ex.Location.X + xChange - mx '元の位置＋移動距離ー余り
        'Dim y As Double = ex.Location.Y + yChange - my
        Dim x As Double = e.HorizontalChange
        Dim y As Double = e.VerticalChange
        Dim nx As Double = x - (x Mod g) + GetLeft(ex)
        Dim ny As Double = y - (y Mod g) + GetTop(ex)

        '座標をセット
        ex.LocationInside = New Point(nx, ny)

        'SetLeft(ex, nx)
        'SetTop(ex, ny)


        '条件
        'グリッド値が8で   g=8
        '元の形の時のExThumbの位置が(24, 16)で  ex.Location=(24,16)
        'マウス移動距離が(15, 3)で   xChange=15,yChange=3
        '
        'mx = 15 Mod 8 = 7 
        'my =  3 Mod 8 = 3
        'x = 24 + 15 - 7 = 32
        'y = 16 +  3 - 3 = 16
        '移動先座標は(32,16)
        '

        'Call DisplayUpdateLocate(ex) '位置ステータスラベル更新

    End Sub

    'マウスドラッグ終了後
    Private Sub _FocusExThumb_DragCompleted(sender As Object, e As DragCompletedEventArgs) Handles _FocusExThumb.DragCompleted
        Call AdjustLocation()
    End Sub


    'ファイルパスからBitmapImage(画像)を作成して返す
    Private Function GetBitmapImage(filePath As String) As BitmapSource
        Dim bmp As New BitmapImage
        Try
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                With bmp
                    .BeginInit()
                    .StreamSource = fs
                    .CacheOption = BitmapCacheOption.OnLoad
                    .EndInit()
                    .Freeze()
                End With
            End Using
        Catch ex As Exception
            'ファイルを画像として取得できなければ空のBitmapImageを返して終了
            bmp = Nothing
            Return bmp
        End Try
        Dim fcb As New FormatConvertedBitmap(bmp, PixelFormats.Bgra32, Nothing, 0)
        Return fcb
    End Function


    Private Function SetOCollectionExThumb() As ExThumb
        Dim ex As New ExThumb()
        'ResourcesのTemplateを取得してExThumbに適用
        ex.Template = Me.Resources.Item("ct")

        ex.SetMain(Me) 'MainWindowをExThumbに登録する

        'リストコレクションに追加、ZIndexは追加した時にObservableOCExThumbの方で処理される

        If cbFocusImage.IsChecked Then
            '選択画像を基準にする場合
            Dim i As Integer = OCollectionExThumb.IndexOf(FocusExThumb)
            If rbAddUpper.IsChecked Then '上層に追加
                OCollectionExThumb.Insert(i + 1, ex)
            ElseIf rbAddLower.IsChecked Then '下層に追加
                OCollectionExThumb.Insert(i, ex)
            End If
        Else
            '最前面か最背面に追加する場合
            If rbAddUpper.IsChecked Then
                '上層に追加
                OCollectionExThumb.Add(ex)
            ElseIf rbAddLower.IsChecked Then
                '下層に追加
                OCollectionExThumb.Insert(0, ex)
            End If
        End If


        'Canvasに追加して表示
        canvas1.Children.Add(ex)

        '追加したExThumbを再描画！！！！！！！！！！！！！！！！！！！！
        '再描画しないとTemplateが取得できないのでその中のExImageも取得できない
        Call ReRender(ex)
        'InvalidateVisual()
        'ex.InvalidateVisual()
        Return ex

    End Function


    'ExThumbを追加するときの表示位置を返す
    Private Function GetNewLocation() As Point
        'ExThumbを表示する位置
        Dim newLocate As New Point(0, 0)
        If OCollectionExThumb.Count = 0 Then
            Return newLocate '最初の画像なら0,0
        End If

        Dim x As Integer = tbSliX.Text
        Dim y As Integer = tbSliY.Text
        If FocusExThumb IsNot Nothing Then
            'スライド
            newLocate = FocusExThumb.LocationInside
            If rbSetLocateFocusImage.IsChecked Then
                '選択画像に重ねる
                newLocate.Offset(x, y)
            ElseIf rbSetLocateAllImage.IsChecked Then
                '全体画像に重ねない
                newLocate = GetUnion.BottomLeft '左下
            End If
        End If
        Return newLocate
    End Function

    '連続追加時の二枚目以降の画像の位置取得
    Private Function GetSlideLocation(ex As ExThumb) As Point
        '最初の画像の位置
        Dim newLocate As Point = ex.LocationInside
        Dim x As Integer = tbSliX.Text
        Dim y As Integer = tbSliY.Text
        If ex.LocationInside.X < 0 Then x = 0
        If ex.LocationInside.Y < 0 Then y = 0
        newLocate.Offset(x, y)
        Return newLocate
    End Function

    'ウィンドウに画像ファイルがドロップされた時
    Private Sub MainWindow_Drop(sender As Object, e As DragEventArgs) Handles Me.Drop
        If e.Data.GetDataPresent(DataFormats.FileDrop) = False Then Return

        Dim filesPath() As String = e.Data.GetData(DataFormats.FileDrop) 'ファイルパス取得(配列)
        Dim listPath As New List(Of String)(filesPath.ToList) 'ファイルパスの配列をリストに変換

        'ソート
        If rbtAscent.IsChecked Then
            listPath.Sort()
        ElseIf rbtDescent.IsChecked Then
            listPath.Sort()
            listPath.Reverse()
        End If

        '最初のExThumbを表示する位置
        Dim x As Integer = tbSliX.Text
        Dim y As Integer = tbSliY.Text
        Dim newLocate As New Point(0, 0)
        If rbSetLocateDrop.IsChecked Then
            newLocate = e.GetPosition(canvas1)
        Else
            newLocate = GetNewLocation()
        End If

        'ファイルパスの数だけExThumb作成して表示
        For i As Integer = 0 To listPath.Count - 1
            'ファイルパスから画像を取得
            'Dim bmp As BitmapImage = GetBitmapImage(listPath(i))
            Dim bmp As BitmapSource = GetBitmapImage(listPath(i))
            If bmp IsNot Nothing Then

                'コレクションに追加してCanvasに表示
                Dim ex As ExThumb = SetOCollectionExThumb()
                'ここで再描画処理が入るのでGetRectとかで今の値が取得できる
                'テンプレートの中のExImage取得してSourceに画像を指定する
                'Dim tmpimg As ExImage = GetTemplateImage(ex)
                Dim img As ExImage = ex.Template.FindName("image1", ex)
                img.RenderTransformOrigin = New Point(0.5, 0.5)
                img.Source = bmp

                'Dim cc As New ColorConvertedBitmap(bmp, New ColorContext(bmp.Format), New ColorContext(PixelFormats.Pbgra32), bmp.Format)


                '各プロパティの指定
                With ex
                    .TemplateImage = img
                    '.RenderTransformOrigin = New Point(0.5, 0.5)
                    '.Source = bmp
                    '.SourceImageSize = New Size(bmp.PixelWidth, bmp.PixelHeight)
                    .FileName = listPath(i)
                    .LocationInside = newLocate '元の位置
                    '.RenderTransformOrigin = New Point(0.5, 0.5)
                    '.SizeSeem = New Size(bmp.PixelWidth, bmp.PixelHeight) '見た目のサイズ、変形後サイズ
                    '.LocationRenderDiff = New Point(0, 0) '実際と見た目の位置の差

                    '.LocationSeem = newLocate '見かけ上の位置、変形後はこの値が変化する
                    '.Height = bmp.PixelHeight 'これとWidthはどうするかな
                    '.Width = bmp.PixelWidth '指定しないと100x100の画像は100.0139になる→
                    '指定して100にするより無指定で100.0139にしておいたほうが綺麗に見える
                    '保存時には関係なさそうなので無指定で
                End With
                '次の位置
                newLocate = GetSlideLocation(ex)

                AddHandler ex.DragDelta, AddressOf ExThumb_DragDelta 'これはマウスドラッグ用

                FocusExThumb = ex '要る？→要る、画像追加時のCollectionに挿入するときに使う

            End If
        Next

    End Sub
    'ファイルドドラッグ時のマウスカーソルエフェクト
    Private Sub MainWindow_PreviewDragOver(sender As Object, e As DragEventArgs) Handles Me.PreviewDragOver
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effects = DragDropEffects.Copy
        Else
            e.Effects = DragDropEffects.None
        End If
        e.Handled = True
    End Sub



    'textBlockの表示更新
    Public Sub UpdateDisplayZIndex()
        If FocusExThumb Is Nothing Then
            stZIndex.Content = "ZIndex = " & "none"
        Else
            stZIndex.Content = "ZIndex = " & GetZIndex(FocusExThumb).ToString
        End If

    End Sub
    '1つ上に移動
    Private Sub btAge_Click(sender As Object, e As RoutedEventArgs) Handles btAge.Click
        Dim z As Integer = OCollectionExThumb.IndexOf(FocusExThumb)
        Call ZOrder(z, z + 1)
    End Sub
    '1つ下に移動
    Private Sub sage_Click(sender As Object, e As RoutedEventArgs) Handles btSage.Click
        Dim z As Integer = OCollectionExThumb.IndexOf(FocusExThumb)
        Call ZOrder(z, z - 1)
    End Sub
    '画像のZOrder指定、ExThumbのZIndex指定
    Private Sub ZOrder(Moto As Integer, Saki As Integer)
        If FocusExThumb Is Nothing Then Return
        OCollectionExThumb.Move(Moto, Saki) '移動元Index、移動先Index
        Call UpdateDisplayZIndex()
    End Sub



    '    プログラミング Windows 第6版 第10章 WPF編 - 荒井省三のBlog - Site Home - MSDN Blogs
    'http://blogs.msdn.com/b/shozoa/archive/2014/08/22/using-programming-windows-chapter10.aspx
    'ExThumbのRectを取得、回転後のRectにも対応
    Public Function GetRect(ex As ExThumb) As Rect
        'RenderSize版100.0139
        'Dim cVisual As GeneralTransform = ex.TransformToVisual(canvas1)
        'Dim r As Rect = cVisual.TransformBounds(New Rect(ex.RenderSize))
        'Return r

        'SourceのPixelWidth版100
        'Dim gt As GeneralTransform = ex.TransformToVisual(canvas1)
        'Dim s As Size = ex.TemplateImage.SourceSize
        'Dim r As Rect = gt.TransformBounds(New Rect(s))

        '内部のImage、PixelWidth版100
        Dim gt As GeneralTransform = ex.TemplateImage.TransformToVisual(canvas1)
        Dim s As Size = ex.TemplateImage.SourceSize
        Dim r As Rect ' = gt.TransformBounds(New Rect(s))

        r = gt.TransformBounds(New Rect(s))

        Return r
    End Function

    'すべてのExThumbのRectのUnionのRectを取得
    Private Function GetUnion() As Rect

        Dim r As Rect = GetRect(OCollectionExThumb(0))
        For i As Integer = 1 To OCollectionExThumb.Count - 1
            r = Rect.Union(r, GetRect(OCollectionExThumb(i)))
        Next
        Return r
    End Function


    '    [VB.NET][WPF] WPF で 描画を更新する | オールトの雲
    'http://ooltcloud.expressweb.jp/201311/article_10192523.html

    '再描画
    Public Sub ReRender(ex As ExThumb)
        ex.Dispatcher.Invoke(Threading.DispatcherPriority.Render, Sub()

                                                                  End Sub)
    End Sub



    '画像ファイルとして保存

    '        キャンバスに描いた絵を画像ファイルとして保存する | HIRO's.NET Blog
    'http://blog.hiros-dot.net/?page_id=3802
    'Daizen Ikehara : [WPF] XamQRCodeBarcode を画像として保存 [Tips]
    'http://blogs.jp.infragistics.com/blogs/dikehara/archive/2014/02/12/wpf-xamqrcodebarcode-tips.aspx
    '    RenderTargetBitmap tips - Jaime Rodriguez - Site Home - MSDN Blogs
    'http://blogs.msdn.com/b/jaimer/archive/2009/07/03/rendertargetbitmap-tips.aspx

    Private Sub SaveAllImage()
        If OCollectionExThumb.Count = 0 Then Return '画像がなければ何もしない

        'ダイアログ設定
        Dim dialogSave As New Microsoft.Win32.SaveFileDialog
        With dialogSave
            .Filter = "*.png|*.png|*.jpg|*.jpg;*.jpeg|*.bmp|*.bmp|*.gif|*.gif|*.tiff|*.tiff"
            .AddExtension = True
        End With

        'ダイアログ表示
        If dialogSave.ShowDialog Then
            '塗る範囲のRect取得、回転や変形させた画像があると小数点以下がつく
            Dim originRect As Rect = GetUnion()

            '保存画像サイズに小数点は存在しないので切り上げしたRect作成
            Dim ceilSize As New Size(Math.Ceiling(originRect.Width), Math.Ceiling(originRect.Height))
            Dim ceilingRect As New Rect(originRect.Location, ceilSize)

            '描画先を作成
            Dim dv As New DrawingVisual
            Using dc As DrawingContext = dv.RenderOpen
                Dim vb As New VisualBrush(canvas1) 'Canvas内に表示されているもの自体を使ってVisualBrush作成
                'dc.DrawRectangle(vb, Nothing, ceilingRect) '四角形にブラシで塗り、サイズ切り上げ
                dc.DrawRectangle(vb, Nothing, originRect) '四角形にブラシで塗り、サイズそのまま
            End Using

            '描画、切り上げザイズの範囲に、そのままの大きさで描画
            Dim rtb As New RenderTargetBitmap(ceilingRect.Width, ceilingRect.Height, 96, 96, PixelFormats.Pbgra32)
            rtb.Render(dv)

            '画像エンコーダ選択
            Dim enc As BitmapEncoder = Nothing
            Select Case dialogSave.FilterIndex
                Case 1
                    enc = New PngBitmapEncoder
                Case 2
                    Dim je As New JpegBitmapEncoder
                    je.QualityLevel = 97 '1-100 初期値は75
                    enc = je
                Case 3
                    enc = New BmpBitmapEncoder
                Case 4
                    enc = New GifBitmapEncoder
                Case 5
                    enc = New TiffBitmapEncoder
            End Select

            'エンコーダに画像フレームを渡す
            Dim bf As BitmapFrame = BitmapFrame.Create(rtb)
            enc.Frames.Add(bf)

            'ファイルとして保存
            Using fs As New FileStream(dialogSave.FileName, FileMode.Create)
                enc.Save(fs)
            End Using
        End If
    End Sub

    '画像ファイルとして保存
    Private Sub save_Click(sender As Object, e As RoutedEventArgs) Handles btSaveAll.Click
        Call SaveAllImage()
    End Sub

    '削除ボタン押した時
    Private Sub remove_Click(sender As Object, e As RoutedEventArgs) Handles btRemove.Click
        If FocusExThumb Is Nothing Then Return
        Call RemoveExThumb(FocusExThumb)
    End Sub
    '全削除ボタン押した時
    Private Sub RemoveAll_Click() Handles btRemoveAll.Click
        If FocusExThumb Is Nothing Then Return
        For i As Integer = 0 To OCollectionExThumb.Count - 1
            Call RemoveExThumb(OCollectionExThumb(0))
        Next
    End Sub

    '選択画像削除
    Private Sub RemoveExThumb(ex As ExThumb)
        Dim i As Integer = OCollectionExThumb.IndexOf(ex) '削除対象のIndex取得
        OCollectionExThumb.Remove(ex) 'リストコレクションから削除
        canvas1.Children.Remove(ex) 'canvas1から削除(表示から消す)
        ''画像が1つもなければ空にする
        '削除後のFocusExThumbはひとつ上のものにする
        '削除ExThumbが一番上ならひとつ下のものにする
        If OCollectionExThumb.Count = 0 Then
            FocusExThumb = Nothing
        ElseIf i = OCollectionExThumb.Count Then
            FocusExThumb = OCollectionExThumb(i - 1)
        Else
            FocusExThumb = OCollectionExThumb(i)
        End If
        Call UpdateDisplayZIndex()
    End Sub



    Private Sub hyouzi11() Handles menu1.Click
        Dim zero As New GridLength(0)

        If gridColumn0.Width = zero Then
            gridColumn0.Width = New GridLength(240)
        Else
            gridColumn0.Width = zero
        End If
    End Sub
    Private Sub hyouzi22() Handles menu2.Click
        If grid2.Height > 0 Then
            grid2.Height = 0
        Else
            grid2.Height = 200
        End If
    End Sub

    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        ''        Re[8]: WPF での画像の拡大
        ''http://bbs.wankuma.com/index.cgi?mode=al2&namber=51404&KLOG=86
        ''Imageコントロールの拡大縮小表示の品質設定
        'RenderOptions.SetBitmapScalingMode(mihon, BitmapScalingMode.HighQuality)
    End Sub




    '    WPFのClipboard.GetImage()のバグ？ ( ソフトウェア ) - Simple Is best - Yahoo!ブログ
    'http://blogs.yahoo.co.jp/elku_simple/35320048.html
    '    BitmapSourceの変換 - 反省しています
    'http://d.hatena.ne.jp/funct/20150929/1443538237

    'クリップボードに画像があったら追加する
    Private Function AddFromClipboard() As BitmapImage
        Dim bi As New BitmapImage 'system.windows.media.imaging.bitmapimage
        If My.Computer.Clipboard.ContainsImage Then
            'Dim b = My.Computer.Clipboard.GetData(DataFormats.Bitmap) 'system.drawing.bitmap
            'Dim dib As MemoryStream = My.Computer.Clipboard.GetData(DataFormats.Dib) 'system.io.memorystream
            'Dim em = Clipboard.GetData(DataFormats.EnhancedMetafile) 'エクセルの図形とか取得できる
            'Dim mp = Clipboard.GetData(DataFormats.MetafilePicture)
            'Dim ti = Clipboard.GetData(DataFormats.Tiff)





            'Alphaがおかしいけどクリップボードから画像取得
            Dim bs As BitmapSource = Clipboard.GetImage 'system.windows.interop.interopBitmap
            'ピクセルフォーマットを変更する
            'Dim bbb As New FormatConvertedBitmap(bs, PixelFormats.Pbgra32, Nothing, 0) 'バグでAlphaがおかしくなる
            Dim bbb As New FormatConvertedBitmap(bs, PixelFormats.Bgr32, Nothing, 0) 'Alphaはなくなるけどそれ以外はまとも
            'Dim bbb As New FormatConvertedBitmap(bs, PixelFormats.Gray2, Nothing, 0) '白黒になる

            'Dim enc As New BmpBitmapEncoder
            ''Dim enc As New PngBitmapEncoder

            'enc.Frames.Add(BitmapFrame.Create(bs))


            'Dim imageB() As Byte = Nothing
            'Using reader As New BinaryReader(dib)
            '    imageB = reader.ReadBytes(dib.Length - 1)
            'End Using
            ''enc.Frames.Add(BitmapFrame.Create(bs.PixelWidth, bs.PixelHeight, 96, 96,
            ''                                   PixelFormats.Bgr32, Nothing, imageB, bs.PixelWidth * 4))
            'Dim bf As BitmapSource
            'bf = BitmapSource.Create(bs.PixelWidth, bs.PixelHeight, 96, 96,
            'PixelFormats.Pbgra32, Nothing, imageB, bs.PixelWidth * 4)
            'enc.Frames.Add(BitmapFrame.Create(bf))

            'Using mst As New MemoryStream()
            '    enc.Save(mst)
            '    mst.Seek(0, SeekOrigin.Begin)
            '    Dim dec = BitmapDecoder.Create(mst, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
            '    bs = dec.Frames(0)

            'End Using


            '作成して表示
            'ファイルパスからの時はBitmapImageだったけど
            'これはBitmapSourceになっているのが気になる
            Dim ex As ExThumb = SetOCollectionExThumb()
            Dim img As Image = GetTemplateImage(ex)


            img.Source = bbb
            With ex
                '.TemplateImage.Source = bbb ' bf ' bs
                .TemplateImage = img
                .RenderTransformOrigin = New Point(0.5, 0.5)
                '.SourceImageSize = New Size(bs.PixelWidth, bs.PixelHeight)
                .LocationInside = GetNewLocation()

            End With
            FocusExThumb = ex
            AddHandler ex.DragDelta, AddressOf ExThumb_DragDelta
            'AddHandler ex.Loaded, AddressOf ExThumb_Loaded
            Return bi
        Else
            MsgBox("クリップボードの中に画像はありませんでした")
            Return New BitmapImage
        End If
        Return New BitmapImage
    End Function

    'クリップボードから画像追加
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs) Handles btClipboard.Click
        Call AddFromClipboard()
    End Sub

    'Private Sub age_Click(sender As Object, e As RoutedEventArgs) Handles age.Click
    '    'Dim r As Rect = GetRect(FocusExThumb)
    '    'Dim u As Rect = GetUnion()
    '    'Call AdjustLocation()
    '    'Call testApplyRotate()
    'End Sub




    '画像追加時のスライド量
    Private Sub sliX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) ' Handles sliX.ValueChanged
        tbSliX.Text = $"{e.NewValue * gridSdr.Value:000}"
    End Sub

    Private Sub sliY_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) 'Handles sliY.ValueChanged
        tbSliY.Text = $"{e.NewValue * gridSdr.Value:000}"
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        '画像追加時のスライド量のスライダーのValueChangedイベントに付加
        AddHandler sliX.ValueChanged, AddressOf sliX_ValueChanged
        AddHandler sliY.ValueChanged, AddressOf sliY_ValueChanged
        AddHandler gridSdr.ValueChanged, AddressOf gridSdr_ValueChanged

        '上のイベントを実行してスライド量の表示更新
        sliX_ValueChanged(sliX, New RoutedPropertyChangedEventArgs(Of Double)(0, sliX.Value))
        sliY_ValueChanged(sliY, New RoutedPropertyChangedEventArgs(Of Double)(0, sliY.Value))

        '拡大縮小描画モード
        AddHandler rbScaleHigh.Checked, AddressOf rbScaleHigh_Checked
        AddHandler rbScaleNearest.Checked, AddressOf rbScaleHigh_Checked
        AddHandler rbScaleNormal.Checked, AddressOf rbScaleHigh_Checked
    End Sub




    '変形
    '選択画像の変形情報をタブに取り込む
    Private Sub GetSetRotate()
        '画像の回転角度をタブのスライダーに反映する
        Dim tf As Transform = FocusExThumb.TemplateImage.RenderTransform
        'Dim gt As GeneralTransform = tf.Inverse '逆変換
        Dim rt As New RotateTransform
        Dim st As New ScaleTransform
        Dim skt As New SkewTransform

        Dim angle As Double = 0
        Dim scale As Double = 1
        Dim skew As Double = 0
        If tf.Value = Matrix.Identity Then
            'Transformが空なら角度0
            angle = 0

        Else
            'TransformをRotateTransformに変換してAngle取得
            'Dim rt As RotateTransform = tf

            Dim tGroup As TransformGroup = tf
            Dim tCollection As TransformCollection = tGroup.Children
            For Each t As Transform In tCollection
                Select Case t.GetType
                    Case rt.GetType
                        rt = t
                        angle = rt.Angle
                    Case st.GetType
                        st = t
                        scale = st.ScaleX
                    Case skt.GetType
                        skt = t
                End Select
            Next
            'angle = rt.Angle
        End If
        'スライダーのValueにセット
        sldKaiten.Value = angle
        sldScale.Value = scale


        '描画モード
        Dim img As ExImage = FocusExThumb.TemplateImage
        Dim mode As BitmapScalingMode = RenderOptions.GetBitmapScalingMode(img)
        Select Case mode
            Case BitmapScalingMode.Fant
                rbScaleHigh.IsChecked = True
            Case BitmapScalingMode.NearestNeighbor
                rbScaleNearest.IsChecked = True
            Case BitmapScalingMode.Unspecified
                rbScaleNormal.IsChecked = True
        End Select
    End Sub


    '選択画像の変形値をタブのスライダーに反映
    Private Sub btGetTransform_Click(sender As Object, e As RoutedEventArgs) Handles btGetTransform.Click
        If FocusExThumb Is Nothing Then Return
        Call GetSetRotate()

    End Sub

    '変形セット
    Private Sub btSetTransform_Click(sender As Object, e As RoutedEventArgs)
        If FocusExThumb Is Nothing Then Return

        'Dim rt As New RotateTransform(sldKaiten.Value)
        'FocusExThumb.RenderTransform = rt
        'FocusExThumb.RotateAngle = sldKaiten.Value

    End Sub

    '回転スライダー
    Private Sub sldKaiten_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldKaiten.ValueChanged
        If FocusExThumb Is Nothing Then Return
        'Thumbを回転じゃなくて中のExImageを回転させればマウス移動の方向が見た目通りになる
        Dim img As ExImage = FocusExThumb.TemplateImage
        Dim rt As New RotateTransform(e.NewValue)
        'Dim ts As Transform = img.RenderTransform
        'Dim rt As RotateTransform = ts

        ''画像の角度がスライダーの値と同じなら何もしないで終了
        'If rt.Angle = e.NewValue Then Return

        rt.Angle = e.NewValue
        Dim st As New ScaleTransform(sldScale.Value, sldScale.Value)
        Dim tCollection As New TransformCollection
        tCollection.Add(st)
        tCollection.Add(rt)
        Dim tGroup As New TransformGroup
        tGroup.Children = tCollection
        img.RenderTransform = tGroup

        ''Dim ma As Integer = e.NewValue Mod 90
        ''Dim bi As BitmapImage = FocusExThumb.Source
        ''Dim tfb As New TransformedBitmap
        ''Dim tf As Transform
        ''tf = New RotateTransform(e.NewValue - ma)
        ''With tfb
        ''    .BeginInit()
        ''    .Source = bi
        ''    .Transform = tf
        ''    .EndInit()
        ''    .Freeze()
        ''End With
        ''Dim enc As New BmpBitmapEncoder
        ''enc.Frames.Add(BitmapFrame.Create(tfb))

        ''Dim bs As BitmapSource
        ''Using ms As New MemoryStream()
        ''    enc.Save(ms)
        ''    ms.Seek(0, SeekOrigin.Begin)
        ''    Dim dec = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
        ''    bs = dec.Frames(0)

        ''End Using

        ''FocusExThumb.Source = bs





    End Sub

    '回転、上下左右ボタン
    Private Sub btAngle_Click(sender As Object, e As RoutedEventArgs) Handles btAngle90.Click, btAngle180.Click, btAngle0.Click, btAngle270.Click
        If FocusExThumb Is Nothing Then Return
        Dim angle As Double = DirectCast(sender, Button).Tag
        sldKaiten.Value = angle

    End Sub
    '回転+-5、+45ボタン
    Private Sub btAngleAdd_Click(sender As Object, e As RoutedEventArgs) Handles btAngleAdd45.Click, btAngleAdd5.Click, btAngleSub5.Click
        If FocusExThumb Is Nothing Then Return
        Dim angle As Double = sldKaiten.Value + DirectCast(sender, Button).Tag
        angle = angle Mod 360
        If angle < 0 Then
            angle += 360
        End If
        sldKaiten.Value = angle

    End Sub

    '拡大スライダー
    Private Sub sldScale_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles sldScale.ValueChanged
        If FocusExThumb Is Nothing Then Return

        'Thumbじゃなくて中のExImageを変形させればマウス移動の方向が見た目通りになる
        Dim img As ExImage = FocusExThumb.TemplateImage
        'Dim tf As Transform = img.RenderTransform
        'Dim st As ScaleTransform = tf

        ''画像の拡大率がスライダーと同じなら何もしないで終了
        'If st.ScaleX = e.NewValue Then Return

        Dim st As New ScaleTransform(e.NewValue, e.NewValue)
        Dim rt As New RotateTransform(sldKaiten.Value)
        Dim tfCollection As New TransformCollection
        tfCollection.Add(st)
        tfCollection.Add(rt)
        Dim tfGroup As New TransformGroup
        tfGroup.Children = tfCollection

        img.RenderTransform = tfGroup
    End Sub

    Private Sub sldScale_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles sldScale.MouseWheel
        Dim sld As Slider = DirectCast(sender, Slider)
        If e.Delta > 0 Then
            sld.Value += 0.01
        Else
            sld.Value -= 0.01
        End If
    End Sub

    '拡大ボタン
    Private Sub tbScale_Click(sender As Object, e As RoutedEventArgs) Handles btSclae1.Click,
                                            btSclae2.Click, btSclae2Divde.Click, btSclae3.Click
        Dim scalevalue As Double = 1 * DirectCast(sender, Button).Tag
        sldScale.Value = scalevalue
    End Sub
    'Private Sub tbScaleNow_Click(sender As Object, e As RoutedEventArgs) Handles btScla2DivdeNow.Click, btSclae2Now.Click
    '    Dim scalevalue As Double = sldScale.Value * DirectCast(sender, Button).Tag
    '    sldScale.Value = scalevalue
    'End Sub

    Private Sub TextBox_MouseWheel(sender As Object, e As MouseWheelEventArgs)
        Dim tb As TextBox = DirectCast(sender, TextBox)
        Dim i As Double = tb.Text
        If e.Delta > 0 Then
            i += 0.01
        Else
            i -= 0.01
        End If
        sldScale.Value = i
    End Sub

    '描画品質設定ラジオボタン
    Private Sub rbScaleHigh_Checked(sender As Object, e As RoutedEventArgs)
        If FocusExThumb Is Nothing Then Return

        Dim img As ExImage = FocusExThumb.TemplateImage
        Select Case True
            Case rbScaleNormal.IsChecked
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Unspecified)
            Case rbScaleNearest.IsChecked
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor)
            Case rbScaleHigh.IsChecked
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant)
        End Select

    End Sub

    Private Sub gridSdr_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        tbSliX.Text = $"{e.NewValue * sliX.Value:000}"

        tbSliY.Text = $"{e.NewValue * sliY.Value:000}"
    End Sub




    'クリックで色取得
    Private Sub btGetColor_Click(sender As Object, e As RoutedEventArgs) Handles btGetColor.Click
        '色取得モードにする、カーソルの形を十字にする、もう一度押されたら元に戻す
        If IsGetColor Then
            IsGetColor = False
            Cursor = Cursors.Arrow
            RemoveHandler canvas1.PreviewMouseLeftButtonDown, AddressOf canvas1_PreviewMouseLeftButtonDown
            RemoveHandler canvas1.PreviewMouseMove, AddressOf canvas1_PreviewMouseMove
            gridGettingColor.Visibility = Visibility.Collapsed
        Else
            IsGetColor = True
            Cursor = Cursors.Cross
            AddHandler canvas1.PreviewMouseLeftButtonDown, AddressOf canvas1_PreviewMouseLeftButtonDown
            AddHandler canvas1.PreviewMouseMove, AddressOf canvas1_PreviewMouseMove
            gridGettingColor.Visibility = Visibility.Visible
        End If
    End Sub

    'マウスの下にある画像から色取得して取得中の色を表示、下に透過されているものは無視される
    Public Sub GetColor(x As Integer, y As Integer, bs As BitmapSource)
        'クリックされた位置の1ｘ1の画像を切り取り作成
        Dim cb As New CroppedBitmap(bs, New Int32Rect(x, y, 1, 1))
        'ストライドがよくわからないから決め打ちするためにBgra32に変換
        Dim cfb As New FormatConvertedBitmap(cb, PixelFormats.Bgra32, Nothing, 0)
        Dim pixels(3) As Byte
        cfb.CopyPixels(pixels, 4, 0) 'strideは4で決め打ち
        Dim a As Integer = pixels(3)
        Dim b As Integer = pixels(2)
        Dim g As Integer = pixels(1)
        Dim r As Integer = pixels(0)
        Dim c As Color = Color.FromArgb(a, b, g, r)
        '取得した色でブラシ作成して表示
        Dim scb As New SolidColorBrush(c)
        rectGetingColor.Fill = scb
        rectGetingColor.Tag = c 'TagにColorを入れておく
        tbGetingColr.Text = $"ARGB={a},{r},{g},{b}"


        If r + g + b > 400 Then
            tbGetingColr.Foreground = Brushes.Black
        Else
            tbGetingColr.Foreground = Brushes.White
        End If
    End Sub
    '選択色を透明にする
    Private Sub btSetTransparent_Click(sender As Object, e As RoutedEventArgs) Handles btSetTransparent.Click
        Call ToTransparent(FocusExThumb)
    End Sub
    '選択色を透明にする
    Private Sub ToTransparent(ex As ExThumb)
        If ex Is Nothing Then Return
        'Tagに入れておいた色を取り出す
        If rectSelectColor.Tag Is Nothing Then Return

        Dim col As Color = rectSelectColor.Tag
        Dim r As Byte = col.R
        Dim g As Byte = col.G
        Dim b As Byte = col.B
        Dim a As Byte = col.A

        Dim bs As BitmapSource = ex.TemplateImage.Source
        Dim w As Integer = bs.PixelWidth
        Dim h As Integer = bs.PixelHeight
        Dim stride As Integer = w * 4 ' 4は決め打ち、PixelFormatをBgra32に固定しているから
        Dim i As Integer = stride * h - 1
        Dim pixels(i) As Byte
        bs.CopyPixels(pixels, stride, 0)

        For y As Integer = 0 To h - 1
            For x As Integer = 0 To w - 1
                Dim ptr As Integer = (y * stride) + x * 4
                '色が一致したらアルファ値を０にする
                If pixels(ptr + 3) = a AndAlso pixels(ptr + 2) = r AndAlso pixels(ptr + 1) = g AndAlso pixels(ptr + 0) = b Then
                    pixels(ptr + 3) = 0
                End If
            Next
        Next

        Dim nbs As BitmapSource = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, Nothing, pixels, stride)
        ex.TemplateImage.Source = nbs
    End Sub


    ''Canvas上での左クリック、画像のあるところだけで発生
    Private Sub canvas1_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) 'Handles canvas1.PreviewMouseLeftButtonDown
        '色取得モードなら終了させる
        If IsGetColor Then
            Cursor = Cursors.Arrow
            IsGetColor = False
            RemoveHandler canvas1.PreviewMouseMove, AddressOf canvas1_PreviewMouseMove
            RemoveHandler canvas1.PreviewMouseLeftButtonDown, AddressOf canvas1_PreviewMouseLeftButtonDown

            Dim c As Color = rectGetingColor.Tag
            rectSelectColor.Fill = New SolidColorBrush(c)
            rectSelectColor.Tag = c 'TagにColorを入れておく
            tbSelectColorARGB.Text = $"ARGB={c.A},{c.R},{c.G},{c.B}"

            gridGettingColor.Visibility = Visibility.Collapsed
        End If

    End Sub

    Private Sub canvas1_PreviewMouseMove(sender As Object, e As MouseEventArgs)
        '画像から色取得
        'Canvas全体画像を作成、クリックした場所の色を取得
        Dim p As Point = e.GetPosition(canvas1) 'クリック位置
        Dim r As Rect = GetUnion() 'Canvas全体のRect取得

        '描画先を作成
        Dim dv As New DrawingVisual
        Using dc As DrawingContext = dv.RenderOpen
            Dim vb As New VisualBrush(canvas1) 'Canvas内に表示されているもの自体を使ってVisualBrush作成
            dc.DrawRectangle(vb, Nothing, r) '四角形にブラシで塗り、サイズそのまま
        End Using

        '描画、切り上げザイズの範囲に、そのままの大きさで描画
        'PixelFormatはPbgra32以外だとエラー？Bgra32はエラーになる
        Dim rtb As New RenderTargetBitmap(r.Width, r.Height, 96, 96, PixelFormats.Pbgra32)
        rtb.Render(dv)

        '色を取得して表示
        Call GetColor(p.X, p.Y, rtb)

    End Sub
End Class
