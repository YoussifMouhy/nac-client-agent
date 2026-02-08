using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Tray;

public class ModernInput : Panel
{
    private TextBox textBox;
    private int _borderRadius = 15;

    // Expose TextBox properties so you can access them from LoginForm
    public override string Text
    {
        get => textBox.Text;
        set => textBox.Text = value;
    }

    public bool UseSystemPasswordChar
    {
        get => textBox.UseSystemPasswordChar;
        set => textBox.UseSystemPasswordChar = value;
    }

    public string PlaceholderText
    {
        get => textBox.PlaceholderText;
        set => textBox.PlaceholderText = value;
    }

    public ModernInput()
    {
        this.Size = new Size(250, 45);
        this.BackColor = Color.Transparent; // Important for rounded corners
        this.Padding = new Padding(10);

        textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(40, 40, 40), // Matches panel background
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11),
            Location = new Point(15, 11), // Position inside the panel
            Width = this.Width - 30
        };

        // Ensure TextBox resizes if the panel resizes
        textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        this.Controls.Add(textBox);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        Color bgColor = Color.FromArgb(40, 40, 40);
        Color borderColor = Color.FromArgb(60, 60, 60);

        Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

        using (GraphicsPath path = GetRoundedPath(rect, _borderRadius))
        using (SolidBrush brush = new SolidBrush(bgColor))
        using (Pen pen = new Pen(borderColor, 1))
        {
            e.Graphics.FillPath(brush, path); // Fill rounded area
            e.Graphics.DrawPath(pen, path);   // Draw border
        }
    }

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
}