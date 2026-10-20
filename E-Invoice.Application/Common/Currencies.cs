namespace EInvoice.Application.Common;

/// <summary>ISO 4217 currencies offered in dropdowns (GCC first).</summary>
public static class Currencies
{
    public static readonly IReadOnlyList<(string Code, string Name)> All =
    [
        ("AED", "UAE Dirham"),
        ("SAR", "Saudi Riyal"),
        ("QAR", "Qatari Riyal"),
        ("KWD", "Kuwaiti Dinar"),
        ("BHD", "Bahraini Dinar"),
        ("OMR", "Omani Rial"),
        ("USD", "US Dollar"),
        ("EUR", "Euro"),
        ("GBP", "Pound Sterling"),
        ("CHF", "Swiss Franc"),
        ("INR", "Indian Rupee"),
        ("PKR", "Pakistani Rupee"),
        ("EGP", "Egyptian Pound"),
        ("JOD", "Jordanian Dinar"),
        ("CNY", "Chinese Yuan"),
        ("JPY", "Japanese Yen"),
        ("SGD", "Singapore Dollar"),
        ("HKD", "Hong Kong Dollar"),
        ("AUD", "Australian Dollar"),
        ("CAD", "Canadian Dollar"),
    ];

    public static bool IsSupported(string? code) => code is not null && All.Any(c => c.Code == code);
}
