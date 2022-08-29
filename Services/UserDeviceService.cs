using Microsoft.EntityFrameworkCore;
using UserDeviceApi.Context;
using UserDeviceApi.Helpers;
using UserDeviceApi.Model;

namespace UserDeviceApi.Services
{
    public class UserDeviceService
    {
        private UserDevicesDB _context;
        
        public UserDeviceService(UserDevicesDB context)
        {
            _context = context;
        }

        public IQueryable<UserDevice> GetUserDevices()
        {
            var result = _context.UserDevices
                .Include(x => x.Code)
                .Include(x => x.Code)
                .Where(x => x.Code.ExpirationDate <= TimeHelper.GetUnixTime())
                .AsQueryable();

            return result;
        }

        public async Task<UserDevice> AddAsync(UserDevice userDevice)
        {
            var result = await _context.UserDevices.AddAsync(userDevice);
            return result.Entity;
        }

        public UserDevice Update(UserDevice userDevice)
        {
            var result = _context.UserDevices.Update(userDevice);
            return result.Entity;
        }
    }
}