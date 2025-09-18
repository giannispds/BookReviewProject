using BookReviews.Data;
using BookReviews.Models;



namespace BookReviews.Repositories
{
    public interface IRepository
    {
        Task<T> CreateAsync<T>(T entity) where T : BaseEntity;
        Task<T> UpdateAsync<T>(T entity) where T : BaseEntity;
        Task<T> GetByIdAsync<T>(Guid id) where T : BaseEntity;
    }

    public class EfRepository : IRepository
    {
        private readonly AppDbContext _dbContext;

        public EfRepository(AppDbContext dbContext)
        {   
            _dbContext = dbContext;
        }

        // Create entity
        public async Task<T> CreateAsync<T>(T entity) where T : BaseEntity
        {
            entity.InitNew();
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<T> GetByIdAsync<T>(Guid id) where T : BaseEntity
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        // Update entity
        public async Task<T> UpdateAsync<T>(T entity) where T : BaseEntity
        {
            entity.ValidateId();
            entity.WasUpdated();
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}