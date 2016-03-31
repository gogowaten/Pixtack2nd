'【WPF】【XAML】SliderでNumericUpDown | 創造的プログラミングと粘土細工
'http://pro.art55.jp/?eid=894322


'WPF40.5入門 その53 「ユーザーコントロール」 - かずきのBlog@hatena
'http://blog.okazuki.jp/entry/2014/09/08/203943


Public Class NumericUpDown
    Public Sub New()
        Frequency = 1 '既定値セット

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。


    End Sub

    Private Sub NumericUpDown_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        '小数点桁数用、StringFormatを設定"#0.00"とか
        DigitsStringFormat = GetStringFormat()
    End Sub

    'Valueプロパティ
    'Public Shared ReadOnly ValueProperty As DependencyProperty =
    '        DependencyProperty.Register("Value", GetType(Single), GetType(NumericUpDown), New PropertyMetadata())
    Public Shared ValueProperty As DependencyProperty =
            DependencyProperty.Register("Value", GetType(Single), GetType(NumericUpDown), New PropertyMetadata())
    Public Property Value As Single
        Get
            Return GetValue(ValueProperty)
        End Get
        Set(value As Single)
            If value > Maximum Then
                value = Maximum
            End If
            If value < Minimum Then
                value = Minimum
            End If
            SetValue(ValueProperty, value)
            'textBlockValue.Text = Format(value, "#0.00") ' value '必要ないけどこっちで表示したほうがよさそう
            textBlockValue.Text = Format(value, DigitsStringFormat)
        End Set
    End Property


    'Frequencyプロパティ、ボタン押した時に変化させる値
    Public Shared ReadOnly FrequencyProperty As DependencyProperty =
        DependencyProperty.Register("Frequency", GetType(Single), GetType(NumericUpDown), New PropertyMetadata())

    Public Property Frequency As Single
        Get
            Return GetValue(FrequencyProperty)
        End Get
        Set(value As Single)
            SetValue(FrequencyProperty, value)
        End Set
    End Property

    '表示する小数点桁数
    Private DigitsStringFormat As String
    Public Property Digits As Integer
    Private Function GetStringFormat() As String
        Dim d As String = "#0"

        If Digits > 0 Then
            d &= "."
            For i As Integer = 1 To Digits
                d &= "0"
            Next
        End If

        Return d
    End Function

    Public Property Maximum As Single
    Public Property Minimum As Single
    '↓を使うとスライダーの最大値とかとBindingできるけどアプリ起動時には反映されていなくて0が最大値になって表示される値が0に変更されてしまう
    'Public Shared ReadOnly MaxProperty As DependencyProperty =
    '    DependencyProperty.Register("Max", GetType(Single), GetType(NumericUpDown), New PropertyMetadata())
    'Public Property Max As Single
    '    Get
    '        Return GetValue(MaxProperty)
    '    End Get
    '    Set(value As Single)
    '        SetValue(MaxProperty, value)
    '    End Set
    'End Property

    'Public Shared ReadOnly MinProperty As DependencyProperty =
    '    DependencyProperty.Register("Min", GetType(Single), GetType(NumericUpDown), New PropertyMetadata())
    'Public Property Min As Single
    '    Get
    '        Return GetValue(MinProperty)
    '    End Get
    '    Set(value As Single)
    '        SetValue(MinProperty, value)
    '    End Set
    'End Property


    Private Sub UpButton_Click(sender As Object, e As RoutedEventArgs)
        'Value += 1
        'textBlockValue.Text += Frequency
        Value += Frequency
        'textBlockValue.Text = Value

    End Sub

    Private Sub DownButton_Click(sender As Object, e As RoutedEventArgs)
        'Value -= 1
        'textBlockValue.Text -= Frequency
        Value -= Frequency
    End Sub


    'テキストボックスに値を入力した時
    Private Sub textBlockValue_TextChanged(sender As Object, e As TextChangedEventArgs) Handles textBlockValue.TextChanged
        'PreviewTextImputイベントのほうで処理したから要らないかも？
        '数値型のValueに入力された内容を入れてみてエラーならメッセージ表示
        Dim t As String = textBlockValue.Text
        Try
            Value = t
        Catch ex As Exception
            '入力内容が空白の時や-+以外ならメッセージ表示、テキスト選択状態
            If t <> "" Then
                If (t = "-" Or t = "+") = False Then
                    MsgBox("入力できるのは数値と+-だけです")
                    textBlockValue.SelectAll()
                    e.Handled = True
                End If
            End If
        End Try
    End Sub

    Private Sub textBlockValue_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles textBlockValue.PreviewTextInput
        '入力キーが数値か-マイナス記号以外ならスルー
        Dim s = textBlockValue.SelectionStart '入力カーソルキーの位置
        Dim t As String = e.Text '入力された文字(キー)

        'カーソル位置が先頭(0)以外なら数値だけ受付
        'カーソル位置が先頭なら-も受付だけど1番めの文字が-ならスルー
        If IsNumeric(t) Then

        ElseIf t = "-" Then
            If s <> 0 Then
                e.Handled = True
            End If
        Else
            e.Handled = True
        End If
    End Sub

    Private Sub NumericUpDown_MouseWheel(sender As Object, e As MouseWheelEventArgs) Handles Me.MouseWheel
        If e.Delta > 0 Then
            textBlockValue.Text += Frequency
        Else
            textBlockValue.Text -= Frequency
        End If

    End Sub


End Class
