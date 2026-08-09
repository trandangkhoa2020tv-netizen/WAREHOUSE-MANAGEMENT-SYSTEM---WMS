using System;
using System.Windows.Forms;
using InventoryManagement.ApiClients;
using InventoryManagement.Forms;

namespace InventoryManagement
{
    /// <summary>
    /// Điểm khởi động chính của chương trình.
    /// File này chịu trách nhiệm khởi tạo cấu hình WinForms và mở màn hình đăng nhập đầu tiên.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Khởi tạo cấu hình WinForms, bảo đảm API đã chạy và mở màn hình đăng nhập đầu tiên.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Khởi tạo cấu hình mặc định cho WinForms: font, DPI, visual style.
            ApplicationConfiguration.Initialize();

            try
            {
                ApiServerLauncher.EnsureStarted();

                // Màn hình đầu tiên của hệ thống là form đăng nhập.
                Application.Run(new FrmDangNhap());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Loi khoi dong he thong: " + ex.Message,
                    "Loi he thong",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                ApiServerLauncher.StopIfStartedByApp();
            }
        }
    }
}
