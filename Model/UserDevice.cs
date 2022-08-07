class UserDevice
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public DeviceType DeviceType { get; set; }
    public UserDeviceCode Code { get; set; }
    public int UserId { get; set; }
    public Guid UserDeviceCodeId { get; set; }
    public User User { get; set; }
}

class UserDeviceCode
{
    public Guid UserDeviceId { get; set; }
    public string Code { get; set; }
    public long ExpirationDate { get; set; }
}

public enum DeviceType
{
    Unknown = -1,
    NotDefined = 0,
    AndroidTV,
    Roku,
    AppleTV
}