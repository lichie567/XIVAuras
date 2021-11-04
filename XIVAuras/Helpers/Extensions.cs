using System.Numerics;

namespace XIVAuras.Helpers
{
    public static class Extensions
    {
        public static Vector2 AddX(this Vector2 v, float offset)
        {
            return new Vector2(v.X + offset, v.Y);
        }

        public static Vector2 AddY(this Vector2 v, float offset)
        {
            return new Vector2(v.X, v.Y + offset);
        }
    }
}
