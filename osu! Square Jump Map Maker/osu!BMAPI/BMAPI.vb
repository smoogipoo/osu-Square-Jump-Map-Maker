Imports System.Reflection
Namespace BMAPI
    Public Class Beatmap
        Inherits BeatmapInfo

        Private Info As New BeatmapInfo
        Private BM_Sections As New Dictionary(Of String, String)
        Private WriteBuffer As New List(Of String)
        Private SectionLength As New Dictionary(Of String, Integer)

        Sub New(Optional ByVal beatmapfile As String = "")
            Dim lastculture As Globalization.CultureInfo = Threading.Thread.CurrentThread.CurrentCulture
            Threading.Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo("en-US", False)

            'Variable init
            BM_Sections.Add("AudioFilename,AudioLeadIn,PreviewTime,Countdown,SampleSet,StackLeniency,Mode,LetterboxInBreaks,SpecialStyle,CountdownOffset,OverlayPosition,SkinPreference,WidescreenStoryboard,UseSkinSprites,StoryFireInFront,EpilepsyWarning,CustomSamples,EditorDistanceSpacing,AudioHash,AlwaysShowPlayfield", "General")
            BM_Sections.Add("GridSize,BeatDivisor,DistanceSpacing,CurrentTime", "Editor")
            BM_Sections.Add("Title,TitleUnicode,Artist,ArtistUnicode,Creator,Version,Source,BeatmapID,BeatmapSetID", "Metadata")
            BM_Sections.Add("HPDrainRate,CircleSize,OverallDifficulty,ApproachRate,SliderMultiplier,SliderTickRate", "Difficulty")

            If beatmapfile <> "" Then
                If IO.File.Exists(beatmapfile) Then
                    Try
                        Parse(beatmapfile)
                    Catch ex As Exception
                        Throw New Exception("Parsing beatmap failed.", New Exception(ex.Message, ex.InnerException))
                    End Try
                End If
            End If
            Threading.Thread.CurrentThread.CurrentCulture = lastculture
        End Sub

        Private Sub Parse(ByVal bm As String)
            Using sR As New IO.StreamReader(bm)
                Dim currentsection As String = ""

                'Get the beatmap version
                Dim versionline As String = sR.ReadLine
                For i = 3 To 12 'Supported versions: 3-12
                    If versionline = "osu file format v" & i Then
                        Info.Format = i
                    End If
                Next

                If Info.Format <> -1 Then
                    Do While sR.Peek <> -1
                        Dim line As String = sR.ReadLine

                        'Check for blank lines
                        If (line = "") Or (line.Length < 2) Then
                            Continue Do
                        End If

                        'Check for section tag
                        If line.Substring(0, 1) = "[" Then
                            currentsection = line
                            Continue Do
                        End If


                        'Check for commented-out line
                        If line.Substring(0, 2) = "//" Then
                            Continue Do
                        End If

                        'Do work for [General], [Metadata] and [Difficulty] sections
                        If (currentsection = "[General]") Or (currentsection = "[Metadata]") Or (currentsection = "[Difficulty]") Or (currentsection = "[Editor]") Then
                            Dim cProperty As String = line.Substring(0, line.IndexOf(":"))
                            Dim cValue As String

                            'Check for blank value
                            If line.Count = line.IndexOf(":") + 1 Then
                                cValue = ""
                            Else
                                'Check if there is a space between : and data
                                If line.Substring(line.IndexOf(":") + 1, 1) = " " Then
                                    cValue = line.Substring(line.IndexOf(":") + 2)
                                Else
                                    cValue = line.Substring(line.IndexOf(":") + 1)
                                End If
                            End If

                            'Import properties into Info
                            If cProperty = "EditorBookmarks" Then
                                Dim marks() As String = cValue.Split(",")
                                For Each m In marks
                                    If m <> "" Then
                                        Info.EditorBookmarks.Add(CInt(m))
                                    End If
                                Next
                            ElseIf cProperty = "Bookmarks" Then
                                Dim marks() As String = cValue.Split(",")
                                For Each m In marks
                                    If m <> "" Then
                                        Info.Bookmarks.Add(CInt(m))
                                    End If
                                Next
                            ElseIf cProperty = "Tags" Then
                                Dim tags() As String = cValue.Split(" ")
                                For Each t In tags
                                    Info.Tags.Add(t)
                                Next
                            ElseIf cProperty = "Mode" Then
                                Select Case cValue
                                    Case 0
                                        Info.Mode = BeatmapInfo.GameMode.osu
                                    Case 1
                                        Info.Mode = BeatmapInfo.GameMode.Taiko
                                    Case 2
                                        Info.Mode = BeatmapInfo.GameMode.CatchtheBeat
                                    Case 3
                                        Info.Mode = BeatmapInfo.GameMode.osuMania
                                End Select
                            ElseIf cProperty = "OverlayPosition" Then
                                Select Case cValue
                                    Case "Above"
                                        Info.OverlayPosition = BeatmapInfo.OverlayOptions.Above
                                    Case "Below"
                                        Info.OverlayPosition = BeatmapInfo.OverlayOptions.Below
                                End Select
                            ElseIf cProperty = "AlwaysShowPlayfield" Then
                                Info.AlwaysShowPlayfield = CBool(cValue)
                            Else
                                'Import other values Info
                                Dim fi As FieldInfo = Info.GetType().GetField(cProperty)
                                If fi.FieldType Is GetType(Double) Then
                                    fi.SetValue(Info, CDbl(cValue))
                                ElseIf fi.FieldType Is GetType(Integer) Then
                                    fi.SetValue(Info, CInt(cValue))
                                ElseIf fi.FieldType Is GetType(String) Then
                                    fi.SetValue(Info, CStr(cValue))
                                End If

                                Continue Do
                            End If
                        End If

                        'The following are version-dependent, the version is stored as a numeric value inside Info.Format

                        'Do work for [Events] section
                        If currentsection = "[Events]" Then
                            Select Case Info.Format
                                Case 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
                                    Dim eventtype As String = SubStr(line, 0, nthDexOf(line, ",", 0))
                                    If eventtype = "0" Then
                                        Info.Events.Add(New BackgroundInfo)
                                        Info.Events(Info.Events.Count - 1).Time = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.Events(Info.Events.Count - 1).Filename = SubStr(line, nthDexOf(line, ",", 1) + 1).Replace(Chr(34), "")
                                    ElseIf (eventtype = "1") Or (eventtype.ToLower = "video") Then

                                        Info.Events.Add(New VideoInfo)
                                        Info.Events(Info.Events.Count - 1).Time = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.Events(Info.Events.Count - 1).Filename = SubStr(line, nthDexOf(line, ",", 1) + 1).Replace(Chr(34), "")
                                    ElseIf eventtype = "2" Then

                                        Info.Events.Add(New BreakInfo)
                                        Info.Events(Info.Events.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.Events(Info.Events.Count - 1).EndTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1))
                                    ElseIf eventtype = "3" Then
                                        Info.Events.Add(New ColourInfo)
                                        Info.Events(Info.Events.Count - 1).Time = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.Events(Info.Events.Count - 1).R = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        Info.Events(Info.Events.Count - 1).G = CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3)))
                                        Info.Events(Info.Events.Count - 1).B = CInt(SubStr(line, nthDexOf(line, ",", 3) + 1))
                                    End If
                            End Select
                        End If

                        'Do work for [TimingPoints] section
                        If currentsection = "[TimingPoints]" Then
                            Select Case Info.Format
                                Case 3
                                    Info.TimingPoints.Add(New TimingPointInfo)
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).Time = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).BPMDelay = CDbl(SubStr(line, nthDexOf(line, ",", 0) + 1))
                                Case 4
                                    Info.TimingPoints.Add(New TimingPointInfo)
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).Time = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).BPMDelay = CDbl(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).TimeSignature = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).SampleSet = CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).CustomSampleSet = CInt(SubStr(line, nthDexOf(line, ",", 3) + 1))
                                Case 5
                                    Dim splitstring() As String = line.Split(",")
                                    Select Case splitstring.Count
                                        Case 6
                                            Info.TimingPoints.Add(New TimingPointInfo)
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).Time = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).BPMDelay = CDbl(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).TimeSignature = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).SampleSet = CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).CustomSampleSet = CInt(SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).VolumePercentage = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1))
                                        Case 7
                                            Info.TimingPoints.Add(New TimingPointInfo)
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).Time = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).BPMDelay = CDbl(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).TimeSignature = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).SampleSet = CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).CustomSampleSet = CInt(SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).VolumePercentage = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).IsInherited = Not CBool(SubStr(line, nthDexOf(line, ",", 5) + 1))
                                        Case 8
                                            Info.TimingPoints.Add(New TimingPointInfo)
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).Time = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).BPMDelay = CDbl(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).TimeSignature = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).SampleSet = CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).CustomSampleSet = CInt(SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).VolumePercentage = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)))
                                            Info.TimingPoints(Info.TimingPoints.Count - 1).IsInherited = Not CBool(SubStr(line, nthDexOf(line, ",", 5) + 1, nthDexOf(line, ",", 6)))
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1) = "1" Then
                                                Info.TimingPoints(Info.TimingPoints.Count - 1).KiaiTime = True
                                            ElseIf SubStr(line, nthDexOf(line, ",", 6) + 1) = "8" Then
                                                Info.TimingPoints(Info.TimingPoints.Count - 1).OmitFirstBarLine = True
                                            ElseIf SubStr(line, nthDexOf(line, ",", 6) + 1) = "9" Then
                                                Info.TimingPoints(Info.TimingPoints.Count - 1).KiaiTime = True
                                                Info.TimingPoints(Info.TimingPoints.Count - 1).OmitFirstBarLine = True
                                            End If
                                    End Select
                                Case 6, 7, 8, 9, 10, 11, 12
                                    Info.TimingPoints.Add(New TimingPointInfo)
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).Time = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).BPMDelay = CDbl(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).TimeSignature = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).SampleSet = CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).CustomSampleSet = CInt(SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).VolumePercentage = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)))
                                    Info.TimingPoints(Info.TimingPoints.Count - 1).IsInherited = Not CBool(SubStr(line, nthDexOf(line, ",", 5) + 1, nthDexOf(line, ",", 6)))
                                    If SubStr(line, nthDexOf(line, ",", 6) + 1) = "1" Then
                                        Info.TimingPoints(Info.TimingPoints.Count - 1).KiaiTime = True
                                    ElseIf SubStr(line, nthDexOf(line, ",", 6) + 1) = "8" Then
                                        Info.TimingPoints(Info.TimingPoints.Count - 1).OmitFirstBarLine = True
                                    ElseIf SubStr(line, nthDexOf(line, ",", 6) + 1) = "9" Then
                                        Info.TimingPoints(Info.TimingPoints.Count - 1).KiaiTime = True
                                        Info.TimingPoints(Info.TimingPoints.Count - 1).OmitFirstBarLine = True
                                    End If
                            End Select
                        End If

                        'Do work for [Colours] section
                        If currentsection = "[Colours]" Then
                            Select Case Info.Format
                                Case 5, 6, 7, 8, 9, 10, 11, 12
                                    If line.Substring(0, line.IndexOf(":")).Replace(" ", "") = "SliderBorder" Then
                                        Dim value As String = line.Substring(line.IndexOf(":") + 1).Replace(" ", "")
                                        Info.SliderBorder = New BMAPI.ColourInfo
                                        Info.SliderBorder.R = CInt(SubStr(value, 0, nthDexOf(value, ",", 0)))
                                        Info.SliderBorder.G = CInt(SubStr(value, nthDexOf(value, ",", 0) + 1, nthDexOf(value, ",", 1)))
                                        Info.SliderBorder.B = CInt(SubStr(value, nthDexOf(value, ",", 1) + 1))
                                    ElseIf line.Substring(0, 5) = "Combo" Then
                                        Info.ComboColours.Add(New BMAPI.ComboInfo)
                                        Info.ComboColours(Info.ComboColours.Count - 1).ComboNumber = CInt(line.Substring(5, 1))
                                        Dim value As String = line.Substring(line.IndexOf(":") + 1).Replace(" ", "")
                                        Info.ComboColours(Info.ComboColours.Count - 1).Colour = New BMAPI.ColourInfo
                                        Info.ComboColours(Info.ComboColours.Count - 1).Colour.R = CInt(SubStr(value, 0, nthDexOf(value, ",", 0)))
                                        Info.ComboColours(Info.ComboColours.Count - 1).Colour.G = CInt(SubStr(value, nthDexOf(value, ",", 0) + 1, nthDexOf(value, ",", 1)))
                                        Info.ComboColours(Info.ComboColours.Count - 1).Colour.B = CInt(SubStr(value, nthDexOf(value, ",", 1) + 1))
                                    End If
                            End Select
                        End If

                        'Do work for [HitObjects] section
                        If currentsection = "[HitObjects]" Then
                            Select Case Info.Format
                                Case 3, 4
                                    If line.Substring(line.Count - 1, 1) = "," Then
                                        line = line.Substring(0, line.Count - 1)
                                    End If
                                    Dim splitstring() As String = line.Split(",")
                                    If splitstring.Count = 5 Then
                                        'Circle
                                        Info.HitObjects.Add(New CircleInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1)
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapWhistleFinish
                                        End Select
                                    ElseIf (splitstring(5).Substring(0, 1) = "B") Or (splitstring(5).Substring(0, 1) = "C") Or (splitstring(5).Substring(0, 1) = "L") Or (splitstring(5).Substring(0, 1) = "P") Then
                                        'Slider
                                        Info.HitObjects.Add(New SliderInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1)
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapWhistleFinish
                                        End Select
                                        Select Case splitstring(5).Substring(0, 1)
                                            Case "B"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Bezier
                                            Case "C"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Cumulative
                                            Case "L"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Linear
                                            Case "P"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.PassThrough
                                        End Select
                                        Dim pts() As String = SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)).Split("|")
                                        For i = 1 To pts.Count - 1  'To Check: If both 3 & 4 start their point definitions with the slider start point
                                            Dim p As New BMAPI.PointInfo
                                            p.X = CInt(pts(i).Substring(0, pts(i).IndexOf(":")))
                                            p.Y = CInt(pts(i).Substring(pts(i).IndexOf(":") + 1))
                                            Info.HitObjects(Info.HitObjects.Count - 1).Points.Add(p)
                                        Next
                                        Info.HitObjects(Info.HitObjects.Count - 1).RepeatCount = CInt(SubStr(line, nthDexOf(line, ",", 5) + 1, nthDexOf(line, ",", 6)))
                                        If (IsNumeric(SubStr(line, nthDexOf(line, ",", 6) + 1))) Then
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1) <> "NaN" Then
                                                Info.HitObjects(Info.HitObjects.Count - 1).MaxPoints = CInt(SubStr(line, nthDexOf(line, ",", 6) + 1))
                                            End If
                                        Else
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1, nthDexOf(line, ",", 7)) <> "NaN" Then
                                                Info.HitObjects(Info.HitObjects.Count - 1).MaxPoints = CInt(SubStr(line, nthDexOf(line, ",", 6) + 1, nthDexOf(line, ",", 7)))
                                            End If
                                        End If
                                    Else
                                        'Spinner
                                        Info.HitObjects.Add(New SpinnerInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1)
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapWhistleFinish
                                        End Select
                                        Info.HitObjects(Info.HitObjects.Count - 1).EndTime = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1))
                                    End If
                                Case 5, 6, 7, 8, 9 'Note: Until I find out what the last few bytes at the end of these versions sliders represent, I will ignore them.
                                    Dim splitstring() As String = line.Split(",")
                                    If splitstring.Count = 5 Then
                                        'Circle
                                        Info.HitObjects.Add(New CircleInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1)
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapWhistleFinish
                                        End Select
                                    ElseIf (splitstring(5).Substring(0, 1) = "B") Or (splitstring(5).Substring(0, 1) = "C") Or (splitstring(5).Substring(0, 1) = "L") Or (splitstring(5).Substring(0, 1) = "P") Then
                                        'Slider
                                        Info.HitObjects.Add(New SliderInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4))
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapWhistleFinish
                                        End Select
                                        Select Case splitstring(5).Substring(0, 1)
                                            Case "B"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Bezier
                                            Case "C"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Cumulative
                                            Case "L"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Linear
                                            Case "P"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.PassThrough
                                        End Select
                                        Dim pts() As String = SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)).Split("|")
                                        For i = 1 To pts.Count - 1
                                            Dim p As New BMAPI.PointInfo
                                            p.X = CInt(pts(i).Substring(0, pts(i).IndexOf(":")))
                                            p.Y = CInt(pts(i).Substring(pts(i).IndexOf(":") + 1))
                                            Info.HitObjects(Info.HitObjects.Count - 1).Points.Add(p)
                                        Next
                                        Info.HitObjects(Info.HitObjects.Count - 1).RepeatCount = CInt(SubStr(line, nthDexOf(line, ",", 5) + 1, nthDexOf(line, ",", 6)))
                                        If (IsNumeric(SubStr(line, nthDexOf(line, ",", 6) + 1))) Then
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1) <> "NaN" Then
                                                Info.HitObjects(Info.HitObjects.Count - 1).MaxPoints = CInt(SubStr(line, nthDexOf(line, ",", 6) + 1))
                                            End If
                                        Else
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1, nthDexOf(line, ",", 7)) <> "NaN" Then
                                                Info.HitObjects(Info.HitObjects.Count - 1).MaxPoints = CInt(SubStr(line, nthDexOf(line, ",", 6) + 1, nthDexOf(line, ",", 7)))
                                            End If
                                        End If
                                    Else
                                        'Spinner
                                        Info.HitObjects.Add(New SpinnerInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4))
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapWhistleFinish
                                        End Select
                                        Info.HitObjects(Info.HitObjects.Count - 1).EndTime = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1))
                                    End If
                                Case 10, 11, 12 'Note: Until I figure out what these last bytes represent, I will ignore them
                                    Dim splitstring() As String = line.Split(",")
                                    Dim circlecount As Integer = 5
                                    If splitstring(splitstring.Count - 1).Contains(":") Then
                                        circlecount = 6
                                    End If
                                    If splitstring.Count = circlecount Then
                                        'Circle
                                        Info.HitObjects.Add(New CircleInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1)
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.ClapWhistleFinish
                                        End Select
                                    ElseIf (splitstring(5).Substring(0, 1) = "B") Or (splitstring(5).Substring(0, 1) = "C") Or (splitstring(5).Substring(0, 1) = "L") Or (splitstring(5).Substring(0, 1) = "P") Then
                                        'Slider
                                        Info.HitObjects.Add(New SliderInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4))
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = CircleInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SliderInfo.EffectType.ClapWhistleFinish
                                        End Select
                                        Select Case splitstring(5).Substring(0, 1)
                                            Case "B"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Bezier
                                            Case "C"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Cumulative
                                            Case "L"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.Linear
                                            Case "P"
                                                Info.HitObjects(Info.HitObjects.Count - 1).Type = SliderInfo.SliderType.PassThrough
                                        End Select
                                        Dim pts() As String = SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)).Split("|")
                                        For i = 1 To pts.Count - 1
                                            Dim p As New BMAPI.PointInfo
                                            p.X = CInt(pts(i).Substring(0, pts(i).IndexOf(":")))
                                            p.Y = CInt(pts(i).Substring(pts(i).IndexOf(":") + 1))
                                            Info.HitObjects(Info.HitObjects.Count - 1).Points.Add(p)
                                        Next
                                        Info.HitObjects(Info.HitObjects.Count - 1).RepeatCount = CInt(SubStr(line, nthDexOf(line, ",", 5) + 1, nthDexOf(line, ",", 6)))
                                        If (IsNumeric(SubStr(line, nthDexOf(line, ",", 6) + 1))) Then
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1) <> "NaN" Then
                                                Info.HitObjects(Info.HitObjects.Count - 1).MaxPoints = CInt(SubStr(line, nthDexOf(line, ",", 6) + 1))
                                            End If
                                        Else
                                            If SubStr(line, nthDexOf(line, ",", 6) + 1, nthDexOf(line, ",", 7)) <> "NaN" Then
                                                Info.HitObjects(Info.HitObjects.Count - 1).MaxPoints = CInt(SubStr(line, nthDexOf(line, ",", 6) + 1, nthDexOf(line, ",", 7)))
                                            End If
                                        End If
                                    Else
                                        'Spinner
                                        Info.HitObjects.Add(New SpinnerInfo)
                                        Info.HitObjects(Info.HitObjects.Count - 1).X = CInt(SubStr(line, 0, nthDexOf(line, ",", 0)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).Y = CInt(SubStr(line, nthDexOf(line, ",", 0) + 1, nthDexOf(line, ",", 1)))
                                        Info.HitObjects(Info.HitObjects.Count - 1).StartTime = CInt(SubStr(line, nthDexOf(line, ",", 1) + 1, nthDexOf(line, ",", 2)))
                                        If CInt(SubStr(line, nthDexOf(line, ",", 2) + 1, nthDexOf(line, ",", 3))) = 5 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = True
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).NewCombo = False
                                        End If
                                        Select Case SubStr(line, nthDexOf(line, ",", 3) + 1, nthDexOf(line, ",", 4))
                                            Case 0
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.None
                                            Case 2
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Whistle
                                            Case 4
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Finish
                                            Case 6
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.WhistleFinish
                                            Case 8
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.Clap
                                            Case 10
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapWhistle
                                            Case 12
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapFinish
                                            Case 14
                                                Info.HitObjects(Info.HitObjects.Count - 1).Effect = SpinnerInfo.EffectType.ClapWhistleFinish
                                        End Select
                                        If splitstring.Count = 7 Then
                                            Info.HitObjects(Info.HitObjects.Count - 1).EndTime = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1, nthDexOf(line, ",", 5)))
                                        Else
                                            Info.HitObjects(Info.HitObjects.Count - 1).EndTime = CInt(SubStr(line, nthDexOf(line, ",", 4) + 1))
                                        End If
                                    End If
                            End Select
                        End If
                    Loop
                End If
            End Using
            For Each fi As FieldInfo In Info.GetType.GetFields()
                Dim ff As FieldInfo = Me.GetType().GetField(fi.Name)
                ff.SetValue(Me, fi.GetValue(Info))
            Next
        End Sub

        Public Sub Save(ByVal filename As String)
            Dim lastculture As Globalization.CultureInfo = Threading.Thread.CurrentThread.CurrentCulture
            Threading.Thread.CurrentThread.CurrentCulture = New Globalization.CultureInfo("en-US", False)

            Save("", "osu file format v" & Info.Format)
            Dim newfields() As FieldInfo = Me.GetType.GetFields()
            Dim oldfields() As FieldInfo = Info.GetType.GetFields()

            For Each f1 In newfields
                For Each f2 In oldfields
                    If f1.Name = f2.Name Then
                        If (f1.Name = "EditorBookmarks") Then
                            Dim temps As List(Of Integer) = f1.GetValue(Me)
                            If Not temps Is Nothing Then
                                Save("General", "EditorBookmarks:" & String.Join(",", temps.Select(Function(t) t.ToString).ToArray))
                            End If
                        ElseIf f1.Name = "Bookmarks" Then
                            Dim temps As List(Of Integer) = f1.GetValue(Me)
                            If Not temps Is Nothing Then
                                Save("Editor", "Bookmarks:" & String.Join(",", temps.Select(Function(t) t.ToString).ToArray))
                            End If
                        ElseIf f1.Name = "Tags" Then
                            Dim temps As List(Of String) = f1.GetValue(Me)
                            If Not temps Is Nothing Then
                                Save("Metadata", "Tags:" & String.Join(" ", temps.ToArray))
                            End If
                        ElseIf f1.Name = "Events" Then
                            If (Info.Format >= 3) And (Info.Format <= 12) Then
                                For Each o In f1.GetValue(Me)
                                    If o.GetType() Is GetType(BackgroundInfo) Then
                                        Save("Events", "0," & o.Time & "," & o.Filename)
                                    ElseIf o.GetType() Is GetType(VideoInfo) Then
                                        Save("Events", "1," & o.Time & "," & o.Filename)
                                    ElseIf o.GetType() Is GetType(BreakInfo) Then
                                        Save("Events", "2," & o.StartTime & "," & o.EndTime)
                                    ElseIf o.GetType() Is GetType(ColourInfo) Then
                                        Save("Events", "3," & o.Time & "," & o.R & "," & o.G & "," & o.B)
                                    End If
                                Next
                            End If
                        ElseIf f1.Name = "TimingPoints" Then
                            Select Case Info.Format
                                Case 3
                                    For Each o In f1.GetValue(Me)
                                        Save("TimingPoints", o.Time & "," & o.BPMDelay)
                                    Next
                                Case 4
                                    For Each o In f1.GetValue(Me)
                                        Save("TimingPoints", o.Time & "," & o.BPMDelay & "," & o.TimeSignature & "," & o.SampleSet & "," & o.CustomSampleSet)
                                    Next
                                Case 5, 6, 7, 8, 9, 10, 11, 12
                                    For Each o In f1.GetValue(Me)
                                        Dim options As Integer = 0
                                        If o.KiaiTime = True Then
                                            options += 1
                                        End If
                                        If o.OmitFirstBarLine = True Then
                                            options += 8
                                        End If
                                        Dim inherited As Integer = 0
                                        If o.isInherited = True Then
                                            inherited = 0
                                        Else
                                            inherited = 1
                                        End If
                                        Save("TimingPoints", o.Time & "," & o.BPMDelay & "," & o.TimeSignature & "," & o.SampleSet & "," & o.CustomSampleSet & "," & o.VolumePercentage & "," & inherited & "," & options)
                                    Next
                            End Select
                        ElseIf f1.Name = "ComboColours" Then
                            If (Info.Format >= 5) And (Info.Format <= 12) Then
                                For Each o In f1.GetValue(Me)
                                    If Not o Is Nothing Then
                                        Save("Colours", "Combo" & o.ComboNumber & " : " & o.Colour.R & "," & o.Colour.G & "," & o.Colour.B)
                                    End If
                                Next
                            End If
                        ElseIf f1.Name = "SliderBorder" Then
                            If (Info.Format >= 5) And (Info.Format <= 12) Then
                                Dim o As ColourInfo = f1.GetValue(Me)
                                If Not o Is Nothing Then
                                    Save("Colours", "SliderBorder:" & o.R & "," & o.G & "," & o.B)
                                End If
                            End If
                        ElseIf f1.Name = "HitObjects" Then
                            Select Case Info.Format
                                Case 3, 4
                                    For Each o In f1.GetValue(Me)
                                        Dim combo As Integer = 5
                                        If o.NewCombo = True Then
                                            combo = 5
                                        Else
                                            combo = 1
                                        End If
                                        If o.GetType Is GetType(CircleInfo) Then
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & ",")
                                        ElseIf o.GetType Is GetType(SliderInfo) Then
                                            Dim pointstring As String = ""
                                            For Each p As PointInfo In o.Points
                                                pointstring = pointstring & "|" & p.X & ":" & p.Y
                                            Next
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & "," & o.Type & pointstring & "," & o.RepeatCount & "," & o.MaxPoints)
                                        ElseIf o.GetType Is GetType(SpinnerInfo) Then
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & "," & o.EndTime)
                                        End If
                                    Next
                                Case 5, 6, 7, 8, 9
                                    For Each o In f1.GetValue(Me)
                                        Dim combo As Integer = 5
                                        If o.NewCombo = True Then
                                            combo = 5
                                        Else
                                            combo = 1
                                        End If
                                        If o.GetType Is GetType(CircleInfo) Then
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect)
                                        ElseIf o.GetType Is GetType(SliderInfo) Then
                                            Dim pointstring As String = ""
                                            For Each p As PointInfo In o.Points
                                                pointstring = pointstring & "|" & p.X & ":" & p.Y
                                            Next
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & "," & o.Type & pointstring & "," & o.RepeatCount & "," & o.MaxPoints)
                                        ElseIf o.GetType Is GetType(SpinnerInfo) Then
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & "," & o.EndTime)
                                        End If
                                    Next
                                Case 10, 11, 12
                                    For Each o In f1.GetValue(Me)
                                        Dim combo As Integer = 5
                                        If o.NewCombo = True Then
                                            combo = 5
                                        Else
                                            combo = 1
                                        End If
                                        If o.GetType Is GetType(CircleInfo) Then
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect)
                                        ElseIf o.GetType Is GetType(SliderInfo) Then
                                            Dim pointstring As String = ""
                                            For Each p As PointInfo In o.Points
                                                pointstring = pointstring & "|" & p.X & ":" & p.Y
                                            Next
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & "," & o.Type & pointstring & "," & o.RepeatCount & "," & o.MaxPoints)
                                        ElseIf o.GetType Is GetType(SpinnerInfo) Then
                                            Save("HitObjects", o.X & "," & o.Y & "," & o.StartTime & "," & combo & "," & o.Effect & "," & o.EndTime)
                                        End If
                                    Next
                            End Select
                        Else
                            If f1.Name <> "Format" Then
                                If Not f1.GetValue(Me) Is Nothing Then
                                    If Not f2.GetValue(Info) Is Nothing Then
                                        Save(GetSection(f1.Name), f1.Name & ":" & f1.GetValue(Me))
                                    Else
                                        Save(GetSection(f2.Name), f2.Name & ":" & f2.GetValue(Info))
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            Next
            FinishSave(filename)
            Threading.Thread.CurrentThread.CurrentCulture = lastculture
        End Sub

        Private Sub Save(ByVal section As String, ByVal contents As String)
            If section = "" Then
                WriteBuffer.Add(contents)
            Else
                If WriteBuffer.Contains("[" & section & "]") = False Then
                    WriteBuffer.Add("")
                    WriteBuffer.Add("[" & section & "]")
                    WriteBuffer.Add(contents)
                    SectionLength.Add(section, 1)
                Else
                    If WriteBuffer.IndexOf("[" & section & "]") + SectionLength(section) = WriteBuffer.Count Then
                        WriteBuffer.Add(contents)
                        SectionLength(section) += 1
                    Else
                        WriteBuffer.Insert(WriteBuffer.IndexOf("[" & section & "]") + SectionLength(section) + 1, contents)
                        SectionLength(section) += 1
                    End If
                End If
            End If
        End Sub
        Private Sub FinishSave(ByVal filename As String)
            Using sW As New IO.StreamWriter(filename)
                For Each l In WriteBuffer
                    sW.WriteLine(l)
                Next
            End Using
        End Sub
        Private Function GetSection(ByVal name As String) As String
            For Each k In BM_Sections.Keys
                If k.Contains(name) Then
                    Return BM_Sections(k)
                End If
            Next
            Return ""
        End Function

        Private Function nthDexOf(ByVal str As String, ByVal splitter As String, ByVal n As Integer) As Integer
            Dim camnt As Integer = -1
            Dim indx As Integer = 0
            Do Until (camnt = n) Or (indx = -1)
                indx = str.IndexOf(splitter, indx + 1)
                If indx = -1 Then
                    Throw New Exception("The specified string does not contain the " & n & "th index of " & Chr(34) & splitter & Chr(34) & ".")
                    Exit Function
                End If
                camnt += 1
            Loop
            Return indx
        End Function
        Private Function SubStr(ByVal str As String, ByVal startindex As Integer, Optional ByVal endindex As Integer = -1) As String
            If endindex = -1 Then
                Return str.Substring(startindex, str.Count - startindex)
            Else
                Return str.Substring(startindex, endindex - startindex)
            End If
        End Function
    End Class
End Namespace
