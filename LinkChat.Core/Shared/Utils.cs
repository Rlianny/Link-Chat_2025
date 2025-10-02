namespace LinkChat.Core.Tools
{
    public static class Tools
    {
        public static byte[] ParseMacAddress(string macAddress)
        {
            return macAddress.Split(':', '-')
                             .Select(b => Convert.ToByte(b, 16))
                             .ToArray();
        }
    }

}