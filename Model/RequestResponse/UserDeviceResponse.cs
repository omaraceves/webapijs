using System;

public class UserDeviceResponse 
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public DeviceType DeviceType { get; set; }
    public string Code { get; set; }
    public User User { get; set; }


    public UserDeviceResponse()
    {

    }

     public UserDeviceResponse(UserDevice userDevice)
    {
        Id = userDevice.Id;
        DeviceId = userDevice.DeviceId;
        DeviceType = userDevice.DeviceType;
    }

    public UserDeviceResponse(UserDevice userDevice, User user)
    {
        Id = userDevice.Id;
        DeviceId = userDevice.DeviceId;
        DeviceType = userDevice.DeviceType;
        User = user;
    }

    public UserDeviceResponse(UserDevice userDevice, UserDeviceCode code)
    {
        Id = userDevice.Id;
        DeviceId = userDevice.DeviceId;
        DeviceType = userDevice.DeviceType;
        Code = code.Code;
    }
}

