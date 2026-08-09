using System.Drawing.Drawing2D;
using InventoryManagement.Models;

namespace InventoryManagement.Controls
{
    /// <summary>
    /// Biểu đồ nhẹ dành riêng cho trang thống kê, không phụ thuộc thư viện ngoài.
    /// </summary>
    public sealed class DashboardChart : Control
    {
        private static readonly Color[] Palette =
        {
            Color.FromArgb(37, 99, 235),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11),
            Color.FromArgb(139, 92, 246),
            Color.FromArgb(239, 68, 68),
            Color.FromArgb(14, 165, 233),
            Color.FromArgb(236, 72, 153),
            Color.FromArgb(100, 116, 139)
        };

        private readonly List<ChartItem> _items = new List<ChartItem>();
        private DashboardChartKind _kind = DashboardChartKind.GroupedColumns;

        public DashboardChart()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(51, 65, 85);
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
        }

        /// <summary>
        /// Hiển thị số lượng nhập và xuất theo tháng.
        /// </summary>
        public void SetMonthlyData(IEnumerable<DashboardMonthlyPoint> data)
        {
            _kind = DashboardChartKind.GroupedColumns;
            _items.Clear();
            _items.AddRange((data ?? Enumerable.Empty<DashboardMonthlyPoint>())
                .Select(item => new ChartItem(
                    $"T{item.Thang}/{item.Nam % 100:D2}",
                    item.SoLuongNhap,
                    item.SoLuongXuat)));
            Invalidate();
        }

        /// <summary>
        /// Hiển thị tỷ lệ số mặt hàng theo loại bằng biểu đồ tròn.
        /// </summary>
        public void SetCategoryData(IEnumerable<DashboardCategoryPoint> data)
        {
            _kind = DashboardChartKind.Donut;
            _items.Clear();

            List<DashboardCategoryPoint> source = (data ?? Enumerable.Empty<DashboardCategoryPoint>())
                .Where(item => item.SoLuongHangHoa > 0)
                .OrderByDescending(item => item.SoLuongHangHoa)
                .ToList();

            foreach (DashboardCategoryPoint item in source.Take(7))
            {
                _items.Add(new ChartItem(item.TenLoaiHang, item.SoLuongHangHoa, 0));
            }

            if (source.Count > 7)
            {
                _items.Add(new ChartItem("Khác", source.Skip(7).Sum(item => item.SoLuongHangHoa), 0));
            }

            Invalidate();
        }

        /// <summary>
        /// Hiển thị các mặt hàng tồn nhiều nhất bằng thanh ngang.
        /// </summary>
        public void SetStockData(IEnumerable<DashboardStockPoint> data)
        {
            _kind = DashboardChartKind.HorizontalBars;
            _items.Clear();
            _items.AddRange((data ?? Enumerable.Empty<DashboardStockPoint>())
                .Select(item => new ChartItem(item.TenHangHoa, item.SoLuongTon, 0)));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (_items.Count == 0)
            {
                DrawEmptyState(e.Graphics);
                return;
            }

            switch (_kind)
            {
                case DashboardChartKind.Donut:
                    DrawDonut(e.Graphics);
                    break;
                case DashboardChartKind.HorizontalBars:
                    DrawHorizontalBars(e.Graphics);
                    break;
                default:
                    DrawGroupedColumns(e.Graphics);
                    break;
            }
        }

        private void DrawEmptyState(Graphics graphics)
        {
            TextRenderer.DrawText(
                graphics,
                "Chưa có dữ liệu",
                Font,
                ClientRectangle,
                Color.FromArgb(148, 163, 184),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void DrawGroupedColumns(Graphics graphics)
        {
            Rectangle bounds = ClientRectangle;
            Rectangle plot = new Rectangle(48, 32, Math.Max(10, bounds.Width - 64), Math.Max(10, bounds.Height - 78));
            double maximum = Math.Max(1, _items.Max(item => Math.Max(item.Value, item.SecondaryValue)));

            using Pen gridPen = new Pen(Color.FromArgb(226, 232, 240), 1F);
            using Brush mutedBrush = new SolidBrush(Color.FromArgb(100, 116, 139));
            using Brush importBrush = new SolidBrush(Palette[0]);
            using Brush exportBrush = new SolidBrush(Palette[1]);

            DrawLegend(graphics, importBrush, "Nhập kho", 8);
            DrawLegend(graphics, exportBrush, "Xuất kho", 92);

            for (int line = 0; line <= 4; line++)
            {
                float y = plot.Bottom - (plot.Height * line / 4F);
                graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);

                string value = FormatCompactNumber(maximum * line / 4D);
                SizeF textSize = graphics.MeasureString(value, Font);
                graphics.DrawString(value, Font, mutedBrush, plot.Left - textSize.Width - 6, y - textSize.Height / 2);
            }

            float groupWidth = plot.Width / (float)_items.Count;
            float barWidth = Math.Max(3F, Math.Min(17F, (groupWidth - 8F) / 2F));
            bool skipAlternateLabels = groupWidth < 44F;

            for (int index = 0; index < _items.Count; index++)
            {
                ChartItem item = _items[index];
                float centerX = plot.Left + groupWidth * index + groupWidth / 2F;
                float importHeight = (float)(plot.Height * item.Value / maximum);
                float exportHeight = (float)(plot.Height * item.SecondaryValue / maximum);

                RectangleF importBar = new RectangleF(
                    centerX - barWidth - 1F,
                    plot.Bottom - importHeight,
                    barWidth,
                    importHeight);
                RectangleF exportBar = new RectangleF(
                    centerX + 1F,
                    plot.Bottom - exportHeight,
                    barWidth,
                    exportHeight);

                graphics.FillRectangle(importBrush, importBar);
                graphics.FillRectangle(exportBrush, exportBar);

                if (!skipAlternateLabels || index % 2 == 0 || index == _items.Count - 1)
                {
                    Rectangle labelBounds = new Rectangle(
                        (int)(centerX - groupWidth / 2F),
                        plot.Bottom + 7,
                        Math.Max(1, (int)groupWidth),
                        22);
                    TextRenderer.DrawText(
                        graphics,
                        item.Label,
                        Font,
                        labelBounds,
                        Color.FromArgb(100, 116, 139),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
                }
            }
        }

        private void DrawDonut(Graphics graphics)
        {
            Rectangle bounds = ClientRectangle;
            int diameter = Math.Max(90, Math.Min(Math.Min(bounds.Height - 24, 190), (int)(bounds.Width * 0.44F)));
            Rectangle pieBounds = new Rectangle(14, Math.Max(8, (bounds.Height - diameter) / 2), diameter, diameter);
            double total = Math.Max(1, _items.Sum(item => item.Value));
            float startAngle = -90F;

            for (int index = 0; index < _items.Count; index++)
            {
                float sweepAngle = (float)(_items[index].Value / total * 360D);
                using Brush brush = new SolidBrush(Palette[index % Palette.Length]);
                graphics.FillPie(brush, pieBounds, startAngle, sweepAngle);
                startAngle += sweepAngle;
            }

            int holeSize = (int)(diameter * 0.56F);
            Rectangle holeBounds = new Rectangle(
                pieBounds.Left + (diameter - holeSize) / 2,
                pieBounds.Top + (diameter - holeSize) / 2,
                holeSize,
                holeSize);
            graphics.FillEllipse(Brushes.White, holeBounds);

            using Font totalFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using Brush totalBrush = new SolidBrush(Color.FromArgb(15, 23, 42));
            string totalText = FormatCompactNumber(total);
            SizeF totalSize = graphics.MeasureString(totalText, totalFont);
            graphics.DrawString(
                totalText,
                totalFont,
                totalBrush,
                pieBounds.Left + (diameter - totalSize.Width) / 2F,
                pieBounds.Top + (diameter - totalSize.Height) / 2F - 5);

            using Font captionFont = new Font("Segoe UI", 8F, FontStyle.Regular);
            string caption = "mặt hàng";
            SizeF captionSize = graphics.MeasureString(caption, captionFont);
            graphics.DrawString(
                caption,
                captionFont,
                Brushes.SlateGray,
                pieBounds.Left + (diameter - captionSize.Width) / 2F,
                pieBounds.Top + diameter / 2F + 15);

            int legendLeft = pieBounds.Right + 24;
            int legendWidth = Math.Max(40, bounds.Width - legendLeft - 10);
            int rowHeight = Math.Max(22, Math.Min(30, (bounds.Height - 12) / Math.Max(1, _items.Count)));
            int legendTop = Math.Max(6, (bounds.Height - rowHeight * _items.Count) / 2);

            for (int index = 0; index < _items.Count; index++)
            {
                ChartItem item = _items[index];
                int y = legendTop + index * rowHeight;
                using Brush colorBrush = new SolidBrush(Palette[index % Palette.Length]);
                graphics.FillEllipse(colorBrush, legendLeft, y + 6, 10, 10);

                int percentage = (int)Math.Round(item.Value / total * 100D);
                Rectangle textBounds = new Rectangle(legendLeft + 18, y, legendWidth - 46, rowHeight);
                TextRenderer.DrawText(
                    graphics,
                    item.Label,
                    Font,
                    textBounds,
                    ForeColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                Rectangle percentBounds = new Rectangle(bounds.Right - 46, y, 38, rowHeight);
                TextRenderer.DrawText(
                    graphics,
                    $"{percentage}%",
                    Font,
                    percentBounds,
                    Color.FromArgb(100, 116, 139),
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawHorizontalBars(Graphics graphics)
        {
            Rectangle bounds = ClientRectangle;
            int labelWidth = Math.Min(155, Math.Max(95, (int)(bounds.Width * 0.34F)));
            int barLeft = labelWidth + 12;
            int barRight = Math.Max(barLeft + 20, bounds.Width - 46);
            int rowHeight = Math.Max(24, Math.Min(38, (bounds.Height - 8) / Math.Max(1, _items.Count)));
            int top = Math.Max(4, (bounds.Height - rowHeight * _items.Count) / 2);
            double maximum = Math.Max(1, _items.Max(item => item.Value));

            Color labelColor = Color.FromArgb(51, 65, 85);
            using Brush trackBrush = new SolidBrush(Color.FromArgb(239, 246, 255));
            using Brush barBrush = new SolidBrush(Palette[0]);

            for (int index = 0; index < _items.Count; index++)
            {
                ChartItem item = _items[index];
                int rowTop = top + index * rowHeight;
                int barTop = rowTop + Math.Max(5, (rowHeight - 13) / 2);
                int availableWidth = barRight - barLeft;
                int valueWidth = Math.Max(2, (int)Math.Round(availableWidth * item.Value / maximum));

                Rectangle labelBounds = new Rectangle(4, rowTop, labelWidth, rowHeight);
                TextRenderer.DrawText(
                    graphics,
                    item.Label,
                    Font,
                    labelBounds,
                    labelColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                graphics.FillRectangle(trackBrush, barLeft, barTop, availableWidth, 13);
                graphics.FillRectangle(barBrush, barLeft, barTop, valueWidth, 13);

                Rectangle valueBounds = new Rectangle(barRight + 5, rowTop, 38, rowHeight);
                TextRenderer.DrawText(
                    graphics,
                    FormatCompactNumber(item.Value),
                    Font,
                    valueBounds,
                    Color.FromArgb(51, 65, 85),
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }

        private void DrawLegend(Graphics graphics, Brush brush, string text, int left)
        {
            graphics.FillRectangle(brush, left, 9, 10, 10);
            graphics.DrawString(text, Font, Brushes.SlateGray, left + 15, 5);
        }

        private static string FormatCompactNumber(double value)
        {
            if (value >= 1_000_000)
            {
                return $"{value / 1_000_000D:0.#}tr";
            }

            if (value >= 1_000)
            {
                return $"{value / 1_000D:0.#}k";
            }

            return Math.Round(value).ToString("N0");
        }

        private sealed record ChartItem(string Label, double Value, double SecondaryValue);

        private enum DashboardChartKind
        {
            GroupedColumns,
            Donut,
            HorizontalBars
        }
    }
}
