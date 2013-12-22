Namespace BMAPI
    Public Class SliderInfo
        Public X? As Integer = Nothing
        Public Y? As Integer = Nothing
        Public StartTime? As Integer = Nothing
        Public NewCombo? As Boolean = True
        Public Effect As EffectType = EffectType.None
        Public Type As SliderType = Nothing
        Public Points As New List(Of Object)
        Public RepeatCount As Integer = 0
        Public MaxPoints? As Integer = Nothing

        Enum SliderType
            Linear = 0
            Cumulative = 1
            Bezier = 2
            PassThrough = 3
        End Enum

        Enum EffectType
            None = 0
            Whistle = 2
            Finish = 4
            WhistleFinish = 6
            Clap = 8
            ClapWhistle = 10
            ClapFinish = 12
            ClapWhistleFinish = 14
        End Enum
    End Class
End Namespace
