using System.Net.NetworkInformation;
namespace LinkChat.Core.Tools;
public static class NetworkInterfaceSelector
{
    public static string GetBestNetworkInterfaceName()
    {
        var allInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        var bestInterface = allInterfaces
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Where(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                        n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            .FirstOrDefault(n => n.GetIPProperties().GetIPv4Properties() != null);
        
        if (bestInterface == null)
        {
            throw new InvalidOperationException("No suitable active network interface found. Please check your network connection.");
        }

        return bestInterface.Name;
    }
}