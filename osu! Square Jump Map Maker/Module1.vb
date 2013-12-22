Imports osu__Square_Jump_Map_Maker.BMAPI
Imports System.Windows.Forms
Imports System.Drawing

Module Module1
    Dim rnd As New Random

    Sub Main()
        Console.WriteLine("Select the timed beatmap file")
        Using ofd As New OpenFileDialog
            If ofd.ShowDialog = DialogResult.OK Then
                Dim bm As New Beatmap(ofd.FileName)
                bm.StackLeniency = 0.2
                Dim timingpoints As New List(Of TimingPointInfo)
                For Each tp As TimingPointInfo In bm.TimingPoints
                    If tp.isInherited = False Then
                        timingpoints.Add(tp)
                    End If
                Next
                If timingpoints.Count = 0 Then
                    Console.WriteLine("No timing points exist in the beatmap. At least one timing point is required for mapping to take place.")
                    Console.ReadKey()
                    Exit Sub
                End If
                If timingpoints.Count < 2 Then
                    Console.WriteLine("Generating maps requires a control point at the first beat and the last beat of the map, please add this manually in the map editor.")
                    Console.ReadKey()
                    Exit Sub
                End If
                Dim timetomap As Integer = timingpoints(timingpoints.Count - 1).Time - timingpoints(0).Time
                Console.WriteLine("Processing beatmap...")
                Dim currenttime As Double = timingpoints(0).Time
                Dim circlecount As Integer = 0
                While currenttime + timingpoints(timingpoints.Count - 1).BPMDelay / 2 < timetomap
                    Console.WriteLine("Current map time: " & currenttime)
                    Dim hc As New CircleInfo
                    Dim pt As Point = GetNextPoint()
                    hc.X = pt.X
                    hc.Y = pt.Y
                    If circlecount = 8 Then
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
            End If
            Console.WriteLine("Beatmap generated! Have fun!")
            Console.ReadKey()
        End Using
    End Sub
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
    Function GetNextPoint() As Point
        If sq_points.Count = 0 Then
            Dim repeatcount As Integer = sq_r.Next(1, 4)
            Dim b_left, b_right, b_top, b_bot As Integer
A:
            sq_startpoint = New Point(sq_r.Next(50, 462), sq_r.Next(50, 334))
            sq_length = sq_r.Next(100, 300)
            sq_angle = sq_r.Next(0, 360) * Math.PI / 180

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

            For i = 1 To repeatcount
                sq_points.Add(p1)
                sq_points.Add(p2)
                sq_points.Add(p3)
                sq_points.Add(p4)
            Next
        End If
        Dim tempp As Point = sq_points(0)
        sq_points.RemoveAt(0)
        Return tempp
    End Function

End Module
