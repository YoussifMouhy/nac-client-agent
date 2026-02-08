using System.Drawing;
using System.Windows.Forms;

namespace Tray;

public static class TrayMenuBuilder
{
    public static ContextMenuStrip Build(
        ToolStripMenuItem statusLabel,
        ToolStripMenuItem loginToggleItem,
        EventHandler exitHandler)
    {
        var menu = new ContextMenuStrip
        {
            Renderer = new ToolStripProfessionalRenderer(new ModernColorTable()),
            ShowImageMargin = false,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.White
        };

        menu.Items.Add(statusLabel);
        menu.Items.Add(new ToolStripSeparator());

        menu.Items.Add(loginToggleItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit Agent")
        {
            Font = new Font("Segoe UI", 9)
        };
        exitItem.Click += exitHandler;

        menu.Items.Add(exitItem);

        return menu;
    }
}