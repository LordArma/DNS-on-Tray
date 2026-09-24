using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DNS_on_Tray
{
    /// <summary>
    /// Measures DNS servers by sending a real DNS query (UDP port 53) instead of an ICMP ping,
    /// since many resolvers drop pings but still answer queries.
    /// </summary>
    public static class DnsProbe
    {
        private const string TestDomain = "google.com";
        private const int TimeoutMs = 2000;

        /// <summary>
        /// Returns the round-trip time in milliseconds, or null when the server did not answer
        /// in time or refused the query.
        /// </summary>
        public static Task<int?> Measure(string server, CancellationToken cancellationToken)
        {
            if (!IPAddress.TryParse(server, out IPAddress? ip))
                return Task.FromResult<int?>(null);

            return Measure(new IPEndPoint(ip, 53), TimeoutMs, cancellationToken);
        }

        /// <summary>
        /// Queries a DNS server at any endpoint (tests use a local fake server on another port).
        /// </summary>
        internal static async Task<int?> Measure(IPEndPoint endpoint, int timeoutMs, CancellationToken cancellationToken)
        {
            IPAddress ip = endpoint.Address;

            byte[] query = BuildQuery((ushort)Random.Shared.Next(ushort.MaxValue), TestDomain);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(timeoutMs);

            try
            {
                using var udp = new UdpClient(ip.AddressFamily);

                Stopwatch sw = Stopwatch.StartNew();
                await udp.SendAsync(query, endpoint, timeout.Token);

                while (true)
                {
                    UdpReceiveResult reply = await udp.ReceiveAsync(timeout.Token);
                    byte[] b = reply.Buffer;

                    // Skip anything that is not the response to our query.
                    if (b.Length < 12 || b[0] != query[0] || b[1] != query[1] || (b[2] & 0x80) == 0)
                        continue;

                    // NOERROR or NXDOMAIN both mean the resolver is working; anything else
                    // (SERVFAIL, REFUSED, ...) means it is not usable.
                    int rcode = b[3] & 0x0F;
                    return rcode == 0 || rcode == 3 ? (int)sw.ElapsedMilliseconds : null;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            catch (SocketException)
            {
                return null;
            }
        }

        /// <summary>
        /// Measures every server of a DNS entry in parallel. The result has one value per server,
        /// in the same order as <see cref="DNS.Servers"/>.
        /// </summary>
        public static async Task<int?[]> MeasureAll(DNS dns, CancellationToken cancellationToken)
        {
            return await Task.WhenAll(dns.Servers().Select(s => Measure(s, cancellationToken)));
        }

        /// <summary>
        /// True when DNS traffic is being intercepted (typically by a VPN or proxy that answers
        /// every query itself). Detected by querying a reserved address that has no DNS server
        /// (192.0.2.1, RFC 5737); in that case test results only measure the interceptor.
        /// </summary>
        public static async Task<bool> IsIntercepted(CancellationToken cancellationToken)
        {
            return await Measure("192.0.2.1", cancellationToken) != null;
        }

        private static byte[] BuildQuery(ushort id, string domain)
        {
            var packet = new List<byte>
            {
                (byte)(id >> 8), (byte)id,
                0x01, 0x00, // standard query, recursion desired
                0x00, 0x01, // 1 question
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            foreach (string label in domain.Split('.'))
            {
                packet.Add((byte)label.Length);
                packet.AddRange(Encoding.ASCII.GetBytes(label));
            }

            packet.AddRange(new byte[] { 0x00, 0x00, 0x01, 0x00, 0x01 }); // end of name, type A, class IN
            return packet.ToArray();
        }
    }
}
