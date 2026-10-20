using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.RegularExpressions;

namespace EInvoice.Application.Companies;

/// <summary>Format rules for company tax, bank and logo fields, shared by the form and the service.</summary>
public static partial class CompanyRules
{
    public const int MaxLogoBytes = 1024 * 1024;

    public const string TrnMessage = "TRN must be exactly 15 digits.";
    public const string PeppolMessage = "PEPPOL Participant ID must be 'scheme:identifier', e.g. 0088:1234567890123.";
    public const string IbanMessage = "IBAN is not valid. Check the country code, length and digits.";
    public const string SwiftMessage = "SWIFT code must be 8 or 11 characters (e.g. EBILAEAD or EBILAEADXXX).";
    public const string LogoMessage = "Logo must be a PNG, JPEG or WebP image of at most 1 MB.";

    [GeneratedRegex(@"^\d{15}$")]
    private static partial Regex TrnRegex();

    [GeneratedRegex(@"^\d{4}:[^\s:]{1,195}$")]
    private static partial Regex PeppolRegex();

    [GeneratedRegex(@"^[A-Z]{2}\d{2}[A-Z0-9]{11,30}$")]
    private static partial Regex IbanRegex();

    [GeneratedRegex(@"^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$")]
    private static partial Regex SwiftRegex();

    /// <summary>Removes spaces and upper-cases (for IBAN and SWIFT); returns null for blank input.</summary>
    public static string? NormalizeCode(string? value)
    {
        var normalized = value?.Replace(" ", string.Empty).Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(normalized) ? null : normalized;
    }

    public static bool IsValidTrn(string? value) => string.IsNullOrWhiteSpace(value) || TrnRegex().IsMatch(value.Trim());

    public static bool IsValidPeppolId(string? value) => string.IsNullOrWhiteSpace(value) || PeppolRegex().IsMatch(value.Trim());

    public static bool IsValidSwift(string? value)
    {
        var code = NormalizeCode(value);
        return code is null || SwiftRegex().IsMatch(code);
    }

    /// <summary>Structure check plus the ISO 13616 mod-97 checksum.</summary>
    public static bool IsValidIban(string? value)
    {
        var iban = NormalizeCode(value);
        if (iban is null)
        {
            return true;
        }

        if (!IbanRegex().IsMatch(iban))
        {
            return false;
        }

        var rearranged = iban[4..] + iban[..4];
        var digits = string.Concat(rearranged.Select(ch => char.IsLetter(ch) ? (ch - 'A' + 10).ToString() : ch.ToString()));
        return BigInteger.Parse(digits) % 97 == 1;
    }

    /// <summary>Detects the image type from the file's first bytes; returns null when it is not PNG, JPEG or WebP.</summary>
    public static string? DetectLogoContentType(byte[] content)
    {
        if (content.Length is 0 or > MaxLogoBytes)
        {
            return null;
        }

        if (content.Length > 8 && content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47)
        {
            return "image/png";
        }

        if (content.Length > 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (content.Length > 12
            && content[0] == 'R' && content[1] == 'I' && content[2] == 'F' && content[3] == 'F'
            && content[8] == 'W' && content[9] == 'E' && content[10] == 'B' && content[11] == 'P')
        {
            return "image/webp";
        }

        return null;
    }
}

public sealed class TrnAttribute() : ValidationAttribute(CompanyRules.TrnMessage)
{
    public override bool IsValid(object? value) => CompanyRules.IsValidTrn(value as string);
}

public sealed class PeppolIdAttribute() : ValidationAttribute(CompanyRules.PeppolMessage)
{
    public override bool IsValid(object? value) => CompanyRules.IsValidPeppolId(value as string);
}

public sealed class IbanAttribute() : ValidationAttribute(CompanyRules.IbanMessage)
{
    public override bool IsValid(object? value) => CompanyRules.IsValidIban(value as string);
}

public sealed class SwiftCodeAttribute() : ValidationAttribute(CompanyRules.SwiftMessage)
{
    public override bool IsValid(object? value) => CompanyRules.IsValidSwift(value as string);
}
