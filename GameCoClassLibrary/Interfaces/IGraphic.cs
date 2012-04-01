using System.Drawing;

namespace GameCoClassLibrary.Interfaces
{
  internal interface IGraphic
  {
    Region Clip { get; set; }
    void FillRectangle(Brush brush, int x, int y, int width, int height);
    void FillEllipse(Brush brush, float x, float y, float width, float height);
    void DrawString(string s, Font font, Brush brush, PointF point);
    void DrawImage(Image image, int x, int y, int width, int height);
    void DrawImage(Image image, Rectangle rect);
    void DrawLine(Pen pen, Point pt1, Point pt2);
    void DrawLine(Pen pen, float x1, float y1, float x2, float y2);
    void DrawRectangle(Pen pen, Rectangle rect);
    void DrawRectangle(Pen pen, int x, int y, int width, int height);
    void DrawEllipse(Pen pen, float x, float y, float width, float height);
    void Render();
  }
}
