using static DNS_on_Tray.Helper;

namespace DNS_on_Tray.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData("1.1.1.1", true)]
        [InlineData(" 8.8.8.8 ", true)]
        [InlineData("1", false)]
        [InlineData("0x01010101", false)]
        [InlineData("1.1.1", false)]
        [InlineData("256.1.1.1", false)]
        [InlineData("::1", false)]
        [InlineData("1.1.1.1\"); calc & (\"", false)]
        [InlineData("", false)]
        public void IsValidIPv4(string text, bool expected)
        {
            Assert.Equal(expected, Helper.IsValidIPv4(text));
        }

        [Theory]
        [InlineData("2606:4700:4700::1111", true)]
        [InlineData("::1", true)]
        [InlineData("fe80::1%12", false)]
        [InlineData("1.1.1.1", false)]
        [InlineData("2606:4700::1111'; calc", false)]
        [InlineData("", false)]
        public void IsValidIPv6(string text, bool expected)
        {
            Assert.Equal(expected, Helper.IsValidIPv6(text));
        }

        [Theory]
        [InlineData("https://dns.google/dns-query", true)]
        [InlineData("http://dns.google/dns-query", false)]
        [InlineData("https://x/a'b", false)]
        [InlineData("https://x/a b", false)]
        [InlineData("dns.google", false)]
        public void IsValidDoH(string text, bool expected)
        {
            Assert.Equal(expected, Helper.IsValidDoH(text));
        }

        [Fact]
        public void IsValidEntry_RequiresDns1AndChecksOptionalFields()
        {
            Assert.True(IsValidEntry(new DNS("a", "1.1.1.1", "")));
            Assert.True(IsValidEntry(new DNS("a", "1.1.1.1", "1.0.0.1", "2606:4700:4700::1111", "", "https://cloudflare-dns.com/dns-query")));
            Assert.False(IsValidEntry(new DNS("a", "", "1.0.0.1")));
            Assert.False(IsValidEntry(new DNS("a", "1.1.1.1", "", "zzz")));
            Assert.False(IsValidEntry(new DNS("a", "1.1.1.1", "", "", "", "http://insecure")));
        }

        [Fact]
        public async Task AddDNS_RejectsInvalidEntryWithoutRunningAnything()
        {
            Assert.Equal(DnsChangeResult.InvalidAddress, await AddDNS(new DNS("x", "1.1.1.1", "x'; calc")));
        }

        [Fact]
        public void PopularDNS_AreAllValid()
        {
            Assert.All(PopularDNS(), dns => Assert.True(IsValidEntry(dns), dns.Name()));
        }
    }
}
