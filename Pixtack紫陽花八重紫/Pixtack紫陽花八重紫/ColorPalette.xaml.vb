Imports System.Windows.Controls.Primitives
Imports System.IO
Imports System.ComponentModel

'Paletteの色管理データ、シリアライズする
<Serializable>
Public Class PaletteARGB
    Inherits Collections.ObjectModel.ObservableCollection(Of IntARGB)
    'Public Property ColorName As List(Of String)
End Class

'Paletteの色記録用
<Serializable>
Public Structure IntARGB
    Public A As Integer
    Public R As Integer
    Public G As Integer
    Public B As Integer
    Public Sub New(a As Integer, r As Integer, g As Integer, b As Integer)
        Me.A = a
        Me.R = r
        Me.G = g
        Me.B = b
    End Sub

    Public Sub New(col As Color)
        With col
            A = .A
            R = .R
            G = .G
            B = .B
        End With
    End Sub
End Structure

Public Class ColorPalette
    Private Const MaxSV As Integer = 255 'SとVの最大値、100%表示なら100、詳細なら255指定
    Private IsDrag As Boolean 'マーカーを移動中フラグ
    Private IsChangeHSV As Boolean 'HSVのスライダーで値変更中フラグ
    Private IsChangeRGB As Boolean 'RGBのスライダーで値変更中フラグ
    Private IsYFixed As Boolean     '輝度固定中フラグ
    Private FocusPalette As Border  '選択中のパレットのマス記録用


    'シリアライズするパレット用
    'Private PaletteColors As New List(Of Border) 'パレットのマス表示用のBorderのリスト
    'Private PaletteDataARGB As New PaletteARGB 'Paletteの色管理データ、シリアライズする

    'シリアライズする複数タブパレット用
    Private PaletteBorderMainList As New List(Of List(Of Border)) 'パレットのマス表示用のBorderのリスト
    Private PaletteArgbMainList As New List(Of PaletteARGB) '複数タブパレットの色データ



    Public Structure HSV
        Public H As Double
        Public S As Double
        Public V As Double
        Public Sub New(h As Double, s As Double, v As Double)
            Me.H = h
            Me.S = s
            Me.V = v
        End Sub
    End Structure

    Public Structure RGB
        Public R As Double
        Public G As Double
        Public B As Double
        Public Sub New(r As Double, g As Double, b As Double)
            Me.R = r
            Me.G = g
            Me.B = b
        End Sub
    End Structure

    Public Function ARGBtoSolidBrush(argb As IntARGB) As SolidColorBrush
        Return New SolidColorBrush(Color.FromArgb(argb.A, argb.R, argb.G, argb.B))
    End Function
    Public Function SolidBrushtoARGB(b As SolidColorBrush) As IntARGB
        Dim col As Color = b.Color
        Return New IntARGB(col.A, col.R, col.G, col.B)
    End Function

    Private Function RGBtoHSV(c As Color) As HSV
        Dim h, s, v As Double

        Dim r As Integer = c.R
        Dim g As Integer = c.G
        Dim b As Integer = c.B
        Dim min As Integer = Math.Min(b, Math.Min(r, g))
        Dim max As Integer = Math.Max(b, Math.Max(r, g))

        If max = min Then
            h = 0
            If max = 0 Then
                s = 0
                v = 0
                Return New HSV(0, 0, 0)
            End If
        ElseIf max = r Then
            h = 60 * ((g - b) / (max - min))
        ElseIf max = g Then
            h = 60 * ((b - r) / (max - min)) + 120
        ElseIf max = b Then
            h = 60 * ((r - g) / (max - min)) + 240
        End If

        'hがマイナスなら360足す、RとBが同じ値だとマイナスになることがある
        If h < 0 Then h += 360

        ''100%表示の時のSV
        's = ((max - min) / max) * 100
        'v = (max / 255) * 100

        ''最大値が255表示の時のSV
        's = (max - min) / max * 255
        'v = max

        'Const設定のSV
        s = ((max - min) / max) * MaxSV
        v = (max / 255) * MaxSV

        Dim hsv As New HSV(h, s, v)
        'With hsv
        '    .H = h
        '    .S = s
        '    .V = v
        'End With
        Return hsv
    End Function


    '    HSV色空間 - Wikipedia
    'https://ja.wikipedia.org/wiki/HSV%E8%89%B2%E7%A9%BA%E9%96%93
    ''' <summary>
    ''' HSVをRGB(Color)に変換する、Hは0-360、SとVは0-100で受け取って、RGBは小数点付きの0-255で返す
    ''' </summary>
    ''' <param name="hsv"></param>
    ''' <returns></returns>
    Private Function HSV2Color(hsv As HSV) As Color
        Dim h As Double = hsv.H / 360
        Dim s As Double = hsv.S / MaxSV ' 255 ' 100
        Dim v As Double = hsv.V / MaxSV ' 255 ' 100
        Dim r As Double = v
        Dim g As Double = v
        Dim b As Double = v
        Dim neko As Double
        If s > 0 Then
            h *= 6
            Dim i As Integer = Math.Floor(h)
            Dim f As Double = h - i
            Select Case i
                Case 0
                    g *= 1 - s * (1 - f)
                    b *= 1 - s
                Case 1
                    r *= 1 - s * f
                    b *= 1 - s
                Case 2
                    r *= 1 - s
                    b *= 1 - s * (1 - f)
                Case 3
                    r *= 1 - s
                    g *= 1 - s * f
                Case 4
                    neko = r * (1 - s * (1 - f))
                    r *= 1 - s * (1 - f)
                    g *= 1 - s

                Case 5
                    g *= 1 - s
                    b *= 1 - s * f
            End Select

        End If

        r *= 255
        g *= 255
        b *= 255
        Dim col As Color = Color.FromRgb(r, g, b)
        Return col

    End Function

    ''' <summary>
    ''' HSVをDouble型のRGBに変換
    ''' </summary>
    ''' <param name="hsv"></param>
    ''' <returns></returns>
    Private Function HSV2RGB(hsv As HSV) As RGB
        Dim h As Double = hsv.H / 360
        Dim s As Double = hsv.S / MaxSV ' 255 ' 100
        Dim v As Double = hsv.V / MaxSV ' 255 ' 100
        Dim r As Double = v
        Dim g As Double = v
        Dim b As Double = v
        If s > 0 Then
            h *= 6
            Dim i As Integer = Math.Floor(h)
            Dim f As Double = h - i
            Select Case i
                Case 0
                    g *= 1 - s * (1 - f)
                    b *= 1 - s
                Case 1
                    r *= 1 - s * f
                    b *= 1 - s
                Case 2
                    r *= 1 - s
                    b *= 1 - s * (1 - f)
                Case 3
                    r *= 1 - s
                    g *= 1 - s * f
                Case 4
                    r *= 1 - s * (1 - f)
                    g *= 1 - s

                Case 5
                    g *= 1 - s
                    b *= 1 - s * f
            End Select

        End If

        r *= 255
        g *= 255
        b *= 255
        'Dim col As Color = Color.FromRgb(r, g, b)
        Return New RGB(r, g, b)

    End Function


    Private Sub AddHueBar()
        '色相の画像作成
        Dim w As Integer = 2 ' 20 ' imgHue.Width
        Dim h As Integer = 360 ' imgHue.Height '
        Dim wb As New WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, Nothing)
        Dim stride As Integer = wb.BackBufferStride
        Dim pixels(h * stride * w - 1) As Byte
        Dim p As Integer
        wb.CopyPixels(pixels, stride, 0)

        Dim col As Color
        Dim hsv As New HSV(0, MaxSV, MaxSV)

        For y As Integer = 0 To h - 1
            hsv.H = y
            col = HSV2Color(hsv)
            For x As Integer = 0 To w - 1
                p = y * stride + (x * 4)
                pixels(p + 0) = col.B
                pixels(p + 1) = col.G
                pixels(p + 2) = col.R
                pixels(p + 3) = 255
            Next
        Next

        Dim sourceRect As Int32Rect = New Int32Rect(0, 0, w, h)
        wb.WritePixels(sourceRect, pixels, stride, 0)
        '縦の色相バー
        'imgHue.Source = wb

        '横の色相バーはこっちで270度回転させる
        Dim rotate As New RotateTransform(270)
        Dim rb As New TransformedBitmap(wb, rotate)
        imgHue.Source = rb

    End Sub

    ''' <summary>
    ''' 彩度(S)と明度(V)の画像更新
    ''' </summary>
    ''' <param name="img">対象となるImage</param>
    Private Sub UpdateImageSV(img As Image)
        '彩度の画像作成
        Dim w As Integer = 2 ' imgHue.Width
        'Dim h As Integer = MaxSV ' imgHue.Height '
        Dim wb As New WriteableBitmap(w, MaxSV, 96, 96, PixelFormats.Bgra32, Nothing)
        Dim stride As Integer = wb.BackBufferStride
        Dim pixels(MaxSV * stride * w - 1) As Byte
        Dim p As Integer
        wb.CopyPixels(pixels, stride, 0)

        Dim col As Color
        Dim hsv As New HSV(sldHue.Value, MaxSV, MaxSV)

        If img.Equals(imgV) = False Then
            '彩度画像の更新
            For y As Integer = 0 To MaxSV
                hsv.S = y
                col = HSV2Color(hsv)
                For x As Integer = 0 To w - 1
                    p = y * stride + (x * 4)
                    pixels(p + 0) = col.B
                    pixels(p + 1) = col.G
                    pixels(p + 2) = col.R
                    pixels(p + 3) = 255
                Next
            Next
        Else
            '明度(V)の更新
            For y As Integer = 0 To MaxSV
                hsv.V = y
                col = HSV2Color(hsv)
                For x As Integer = 0 To w - 1
                    p = y * stride + (x * 4)
                    pixels(p + 0) = col.B
                    pixels(p + 1) = col.G
                    pixels(p + 2) = col.R
                    pixels(p + 3) = 255
                Next
            Next
        End If

        Dim sourceRect As Int32Rect = New Int32Rect(0, 0, w, MaxSV)
        wb.WritePixels(sourceRect, pixels, stride, 0)
        '縦
        'imgS.Source = wb

        '横は270度回転させて作成
        Dim rotate As New RotateTransform(270)
        Dim rb As New TransformedBitmap(wb, rotate)
        img.Source = rb

    End Sub



    'sv画像、クリックする用の画像
    Private Sub ChangeImageSV(hue As Double)
        Dim w As Integer = MaxSV + 1 '100％表示なら0から100なので101ピクセル必要、255表示なら256ピクセル必要なので+1
        Dim h As Integer = MaxSV + 1 ' 255 'imgHue.Height
        Dim wb As New WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, Nothing)
        Dim stride As Integer = wb.BackBufferStride
        Dim pixels(h * stride * w - 1) As Byte
        Dim p As Integer
        wb.CopyPixels(pixels, stride, 0)
        Dim hsv As New HSV(hue, MaxSV, MaxSV)

        Dim col As Color
        For y As Integer = 0 To h - 1
            hsv.V = y ' 255 - y
            For x As Integer = 0 To w - 1
                hsv.S = x
                col = HSV2Color(hsv)
                p = y * stride + (x * 4)
                pixels(p + 0) = col.B
                pixels(p + 1) = col.G
                pixels(p + 2) = col.R
                pixels(p + 3) = 255
            Next
        Next

        Dim sourceRect As Int32Rect = New Int32Rect(0, 0, w, h)
        wb.WritePixels(sourceRect, pixels, stride, 0)
        imgSV.Source = wb
    End Sub

    '市松模様作成
    Private Sub CreateTransparentImage()
        Dim grid As Integer = imgTransparent.Height / 10 '5 'マスの大きさ指定、なぜか4とかだと市松模様にならない
        Dim gray As Integer = 200 '灰色の指定
        Dim iro As Integer = 255 '白色の指定、255固定

        Dim w As Integer = imgTransparent.Width
        Dim h As Integer = imgTransparent.Height
        Dim wb As New WriteableBitmap(100, 100, 96, 96, PixelFormats.Bgra32, Nothing)
        Dim stride As Integer = wb.BackBufferStride
        Dim pixels(h * stride * w - 1) As Byte
        wb.CopyPixels(pixels, stride, 0)
        Dim p As Integer = 0

        For y As Integer = 0 To h - 1
            'マスの大きさによって色を変える、白と灰色
            If y Mod grid = 0 Then
                If iro = 255 Then
                    iro = gray
                Else
                    iro = 255
                End If
            End If
            'マスの大きさによって色を変える、白と灰色
            For x As Integer = 0 To w - 1
                If x Mod grid = 0 Then
                    If iro = 255 Then
                        iro = gray
                    Else
                        iro = 255
                    End If
                End If

                p = y * stride + (x * 4)

                pixels(p + 0) = iro
                pixels(p + 1) = iro
                pixels(p + 2) = iro
                pixels(p + 3) = 255

            Next

        Next

        Dim sourceRect As New Int32Rect(0, 0, w, h)
        wb.WritePixels(sourceRect, pixels, stride, 0)
        imgTransparent.Source = wb

    End Sub





    '複数タブパレット

    'パレットのタブ作成
    Private Sub AddPaletteTabItem()
        Dim sukima As Integer = 2 '隣のマスとの間隔
        Dim bdSize As Integer = 20 'マスのサイズ
        For i As Integer = 0 To 9 'タブベージの枚数
            Dim tItem As New TabItem
            Dim wp As New WrapPanel 'ラップパネル

            With tItem
                .Header = "P" & i
                .Content = wp 'タブアイテムにラップパネル追加
            End With
            tabPalette.Items.Add(tItem) 'タブコントロールにタブアイテム追加

            'ラップパネルにマス追加
            Call AddPaletteAll(wp, sukima, bdSize)
        Next


        'タブコントロールの幅決定、横に12マス
        'tab2.BorderThickness = New Thickness(0, 0, 0, 0)
        With tabPalette
            Dim tabW As Integer = (sukima + bdSize) * 12 +
            .Padding.Left + .Padding.Right +
            .BorderThickness.Left * .BorderThickness.Right +
            SystemParameters.BorderWidth * 2 ' (sukima * 2)

            .Width = tabW
        End With
    End Sub

    'パレットデータをシリアライズしてファイルに保存
    Private Sub SavePaletteData()

        'xmlシリアライズ
        Dim appPath As String = My.Application.Info.DirectoryPath
        Dim fileName As String = "PaletteColorData.xml"
        Dim filePath As String = appPath & "\" & fileName
        Dim serializer As New Xml.Serialization.XmlSerializer(GetType(List(Of PaletteARGB)))
        Try
            Using sw As New StreamWriter(filePath, False, New Text.UTF8Encoding(False))
                serializer.Serialize(sw, PaletteArgbMainList)
            End Using
        Catch ex As Exception
            Dim str As String = "ファイルの保存に失敗しました" & vbNewLine
            MsgBox(str & ex.Message)
        End Try
    End Sub

    'パレットデータをデシリアライズして読み込む
    Private Sub LoadPaletteData()

        'xmlデシリアライズ
        Dim appPath As String = My.Application.Info.DirectoryPath
        Dim fileName As String = "PaletteColorData.xml"
        Dim filePath As String = appPath & "\" & fileName
        If File.Exists(filePath) = False Then Return 'ファイルが存在しなければなにもしないで終了

        Dim serializer As New Xml.Serialization.XmlSerializer(GetType(List(Of PaletteARGB)))
        Try
            Using sr As New StreamReader(filePath, New Text.UTF8Encoding(False))
                PaletteArgbMainList = serializer.Deserialize(sr)
            End Using
        Catch ex As Exception
            Dim str As String = "ファイルの読み込みに失敗しました" & vbNewLine
            MsgBox(str & ex.Message)
        End Try
    End Sub

    '色を追加
    Private Sub PaletteAddColor()
        FocusPalette.Background = rectMihon.Fill
        'ポップアップ
        'Dim tt As New ToolTip
        'tt.Content = FocusPalette.Background.ToString
        'FocusPalette.ToolTip = tt

        Dim ti As Integer = tabPalette.SelectedIndex '現在のタブindex
        Dim i As Integer = PaletteBorderMainList(ti).IndexOf(FocusPalette) '選択中のマスIndex
        Dim argbList As PaletteARGB = PaletteArgbMainList(ti) '対応するPaletteARGBリスト
        argbList(i) = SolidBrushtoARGB(FocusPalette.Background)

        'パレットデータをセーブ
        Call SavePaletteData()

    End Sub
    '色を削除
    Private Sub PaletteColorDelete()
        FocusPalette.Background = New SolidColorBrush(Colors.Transparent) '透明色にする
        Dim ti As Integer = tabPalette.SelectedIndex '現在のタブindex
        Dim i As Integer = PaletteBorderMainList(ti).IndexOf(FocusPalette) '選択中のマスIndex
        Dim argbList As PaletteARGB = PaletteArgbMainList(ti) '対応するPaletteARGBリスト
        argbList(i) = New IntARGB(Colors.Transparent) '透明色を書き込み

        'パレットデータをセーブ
        Call SavePaletteData()
    End Sub
    Private Function AddContextMeruPalette() As ContextMenu
        '右クリックメニュー作成
        Dim cm As New ContextMenu
        Dim mi As New MenuItem
        mi.Header = "色を追加"
        cm.Items.Add(mi)
        AddHandler mi.Click, AddressOf PaletteAddColor
        Dim mi2 As New MenuItem
        mi2.Header = "色を削除"
        cm.Items.Add(mi2)
        AddHandler mi2.Click, AddressOf PaletteColorDelete
        Return cm
    End Function

    '現在のパレットからARGBData再構築、バージョンアップでパレットの枚数を増やした時用
    Private Sub RebuildingArgbData()
        Dim tempData As New List(Of PaletteARGB)
        For i As Integer = 0 To PaletteBorderMainList.Count - 1
            Dim argbList As New PaletteARGB
            Dim bList As List(Of Border) = PaletteBorderMainList(i)
            For j As Integer = 0 To PaletteBorderMainList(0).Count - 1

                argbList.Add(SolidBrushtoARGB(bList(j).Background))
            Next
            tempData.Add(argbList)
        Next
        PaletteArgbMainList.Clear()
        PaletteArgbMainList = tempData
        Call SavePaletteData()
    End Sub
    Private Sub UpdatePalette()
        'デシリアライズでパレットデータを読込
        Call LoadPaletteData()

        '読み込んだデータをPaletteに反映させる
        For i As Integer = 0 To PaletteArgbMainList.Count - 1
            Dim argbList As PaletteARGB = PaletteArgbMainList(i)
            Dim bList As List(Of Border) = PaletteBorderMainList(i)
            For j As Integer = 0 To argbList.Count - 1
                bList(j).Background = ARGBtoSolidBrush(argbList(j))
            Next
        Next

        '読み込んだデータのARGBDataがパレットの枚数より少なかったら再構築
        If PaletteArgbMainList.Count < PaletteBorderMainList.Count Then
            Call RebuildingArgbData()
        End If
    End Sub
    '選択中のPaletteを変更
    Private Sub FocusedPalette2(sender As Object, e As RoutedEventArgs)
        FocusPalette = sender
    End Sub

    'Paletteクリックした時、hsvとαの各スライダーを変化させる
    Private Sub Palette_Click2(sender As Object, e As RoutedEventArgs)
        Dim b As SolidColorBrush = FocusPalette.Background
        '透明色(アルファ値が0)なら何もしないで終了
        If b.Color.A = 0 Then Return

        Dim hsv As HSV = RGBtoHSV(b.Color)
        sldHue.Value = hsv.H
        sldS.Value = hsv.S
        sldV.Value = hsv.V
        sldA.Value = b.Color.A
    End Sub

    Private Sub AddPaletteAll(wp As WrapPanel, sukima As Integer, bdSize As Integer)
        '右クリックメニュー作成
        Dim cm As ContextMenu = AddContextMeruPalette()

        'Palette作成
        'Dim bdSize As Integer = 20
        'Dim sukima As Integer = 3
        Dim bList As New List(Of Border)
        Dim argbList As New PaletteARGB

        For i As Integer = 1 To 120
            Dim b As New Border
            With b
                .Background = New SolidColorBrush(Colors.Transparent) '最初は透明色を指定
                .Width = bdSize
                .Height = bdSize
                .BorderThickness = New Thickness(1)
                .BorderBrush = New SolidColorBrush(Colors.LightGray)
                .Margin = New Thickness(sukima, sukima, 0, 0)
                .ContextMenu = cm '右クリックメニュー指定

            End With

            'WrapPanelに追加
            wp.Children.Add(b)
            'イベントとメソッドの関連付け
            AddHandler b.PreviewMouseDown, AddressOf FocusedPalette2
            AddHandler b.MouseLeftButtonDown, AddressOf Palette_Click2

            'BorderのSubリストに追加
            bList.Add(b)
            'シリアライズ用のARGBのSubリストに追加
            argbList.Add(SolidBrushtoARGB(b.Background))
            'argbList.ColorName.Add(b.Background.ToString)
        Next

        'メインリストに追加
        PaletteArgbMainList.Add(argbList)
        PaletteBorderMainList.Add(bList)

    End Sub




    ''' <summary>
    ''' 画像(BitmapSource)の指定座標の色を取得
    ''' </summary>
    ''' <param name="p">座標</param>
    ''' <param name="bmp">画像</param>
    ''' <returns></returns>
    Private Function GetPixelColor(p As Point, bmp As BitmapSource) As Color
        Dim w As Integer = bmp.PixelWidth
        Dim h As Integer = bmp.PixelHeight
        Dim x As Integer = Math.Floor(p.X)
        Dim y As Integer = Math.Floor(p.Y)

        If x >= w Then x = w - 1
        If y >= h Then y = h - 1

        Dim cb As New CroppedBitmap(bmp, New Int32Rect(x, y, 1, 1))
        Dim fcb As New FormatConvertedBitmap(cb, PixelFormats.Bgra32, Nothing, 0)
        Dim pixels(3) As Byte 'コピー先の場所作成
        fcb.CopyPixels(pixels, 4, 0)

        Return Color.FromArgb(pixels(3), pixels(2), pixels(1), pixels(0))
    End Function

    'Thumbの位置修正、中心になる座標を渡すとSetするべき座標を返す
    Private Function ConvertThumbPoint(p As Point) As Point
        Dim w As Integer = Math.Floor(thumb1.ActualWidth / 2)
        Dim h As Integer = Math.Floor(thumb1.ActualHeight / 2)
        Return New Point(p.X - w, p.Y - h)
    End Function


    'マーカーを移動して選択色表示
    ''' <summary>
    ''' マーカーを移動させる
    ''' </summary>
    ''' <param name="p">マーカーの中心座標</param>
    Private Sub MoveThumb(p As Point)
        If IsDrag Then Return

        Dim t As Thumb = thumb1
        Dim np As Point = ConvertThumbPoint(p)
        Canvas.SetLeft(t, np.X)
        Canvas.SetTop(t, np.Y)

    End Sub

    Private Sub ChangeMihon(col As Color)
        '見本色の更新
        rectMihon.Fill = New SolidColorBrush(col)

        'textblockの更新
        tbkRGB.Text = $"ARGB = {col.ToString}"
        tbkRGB2.Text = $"ARGB = {col.A:0},{col.R:0},{col.G:0},{col.B:0}"

        tbkNewHSV.Text = $"HSV={sldHue.Value:#.0}, {sldS.Value:#.0}, {sldV.Value:#.0}"


        '輝度値の表示更新
        Call GetKido()

    End Sub

    'HSVの変更のときRGBを更新
    Private Sub ValueChangedHSV()
        Dim col As Color = HSV2Color(New HSV(sldHue.Value, sldS.Value, sldV.Value))
        'RGBの指定
        sldR.Value = col.R
        sldG.Value = col.G
        sldB.Value = col.B

        '見本色とTextblockの更新
        Dim argb As Color = Color.FromArgb(sldA.Value, col.R, col.G, col.B)
        Call ChangeMihon(argb)

    End Sub

    'RGBの変更の時HSVを更新
    Private Function ChangeHSV() As HSV
        Dim hsv = RGBtoHSV(Color.FromArgb(255, sldR.Value, sldG.Value, sldB.Value))
        'HSVの指定
        sldHue.Value = hsv.H
        sldS.Value = hsv.S
        sldV.Value = hsv.V
        Return hsv
    End Function








    'ここから下はイベント
    Private Sub MainWindow_Initialized(sender As Object, e As EventArgs) Handles Me.Initialized
        '色相バー作成
        Call AddHueBar()
        '透明用の市松模様画像作成
        Call CreateTransparentImage()

        'SVの最大値設定
        sldS.Maximum = MaxSV
        sldV.Maximum = MaxSV

        '初期SV画像作成、色相は0(赤)で作成
        Call ChangeImageSV(0)
        imgSV.Width = MaxSV + 1 '100％表示なら0から100なので101ピクセル必要、255表示なら256ピクセル必要なので+1
        imgSV.Height = MaxSV + 1
        canvasSV.Width = imgSV.Width
        canvasSV.Height = imgSV.Height

        imgTransparent.Width = MaxSV
        imgTransparent.Height = MaxSV
        Call cbTransparent_Click(cbTransparent, New RoutedEventArgs)
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded


        AddHandler sldHue.ValueChanged, AddressOf sldHue_ValueChanged
        AddHandler sldS.ValueChanged, AddressOf sldS_ValueChanged
        AddHandler sldV.ValueChanged, AddressOf sldV_ValueChanged

        AddHandler sldA.ValueChanged, AddressOf sldA_ValueChanged

        AddHandler sldR.ValueChanged, AddressOf sldRGB_ValueChanged
        AddHandler sldG.ValueChanged, AddressOf sldRGB_ValueChanged
        AddHandler sldB.ValueChanged, AddressOf sldRGB_ValueChanged

        AddHandler sldGamma.ValueChanged, AddressOf sldGamma_ValueChanged


        '初期値設定
        'sldHue.Value = 0
        'sldS.Value = MaxSV
        'sldV.Value = MaxSV
        'sldA.Value = 255
        'sldR.Value = 255

        AddHandler sldHue.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldS.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldV.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldA.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldR.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldG.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldB.MouseWheel, AddressOf Slider_MouseWheel
        AddHandler sldGamma.MouseWheel, AddressOf Slider_MouseWheel

        AddHandler tbxH.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxS.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxV.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxA.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxR.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxG.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxB.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxGamma.MouseWheel, AddressOf TextBox_MouseWheel2

        Call sldRGB_ValueChanged(sldR, New RoutedPropertyChangedEventArgs(Of Double)(sldR.Value, sldR.Value))

        Call UpdateImageSV(imgS)
        Call UpdateImageSV(imgV)




        'パレット部分作成
        Call AddPaletteTabItem()
        Call UpdatePalette()
        'Call AddPalette()

        'ウィンドウの幅調節
        Dim w As Double = spp2.DesiredSize.Width
        If w < 250 Then
            spp2.Width = 250
            Width = tabPalette.Width + 250 + 20 ' spp.ActualWidth + spp2.Width + 20
        Else
            Width = tabPalette.Width + spp2.ActualWidth + 20
        End If

        'ウィンドウの高さ自動調節
        SizeToContent = SizeToContent.Height

    End Sub


    Private Sub thumb1_DragCompleted(sender As Object, e As DragCompletedEventArgs) Handles thumb1.DragCompleted

        IsDrag = False

    End Sub

    'マーカードラッグ移動
    Private Sub thumb1_DragDelta(sender As Object, e As DragDeltaEventArgs) Handles thumb1.DragDelta
        IsDrag = True

        Dim x As Integer = e.HorizontalChange
        Dim y As Integer = e.VerticalChange
        Dim t As Thumb = DirectCast(sender, Thumb)
        Dim w As Integer = Math.Floor(t.ActualWidth / 2)
        Dim h As Integer = Math.Floor(t.ActualHeight / 2)

        Dim nx As Integer = x + Canvas.GetLeft(t)
        Dim ny As Integer = y + Canvas.GetTop(t)
        If nx < -w Then nx = -w
        If nx > MaxSV - w Then nx = MaxSV - w
        If ny < -h Then ny = -h
        If ny > MaxSV - h Then ny = MaxSV - h

        Canvas.SetLeft(t, nx)
        Canvas.SetTop(t, ny)

        Dim p As Point = New Point(nx + w, ny + h)

        'スライダー移動
        sldS.Value = p.X
        sldV.Value = p.Y
    End Sub


    'スライダー
    Private Sub sldS_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If IsChangeRGB Then Return

        IsChangeHSV = True
        Call MoveThumb(New Point(e.NewValue, sldV.Value))
        Call ValueChangedHSV()
        IsChangeHSV = False
    End Sub

    Private Sub sldV_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If IsChangeRGB Then Return

        IsChangeHSV = True
        Call MoveThumb(New Point(sldS.Value, e.NewValue))
        Call ValueChangedHSV()
        IsChangeHSV = False
    End Sub

    '色相スライダー変化
    Private Sub sldHue_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If IsChangeRGB Then Return

        If IsYFixed Then
            sldV.Value = GetVFixedY()
        End If
        IsChangeHSV = True
        'SV画像更新
        Call ChangeImageSV(e.NewValue)
        '
        Call ValueChangedHSV()
        Call UpdateImageSV(imgS)
        Call UpdateImageSV(imgV)
        IsChangeHSV = False
    End Sub

    'RGBスライダー
    Private Sub sldRGB_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))

        If IsChangeHSV Then Return

        IsChangeRGB = True
        Dim hsv As HSV = ChangeHSV()
        Call MoveThumb(New Point(hsv.S, hsv.V))
        Call ChangeImageSV(hsv.H)
        Call ChangeMihon(Color.FromArgb(sldA.Value, sldR.Value, sldG.Value, sldB.Value))
        Call UpdateImageSV(imgS)
        Call UpdateImageSV(imgV)
        IsChangeRGB = False
    End Sub

    'アルファスライダー
    Private Sub sldA_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Call ChangeMihon(Color.FromArgb(sldA.Value, sldR.Value, sldG.Value, sldB.Value))
    End Sub


    'SV画像クリック
    Private Sub imgSV_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles imgSV.MouseLeftButtonDown
        Dim clickPoint As Point = e.GetPosition(imgSV)

        'マーカーを移動
        Call MoveThumb(clickPoint)
        'スライダー移動
        sldS.Value = clickPoint.X
        sldV.Value = clickPoint.Y

    End Sub

    'マウスホイール、スライダー
    Private Sub Slider_MouseWheel(sender As Slider, e As MouseWheelEventArgs)
        Dim sld As Slider = DirectCast(sender, Slider)
        If e.Delta > 0 Then
            sld.Value += sld.SmallChange
        Else
            sld.Value -= sld.SmallChange
        End If
    End Sub
    'マウスホイール、textbox
    Private Sub TextBox_MouseWheel(sender As TextBox, e As MouseWheelEventArgs)
        If e.Delta > 0 Then
            sender.Text += 1
        Else
            sender.Text -= 1
        End If
    End Sub
    Private Sub TextBox_MouseWheel2(sender As TextBox, e As MouseWheelEventArgs)
        If e.Delta > 0 Then
            sender.Text += 0.1
        Else
            sender.Text -= 0.1
        End If
    End Sub

    '市松模様のオンオフ
    Private Sub cbTransparent_Click(sender As Object, e As RoutedEventArgs) Handles cbTransparent.Click
        Dim cb As CheckBox = DirectCast(sender, CheckBox)
        With imgTransparent

            If cb.IsChecked Then
                .Width = rectMihon.Width
                .Height = rectMihon.Height
            Else
                .Width = 0
                .Height = 0
            End If
        End With
    End Sub



    Private Sub GetKido()
        Dim y As Double
        Dim r As Double = sldR.Value
        Dim g As Double = sldG.Value
        Dim b As Double = sldB.Value
        'Dim c1 As Color = Colors.Red
        'Dim c2 As Color = Colors.Blue
        'Dim cAdd As Color = Color.Add(c1, c2)
        'y = r * 0.29891 + g * 0.58661 + b * 0.11448
        'sldY.Value = y

        Dim x As Double = sldGamma.Value
        y = (r ^ x) * 0.222015 + (g ^ x) * 0.706655 + (b ^ x) * 0.07133
        y = y ^ (1 / x)
        'sldHDTVY.Value = y
        tbkNewY.Text = $"{y:0.0}"

        'Dim min As Double = Math.Min(r, Math.Min(g, b))
        'Dim max As Double = Math.Max(r, Math.Max(g, b))
        'sldMiddleY.Value = (min + max) / 2

        'x = 3.2
        'y = (r ^ x) * 0.222015 + (g ^ x) * 0.706655 + (b ^ x) * 0.07133
        'y = y ^ (1 / x)
        'sldTestY.Value = y


    End Sub


    Private Sub cttext(sender As Object, e As RoutedEventArgs)

        'r02.Fill = rectMihon.Fill

    End Sub

    '色相を変化させた時に元の色の輝度に合うV(明るさ)を返す
    Private Function GetVFixedY() As Double
        Dim h As Double = sldHue.Value
        Dim s As Double = sldS.Value
        Dim v As Double = sldV.Value
        Dim b As SolidColorBrush = rectY.Fill
        Dim col As Color = b.Color
        Dim gamma As Double = sldGamma.Value
        Dim oldY As Double = tbkOldY.Text ' 基準になる輝度
        Dim nh As Double = sldHue.Value
        Dim newY As Double = HSVtoHDTVY(New HSV(nh, s, v), gamma)
        Dim nv As Double

        If newY > oldY Then
            'v下げ、基準輝度を下回るまでVを1づつ下げる、下回ったら1足してから0.1づつ下げて探す
            For i As Integer = v To 0 Step -1
                newY = HSVtoHDTVY(New HSV(nh, s, i), gamma)
                If newY < oldY Then
                    For d As Double = i + 1 To 0 Step -0.1
                        newY = HSVtoHDTVY(New HSV(nh, s, d), gamma)
                        If newY < oldY Then
                            nv = d + 0.1
                            Return nv
                        End If
                    Next
                    'nv = i + 1
                    'Return nv
                End If
            Next
        Else
            'v上げ
            newY = HSVtoHDTVY(New HSV(nh, s, MaxSV), gamma)
            If newY < oldY Then
                nv = MaxSV
                Return nv
            End If
            For i As Integer = v To MaxSV
                newY = HSVtoHDTVY(New HSV(nh, s, i), gamma)
                If newY > oldY Then
                    For d As Double = i - 1 To MaxSV Step 0.1
                        newY = HSVtoHDTVY(New HSV(nh, s, d), gamma)
                        If newY > oldY Then
                            nv = d
                            Return nv
                        End If
                    Next
                    'nv = i
                    'Return nv
                End If
            Next
        End If

        Return nv

    End Function
    Private Function ColortoHDTV(col As Color, gamma As Double) As Double
        Dim y As Double =
            (col.R ^ gamma) * 0.222015 +
            (col.G ^ gamma) * 0.706655 +
            (col.B ^ gamma) * 0.07133
        y = y ^ (1 / gamma)
        Return y
    End Function
    Private Function RGBtoHDTV(rgb As RGB, gamma As Double) As Double
        'HSVtoHDTVY専用かな、通常はColortoHDTVでいい
        Dim y As Double =
            (rgb.R ^ gamma) * 0.222015 +
            (rgb.G ^ gamma) * 0.706655 +
            (rgb.B ^ gamma) * 0.07133
        y = y ^ (1 / gamma)
        Return y
    End Function

    Private Function HSVtoHDTVY(hsv As HSV, gamma As Double) As Double
        'Dim col As Color = HSV2Color(hsv)
        'Return ColortoHDTV(col, gamma) '2.2
        Dim rgb As RGB = HSV2RGB(hsv)
        Return RGBtoHDTV(rgb, gamma)
    End Function

    '輝度固定チェック
    Private Sub cbIsFixed_Click(sender As Object, e As RoutedEventArgs) Handles cbIsFixedY.Click
        If cbIsFixedY.IsChecked Then
            IsYFixed = True
            rectY.Fill = rectMihon.Fill
            tbkOldHSV.Text = $"HSV={sldHue.Value:#.0}, {sldS.Value:#.0}, {sldV.Value:#.0}"
            Dim y As Double = ColortoHDTV(Color.FromArgb(255, sldR.Value, sldG.Value, sldB.Value), sldGamma.Value)
            tbkOldY.Text = $"{y:0.0}"

            For Each c As UIElement In spp.Children

                c.IsEnabled = False
            Next

        Else
            IsYFixed = False

            For Each c As UIElement In spp.Children
                c.IsEnabled = True
            Next

        End If

        dpHue.IsEnabled = True
        dpFixedY.IsEnabled = True
        'gridHue.IsEnabled = True
        tabPalette.IsEnabled = True
    End Sub

    'ガンマ値スライダー
    Private Sub sldGamma_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim b As SolidColorBrush = rectY.Fill
        Dim y As Double = ColortoHDTV(b.Color, e.NewValue)
        tbkOldY.Text = y

        Call sldHue_ValueChanged(sldHue, New RoutedPropertyChangedEventArgs(Of Double)(sldHue.Value, sldHue.Value))
    End Sub











    'OKボタンとキャンセルボタン

    'WPFサンプル:ShowメソッドとShowDaialogメソッド:Gushwell's C# Dev Notes
    'http://gushwell.ldblog.jp/archives/52285322.html

    Private Sub btOK_Click(sender As Object, e As RoutedEventArgs) Handles btOK.Click

        DialogResult = True

    End Sub

    Private Sub btCancel_Click(sender As Object, e As RoutedEventArgs) Handles btCancel.Click

        DialogResult = False

    End Sub



    '    VB ２つ目のフォーム - Show, ShowDialog, DialogResult, Ownerなど
    'http://homepage1.nifty.com/rucio/main/dotnet/shokyu/standard29.htm
    'ダイアログ開いた時、Colorを受け取ってスライダーに反映
    Public Overloads Function ShowDialog(col As Color) As Boolean

        '初期設定のTransparent透明なら赤に強制変換
        If col.Equals(Colors.Transparent) Then
            col = Colors.Red
        End If
        'ARGBのスライダーに値設定、HSVの値はこの後のInitializeイベントで実行
        sldA.Value = col.A
        sldR.Value = col.R
        sldG.Value = col.G
        sldB.Value = col.B

        Return Me.ShowDialog
    End Function


    'テストボタン
    'Private Sub btTest_Click(sender As Object, e As RoutedEventArgs) Handles btTest.Click

    'End Sub

End Class
