
Imports System.Windows.Controls.Primitives

Class ColorPalette
    Private Const MaxSV As Integer = 100 'SとVの最大値、100%表示なら100、詳細なら255指定
    Private IsDrag As Boolean 'マーカーを移動中フラグ
    Private IsChangeHSV As Boolean 'HSVのスライダーで値変更中フラグ
    Private IsChangeRGB As Boolean 'RGBのスライダーで値変更中フラグ

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
    Private Function HSV2RGB(hsv As HSV) As Color
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


    Private Sub AddHueBar()
        '色相の画像作成
        Dim w As Integer = 20 ' imgHue.Width
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
            col = HSV2RGB(hsv)
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
                col = HSV2RGB(hsv)
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

        tbkRGB2.Text = $"ARGB = {col.A:000},{col.R:000},{col.G:000},{col.B:000}"
    End Sub

    'HSVの変更のときRGBを更新
    Private Sub ValueChangedHSV()
        Dim col As Color = HSV2RGB(New HSV(sldHue.Value, sldS.Value, sldV.Value))
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

        AddHandler tbxH.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxS.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxV.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxA.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxR.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxG.MouseWheel, AddressOf TextBox_MouseWheel
        AddHandler tbxB.MouseWheel, AddressOf TextBox_MouseWheel

        Call sldRGB_ValueChanged(sldR, New RoutedPropertyChangedEventArgs(Of Double)(sldR.Value, sldR.Value))

        'ウィンドウの高さ調整
        Dim neko = spp.Height
        neko = spp.ActualHeight
        Height = neko

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

    '色相スライダー変化でSV画像更新
    Private Sub sldHue_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If IsChangeRGB Then Return

        IsChangeHSV = True
        Call ChangeImageSV(e.NewValue)
        Call ValueChangedHSV()
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
        If e.Delta > 0 Then
            sender.Value += 1
        Else
            sender.Value -= 1
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


End Class







