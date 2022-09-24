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
                .Include(x => x.UserDeviceCodes
                                .Where(code => TimeHelper.GetUnixTime() < code.ExpirationDate)
                                 .OrderByDescending(code => code.ExpirationDate))
                .Include(x => x.User)
                .AsQueryable();

            return result;
        }

        public IQueryable<UserDevice> GetAll()
        {
            var result = _context.UserDevices
                .AsQueryable();

            return result;
        }

        public async Task<UserDevice> AddAsync(UserDevice userDevice)
        {
            var result = await _context.UserDevices.AddAsync(userDevice);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public UserDevice Update(UserDevice userDevice)
        {
            var result = _context.UserDevices.Update(userDevice);
            _context.SaveChanges();
            return result.Entity;
        }

        public void BulkAddAsync(List<UserDevice> userDevices)
        {
            _context.UserDevices.AddRange(userDevices);
            _context.SaveChanges();
        }
    }
}