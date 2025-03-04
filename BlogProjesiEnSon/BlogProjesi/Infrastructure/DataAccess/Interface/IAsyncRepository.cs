namespace Infrastructure.DataAccess.Interface
{
    public interface IAsyncRepository
    {
        Task<int> SaveChangeAsync();
    }
}
