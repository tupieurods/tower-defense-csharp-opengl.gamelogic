using System.Drawing;

namespace GameCoClassLibrary.Interfaces
{
  internal interface IGraphic
  {
    /// <summary>
    /// Gets or sets the clip.
    /// </summary>
    /// <value>
    /// The clip.
    /// </value>
    Region Clip { get; set; }
    /// <summary>
    /// Fills the rectangle.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    void FillRectangle(Brush brush, int x, int y, int width, int height);
    /// <summary>
    /// Fills the ellipse.
    /// </summary>
    /// <param name="brush">The brush.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    void FillEllipse(Brush brush, float x, float y, float width, float height);
    /// <summary>
    /// Draws the string.
    /// </summary>
    /// <param name="s">The s.</param>
    /// <param name="font">The font.</param>
    /// <param name="brush">The brush.</param>
    /// <param name="point">The point.</param>
    void DrawString(string s, Font font, Brush brush, PointF point);
    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    void DrawImage(Image image, int x, int y, int width, int height);
    /// <summary>
    /// Draws the image.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="rect">The rect.</param>
    void DrawImage(Image image, Rectangle rect);
    /// <summary>
    /// Draws the line.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="pt1">The PT1.</param>
    /// <param name="pt2">The PT2.</param>
    void DrawLine(Pen pen, Point pt1, Point pt2);
    /// <summary>
    /// Draws the line.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x1">The x1.</param>
    /// <param name="y1">The y1.</param>
    /// <param name="x2">The x2.</param>
    /// <param name="y2">The y2.</param>
    void DrawLine(Pen pen, float x1, float y1, float x2, float y2);
    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="rect">The rect.</param>
    void DrawRectangle(Pen pen, Rectangle rect);
    /// <summary>
    /// Draws the rectangle.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    void DrawRectangle(Pen pen, int x, int y, int width, int height);
    /// <summary>
    /// Draws the ellipse.
    /// </summary>
    /// <param name="pen">The pen.</param>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    void DrawEllipse(Pen pen, float x, float y, float width, float height);
    /// <summary>
    /// Renders this instance.
    /// </summary>
    void Render();
  }
}
