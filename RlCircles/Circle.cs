using System.Numerics;

namespace RlCircles;

public class Circle
{
    private Color _colour;
    public readonly float SpawnOffset = 10;
    public readonly float Speed = 0.85f;

    public Vector2 Position;
    public Vector2 Direction;

    public float Radius;

    public Circle()
    {
        Radius = 30 + Random.Shared.NextSingle() * 30;
        _colour = Color.Green;

        // Pick a random direction to move in.
        float angle = Random.Shared.NextSingle() * MathF.Tau;
        Direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    public bool Overlaps(Circle other)
        => Raylib.CheckCollisionCircles(Position, Radius, other.Position, other.Radius);

    public void Update()
    {
        if (Raylib.CheckCollisionCircles(Raylib.GetMousePosition(), RlCircles.MouseRadius, Position, Radius))
        {
            _colour = Color.Blue;
        }
        else
        {
            _colour = Color.Green;
        }

        if (Position.Y - Radius <= 0)
        {
            Position.Y = Radius;
            Direction.Y *= -1;
        }

        if (Position.Y + Radius >= Raylib.GetScreenHeight())
        {
            Position.Y = Raylib.GetScreenHeight() - Radius;
            Direction.Y *= -1;
        }

        if (Position.X - Radius <= 0)
        {
            Position.X = Radius;
            Direction.X *= -1;
        }

        if (Position.X + Radius >= Raylib.GetScreenWidth())
        {
            Position.X = Raylib.GetRenderWidth() - Radius;
            Direction.X *= -1;
        }

        Position += Direction * Speed;
    }

    public void Draw()
    {
        Raylib.DrawCircleV(Position, Radius, _colour);
    }
}
