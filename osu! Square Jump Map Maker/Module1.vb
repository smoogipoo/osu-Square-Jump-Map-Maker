Imports osu__Square_Jump_Map_Maker.BMAPI
Imports System.Windows.Forms
Imports System.Drawing
Module Module1
    Dim rnd As New Random
    Dim sq_minlength As Integer = 100
    Dim sq_maxlength As Integer = 300
    Dim sq_timesig As TimeSignature = TimeSignature.MapDefined
    Dim sq_consttimesig As Integer = 4
    Public Enum TimeSignature
        MapDefined = 0
        Constant = 1
        Random = 2
    End Enum

    Sub Main()
1:      sq_minlength = My.Settings.s_minlength
        sq_maxlength = My.Settings.s_maxlength
        sq_timesig = My.Settings.s_timesig
        sq_consttimesig = My.Settings.s_consttimesig
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine("osu! Square Jump Map Maker version " & Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf(".")))
        Console.WriteLine("Minimum length: " & sq_minlength)
        Console.WriteLine("Maximum length: " & sq_maxlength)
        If sq_timesig = TimeSignature.Constant Then
            Console.WriteLine("Timesignature setting: " & sq_timesig.ToString & " @ " & sq_consttimesig & "/4")
        Else
            Console.WriteLine("Timesignature setting: " & sq_timesig.ToString)
        End If
6:      Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("Press [0] to change settings")
        Console.WriteLine("Press [1] to generate beatmap")
        Console.ForegroundColor = ConsoleColor.White
        Dim ki As ConsoleKeyInfo = Console.ReadKey
        If ki.Key = ConsoleKey.D0 Then
2:          Console.Clear()
            Console.WriteLine("Minimum length: " & sq_minlength)
            Console.WriteLine("Maximum length: " & sq_maxlength)
            If sq_timesig = TimeSignature.Constant Then
                Console.WriteLine("Timesignature setting: " & sq_timesig.ToString & " @ " & sq_consttimesig & "/4")
            Else
                Console.WriteLine("Timesignature setting: " & sq_timesig.ToString)
            End If
            Console.WriteLine()
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("Press [1] to change minimum length")
            Console.WriteLine("Press [2] to change maximum length")
            Console.WriteLine("Press [3] to change timesignature setting")
            Console.WriteLine("Press [0] to reset settings to default")
            Console.WriteLine("Press any other key to go back")
            Console.WriteLine()
            Console.ForegroundColor = ConsoleColor.White
            ki = Console.ReadKey
            If ki.Key = ConsoleKey.D1 Then
3:              Console.Clear()
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Current minimum length: " & sq_minlength)
                Console.WriteLine()
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine("Enter the minimum length:")
                Dim s As String = Console.ReadLine
                If IsNumeric(s) Then
                    If s > 0 Then
                        sq_minlength = CInt(s)
                        My.Settings.s_minlength = CInt(s)
                        My.Settings.Save()
                        GoTo 2
                    Else
                        GoTo 3
                    End If
                Else
                    GoTo 3
                End If
            ElseIf ki.Key = ConsoleKey.D2 Then
4:              Console.Clear()
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Current maximum length: " & sq_maxlength)
                Console.WriteLine()
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine("Enter the maximum length:")
                Dim s As String = Console.ReadLine
                If IsNumeric(s) Then
                    If s > 0 Then
                        sq_maxlength = CInt(s)
                        My.Settings.s_maxlength = CInt(s)
                        My.Settings.Save()
                        GoTo 2
                    Else
                        GoTo 4
                    End If
                Else
                    GoTo 4
                End If
            ElseIf ki.Key = ConsoleKey.D3 Then
