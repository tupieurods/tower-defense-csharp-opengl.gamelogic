using System.Drawing;
using GameCoClassLibrary.Interfaces;

namespace GameCoClassLibrary.Classes
{
  /// <summary>
  /// WinForms implementation fo IGraphic
  /// </summary>
  internal sealed class WinFormsGraphic : IGraphic
  {
    /// <summary>
    /// For WinForms only
    /// </summary>
    private BufferedGraphics _graphicalBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WinFormsGraphic"/> class.
    /// </summary>
    /// <param name="graphicalBuffer">The graphical buffer.</param>
    internal WinFormsGraphic(BufferedGraphics graphicalBuffer)
    {
      _graphicalBuffer = graphicalBuffer;
    }

    /// <summary>
    /// Gets or sets the clip.
    /// </summary>
    /// <value>
    /// The clip.
    /// </value>
    public Region Clip
    {
      get { return _graphicalBuffer.Graphics.Clip; }
      set { _graphicalBuffer.Graphics.Clip = value; }
    }

    /// <summary>
    /// Sets the new graph buffer.For WinForms only
    /// </summary>
    /// <param name="graphicalBuffer">The graphical buffer.</param>
    internal void SetNewGraphBuffer(BufferedGraphics graphicalBuffer)
    {
      _graphicalBuffer = graphicalBuffer;
    }

    /// <summary>
    /// Fills the rectangle.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void FillRectangle(Brush brush, int x, int y, int width, int height)
    {
      _graphicalBuffer.Graphics.FillRectangle(brush, x, y, width, height);
    }

    /// <summary>
    /// Fills the ellipse.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void FillEllipse(Brush brush, float x, float y, float width, float height)
    {
      _graphicalBuffer.Graphics.FillEllipse(brush, x, y, width, height);
    }

    /// <summary>
    /// Draws the string.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <param name="font">The font.</param>
    /// <param name="brush">The brush.</param>
    /// <param name="point">The point.</param>
    public void DrawString(string s, Font font, Brush brush, PointF point)
    {
      _graphicalBuffer.Graphics.DrawString(s, font, brush, point);
    }

    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void DrawImage(Image image, int x, int y, int width, int height)
    {
      _graphicalBuffer.Graphics.DrawImage(image, x, y, width, height);
    }

    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="rect">The rect.</param>
    public void DrawImage(Image image, Rectangle rect)
    {
      _graphicalBuffer.Graphics.DrawImage(image, rect);
    }

    /// <summary>
    /// Draws the line.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="pt1">The PT1.</param>
    /// <param name="pt2">The PT2.</param>
    public void DrawLine(Pen pen, Point pt1, Point pt2)
    {
      _graphicalBuffer.Graphics.DrawLine(pen, pt1, pt2);
    }

    /// <summary>
    /// Draws the line.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x1">The x1.</param>
    /// <param name="y1">The y1.</param>
    /// <param name="x2">The x2.</param>
    /// <param name="y2">The y2.</param>
    public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
    {
      _graphicalBuffer.Graphics.DrawLine(pen, x1, y1, x2, y2);
    }

    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="rect">The rect.</param>
    public void DrawRectangle(Pen pen, Rectangle rect)
    {
      _graphicalBuffer.Graphics.DrawRectangle(pen, rect);
    }

    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void DrawRectangle(Pen pen, int x, int y, int width, int height)
    {
      _graphicalBuffer.Graphics.DrawRectangle(pen, x, y, width, height);
    }

    /// <summary>
    /// Draws the ellipse.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public void DrawEllipse(Pen pen, float x, float y, float width, float height)
    {
      _graphicalBuffer.Graphics.DrawEllipse(pen, x, y, width, height);
    }

    /// <summary>
    /// Renders this instance.
    /// </summary>
    public void Render()
    {
      _graphicalBuffer.Render();
    }
  }
}
