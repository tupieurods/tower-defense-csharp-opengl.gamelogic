using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;

namespace GameCoClassLibrary.Interfaces
{
  internal interface IGraphic
  {
    Region Clip { get; set; }
    void FillRectangle(Brush brush,int x, int y, int width, int height);
    void DrawString(string s, Font font, Brush brush, PointF point);
    void DrawImage(Image image, int x, int y, int width, int height);
    void DrawLine(Pen pen, Point pt1, Point pt2);
    void DrawRectangle(Pen pen, Rectangle rect);
    void DrawEllipse(Pen pen, float x, float y, float width, float height);
    void Renred();
  }
}
