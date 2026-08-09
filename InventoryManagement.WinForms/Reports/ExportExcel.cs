using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace InventoryManagement.Reports
{
    /// <summary>
    /// Chức năng xuất dữ liệu dạng bảng ra file Excel (.xlsx).
    /// Các form nhập/xuất kho dùng lớp này để xuất chi tiết phiếu đang được chọn.
    /// </summary>
    public class ExportExcel
    {
        private static readonly CultureInfo VietnamCulture = new CultureInfo("vi-VN");

        /// <summary>
        /// Nhận DataTable cần xuất và tên tiêu đề file.
        /// Người dùng chọn nơi lưu bằng SaveFileDialog, sau đó ClosedXML ghi file Excel.
        /// </summary>
        public static void ToExcel(DataTable dt, string titleHeader)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu để xuất Excel!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                sfd.FileName = titleHeader + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Chỉ xuất file khi người dùng thật sự bấm Save.
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (XLWorkbook wb = new XLWorkbook())
                        {
                            // Add(dt, sheetName) tự tạo tiêu đề cột từ DataTable.
                            var ws = wb.Worksheets.Add(dt, "Chi Tiet Phieu");
                            ws.Columns().AdjustToContents();
                            wb.SaveAs(sfd.FileName);
                        }

                        MessageBox.Show("Xuất file Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Xuất phiếu xuất kho đầy đủ ra Excel, gồm thông tin phiếu, bảng chi tiết và phần tổng hợp.
        /// </summary>
        public static void ToPhieuXuatExcel(DataTable thongTinPhieu, DataTable chiTietPhieu, string titleHeader)
        {
            if (thongTinPhieu == null || thongTinPhieu.Rows.Count == 0 || chiTietPhieu == null || chiTietPhieu.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu phiếu xuất để xuất Excel!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                sfd.FileName = titleHeader + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        IXLWorksheet ws = wb.Worksheets.Add("Bao Cao Xuat Kho");
                        DataRow info = thongTinPhieu.Rows[0];

                        BuildHeader(ws, info);
                        BuildInfoBlock(ws, info);
                        int totalRow = BuildDetailTable(ws, chiTietPhieu);
                        BuildReportSummary(ws, totalRow, chiTietPhieu);

                        ws.Columns().AdjustToContents();
                        ws.Column(1).Width = Math.Max(ws.Column(1).Width, 8);
                        ws.Column(2).Width = Math.Max(ws.Column(2).Width, 30);
                        ws.Column(4).Width = Math.Max(ws.Column(4).Width, 12);
                        ws.SheetView.FreezeRows(10);
                        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
                        ws.PageSetup.FitToPages(1, 0);
                        ws.PageSetup.Margins.Top = 0.5;
                        ws.PageSetup.Margins.Bottom = 0.5;
                        ws.PageSetup.Margins.Left = 0.4;
                        ws.PageSetup.Margins.Right = 0.4;

                        wb.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Xuất báo cáo Excel thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất báo cáo Excel: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Tạo phần đầu báo cáo Excel gồm tên hệ thống, tiêu đề, ngày in và mã phiếu.
        /// </summary>
        private static void BuildHeader(IXLWorksheet ws, DataRow info)
        {
            ws.Range("A1:F1").Merge().Value = "HỆ THỐNG QUẢN LÝ KHO HÀNG";
            ws.Range("A1:F1").Style.Font.Bold = true;
            ws.Range("A1:F1").Style.Font.FontSize = 13;

            ws.Range("A2:F2").Merge().Value = "Báo cáo phục vụ đối chiếu và tổng hợp dữ liệu xuất kho";
            ws.Range("A2:F2").Style.Font.FontColor = XLColor.FromHtml("#495057");

            ws.Range("A4:F4").Merge().Value = "BÁO CÁO CHI TIẾT PHIẾU XUẤT KHO";
            ws.Range("A4:F4").Style.Font.Bold = true;
            ws.Range("A4:F4").Style.Font.FontSize = 18;
            ws.Range("A4:F4").Style.Font.FontColor = XLColor.FromHtml("#0056B3");
            ws.Range("A4:F4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell("A5").Value = "Ngày in:";
            ws.Cell("B5").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm", VietnamCulture);
            ws.Cell("E5").Value = "Mã phiếu:";
            ws.Cell("F5").Value = GetValue(info, "Mã Phiếu");
            ws.Range("A5:F5").Style.Font.FontColor = XLColor.FromHtml("#495057");
        }

        /// <summary>
        /// Tạo khối thông tin khách hàng, ngày xuất, nhân viên lập và ghi chú của phiếu.
        /// </summary>
        private static void BuildInfoBlock(IXLWorksheet ws, DataRow info)
        {
            ws.Cell("A7").Value = "Khách hàng";
            ws.Cell("B7").Value = GetValue(info, "Khách Hàng");
            ws.Cell("D7").Value = "Ngày xuất";
            ws.Cell("E7").Value = FormatDate(GetValue(info, "Ngày Xuất"));

            ws.Cell("A8").Value = "Số điện thoại";
            ws.Cell("B8").Value = GetValue(info, "Số Điện Thoại");
            ws.Cell("D8").Value = "Nhân viên lập";
            ws.Cell("E8").Value = GetValue(info, "Nhân Viên Lập");

            ws.Cell("A9").Value = "Địa chỉ";
            ws.Cell("B9").Value = GetValue(info, "Địa Chỉ");
            ws.Cell("D9").Value = "Ghi chú";
            ws.Cell("E9").Value = GetValue(info, "Ghi Chú");

            ws.Range("A7:A9").Style.Font.Bold = true;
            ws.Range("D7:D9").Style.Font.Bold = true;
            ws.Range("A7:F9").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A7:F9").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A7:F9").Style.Border.OutsideBorderColor = XLColor.FromHtml("#B2BECD");
            ws.Range("A7:F9").Style.Border.InsideBorderColor = XLColor.FromHtml("#D2DAE4");
            ws.Range("A7:A9").Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F7FB");
            ws.Range("D7:D9").Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F7FB");
            ws.Range("B9:C9").Merge();
            ws.Range("E9:F9").Merge();
        }

        /// <summary>
        /// Tạo bảng chi tiết mặt hàng xuất và trả về dòng tổng tiền để phần sau tham chiếu.
        /// </summary>
        private static int BuildDetailTable(IXLWorksheet ws, DataTable chiTietPhieu)
        {
            int headerRow = 11;
            string[] headers = { "STT", "Tên mặt hàng", "Số lượng", "Đơn vị", "Đơn giá", "Thành tiền" };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(headerRow, i + 1).Value = headers[i];
            }

            IXLRange header = ws.Range(headerRow, 1, headerRow, headers.Length);
            header.Style.Font.Bold = true;
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0056B3");
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int currentRow = headerRow + 1;
            int index = 1;
            foreach (DataRow row in chiTietPhieu.Rows)
            {
                decimal donGia = ParseDecimal(GetValue(row, "Đơn Giá"));
                decimal thanhTien = ParseDecimal(GetValue(row, "Thành Tiền"));

                ws.Cell(currentRow, 1).Value = index;
                ws.Cell(currentRow, 2).Value = GetValue(row, "Tên Mặt Hàng");
                ws.Cell(currentRow, 3).Value = ParseDecimal(GetValue(row, "Số Lượng"));
                ws.Cell(currentRow, 4).Value = GetValue(row, "Đơn Vị");
                ws.Cell(currentRow, 5).Value = donGia;
                ws.Cell(currentRow, 6).Value = thanhTien;

                currentRow++;
                index++;
            }

            int lastDataRow = currentRow - 1;
            IXLRange dataRange = ws.Range(headerRow, 1, lastDataRow, headers.Length);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#B2BECD");
            dataRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D2DAE4");
            dataRange.SetAutoFilter();

            ws.Range(headerRow + 1, 1, lastDataRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(headerRow + 1, 3, lastDataRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(headerRow + 1, 5, lastDataRow, 6).Style.NumberFormat.Format = "#,##0";

            int totalRow = lastDataRow + 1;
            ws.Range(totalRow, 1, totalRow, 5).Merge().Value = "TỔNG TIỀN";
            ws.Cell(totalRow, 6).FormulaA1 = $"SUM(F{headerRow + 1}:F{lastDataRow})";
            ws.Range(totalRow, 1, totalRow, 6).Style.Font.Bold = true;
            ws.Range(totalRow, 1, totalRow, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#EAF7EF");
            ws.Cell(totalRow, 6).Style.NumberFormat.Format = "#,##0";
            ws.Range(totalRow, 1, totalRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            return totalRow;
        }

        /// <summary>
        /// Tạo phần tóm tắt báo cáo gồm số dòng hàng, tổng số lượng và tổng tiền.
        /// </summary>
        private static void BuildReportSummary(IXLWorksheet ws, int totalRow, DataTable chiTietPhieu)
        {
            decimal tongSoLuong = 0;
            foreach (DataRow row in chiTietPhieu.Rows)
            {
                tongSoLuong += ParseDecimal(GetValue(row, "Số Lượng"));
            }

            int summaryRow = totalRow + 2;
            ws.Range(summaryRow, 1, summaryRow, 6).Merge().Value = "TÓM TẮT BÁO CÁO";
            ws.Range(summaryRow, 1, summaryRow, 6).Style.Font.Bold = true;
            ws.Range(summaryRow, 1, summaryRow, 6).Style.Font.FontColor = XLColor.White;
            ws.Range(summaryRow, 1, summaryRow, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#0056B3");
            ws.Range(summaryRow, 1, summaryRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int metricRow = summaryRow + 1;
            ws.Cell(metricRow, 1).Value = "Số dòng hàng";
            ws.Cell(metricRow, 2).Value = chiTietPhieu.Rows.Count;
            ws.Cell(metricRow, 3).Value = "Tổng số lượng";
            ws.Cell(metricRow, 4).Value = tongSoLuong;
            ws.Cell(metricRow, 5).Value = "Tổng tiền";
            ws.Cell(metricRow, 6).FormulaA1 = $"F{totalRow}";
            ws.Range(metricRow, 1, metricRow, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(metricRow, 1, metricRow, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(metricRow, 1, metricRow, 6).Style.Border.OutsideBorderColor = XLColor.FromHtml("#B2BECD");
            ws.Range(metricRow, 1, metricRow, 6).Style.Border.InsideBorderColor = XLColor.FromHtml("#D2DAE4");
            ws.Range(metricRow, 1, metricRow, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F7FB");
            ws.Range(metricRow, 1, metricRow, 6).Style.Font.Bold = true;
            ws.Cell(metricRow, 2).Style.NumberFormat.Format = "#,##0";
            ws.Cell(metricRow, 4).Style.NumberFormat.Format = "#,##0";
            ws.Cell(metricRow, 6).Style.NumberFormat.Format = "#,##0";

            int noteRow = metricRow + 2;
            ws.Range(noteRow, 1, noteRow, 6).Merge().Value =
                "Ghi chú: Báo cáo dùng để kiểm tra chi tiết hàng xuất, đối chiếu số lượng và tổng tiền trên hệ thống.";
            ws.Range(noteRow, 1, noteRow, 6).Style.Alignment.WrapText = true;
            ws.Range(noteRow, 1, noteRow, 6).Style.Font.Italic = true;
            ws.Range(noteRow, 1, noteRow, 6).Style.Font.FontColor = XLColor.FromHtml("#495057");
        }

        /// <summary>
        /// Lấy giá trị chuỗi từ DataRow theo tên cột, trả chuỗi rỗng nếu cột thiếu hoặc giá trị null.
        /// </summary>
        private static string GetValue(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName]) ?? string.Empty
                : string.Empty;
        }

        /// <summary>
        /// Định dạng chuỗi ngày giờ theo chuẩn dd/MM/yyyy HH:mm nếu parse được.
        /// </summary>
        private static string FormatDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date)
                ? date.ToString("dd/MM/yyyy HH:mm", VietnamCulture)
                : value;
        }

        /// <summary>
        /// Chuyển chuỗi số theo văn hóa Việt Nam hoặc invariant culture sang decimal.
        /// </summary>
        private static decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, NumberStyles.Any, VietnamCulture, out decimal viValue))
            {
                return viValue;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal invariantValue))
            {
                return invariantValue;
            }

            return 0;
        }
    }
}
