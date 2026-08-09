using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace InventoryManagement.Reports
{
    /// <summary>
    /// Chức năng xuất dữ liệu dạng bảng ra file PDF.
    /// Lớp này dùng iTextSharp để tạo tài liệu A4, tiêu đề và bảng chi tiết phiếu.
    /// </summary>
    public class ExportPdf
    {
        private static readonly CultureInfo VietnamCulture = new CultureInfo("vi-VN");

        /// <summary>
        /// Nhận DataTable cần in và tiêu đề báo cáo.
        /// Dữ liệu được ghi ra file PDF tại vị trí người dùng chọn.
        /// </summary>
        public static void ToPdf(DataTable dt, string titleHeader)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất PDF!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Document (*.pdf)|*.pdf";
                sfd.FileName = titleHeader.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Chỉ tạo file khi người dùng xác nhận vị trí lưu.
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 15f, 15f, 20f, 20f);
                        iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));

                        document.Open();

                        // Dùng Arial để PDF hiển thị được tiếng Việt nếu dữ liệu có dấu.
                        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", "Arial.ttf");
                        iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);

                        iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.Blue);
                        iTextSharp.text.Font fontHeader = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.White);
                        iTextSharp.text.Font fontBody = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.Black);

                        // Tiêu đề báo cáo đặt ở giữa trang.
                        iTextSharp.text.Paragraph prgTitle = new iTextSharp.text.Paragraph(titleHeader.ToUpper(), fontTitle);
                        prgTitle.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                        prgTitle.SpacingAfter = 20;
                        document.Add(prgTitle);

                        // Tạo bảng có số cột bằng DataTable.
                        iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(dt.Columns.Count);
                        table.WidthPercentage = 100;

                        foreach (DataColumn column in dt.Columns)
                        {
                            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(column.ColumnName, fontHeader));
                            cell.BackgroundColor = iTextSharp.text.BaseColor.DarkGray;
                            cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                            cell.Padding = 6;
                            table.AddCell(cell);
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            foreach (object cellValue in row.ItemArray)
                            {
                                string cellText = cellValue?.ToString() ?? string.Empty;
                                iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(cellText, fontBody));
                                cell.Padding = 5;
                                cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                                table.AddCell(cell);
                            }
                        }

                        document.Add(table);
                        document.Close();

                        MessageBox.Show("Xuất file PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xuất file PDF: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Xuất phiếu xuất kho đầy đủ ra PDF, gồm thông tin phiếu, bảng chi tiết và chữ ký.
        /// </summary>
        public static void ToPhieuXuatPdf(DataTable thongTinPhieu, DataTable chiTietPhieu, string titleHeader)
        {
            if (thongTinPhieu == null || thongTinPhieu.Rows.Count == 0 || chiTietPhieu == null || chiTietPhieu.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu phiếu xuất để xuất PDF!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PDF Document (*.pdf)|*.pdf";
                sfd.FileName = titleHeader.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 32f, 32f, 28f, 28f);
                    iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));
                    document.Open();

                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts", "Arial.ttf");
                    iTextSharp.text.pdf.BaseFont bf = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);

                    iTextSharp.text.Font fontCompany = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(33, 37, 41));
                    iTextSharp.text.Font fontSmall = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.NORMAL, new iTextSharp.text.BaseColor(73, 80, 87));
                    iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(bf, 18, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(0, 86, 179));
                    iTextSharp.text.Font fontSection = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(33, 37, 41));
                    iTextSharp.text.Font fontHeader = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.White);
                    iTextSharp.text.Font fontBody = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.Black);
                    iTextSharp.text.Font fontBold = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.BOLD, iTextSharp.text.BaseColor.Black);
                    iTextSharp.text.Font fontTotal = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(0, 110, 65));

                    DataRow info = thongTinPhieu.Rows[0];

                    AddHeader(document, fontCompany, fontSmall);
                    AddTitle(document, "PHIẾU XUẤT KHO", fontTitle);
                    AddInfoBlock(document, info, fontSection, fontBody);
                    AddDetailTable(document, chiTietPhieu, fontHeader, fontBody, fontBold);
                    AddSummary(document, info, fontBody, fontTotal);
                    AddSignatureBlock(document, fontBody, fontBold);

                    document.Close();
                    MessageBox.Show("Xuất phiếu xuất PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất phiếu xuất PDF: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Thêm phần đầu PDF gồm tên hệ thống, ghi chú nội bộ và ngày in.
        /// </summary>
        private static void AddHeader(iTextSharp.text.Document document, iTextSharp.text.Font fontCompany, iTextSharp.text.Font fontSmall)
        {
            iTextSharp.text.pdf.PdfPTable header = new iTextSharp.text.pdf.PdfPTable(2);
            header.WidthPercentage = 100;
            header.SetWidths(new float[] { 65f, 35f });

            iTextSharp.text.pdf.PdfPCell companyCell = NoBorderCell();
            companyCell.AddElement(new iTextSharp.text.Paragraph("HỆ THỐNG QUẢN LÝ KHO HÀNG", fontCompany));
            companyCell.AddElement(new iTextSharp.text.Paragraph("Quản lý nhập xuất tồn - Phiếu in từ phần mềm", fontSmall));
            companyCell.AddElement(new iTextSharp.text.Paragraph("Ngày in: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm", VietnamCulture), fontSmall));
            header.AddCell(companyCell);

            iTextSharp.text.pdf.PdfPCell noteCell = NoBorderCell();
            noteCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
            noteCell.AddElement(new iTextSharp.text.Paragraph("Bản lưu nội bộ", fontSmall) { Alignment = iTextSharp.text.Element.ALIGN_RIGHT });
            noteCell.AddElement(new iTextSharp.text.Paragraph("Kiểm tra hàng trước khi ký nhận", fontSmall) { Alignment = iTextSharp.text.Element.ALIGN_RIGHT });
            header.AddCell(noteCell);

            document.Add(header);
            AddDivider(document);
        }

        /// <summary>
        /// Thêm tiêu đề chính của tài liệu PDF.
        /// </summary>
        private static void AddTitle(iTextSharp.text.Document document, string title, iTextSharp.text.Font fontTitle)
        {
            iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph(title, fontTitle);
            paragraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            paragraph.SpacingBefore = 10;
            paragraph.SpacingAfter = 12;
            document.Add(paragraph);
        }

        /// <summary>
        /// Thêm khối thông tin phiếu xuất như mã phiếu, ngày xuất, khách hàng và nhân viên.
        /// </summary>
        private static void AddInfoBlock(iTextSharp.text.Document document, DataRow info, iTextSharp.text.Font fontSection, iTextSharp.text.Font fontBody)
        {
            iTextSharp.text.Paragraph sectionTitle = new iTextSharp.text.Paragraph("Thông tin phiếu", fontSection);
            sectionTitle.SpacingAfter = 6;
            document.Add(sectionTitle);

            iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(4);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 18f, 32f, 18f, 32f });

            AddInfoCell(table, "Mã phiếu:", fontSection, true);
            AddInfoCell(table, GetValue(info, "Mã Phiếu"), fontBody, false);
            AddInfoCell(table, "Ngày xuất:", fontSection, true);
            AddInfoCell(table, FormatDate(GetValue(info, "Ngày Xuất")), fontBody, false);

            AddInfoCell(table, "Khách hàng:", fontSection, true);
            AddInfoCell(table, GetValue(info, "Khách Hàng"), fontBody, false);
            AddInfoCell(table, "Nhân viên:", fontSection, true);
            AddInfoCell(table, GetValue(info, "Nhân Viên Lập"), fontBody, false);

            AddInfoCell(table, "Số điện thoại:", fontSection, true);
            AddInfoCell(table, GetValue(info, "Số Điện Thoại"), fontBody, false);
            AddInfoCell(table, "Địa chỉ:", fontSection, true);
            AddInfoCell(table, GetValue(info, "Địa Chỉ"), fontBody, false);

            AddInfoCell(table, "Ghi chú:", fontSection, true);
            iTextSharp.text.pdf.PdfPCell note = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(GetValue(info, "Ghi Chú"), fontBody));
            note.Colspan = 3;
            note.Padding = 6;
            note.BorderColor = new iTextSharp.text.BaseColor(210, 218, 228);
            table.AddCell(note);

            table.SpacingAfter = 12;
            document.Add(table);
        }

        /// <summary>
        /// Thêm bảng chi tiết hàng xuất vào PDF.
        /// </summary>
        private static void AddDetailTable(iTextSharp.text.Document document, DataTable chiTietPhieu, iTextSharp.text.Font fontHeader, iTextSharp.text.Font fontBody, iTextSharp.text.Font fontBold)
        {
            iTextSharp.text.Paragraph sectionTitle = new iTextSharp.text.Paragraph("Chi tiết hàng xuất", fontBold);
            sectionTitle.SpacingAfter = 6;
            document.Add(sectionTitle);

            iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(6);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 7f, 35f, 12f, 10f, 18f, 18f });

            string[] headers = { "STT", "Tên mặt hàng", "Số lượng", "Đơn vị", "Đơn giá", "Thành tiền" };
            foreach (string header in headers)
            {
                iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(header, fontHeader));
                cell.BackgroundColor = new iTextSharp.text.BaseColor(0, 86, 179);
                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                cell.Padding = 7;
                table.AddCell(cell);
            }

            int index = 1;
            foreach (DataRow row in chiTietPhieu.Rows)
            {
                AddBodyCell(table, index.ToString(), fontBody, iTextSharp.text.Element.ALIGN_CENTER);
                AddBodyCell(table, GetValue(row, "Tên Mặt Hàng"), fontBody, iTextSharp.text.Element.ALIGN_LEFT);
                AddBodyCell(table, GetValue(row, "Số Lượng"), fontBody, iTextSharp.text.Element.ALIGN_CENTER);
                AddBodyCell(table, GetValue(row, "Đơn Vị"), fontBody, iTextSharp.text.Element.ALIGN_CENTER);
                AddBodyCell(table, FormatMoney(GetValue(row, "Đơn Giá")), fontBody, iTextSharp.text.Element.ALIGN_RIGHT);
                AddBodyCell(table, FormatMoney(GetValue(row, "Thành Tiền")), fontBody, iTextSharp.text.Element.ALIGN_RIGHT);
                index++;
            }

            table.SpacingAfter = 10;
            document.Add(table);
        }

        /// <summary>
        /// Thêm ghi chú giao nhận và tổng tiền của phiếu xuất.
        /// </summary>
        private static void AddSummary(iTextSharp.text.Document document, DataRow info, iTextSharp.text.Font fontBody, iTextSharp.text.Font fontTotal)
        {
            decimal total = ParseDecimal(GetValue(info, "Tổng Tiền"));

            iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 65f, 35f });

            iTextSharp.text.pdf.PdfPCell textCell = NoBorderCell();
            textCell.AddElement(new iTextSharp.text.Paragraph("Ghi chú giao nhận: Phiếu được lập từ hệ thống quản lý kho. Người nhận kiểm tra số lượng, chủng loại và tình trạng hàng trước khi ký.", fontBody));
            table.AddCell(textCell);

            iTextSharp.text.pdf.PdfPCell totalCell = NoBorderCell();
            iTextSharp.text.Paragraph totalParagraph = new iTextSharp.text.Paragraph("TỔNG TIỀN: " + total.ToString("N0", VietnamCulture) + " VNĐ", fontTotal);
            totalParagraph.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
            totalCell.AddElement(totalParagraph);
            table.AddCell(totalCell);

            table.SpacingAfter = 20;
            document.Add(table);
        }

        /// <summary>
        /// Thêm ba ô chữ ký cho người lập phiếu, người giao và người nhận hàng.
        /// </summary>
        private static void AddSignatureBlock(iTextSharp.text.Document document, iTextSharp.text.Font fontBody, iTextSharp.text.Font fontBold)
        {
            iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(3);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 33f, 34f, 33f });

            AddSignatureCell(table, "Người lập phiếu", fontBold, fontBody);
            AddSignatureCell(table, "Người giao hàng", fontBold, fontBody);
            AddSignatureCell(table, "Người nhận hàng", fontBold, fontBody);

            document.Add(table);
        }

        /// <summary>
        /// Thêm một ô chữ ký vào bảng chữ ký.
        /// </summary>
        private static void AddSignatureCell(iTextSharp.text.pdf.PdfPTable table, string title, iTextSharp.text.Font fontBold, iTextSharp.text.Font fontBody)
        {
            iTextSharp.text.pdf.PdfPCell cell = NoBorderCell();
            cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
            cell.AddElement(new iTextSharp.text.Paragraph(title, fontBold) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
            cell.AddElement(new iTextSharp.text.Paragraph("(Ký, ghi rõ họ tên)", fontBody) { Alignment = iTextSharp.text.Element.ALIGN_CENTER });
            cell.FixedHeight = 80;
            table.AddCell(cell);
        }

        /// <summary>
        /// Thêm đường kẻ phân cách giữa phần đầu và nội dung PDF.
        /// </summary>
        private static void AddDivider(iTextSharp.text.Document document)
        {
            iTextSharp.text.pdf.PdfPTable divider = new iTextSharp.text.pdf.PdfPTable(1);
            divider.WidthPercentage = 100;
            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(" "));
            cell.BorderWidthTop = 1;
            cell.BorderWidthLeft = 0;
            cell.BorderWidthRight = 0;
            cell.BorderWidthBottom = 0;
            cell.BorderColorTop = new iTextSharp.text.BaseColor(190, 202, 216);
            cell.FixedHeight = 8;
            divider.AddCell(cell);
            document.Add(divider);
        }

        /// <summary>
        /// Thêm một ô thông tin vào bảng thông tin phiếu, có thể tô nền cho ô nhãn.
        /// </summary>
        private static void AddInfoCell(iTextSharp.text.pdf.PdfPTable table, string text, iTextSharp.text.Font font, bool shaded)
        {
            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(text, font));
            cell.Padding = 6;
            cell.BorderColor = new iTextSharp.text.BaseColor(210, 218, 228);
            if (shaded)
            {
                cell.BackgroundColor = new iTextSharp.text.BaseColor(244, 247, 251);
            }

            table.AddCell(cell);
        }

        /// <summary>
        /// Thêm một ô dữ liệu vào bảng chi tiết với căn lề được truyền vào.
        /// </summary>
        private static void AddBodyCell(iTextSharp.text.pdf.PdfPTable table, string text, iTextSharp.text.Font font, int alignment)
        {
            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(text, font));
            cell.Padding = 6;
            cell.HorizontalAlignment = alignment;
            cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
            cell.BorderColor = new iTextSharp.text.BaseColor(210, 218, 228);
            table.AddCell(cell);
        }

        /// <summary>
        /// Tạo ô PDF không viền dùng cho các vùng bố cục tự do.
        /// </summary>
        private static iTextSharp.text.pdf.PdfPCell NoBorderCell()
        {
            iTextSharp.text.pdf.PdfPCell cell = new iTextSharp.text.pdf.PdfPCell();
            cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            cell.Padding = 4;
            return cell;
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
        /// Định dạng chuỗi số thành tiền tệ không phần thập phân theo văn hóa Việt Nam.
        /// </summary>
        private static string FormatMoney(string value)
        {
            decimal amount = ParseDecimal(value);
            return amount.ToString("N0", VietnamCulture);
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
