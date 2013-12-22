Namespace BMAPI
    Public Class BeatmapInfo
        Public Format As Integer = 12

        'General
        Public AudioFilename As String = Nothing
        Public AudioLeadIn? As Integer = 0
        Public PreviewTime As Integer = -1
        Public Countdown As Integer = 1
        Public SampleSet As String = "None"
        Public StackLeniency? As Double = 0.7
        Public Mode As GameMode = GameMode.osu
        Public LetterboxInBreaks As Integer = 1
        Public SpecialStyle? As Integer = Nothing
        Public CountdownOffset? As Integer = Nothing
        Public OverlayPosition As OverlayOptions
        Public SkinPreference As String = Nothing
        Public WidescreenStoryboard As Integer = 0
        Public UseSkinSprites? As Integer = Nothing
        Public StoryFireInFront As Integer = 0
        Public EpilepsyWarning As Integer = 0
        Public CustomSamples? As Integer = Nothing
        Public EditorBookmarks As New List(Of Integer)
        Public EditorDistanceSpacing? As Double = Nothing
        Public AudioHash As String = Nothing
        Public AlwaysShowPlayfield? As Boolean = Nothing

        'Editor (Other Editor tag stuff (v12))
        Public GridSize As Integer = 8
        Public Bookmarks As New List(Of Integer)
        Public BeatDivisor As Integer = 4
        Public DistanceSpacing As Double = 1
        Public CurrentTime? As Integer = Nothing

        'Metadata
        Public Title As String = ""
        Public TitleUnicode As String = ""
        Public Artist As String = ""
        Public ArtistUnicode As String = ""
        Public Creator As String = ""
        Public Version As String = ""
        Public Source As String = ""
        Public Tags As New List(Of String)
        Public BeatmapID As Integer = 0
        Public BeatmapSetID As Integer = -1

        'Difficulty
        Public HPDrainRate As Integer = 5
        Public CircleSize As Integer = 5
        Public OverallDifficulty As Integer = 5
        Public ApproachRate As Integer = 5
        Public SliderMultiplier As Double = 1.4
        Public SliderTickRate As Double = 1

        'Events
        Public Events As New List(Of Object)

        'Timingpoints
        Public TimingPoints As New List(Of Object)

        'Colours
        Public ComboColours As New List(Of BMAPI.ComboInfo)
        Public SliderBorder As BMAPI.ColourInfo

        'Hitobjects
        Public HitObjects As New List(Of Object)

        Enum OverlayOptions
            Above = 0
            Below = 1
        End Enum
        Enum GameMode
            osu = 0
            Taiko = 1
            CatchtheBeat = 2
            osuMania = 3
        End Enum
    End Class
End Namespace