5:              Console.Clear()
                Console.ForegroundColor = ConsoleColor.Green
                If sq_timesig = TimeSignature.Constant Then
                    Console.WriteLine("Current setting: " & sq_timesig.ToString & " @ " & sq_consttimesig & "/4")
                Else
                    Console.WriteLine("Current setting: " & sq_timesig.ToString)
                End If
                Console.WriteLine()
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine("Possible options:")
                Console.WriteLine("[1] - Map Defined (Use the control point time signature)")
                Console.WriteLine("[2] - Constant (Set a constant time signature)")
                Console.WriteLine("[3] - Random (Repeats in a random time signature of 1-4 beats)")
                Console.WriteLine("Press any other key to go back")
                Console.WriteLine()
                Console.WriteLine("Enter your selection:")
                ki = Console.ReadKey
                If ki.Key = ConsoleKey.D1 Then
                    sq_timesig = TimeSignature.MapDefined
                    My.Settings.s_timesig = TimeSignature.MapDefined
                    My.Settings.Save()
                    GoTo 2
                ElseIf ki.Key = ConsoleKey.D2 Then
                    Console.ForegroundColor = ConsoleColor.White
                    Console.WriteLine()
                    Console.WriteLine("Write the number of beats per measure (i.e. 3 for 3/4 or 4 for 4/4)")
                    Dim s As String = Console.ReadLine
                    If IsNumeric(s) Then
                        If s > 0 Then
                            sq_timesig = TimeSignature.Constant
                            My.Settings.s_timesig = TimeSignature.Constant
                            sq_consttimesig = CInt(s)
                            My.Settings.s_consttimesig = CInt(s)
                            My.Settings.Save()
                            GoTo 2
                        Else
                            GoTo 5
                        End If
                    Else
                        GoTo 5
                    End If
                ElseIf ki.Key = ConsoleKey.D3 Then
                    sq_timesig = TimeSignature.Random
                    My.Settings.s_timesig = TimeSignature.Random
                    My.Settings.Save()
                    GoTo 2
                Else
                    GoTo 2
                End If
            ElseIf ki.Key = ConsoleKey.D0 Then
                My.Settings.s_minlength = 100
                My.Settings.s_maxlength = 300
                My.Settings.s_timesig = TimeSignature.MapDefined
                My.Settings.s_consttimesig = 4
                My.Settings.Save()
                sq_minlength = 100
                sq_maxlength = 300
                sq_timesig = TimeSignature.MapDefined
                sq_consttimesig = 4
                GoTo 2
            Else
                GoTo 1
            End If
        ElseIf ki.Key = ConsoleKey.D1 Then
            Console.Clear()
            Console.WriteLine("Select the timed beatmap file")
            Using ofd As New OpenFileDialog
                If ofd.ShowDialog = DialogResult.OK Then
                    Console.Clear()
                    Dim bm As New Beatmap(ofd.FileName)
                    Dim timingpoints As New List(Of TimingPointInfo)
                    For Each tp As TimingPointInfo In bm.TimingPoints
                        If tp.isInherited = False Then
                            timingpoints.Add(tp)
                        End If
                    Next
                    If timingpoints.Count < 2 Then
                        Console.WriteLine("Generating maps requires a control point at the first beat and the last beat of the map, please add this manually in the map editor.")
                        GoTo 6
                    End If
                    Dim timetomap As Integer = timingpoints(timingpoints.Count - 1).Time - timingpoints(0).Time
                    Console.WriteLine("Processing beatmap...")
                    Dim currenttime As Double = timingpoints(0).Time
                    Dim circlecount As Integer = 0
                    While currenttime + timingpoints(timingpoints.Count - 1).BPMDelay / 2 < timetomap
                        Console.SetCursorPosition(0, 1)
                        Console.Write("Current map time: " & currenttime)
                        Dim hc As New CircleInfo
                        Dim maxcircles As Integer
                        If sq_timesig = TimeSignature.MapDefined Then
                            maxcircles = 2 * GetCurrentTS(currenttime, timingpoints)
                        ElseIf sq_timesig = TimeSignature.Constant Then
                            maxcircles = 2 * maxcircles
                        ElseIf sq_timesig = TimeSignature.Random Then
                            maxcircles = 4 * GetRandomTS()
                        End If

                        Dim pt As Point = GetNextPoint(maxcircles)
                        hc.X = pt.X
                        hc.Y = pt.Y
                        If circlecount = maxcircles Then
                            hc.NewCombo = True
                            circlecount = 0
                        Else
                            hc.NewCombo = False
                        End If
                        hc.StartTime = CInt(currenttime)
                        hc.Effect = CircleInfo.EffectType.None
                        bm.HitObjects.Add(hc)
                        circlecount += 1
                        AdvanceTime(currenttime, timingpoints, 1)
                    End While
                    bm.Save(ofd.FileName)
                    Console.WriteLine()
                    Console.WriteLine("Beatmap generated!")
                    GoTo 6
                Else
                    GoTo 1
                End If
            End Using
        End If
    End Sub
    Dim sq_lastrandomts As Integer
    Function GetRandomTS() As Integer
        If sq_points.Count = sq_lastrandomts * 4 Then
            sq_lastrandomts = sq_r.Next(1, 5)
            Return sq_lastrandomts
        Else
            Return sq_lastrandomts
        End If
    End Function
    Function GetCurrentTS(ByVal currenttime As Double, ByVal timingpoints As List(Of TimingPointInfo))
        If timingpoints.Count = 1 Then
            Return timingpoints(0).TimeSignature
        ElseIf timingpoints(timingpoints.Count - 1).Time <= currenttime Then
            Return timingpoints(timingpoints.Count - 1).TimeSignature
        Else
            For Each tp In timingpoints
                If tp.Time > currenttime Then
                    Return timingpoints(timingpoints.IndexOf(tp) - 1).TimeSignature
                    Exit For
                End If
            Next
        End If
        Return 4
    End Function
    Sub AdvanceTime(ByRef currenttime As Double, ByVal timingpoints As List(Of TimingPointInfo), ByVal beats As Integer)
        For beat = 1 To beats
            Dim advanceamount As Double = 0
            If timingpoints.Count = 1 Then
                advanceamount = timingpoints(0).BPMDelay / 2
            ElseIf timingpoints(timingpoints.Count - 1).Time <= currenttime Then
                advanceamount = timingpoints(timingpoints.Count - 1).BPMDelay / 2
            Else
                For Each tp In timingpoints
                    If tp.Time > currenttime Then
                        advanceamount = timingpoints(timingpoints.IndexOf(tp) - 1).BPMDelay / 2
                        Exit For
                    End If
                Next
            End If
            currenttime += advanceamount
        Next
    End Sub

    Dim sq_startpoint As Point
    Dim sq_length As Integer
    Dim sq_angle As Double
    Dim sq_pointcount As Integer = 0
    Dim sq_points As New List(Of Point)
    Dim sq_r As New Random
    Function GetNextPoint(ByVal maxpoints As Integer) As Point
        If sq_points.Count = 0 Then
            Dim b_left, b_right, b_top, b_bot As Integer
