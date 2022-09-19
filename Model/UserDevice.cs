using UserDeviceApi.Helpers;

namespace UserDeviceApi.Model
{
    public class UserDevice
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public DeviceType DeviceType { get; set; }
        public UserDeviceCode UserDeviceCode { get; set; }
        public User User { get; set; }

        public UserDevice()
        {

        }

        public UserDevice(Guid deviceId, DeviceType deviceType, string code)
        {
            UserDeviceCode = new UserDeviceCode(code);
            DeviceId = deviceId;
            DeviceType = deviceType;
        }
    }

    public class UserDeviceCode
    {
        public Guid Id { get; set; }
        public Guid UserDeviceId { get; set; }
        public string Code { get; set; }
        public long ExpirationDate { get; set; }

        public UserDeviceCode()
        {
            
        }

        public UserDeviceCode(string code)
        {
            Code = code;
            ExpirationDate = TimeHelper.GetExpirationDate();
        }
    }

    public enum DeviceType
    {
        Unknown = -1,
        NotDefined = 0,
        AndroidTV,
        Roku,
        AppleTV
    }
}

