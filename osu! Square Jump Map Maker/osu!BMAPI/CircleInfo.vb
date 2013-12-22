Namespace BMAPI
    Public Class CircleInfo
        Public X? As Integer = Nothing
        Public Y? As Integer = Nothing
        Public StartTime? As Integer = Nothing
        Public NewCombo? As Boolean = True
        Public Effect? As EffectType = EffectType.None

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
