﻿'------------------------------------------------------------------------------
' <auto-generated>
'     このコードはツールによって生成されました。
'     ランタイム バージョン:4.0.30319.42000
'
'     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
'     コードが再生成されるときに損失したりします。
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On



<Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
 Global.System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")>  _
Partial Friend NotInheritable Class Settings1
    Inherits Global.System.Configuration.ApplicationSettingsBase
    
    Private Shared defaultInstance As Settings1 = CType(Global.System.Configuration.ApplicationSettingsBase.Synchronized(New Settings1()),Settings1)
    
    Public Shared ReadOnly Property [Default]() As Settings1
        Get
            Return defaultInstance
        End Get
    End Property
    
    <Global.System.Configuration.UserScopedSettingAttribute(),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Configuration.DefaultSettingValueAttribute("0")>  _
    Public Property MainWindow_Left() As Double
        Get
            Return CType(Me("MainWindow_Left"),Double)
        End Get
        Set
            Me("MainWindow_Left") = value
        End Set
    End Property
    
    <Global.System.Configuration.UserScopedSettingAttribute(),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Configuration.DefaultSettingValueAttribute("0")>  _
    Public Property MainWindow_Top() As Double
        Get
            Return CType(Me("MainWindow_Top"),Double)
        End Get
        Set
            Me("MainWindow_Top") = value
        End Set
    End Property
    
    <Global.System.Configuration.UserScopedSettingAttribute(),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Configuration.DefaultSettingValueAttribute("0,0,902,703")>  _
    Public Property MainWindow_Bounds() As Global.System.Windows.Rect
        Get
            Return CType(Me("MainWindow_Bounds"),Global.System.Windows.Rect)
        End Get
        Set
            Me("MainWindow_Bounds") = value
        End Set
    End Property
End Class
