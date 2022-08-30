using UserDeviceApi.Context;
using UserDeviceApi.Model;

namespace UserDeviceApi.Services
{
    public class UserDeviceCodeService
    {
        private UserDevicesDB _context;
        
        public UserDeviceCodeService(UserDevicesDB context)
        {
            _context = context;
        }

        public IQueryable<UserDeviceCode> GetUserDeviceCodes()
        {
            var result = _context.UserDeviceCodes
                .AsQueryable();

            return result;
        }

    }
}