using Microsoft.EntityFrameworkCore;
using UserDeviceApi.Model;

namespace UserDeviceApi.Context
{
    public class UserDevicesDB : DbContext
    {
        public UserDevicesDB(DbContextOptions<UserDevicesDB> options) : base(options) { }
        public DbSet<UserDevice> UserDevices => Set<UserDevice>();
        public DbSet<UserDeviceCode> UserDeviceCodes => Set<UserDeviceCode>();
        public DbSet<User> Users => Set<User>();
    }
}