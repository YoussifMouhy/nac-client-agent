using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Tray;

public class ModernButton : Button
{
    private int _borderRadius = 15;

    public int BorderRadius
    {
        get => _borderRadius;
        set { _borderRadius = value; Invalidate(); }
    }

    public ModernButton()
    {
        this.FlatStyle = FlatStyle.Flat;
        this.FlatAppearance.BorderSize = 0;
        this.BackColor = Color.FromArgb(46, 204, 113); // Emerald Green
        this.ForeColor = Color.White;
        this.Cursor = Cursors.Hand;
        this.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        this.Size = new Size(150, 40);
    }

    // Helper for rounded path
    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        float curveSize = radius * 2F;
        path.StartFigure();
        path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
        path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
        path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
        path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

        // 1. Create the path FIRST
        using (GraphicsPath path = GetRoundedPath(rect, _borderRadius))
        using (SolidBrush brush = new SolidBrush(this.BackColor))
        {
            // 2. NOW we can use 'path' to set the Region (Cuts off the square corners)
            this.Region = new Region(path);

            // 3. Draw rounded background
            pevent.Graphics.FillPath(brush, path);

            // 4. Draw Text manually
            TextRenderer.DrawText(
                pevent.Graphics,
                this.Text,
                this.Font,
                rect,
                this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }
    }
}