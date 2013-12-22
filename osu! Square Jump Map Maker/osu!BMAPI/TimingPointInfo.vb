Namespace BMAPI
    Public Class TimingPointInfo
        Public Time? As Integer = Nothing
        Public BPMDelay? As Double = Nothing
        Public TimeSignature? As Integer = Nothing
        Public SampleSet? As Integer = Nothing
        Public CustomSampleSet? As Integer = Nothing
        Public VolumePercentage As Integer = 100
        Public isInherited As Boolean = False
        Public KiaiTime As Boolean = False
        Public OmitFirstBarLine As Boolean = False
    End Class
End Namespace
