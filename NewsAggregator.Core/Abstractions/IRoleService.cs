namespace NewsAggregator.Core.Abstractions
{
    public interface IRoleService
    {
        Task<string> GetRoleNameByIdAsync(Guid id);
        Task<Guid?> GetRoleIdByNameAsync(string name);
    }
}