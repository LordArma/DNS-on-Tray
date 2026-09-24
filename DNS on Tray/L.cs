using System.Globalization;
using static DNS_on_Tray.Helper;

namespace DNS_on_Tray
{
    /// <summary>
    /// User interface text in English and Farsi. Kept in code (rather than satellite resource
    /// assemblies) so it works unchanged in the single-file build.
    /// </summary>
    public static class L
    {
        public const string Auto = "auto";
        public const string English = "en";
        public const string Farsi = "fa";

        /// <summary>
        /// The saved choice: "auto" (follow Windows), "en" or "fa".
        /// </summary>
        public static string Setting
        {
            get => LanguageSetting ?? Auto;
            set => LanguageSetting = value == Auto ? null : value;
        }

        /// <summary>
        /// The language in use after resolving "auto".
        /// </summary>
        public static string Current =>
            Setting != Auto ? Setting
            : CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == Farsi ? Farsi
            : English;

        public static bool IsRtl => Current == Farsi;

        private static string T(string en, string fa) => Current == Farsi ? fa : en;

        /// <summary>
        /// Keeps left-to-right text (server names, addresses) in its own order inside Farsi text;
        /// without this "403.online" would show as "online.403".
        /// </summary>
        public static string Ltr(string text) => IsRtl ? "\u202A" + text + "\u202C" : text;

        /// <summary>
        /// A field label such as "IPv4 1:" whose colon lands on the correct side in either direction.
        /// </summary>
        public static string FieldLabel(string text) => Ltr(text) + ":";

        // Window
        public static string Servers => T("Servers", "سرورها");
        public static string Adapter => T("Adapter:", "کارت شبکه:");
        public static string Test => T("Test", "تست");
        public static string TestAll => T("Test all", "تست همه");
        public static string Edit => T("Edit", "ویرایش");
        public static string Remove => T("Remove", "حذف");
        public static string Set => T("Set", "اعمال");
        public static string HealthCheck => T("DNS Health Check:", "بررسی سلامت DNS:");
        public static string AddNew => T("Add a new custom DNS", "افزودن DNS دلخواه");
        public static string EditEntry(string name) => T($"Edit \"{name}\"", $"ویرایش «{Ltr(name)}»");
        public static string DnsName => T("DNS Name:", "نام:");
        public static string DoHUrl => T("DoH URL:", "آدرس DoH:");
        public static string Optional => T("optional", "اختیاری");
        public static string OptionalDoH => T("optional, https://.../dns-query", "اختیاری، https://.../dns-query");
        public static string DoHNeedsWindows11 => T("needs Windows 11 (saved, not used)", "نیاز به ویندوز ۱۱ (ذخیره می‌شود، استفاده نمی‌شود)");
        public static string Import => T("Import...", "ورود...");
        public static string Export => T("Export...", "خروجی...");
        public static string Cancel => T("Cancel", "انصراف");
        public static string Add => T("Add", "افزودن");
        public static string Save => T("Save", "ذخیره");
        public static string LaunchOnStartup => T("Launch on startup", "اجرا هنگام ورود به ویندوز");
        public static string RunAsAdmin => T("Run as administrator (no prompt when switching)", "اجرا با دسترسی مدیر (بدون درخواست مجوز هنگام تغییر)");

        // Server list
        public static string ClearEntry => T("Clear (automatic / DHCP)", "پاک کردن (خودکار / DHCP)");
        public static string Milliseconds(int ms) => T($"{ms} ms", $"{ms} میلی‌ثانیه");
        public static string NoAnswer => T("no answer", "بدون پاسخ");

        // Current DNS and adapters
        public static string CurrentDns(string description) => T($"Current DNS: {description}", $"DNS فعلی: {description}");
        public static string SelectedAdapterNotConnected => T("selected adapter is not connected", "کارت شبکهٔ انتخاب‌شده وصل نیست");
        public static string NoConnectedAdapter => T("no connected adapter", "هیچ کارت شبکهٔ متصلی نیست");
        public static string AutomaticDhcp => T("Automatic (DHCP)", "خودکار (DHCP)");
        public static string AutomaticAdapters => T("Automatic (connected adapters)", "خودکار (کارت‌های شبکهٔ متصل)");
        public static string Disconnected(string name) => T($"{name} (disconnected)", $"{Ltr(name)} (قطع)");
        public static string SavedAdapterMissing => T("Saved adapter (not available)", "کارت شبکهٔ ذخیره‌شده (در دسترس نیست)");

