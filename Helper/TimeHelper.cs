
namespace UserDeviceApi.Helper
{
    public static class TimeHelper
    {
        public static long GetUnixTime()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            long secondsSinceEpoch = (int)t.TotalSeconds;

            return secondsSinceEpoch;
        }

        public static long GetExpirationDate()
        {
            var now = GetUnixTime();
            var expirationDate = now += 3600;

            return expirationDate;
        }
    }
}
   