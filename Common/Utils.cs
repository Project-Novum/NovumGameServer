namespace Common;

public static class Utils
{
    public static ulong MilisUnixTimeStampUTC(DateTime? time = null)
    {
        ulong unixTimeStamp;
        var currentTime = time ?? DateTime.Now;
        var zuluTime = currentTime.ToUniversalTime();
        var unixEpoch = new DateTime(1970, 1, 1);
        unixTimeStamp = (ulong)zuluTime.Subtract(unixEpoch).TotalMilliseconds;

        return unixTimeStamp;
    }
}