        // Applying DNS
        public static string DnsSet(string name, string servers) => T($"DNS set to {name} ({servers}).", $"DNS روی {Ltr(name)} ({Ltr(servers)}) تنظیم شد.");
        public static string DnsReset => T("DNS reset to automatic (DHCP).", "DNS به حالت خودکار (DHCP) برگشت.");
        public static string PermissionDeclined => T("DNS was not changed: administrator permission was declined.", "DNS تغییر نکرد: مجوز مدیر داده نشد.");
        public static string SelectedAdapterNotConnectedError => T("DNS was not changed: the selected adapter is not connected.", "DNS تغییر نکرد: کارت شبکهٔ انتخاب‌شده وصل نیست.");
        public static string NoAdapterError => T("DNS was not changed: no connected network adapter was found.", "DNS تغییر نکرد: هیچ کارت شبکهٔ متصلی پیدا نشد.");
        public static string InvalidAddressError => T("DNS was not changed: the saved addresses are not valid.", "DNS تغییر نکرد: آدرس‌های ذخیره‌شده معتبر نیستند.");
        public static string ChangeFailed => T("Failed to change DNS.", "تغییر DNS ناموفق بود.");

        // Health check
        public static string Intercepted => T("Can't test: a VPN or proxy is answering all DNS queries.", "تست ممکن نیست: یک VPN یا پروکسی به همهٔ درخواست‌های DNS پاسخ می‌دهد.");
        public static string SelectServerToTest => T("Select a server to test.", "یک سرور را برای تست انتخاب کنید.");
        public static string Testing(string name) => T($"Testing {name}...", $"در حال تست {Ltr(name)}...");
        public static string TestingCount(int count) => T($"Testing {count} servers...", $"در حال تست {count} سرور...");
        public static string Fastest(string name, int ms, int answered, int total) =>
            T($"Fastest: {name} ({ms} ms), {answered}/{total} answered", $"سریع‌ترین: {Ltr(name)} ({ms} میلی‌ثانیه)، {answered} از {total} پاسخ دادند");
        public static string NoServerAnswered => T("No server answered.", "هیچ سروری پاسخ نداد.");

        // Import / export
        public static string FileFilter => T("DNS on Tray list (*.json)|*.json|All files (*.*)|*.*", "فهرست DNS on Tray (*.json)|*.json|همهٔ فایل‌ها (*.*)|*.*");
        public static string ExportFilter => T("DNS on Tray list (*.json)|*.json", "فهرست DNS on Tray (*.json)|*.json");
        public static string ImportTitle => T("Import DNS servers", "ورود سرورهای DNS");
        public static string ExportTitle => T("Export DNS servers", "خروجی گرفتن از سرورهای DNS");
        public static string Imported(int count) => T($"Imported {count} server(s).", $"{count} سرور وارد شد.");
        public static string Skipped(int count) => T($"Skipped {count} (name already used or invalid addresses).", $"{count} مورد رد شد (نام تکراری یا آدرس نامعتبر).");
        public static string ImportFailed(string error) => T($"Could not import the file:\n{error}", $"ورود فایل ناموفق بود:\n{error}");
        public static string Exported(int count) => T($"Exported {count} server(s).", $"{count} سرور در فایل ذخیره شد.");
        public static string ExportFailed(string error) => T($"Could not export the file:\n{error}", $"ذخیرهٔ فایل ناموفق بود:\n{error}");
        public static string LoadFailed(string error) => T($"Could not load the saved DNS list:\n{error}", $"فهرست DNSهای ذخیره‌شده بارگذاری نشد:\n{error}");

        // Startup and administrator mode
        public static string StartupUpdateFailed => T("Could not update the startup setting.", "تنظیم اجرای خودکار به‌روز نشد.");
        public static string AdminNotOn => T("Administrator mode was not turned on.", "حالت مدیر فعال نشد.");
        public static string AdminNotOff => T("Administrator mode was not turned off.", "حالت مدیر غیرفعال نشد.");
        public static string AdminOff => T("Administrator mode is off. Windows will ask for permission on each DNS change after the app restarts.",
                                           "حالت مدیر خاموش شد. پس از اجرای دوبارهٔ برنامه، ویندوز برای هر تغییر DNS مجوز می‌خواهد.");

        // Tray menu
        public static string Clear => T("Clear", "پاک کردن");
        public static string Settings => T("Settings", "تنظیمات");
        public static string Exit => T("Exit", "خروج");
        public static string Language => T("Language", "زبان");
        public static string LanguageAuto => T("Automatic (Windows language)", "خودکار (زبان ویندوز)");
    }
}
