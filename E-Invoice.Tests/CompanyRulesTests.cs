using EInvoice.Application.Companies;

namespace EInvoice.Tests;

public class CompanyRulesTests
{
    [Theory]
    [InlineData("AE070331234567890123456")]
    [InlineData("AE07 0331 2345 6789 0123 456")]
    [InlineData("ae070331234567890123456")]
    [InlineData("DE89370400440532013000")]
    [InlineData("GB82WEST12345698765432")]
    [InlineData(null)]
    [InlineData("")]
    public void ValidIban_IsAccepted(string? iban) => Assert.True(CompanyRules.IsValidIban(iban));

    [Theory]
    [InlineData("AE070331234567890123457")] // last digit changed: checksum fails
    [InlineData("AE0703312345")]           // too short
    [InlineData("0E070331234567890123456")] // no country code
    public void InvalidIban_IsRejected(string iban) => Assert.False(CompanyRules.IsValidIban(iban));

    [Theory]
    [InlineData("100123456700003", true)]
    [InlineData(null, true)]
    [InlineData("10012345670000", false)]
    [InlineData("10012345670000A", false)]
    public void Trn_MustBe15Digits(string? trn, bool expected) => Assert.Equal(expected, CompanyRules.IsValidTrn(trn));

    [Theory]
    [InlineData("EBILAEAD", true)]
    [InlineData("ebilaeadxxx", true)]
    [InlineData("EBILAEA", false)]
    [InlineData("EBILAEADXX", false)]
    public void Swift_Is8Or11Characters(string swift, bool expected) => Assert.Equal(expected, CompanyRules.IsValidSwift(swift));

    [Theory]
    [InlineData("0088:1234567890123", true)]
    [InlineData("0235:100123456700003", true)]
    [InlineData("1234567890", false)]
    [InlineData("88:123", false)]
    [InlineData("0088:12 34", false)]
    public void PeppolId_IsSchemeColonIdentifier(string id, bool expected) => Assert.Equal(expected, CompanyRules.IsValidPeppolId(id));

    [Fact]
    public void Logo_TypeIsDetectedFromContent()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
        byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE0];
        byte[] webp = "RIFF\0\0\0\0WEBPVP8 "u8.ToArray();
        byte[] text = "<svg onload=alert(1)>"u8.ToArray();

        Assert.Equal("image/png", CompanyRules.DetectLogoContentType(png));
        Assert.Equal("image/jpeg", CompanyRules.DetectLogoContentType(jpeg));
        Assert.Equal("image/webp", CompanyRules.DetectLogoContentType(webp));
        Assert.Null(CompanyRules.DetectLogoContentType(text));
        Assert.Null(CompanyRules.DetectLogoContentType(new byte[CompanyRules.MaxLogoBytes + 1]));
    }
}
