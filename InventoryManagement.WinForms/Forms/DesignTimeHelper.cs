using System.ComponentModel;
using System.Diagnostics;

namespace InventoryManagement.Forms
{
    /// <summary>
    /// Nhận biết tiến trình WinForms Designer để không chạy API hoặc logic runtime khi mở tab Design.
    /// </summary>
    internal static class DesignTimeHelper
    {
        public static bool IsDesignMode =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
            Process.GetCurrentProcess().ProcessName.Contains(
                "DesignToolsServer",
                StringComparison.OrdinalIgnoreCase) ||
            AppDomain.CurrentDomain.FriendlyName.Contains(
                "DesignToolsServer",
                StringComparison.OrdinalIgnoreCase);
    }
}
