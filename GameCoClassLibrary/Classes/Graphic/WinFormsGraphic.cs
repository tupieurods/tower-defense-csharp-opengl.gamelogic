using System.Drawing;
using GameCoClassLibrary.Interfaces;

namespace GameCoClassLibrary.Classes
{
  internal sealed class WinFormsGraphic : IGraphic
  {
    private BufferedGraphics _graphicalBuffer;

    internal WinFormsGraphic(BufferedGraphics graphicalBuffer)
    {
      _graphicalBuffer = graphicalBuffer;
    }

    public Region Clip
    {
      get { return _graphicalBuffer.Graphics.Clip; }
      set { _graphicalBuffer.Graphics.Clip = value; }
    }

    internal void SetNewGraphBuffer(BufferedGraphics graphicalBuffer)
    {
      _graphicalBuffer = graphicalBuffer;
    }

    public void FillRectangle(Brush brush, int x, int y, int width, int height)
    {
      _graphicalBuffer.Graphics.FillRectangle(brush, x, y, width, height);
    }

    public void FillEllipse(Brush brush, float x, float y, float width, float height)
    {
      _graphicalBuffer.Graphics.FillEllipse(brush, x, y, width, height);
    }

    public void DrawString(string s, Font font, Brush brush, PointF point)
    {
      _graphicalBuffer.Graphics.DrawString(s, font, brush, point);
    }

    public void DrawImage(Image image, int x, int y, int width, int height)
    {
      _graphicalBuffer.Graphics.DrawImage(image, x, y, width, height);
    }

    public void DrawImage(Image image, Rectangle rect)
    {
      _graphicalBuffer.Graphics.DrawImage(image, rect);
    }

    public void DrawLine(Pen pen, Point pt1, Point pt2)
    {
      _graphicalBuffer.Graphics.DrawLine(pen, pt1, pt2);
    }

    public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
    {
      _graphicalBuffer.Graphics.DrawLine(pen, x1, y1, x2 ,y2);
    }

    public void DrawRectangle(Pen pen, Rectangle rect)
    {
      _graphicalBuffer.Graphics.DrawRectangle(pen, rect);
    }

    public void DrawRectangle(Pen pen, int x, int y, int width, int height)
    {
      _graphicalBuffer.Graphics.DrawRectangle(pen, x, y, width, height);
    }

    public void DrawEllipse(Pen pen, float x, float y, float width, float height)
    {
      _graphicalBuffer.Graphics.DrawEllipse(pen, x, y, width, height);
    }

    public void Render()
    {
      _graphicalBuffer.Render();
    }
  }
}
