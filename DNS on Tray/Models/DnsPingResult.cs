using System.Net.NetworkInformation;

public sealed record DnsPingResult(PingReply? Dns1, PingReply? Dns2)
{
    public bool Success =>
        Dns1?.Status == IPStatus.Success && Dns2?.Status == IPStatus.Success;
}