A:
            sq_startpoint = New Point(sq_r.Next(50, 462), sq_r.Next(50, 334))
            sq_length = sq_r.Next(sq_minlength, sq_maxlength)
            sq_angle = sq_r.Next(-180, 180) * Math.PI / 180

            Dim p1 As Point = sq_startpoint
            Dim p2 As New Point(p1.X + sq_length * Math.Cos(sq_angle), p1.Y - sq_length * Math.Sin(sq_angle))
            Dim p3 As New Point(p1.X + sq_length * Math.Cos(sq_angle) - sq_length * Math.Sin(sq_angle), p1.Y - sq_length * Math.Sin(sq_angle) - sq_length * Math.Cos(sq_angle))
            Dim p4 As New Point(p1.X - sq_length * Math.Sin(sq_angle), p1.Y - sq_length * Math.Cos(sq_angle))

            If p1.X > p2.X AndAlso p1.X > p3.X AndAlso p1.X > p4.X Then
                b_right = p1.X
            ElseIf p2.X > p1.X AndAlso p2.X > p3.X AndAlso p2.X > p4.X Then
                b_right = p2.X
            ElseIf p3.X > p1.X AndAlso p3.X > p2.X AndAlso p3.X > p4.X Then
                b_right = p3.X
            ElseIf p4.X > p1.X AndAlso p4.X > p2.X AndAlso p4.X > p3.X Then
                b_right = p4.X
            End If
            If p1.X < p2.X AndAlso p1.X < p3.X AndAlso p1.X < p4.X Then
                b_left = p1.X
            ElseIf p2.X < p1.X AndAlso p2.X < p3.X AndAlso p2.X < p4.X Then
                b_left = p2.X
            ElseIf p3.X < p1.X AndAlso p3.X < p2.X AndAlso p3.X < p4.X Then
                b_left = p3.X
            ElseIf p4.X < p1.X AndAlso p4.X < p2.X AndAlso p4.X < p3.X Then
                b_left = p4.X
            End If
            If p1.Y > p2.Y AndAlso p1.Y > p3.Y AndAlso p1.Y > p4.Y Then
                b_bot = p1.Y
            ElseIf p2.Y > p1.Y AndAlso p2.Y > p3.Y AndAlso p2.Y > p4.Y Then
                b_bot = p2.Y
            ElseIf p3.Y > p1.Y AndAlso p3.Y > p2.Y AndAlso p3.Y > p4.Y Then
                b_bot = p3.Y
            ElseIf p4.Y > p1.Y AndAlso p4.Y > p2.Y AndAlso p4.Y > p3.Y Then
                b_bot = p4.Y
            End If
            If p1.Y < p2.Y AndAlso p1.Y < p3.Y AndAlso p1.Y < p4.Y Then
                b_top = p1.Y
            ElseIf p2.Y < p1.Y AndAlso p2.Y < p3.Y AndAlso p2.Y < p4.Y Then
                b_top = p2.Y
            ElseIf p3.Y < p1.Y AndAlso p3.Y < p2.Y AndAlso p3.Y < p4.Y Then
                b_top = p3.Y
            ElseIf p4.Y < p1.Y AndAlso p4.Y < p2.Y AndAlso p4.Y < p3.Y Then
                b_top = p4.Y
            End If
            If b_left < 50 Then
                sq_startpoint.X = sq_startpoint.X + sq_r.Next(50 - b_left, 50 - b_left + 462)
                GoTo A
            End If
            If b_right > 462 Then
                sq_startpoint.X = sq_startpoint.X - sq_r.Next(b_right - 462, b_right)
                GoTo A
            End If
            If b_top < 50 Then
                sq_startpoint.Y = sq_startpoint.Y + sq_r.Next(50 - b_top, 50 - b_top + 334)
                GoTo A
            End If
            If b_bot > 334 Then
                sq_startpoint.Y = sq_startpoint.Y - sq_r.Next(b_bot - 334, b_bot)
                GoTo A
            End If

            If sq_r.Next(0, 2) = 0 Then
                'Anticlockwise
                While sq_points.Count < maxpoints
                    sq_points.Add(p1)
                    sq_points.Add(p2)
                    sq_points.Add(p3)
                    sq_points.Add(p4)
                End While
            Else
                'Clockwise
                While sq_points.Count < maxpoints
                    sq_points.Add(p1)
                    sq_points.Add(p4)
                    sq_points.Add(p3)
                    sq_points.Add(p2)
                End While
            End If
            For i = 0 To sq_points.Count - maxpoints - 1
                sq_points.RemoveAt(sq_points.Count - 1)
            Next
        End If
        Dim tempp As Point = sq_points(0)
        sq_points.RemoveAt(0)
        Return tempp
    End Function

End Module
