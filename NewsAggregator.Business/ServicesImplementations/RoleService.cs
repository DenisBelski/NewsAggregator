using Microsoft.EntityFrameworkCore;
using NewsAggregator.Core.Abstractions;
using NewsAggregator.Data.Abstractions;

namespace NewsAggregator.Business.ServicesImplementations
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GetRoleNameByIdAsync(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);


            return role != null 
                ? role.Name 
                : string.Empty;
        }

        public async Task<Guid?> GetRoleIdByNameAsync(string name)
        {
            var role = await _unitOfWork.Roles
                .FindBy(currentRole => currentRole.Name.Equals(name))
                .FirstOrDefaultAsync();

            return role != null
                ? role.Id
                : null;
        }
    }
}