using System.Drawing.Drawing2D;

namespace SimplePendulum;

public static class VectorRenderer
{
    public static void DrawForceVector(Graphics gFx, Color color, int startX, int startY, double vecX, double vecY)
    {
        // DO NOT DRAW IF THE VECTOR IS TOO SMALL
        if (Math.Abs(vecX) < 1 && Math.Abs(vecY) < 1) return;

        using(Pen pen = new Pen(color, 2))
        {
            pen.CustomEndCap = new AdjustableArrowCap(4, 4);
            gFx.DrawLine(pen, startX, startY , startX + (int)vecX, startY - (int)vecY);
        }
    }
}