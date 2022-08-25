using Microsoft.EntityFrameworkCore;
using UserDeviceApi.Model;

namespace UserDeviceApi.Context
{
    class UserDevicesDB : DbContext
    {
        public UserDevicesDB(DbContextOptions<UserDevicesDB> options) : base(options) { }
        public DbSet<UserDeviceCode> UserDeviceCodes => Set<UserDeviceCode>();
        public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    }
}