class User 
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    List<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}