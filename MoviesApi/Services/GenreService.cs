using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Services
{
    public class GenreService : IGenreService
    {
        private readonly ApplicationDbContext _dbContext;

        public GenreService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _dbContext.Genres.OrderBy(g => g.Name).ToListAsync();
        }


        public async Task<Genre> AddAsync(Genre genre)
        {
            await _dbContext.Genres.AddAsync(genre);
            _dbContext.SaveChanges();

            return genre;

        }

        public Genre Delete(Genre genre)
        {
            _dbContext.Genres.Remove(genre);
            _dbContext.SaveChanges();

            return genre;
        }


        public Genre Update(Genre genre)
        {
            _dbContext.Genres.Update(genre);
            _dbContext.SaveChanges();

            return genre;
        }

        public async Task<Genre> GetByIdAsync(byte id)
        {
            return await _dbContext.Genres.SingleOrDefaultAsync(G => G.Id == id);
        }
    }
}
