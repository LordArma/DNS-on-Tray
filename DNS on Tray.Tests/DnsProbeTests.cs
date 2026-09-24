using System.Net;
using System.Net.Sockets;

namespace DNS_on_Tray.Tests
{
    /// <summary>
    /// Runs the probe against a fake DNS server on localhost.
    /// </summary>
    public class DnsProbeTests
    {
        /// <summary>
        /// Starts a UDP server that answers one query using <paramref name="respond"/>
        /// (return null to stay silent).
        /// </summary>
        private static (IPEndPoint Endpoint, Task Server) FakeServer(Func<byte[], byte[]?> respond)
        {
            UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            var endpoint = (IPEndPoint)server.Client.LocalEndPoint!;

            Task task = Task.Run(async () =>
            {
                using (server)
                {
                    UdpReceiveResult query = await server.ReceiveAsync();
                    byte[]? reply = respond(query.Buffer);
                    if (reply != null)
                        await server.SendAsync(reply, query.RemoteEndPoint);
                    else
                        await Task.Delay(3000);
                }
            });

            return (endpoint, task);
        }

        private static byte[] Reply(byte[] query, int rcode, bool sameId = true)
        {
            byte[] reply = (byte[])query.Clone();
            if (!sameId)
                reply[0] ^= 0xFF;
            reply[2] |= 0x80; // response
            reply[3] = (byte)((reply[3] & 0xF0) | rcode);
            return reply;
        }

        [Fact]
        public async Task Answer_ReturnsLatency()
        {
            var (endpoint, _) = FakeServer(q => Reply(q, 0));
            Assert.NotNull(await DnsProbe.Measure(endpoint, 2000, default));
        }

        [Fact]
        public async Task NxDomain_CountsAsWorking()
        {
            var (endpoint, _) = FakeServer(q => Reply(q, 3));
            Assert.NotNull(await DnsProbe.Measure(endpoint, 2000, default));
        }

        [Fact]
        public async Task Refused_ReturnsNull()
        {
            var (endpoint, _) = FakeServer(q => Reply(q, 5));
            Assert.Null(await DnsProbe.Measure(endpoint, 2000, default));
        }

        [Fact]
        public async Task ReplyWithOtherId_IsIgnored()
        {
            var (endpoint, _) = FakeServer(q => Reply(q, 0, sameId: false));
            Assert.Null(await DnsProbe.Measure(endpoint, 500, default));
        }

        [Fact]
        public async Task NoReply_TimesOut()
        {
            var (endpoint, _) = FakeServer(_ => null);
            Assert.Null(await DnsProbe.Measure(endpoint, 300, default));
        }

        [Fact]
        public async Task Query_AsksForGoogleComTypeA()
        {
            byte[]? seen = null;
            var (endpoint, _) = FakeServer(q => { seen = q; return Reply(q, 0); });
            await DnsProbe.Measure(endpoint, 2000, default);

            Assert.NotNull(seen);
            Assert.Equal(new byte[] { 0x01, 0x00, 0x00, 0x01 }, seen![2..6]);  // RD flag, 1 question
            Assert.Equal(new byte[] { 6, (byte)'g', (byte)'o', (byte)'o', (byte)'g', (byte)'l', (byte)'e', 3, (byte)'c', (byte)'o', (byte)'m', 0, 0, 1, 0, 1 }, seen[12..]);
        }

        [Fact]
        public async Task Cancellation_Throws()
        {
            var (endpoint, _) = FakeServer(_ => null);
            using var cts = new CancellationTokenSource(100);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DnsProbe.Measure(endpoint, 2000, cts.Token));
        }
    }
}
