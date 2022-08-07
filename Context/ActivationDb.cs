using Microsoft.EntityFrameworkCore;

class ActivationDb : DbContext
{
    public ActivationDb(DbContextOptions<ActivationDb> options) : base(options) { }
    public DbSet<UserDeviceCode> UserDeviceCodes => Set<UserDeviceCode>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
}