Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Controls.Panel
Imports System.Windows.Controls.Canvas


Class MainWindow
    'すべてのExImageを入れておくリストコレクション
    Private CollectionExImage As New ObservableCollectionExImage(Me)

    'Locate表示のtextBlockの更新
    Public Sub StatusBarDisplayUpdate(ex As ExImage)
        If ex Is Nothing Then
            stLocate.Content = "Locate = "
        Else
            Dim saveSize As Size = GetUnion().Size
            '保存サイズは小数点以下切り上げ
            saveSize = New Size(Math.Ceiling(saveSize.Width), Math.Ceiling(saveSize.Height))
            Dim r As Rect = GetRect(ex)

            'stSaveSize.Content = "保存画像サイズ(幅=" & saveSize.Width.ToString & "x高さ=" & saveSize.Height.ToString & ")"
            stSaveSize.Content = $"保存画像サイズ(幅{saveSize.Width}x高さ{saveSize.Height})"
            stLocate.Content = $"Locate({ex.Location.ToString})"
            stImaSize.Content = $"Size({r.Size:#.00})"
            'stMotoSize.Content = "元のサイズ(" & ex.ImageSize.ToString & ")"
            stMotoSize.Content = $"元のサイズ({ex.SourceImageSize:#})"

        End If
    End Sub

    '    VB プロパティの作成
    'http://homepage1.nifty.com/rucio/main/dotnet/shokyu/standard47.htm
    '選択中の画像(ExImage)を記録しておくプロパティ
    '選択画像が変わった時に見本画像更新したくて作った
    Private WithEvents _FocusExImage As ExImage
    Public Property FocusExImage As ExImage
        Get
            Return _FocusExImage
        End Get
        Set(value As ExImage)
            'プロパティに値をセット、つまり中身が変化する
            _FocusExImage = value

            '見本画像更新とか
            If value Is Nothing Then
                mihon.Source = Nothing
            Else
                mihon.Source = value.Source
                '表示位置とCanvasサイズ調整、これは実行しなくてもいいかなあ
                Call AjustLocation()
            End If

            ''FocusExImageの中身が入れ替わった時実行
            Call StatusBarDisplayUpdate(value)
        End Set
    End Property

    '位置調整、移動後の座標がマイナスだった時に全部の画像をマイナス分をプラスにして調整する
    '、左上の画像を削除して空白ができた時に
    Public Sub AjustLocation()
        'すべての画像がピッタリ収まるRect
        Dim r As Rect = GetUnion()
        'Canvasサイズを変更
        canvas1.Width = r.Width
        canvas1.Height = r.Height

        ''移動した画像がマイナス座標じゃなければ終了
        'If FocusExImage.Location.X > 0 AndAlso FocusExImage.Location.Y > 0 Then Return

        'すべての画像の位置を調節
        If r.X <> 0 OrElse r.Y <> 0 Then
            For Each ex As ExImage In CollectionExImage
                '座標をセット
                ex.Location -= r.Location
            Next
        End If
    End Sub



    'ExImageを左上のグリッドに移動
    '画像を回転したり拡大表示などの変形すると見た目の大きさが変わるだけで
    '位置を指定するCanvas.SetLeftとかで使われるのは変形する前の元の大きさになる
    'でもグリッドに合わせることになるのは変形後の今表示している形の位置なので
    '変形後の位置がグリッドかどれだけずれているかを求めて
    'そのズレの分を元の画像に適用すればいい
    Public Sub AjustGrid(ex As ExImage)
        Dim p As Point = GetRect(ex).Location '変形位置取得、今表示している形の位置
        'Dim p As Point = ex.Location '変形位置取得、今表示している形の位置

        Dim g As Integer = gridSdr.Value '指定グリッド数値取得

        'ズレの取得、今表示している形の位置がグリッドからどれだけずれているか
        Dim xm As Double = p.X Mod g '横位置をグリッドで割った余り
        Dim ym As Double = p.Y Mod g '縦位置を～

        '元の画像の位置にズレを適用したPoint作成
        Dim setP As New Point(ex.Location.X - xm, ex.Location.Y - ym)
        Dim neko As Point = Point.Subtract(setP, ex.Location)

        If setP.Equals(ex.Location) = False Then
            '座標をセット
            ex.Location = setP
        End If
    End Sub

    'マウスドラッグ移動、グリッドに合わせた移動
    Private Sub ExImage_DragMove(sender As Object, e As DragDeltaEventArgs)
        Dim g As Integer = gridSdr.Value
        Dim ex As ExImage = DirectCast(sender, ExImage)
        '元の形の位置取得、
        'これは左クリックダウンイベントの時にAjustGridメソッドでグリッドに合わせた位置になっているはずなので
        'あとはマウスの移動距離がグリッド値を超えたら隣のグリッドへ移動させる感じ

        'Pixtack紫陽花と同じ方式
        Dim xChange As Double = e.HorizontalChange
        Dim yChange As Double = e.VerticalChange
        Dim mx As Double = xChange Mod g '横移動距離をグリッドで割った余り
        Dim my As Double = yChange Mod g
        '移動先座標
        Dim x As Double = ex.Location.X + xChange - mx '元の位置＋移動距離ー余り
        Dim y As Double = ex.Location.Y + yChange - my



        '座標をセット
        Dim setP As New Point(x, y)
        If setP.Equals(ex.Location) Then Return

        ex.Location = setP


        '条件
        'グリッド値が8で   g=8
        '元の形の時のExImageの位置が(24, 16)で  ex.Location=(24,16)
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

    'ExImageを作成して追加完了後に位置の修正、マイナス座標だった時に1回だけ必要
    Private Sub ExImage_Loaded(sender As Object, e As RoutedEventArgs)
        Dim ex As ExImage = DirectCast(sender, ExImage)
        FocusExImage = ex
        Call UpdateDisplayZIndex()

        'Call AjustLocation()
        'Call StatusBarDisplayUpdate(ex)
        'もう必要ないのでイベントから剥がす
        RemoveHandler ex.Loaded, AddressOf ExImage_Loaded

        'Call AjustGrid(sender) '最寄りのグリッドに位置を合わせる
    End Sub

    '    WPFのClipboard.GetImage()のバグ？ ( ソフトウェア ) - Simple Is best - Yahoo!ブログ
    'http://blogs.yahoo.co.jp/elku_simple/35320048.html
    '    BitmapSourceの変換 - 反省しています
    'http://d.hatena.ne.jp/funct/20150929/1443538237

    'クリップボードに画像があったら追加する
    Private Function AddFromClipboard() As BitmapImage
        Dim bi As New BitmapImage 'system.windows.media.imaging.bitmapimage
        If My.Computer.Clipboard.ContainsImage Then
            Dim b = My.Computer.Clipboard.GetData(DataFormats.Bitmap) 'system.drawing.bitmap
            Dim dib As MemoryStream = My.Computer.Clipboard.GetData(DataFormats.Dib) 'system.io.memorystream
            Dim em = Clipboard.GetData(DataFormats.EnhancedMetafile) 'エクセルの図形とか取得できる
            Dim mp = Clipboard.GetData(DataFormats.MetafilePicture)
            Dim ti = Clipboard.GetData(DataFormats.Tiff)





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
            Dim ex As ExImage = SetCollectionExImage()
            With ex
                .Source = bbb ' bf ' bs
                .SourceImageSize = New Size(bs.PixelWidth, bs.PixelHeight)
                .Location = GetNewLocation()

            End With
            FocusExImage = ex
            AddHandler ex.ExDragDelta, AddressOf ExImage_DragMove
            'AddHandler ex.Loaded, AddressOf ExImage_Loaded
            Return bi
        Else
            MsgBox("クリップボードの中に画像はありませんでした")
            Return New BitmapImage
        End If
        Return New BitmapImage
    End Function

    'ファイルパスからBitmapImage(画像)を作成して返す
    Private Function GetBitmapImage(filePath As String) As BitmapImage
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

        Return bmp
    End Function


    Private Function SetCollectionExImage() As ExImage
        Dim ex As New ExImage(Me)
        'リストコレクションに追加、ZIndexは追加した時にObservableCollectionExImageの方で処理される

        If cbFocusImage.IsChecked Then
            '選択画像を基準にする場合
            Dim i As Integer = CollectionExImage.IndexOf(FocusExImage)
            If rbAddUpper.IsChecked Then '上層に追加
                CollectionExImage.Insert(i + 1, ex)
            ElseIf rbAddLower.IsChecked Then '下層に追加
                CollectionExImage.Insert(i, ex)
            End If
        Else
            '最前面か最背面に追加する場合
            If rbAddUpper.IsChecked Then
                '上層に追加
                CollectionExImage.Add(ex)
            ElseIf rbAddLower.IsChecked Then
                '下層に追加
                CollectionExImage.Insert(0, ex)
            End If
        End If
        canvas1.Children.Add(ex)
        Return ex

    End Function


    'ExImageを追加するときの表示位置を返す
    Private Function GetNewLocation() As Point
        'ExImageを表示する位置
        Dim newLocate As New Point(0, 0)
        If CollectionExImage.Count = 0 Then
            Return newLocate '最初の画像なら0,0
        End If

        Dim x As Integer = tbSliX.Text
        Dim y As Integer = tbSliY.Text
        If FocusExImage IsNot Nothing Then
            'スライド
            newLocate = FocusExImage.Location
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

        '最初のExImageを表示する位置
        Dim x As Integer = tbSliX.Text
        Dim y As Integer = tbSliY.Text
        Dim newLocate As New Point(0, 0)
        If rbSetLocateDrop.IsChecked Then
            newLocate = e.GetPosition(canvas1)
        Else
            newLocate = GetNewLocation()
        End If

        'ファイルパスの数だけExImage作成して表示
        For i As Integer = 0 To listPath.Count - 1
            'ファイルパスから画像を取得
            Dim bmp As BitmapImage = GetBitmapImage(listPath(i))
            If bmp IsNot Nothing Then

                'コレクションに追加してCanvasに表示
                Dim ex As ExImage = SetCollectionExImage()
                '各プロパティの指定
                With ex
                    .Source = bmp
                    .SourceImageSize = New Size(bmp.PixelWidth, bmp.PixelHeight)
                    .FileName = listPath(i)
                    .Location = newLocate '元の位置'ここで再描画処理が入るのでGetRectとかで今の値が取得できる
                    .RenderTransformOrigin = New Point(0.5, 0.5)
                    .SizeSeem = New Size(bmp.PixelWidth, bmp.PixelHeight) '見た目のサイズ、変形後サイズ
                    .LocationRenderDiff = New Point(0, 0) '実際と見た目の位置の差

                    '.LocationSeem = newLocate '見かけ上の位置、変形後はこの値が変化する
                    '.Height = bmp.PixelHeight 'これとWidthはどうするかな
                    '.Width = bmp.PixelWidth '指定しないと100x100の画像は100.0139になる→
                    '指定して100にするより無指定で100.0139にしておいたほうが綺麗に見える
                    '保存時には関係なさそうなので無指定で
                End With
                '次の位置
                newLocate.Offset(x, y)

                AddHandler ex.ExDragDelta, AddressOf ExImage_DragMove 'これはマウスドラッグ用

                FocusExImage = ex '要る？→要る、画像追加時のCollectionに挿入するときに使う
                If i = listPath.Count - 1 Then
                    ''最後に追加したExImageのLoadedイベントにメソッド追加
                    Call UpdateDisplayZIndex()
                    'AddHandler ex.Loaded, AddressOf ExImage_Loaded
                    'Call ReRender(ex)
                    Call AjustLocation()

                End If
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
        If FocusExImage Is Nothing Then
            stZIndex.Content = "ZIndex = " & "none"
        Else
            stZIndex.Content = "ZIndex = " & GetZIndex(FocusExImage).ToString
        End If

    End Sub
    '1つ上に移動
    Private Sub btAge_Click(sender As Object, e As RoutedEventArgs) Handles btAge.Click
        Dim z As Integer = CollectionExImage.IndexOf(FocusExImage)
        Call ZOrder(z, z + 1)
    End Sub
    '1つ下に移動
    Private Sub sage_Click(sender As Object, e As RoutedEventArgs) Handles sage.Click, btSage.Click
        Dim z As Integer = CollectionExImage.IndexOf(FocusExImage)
        Call ZOrder(z, z - 1)
    End Sub
    '画像のZOrder指定、ExImageのZIndex指定
    Private Sub ZOrder(Moto As Integer, Saki As Integer)
        If FocusExImage Is Nothing Then Return
        CollectionExImage.Move(Moto, Saki) '移動元Index、移動先Index
        Call UpdateDisplayZIndex()
    End Sub



    '    プログラミング Windows 第6版 第10章 WPF編 - 荒井省三のBlog - Site Home - MSDN Blogs
    'http://blogs.msdn.com/b/shozoa/archive/2014/08/22/using-programming-windows-chapter10.aspx
    'ExImageのRectを取得、回転後のRectにも対応
    Public Function GetRect(ex As ExImage) As Rect
        'RenderSize版100.0139
        'Dim cVisual As GeneralTransform = ex.TransformToVisual(canvas1)
        'Dim r As Rect = cVisual.TransformBounds(New Rect(ex.RenderSize))
        'Return r

        'SourceのPixelWidth版100
        Dim gt As GeneralTransform = ex.TransformToVisual(canvas1)
        'Dim b As BitmapImage = ex.Source
        Dim b As BitmapSource = ex.Source
        Dim r As Rect = gt.TransformBounds(New Rect(New Size(b.PixelWidth, b.PixelHeight)))
        Dim neko = GetLeft(ex)
        Dim ore = GetTop(ex)

        Return r
    End Function

    'すべてのExImageのRectのUnionのRectを取得
    Private Function GetUnion() As Rect

        Dim r As Rect = GetRect(CollectionExImage(0))
        For i As Integer = 1 To CollectionExImage.Count - 1
            r = Rect.Union(r, GetRect(CollectionExImage(i)))
        Next
        Return r
    End Function



    '画像ファイルとして保存

    '        キャンバスに描いた絵を画像ファイルとして保存する | HIRO's.NET Blog
    'http://blog.hiros-dot.net/?page_id=3802
    'Daizen Ikehara : [WPF] XamQRCodeBarcode を画像として保存 [Tips]
    'http://blogs.jp.infragistics.com/blogs/dikehara/archive/2014/02/12/wpf-xamqrcodebarcode-tips.aspx
    '    RenderTargetBitmap tips - Jaime Rodriguez - Site Home - MSDN Blogs
    'http://blogs.msdn.com/b/jaimer/archive/2009/07/03/rendertargetbitmap-tips.aspx

    Private Sub SaveAllImage()
        If CollectionExImage.Count = 0 Then Return '画像がなければ何もしない

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
    Private Sub save_Click(sender As Object, e As RoutedEventArgs) Handles save.Click, btSaveAll.Click
        Call SaveAllImage()
    End Sub
    Private Sub kaiten_Click(sender As Object, e As RoutedEventArgs) Handles kaiten.Click
        Dim rtf As New RotateTransform(45)


        'RenderOptions.SetBitmapScalingMode(sender, BitmapScalingMode.HighQuality)
        FocusExImage.RenderTransformOrigin = New Point(0.5, 0.5)

        FocusExImage.RenderTransform = rtf
    End Sub



    '削除ボタン押した時
    Private Sub remove_Click(sender As Object, e As RoutedEventArgs) Handles remove.Click, btRemove.Click
        If FocusExImage Is Nothing Then Return
        Call RemoveExImage(FocusExImage)
    End Sub
    '全削除ボタン押した時
    Private Sub RemoveAll_Click() Handles btRemoveAll.Click
        If FocusExImage Is Nothing Then Return
        For i As Integer = 0 To CollectionExImage.Count - 1
            Call RemoveExImage(CollectionExImage(0))
        Next
    End Sub

    '選択画像削除
    Private Sub RemoveExImage(ex As ExImage)
        Dim i As Integer = CollectionExImage.IndexOf(ex) '削除対象のIndex取得
        CollectionExImage.Remove(ex) 'リストコレクションから削除
        canvas1.Children.Remove(ex) 'canvas1から削除(表示から消す)
        ''画像が1つもなければ空にする
        '削除後のFocusExImageはひとつ上のものにする
        '削除ExImageが一番上ならひとつ下のものにする
        If CollectionExImage.Count = 0 Then
            FocusExImage = Nothing
        ElseIf i = CollectionExImage.Count Then
            FocusExImage = CollectionExImage(i - 1)
        Else
            FocusExImage = CollectionExImage(i)
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

    'クリップボードから画像追加
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Call AddFromClipboard()
    End Sub

    Private Sub age_Click(sender As Object, e As RoutedEventArgs) Handles age.Click
        'Dim r As Rect = GetRect(FocusExImage)
        'Dim u As Rect = GetUnion()
        'Call AjustLocation()
        Call testApplyRotate()
    End Sub




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
        '上のイベントを実行してスライド量の表示更新
        sliX_ValueChanged(sliX, New RoutedPropertyChangedEventArgs(Of Double)(0, sliX.Value))
        sliY_ValueChanged(sliY, New RoutedPropertyChangedEventArgs(Of Double)(0, sliY.Value))

    End Sub

    Private Sub btKaiten45_Click(sender As Object, e As RoutedEventArgs) Handles btKaiten45.Click
        If FocusExImage Is Nothing Then Return
        Dim rt As New RotateTransform(128)
        FocusExImage.RenderTransform = rt
        'FocusExImage.LocationSeem = GetRect(FocusExImage).Location
        Call ReRender(FocusExImage)
    End Sub


    '変形

    Private Sub testApplyRotate()
        '画像の回転角度をタブのスライダーに反映する
        Dim rent As Transform = FocusExImage.RenderTransform

        Dim rt As RotateTransform = rent
        sldKaiten.Value = rt.Angle

    End Sub


    '変形ゲット
    Private Sub btGetTransform_Click(sender As Object, e As RoutedEventArgs)
        If FocusExImage Is Nothing Then Return

        Dim tf As Transform = FocusExImage.RenderTransform
        Dim rt As RotateTransform = tf
        rt.Angle = sldKaiten.Value
    End Sub

    '変形セット
    Private Sub btSetTransform_Click(sender As Object, e As RoutedEventArgs)
        If FocusExImage Is Nothing Then Return
        'Dim rt As New RotateTransform(sldKaiten.Value)
        'FocusExImage.RenderTransform = rt
        FocusExImage.RotateAngle = sldKaiten.Value

    End Sub

    Private Sub sldKaiten_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If FocusExImage Is Nothing Then Return

        'Dim ma As Integer = e.NewValue Mod 90
        'Dim bi As BitmapImage = FocusExImage.Source
        'Dim tfb As New TransformedBitmap
        'Dim tf As Transform
        'tf = New RotateTransform(e.NewValue - ma)
        'With tfb
        '    .BeginInit()
        '    .Source = bi
        '    .Transform = tf
        '    .EndInit()
        '    .Freeze()
        'End With
        'Dim enc As New BmpBitmapEncoder
        'enc.Frames.Add(BitmapFrame.Create(tfb))

        'Dim bs As BitmapSource
        'Using ms As New MemoryStream()
        '    enc.Save(ms)
        '    ms.Seek(0, SeekOrigin.Begin)
        '    Dim dec = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)
        '    bs = dec.Frames(0)

        'End Using

        'FocusExImage.Source = bs

        FocusExImage.RotateAngle = e.NewValue ' ma

        Dim r As Rect = GetRect(FocusExImage)
        FocusExImage.SizeSeem = GetRect(FocusExImage).Size
        'ExImageコントロールの再描画
        Call ReRender(FocusExImage)
        r = GetRect(FocusExImage)
        FocusExImage.SizeSeem = r.Size
        FocusExImage.LocationRenderDiff = Point.Subtract(r.Location,FocusExImage.Location)
        'Call AjustGrid(FocusExImage)
        Call AjustLocation()
    End Sub


    '    [VB.NET][WPF] WPF で 描画を更新する | オールトの雲
    'http://ooltcloud.expressweb.jp/201311/article_10192523.html

    '再描画
    Public Sub ReRender(ex As ExImage)
        ex.Dispatcher.Invoke(Threading.DispatcherPriority.Render, Sub()

                                                                  End Sub)
    End Sub

    Private Sub testbutton_Click(sender As Object, e As RoutedEventArgs) Handles testbutton.Click
        Dim r As Rect = GetRect(FocusExImage)
        Dim p As Point = FocusExImage.Location
        Dim ld As Point = FocusExImage.LocationRenderDiff

    End Sub




    '透明
    Private Sub btGetColor_Click(sender As Object, e As RoutedEventArgs)
        canvas1.Cursor = Cursors.Cross




    End Sub

    Private Sub btSetTransparent_Click(sender As Object, e As RoutedEventArgs)
        canvas1.Cursor = Cursors.Arrow
    End Sub
End Class
