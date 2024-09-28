using System.Diagnostics;
using System.Numerics;

namespace RlCircles;

public static class RlCircles
{
    public static readonly float MouseRadius = 6;
    private readonly static List<Circle> _circles = [];

    private static bool _stopSpawning = false;

    private static int _loadedCircles;
    private static int _circleCount = 15;

    private static void ParseArguments(string[] args)
    {
        if (args.Length <= 0) return;

        bool countFlag = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (countFlag)
            {
                countFlag = false;
                continue;
            }

            switch (args[i])
            {
                case "--count":
                case "-c":
                    if (args.Length - 1 < i + 1)
                    {
                        Console.Error.WriteLine("WARN: PARSER: No integer provided! Using default value.");
                        return;
                    }

                    string countString = args[i + 1].Trim();

                    bool success = int.TryParse(countString, out int result);
                    if (!success)
                    {
                        Console.Error.WriteLine("WARN: PARSER: Bad integer value provided! Using default value.");
                        return;
                    }

                    _circleCount = result;
                    countFlag = true;
                break;

                default:
                    Console.Error.WriteLine($"WARN: PARSER: Unrecognised flag \"{args[i]}\".");
                break;
            }
        }
    }

    public static float DegreesToRadians(float degrees)
        => degrees * (MathF.PI / 180);

    public static Vector2 CalculateCollisionPoint(Vector2 firstPosition, Vector2 secondPosition)
    {
        Vector2 point = firstPosition - secondPosition;
        return Vector2.Normalize(point);
    }

    public static void Main(string[] args)
    {
        Raylib.InitWindow(1024, 576, "RlCircles");
        Raylib.SetTargetFPS(60);

        // Keep the cursor hidden
        // Because it looks good :troll:
        Raylib.HideCursor();

        // Space to differenciate raylib debug logs
        // V.S. my debug logs.
        Console.WriteLine();

        ParseArguments(args);
        _loadedCircles = _circleCount;

        Stopwatch watch = new Stopwatch();
        watch.Start();

        for (int i = 0; i < _circleCount; i++)
        {
            Circle circle = new Circle();
            bool overlaps;

            int iter = 0;
            do
            {
                if (iter >= 15)
                {
                    Console.Error.WriteLine($"WARN: CIRCLES: Unable to load circles. No space left on the screen. {i} Circles spawned.");
                    _stopSpawning = true;
                    _loadedCircles = i;
                    break;
                }

                overlaps = false;
                circle.Position = new Vector2(
                    Random.Shared.Next((int)(circle.Radius + circle.SpawnOffset), (int)(Raylib.GetScreenWidth() - circle.SpawnOffset - circle.Radius)),
                    Random.Shared.Next((int)(circle.Radius + circle.SpawnOffset), (int)(Raylib.GetScreenHeight() - circle.SpawnOffset - circle.Radius))
                );

                foreach (Circle existingCircle in _circles)
                {
                    if (circle.Overlaps(existingCircle))
                    {
                        overlaps = true; 
                        break;
                    }
                }
                
                iter++;
            } while (overlaps);

            _circles.Add(circle);

            if (_stopSpawning) break;
        }

        watch.Stop();
        Console.WriteLine($"INFO: CIRCLES: Loaded {_loadedCircles} circles in {watch.Elapsed.TotalMicroseconds}μs.");

        Stopwatch updateWatch = new Stopwatch();
        Stopwatch drawWatch = new Stopwatch();
        while (!Raylib.WindowShouldClose())
        {
            updateWatch.Start();

            foreach (Circle circle in _circles)
            {
                circle.Update();
            }

            // Check collisions.
            for (int i = 0; i < _circles.Count; i++)
            {
                Circle circle = _circles[i];
                for (int j = i + 1; j < _circles.Count; j++)
                { 
                    Circle otherCircle = _circles[j];

                    if (circle.Overlaps(otherCircle))
                    {
                        circle.Direction = CalculateCollisionPoint(circle.Position, otherCircle.Position);
                        otherCircle.Direction = CalculateCollisionPoint(otherCircle.Position, circle.Position);
                    }
                }
            }

            updateWatch.Stop();

            Raylib.BeginDrawing();
            {
                drawWatch.Start();

                Raylib.ClearBackground(Color.RayWhite);

                foreach (Circle circle in _circles)
                {
                    circle.Draw();
                }

                foreach (Circle circle in _circles)
                { 
                    if (Raylib.CheckCollisionCircles(Raylib.GetMousePosition(), MouseRadius, circle.Position, circle.Radius))
                    {
                        Raylib.DrawText($"Radius: {circle.Radius:F2}\nPosition: {circle.Position:F2}\nDirection: {circle.Direction:F2}", 5, 5, 20, Color.Red); 
                    }
                }

                Raylib.DrawCircleV(Raylib.GetMousePosition(), MouseRadius, Color.Orange);

                Raylib.DrawText($"Frame Update: {updateWatch.Elapsed.TotalMicroseconds} micros", 5, Raylib.GetScreenHeight() - 20, 20, Color.Red);
                Raylib.DrawText($"Frame Draw: {drawWatch.Elapsed.TotalMicroseconds} micros", 5, Raylib.GetScreenHeight() - 40, 20, Color.Red);

                drawWatch.Stop();
            }   
            Raylib.EndDrawing();

            // Reset both watches
            updateWatch.Reset();
            drawWatch.Reset();
        }

        Raylib.CloseWindow();
    }
}