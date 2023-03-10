using Microsoft.EntityFrameworkCore;
using MoviesApi.Models;

namespace MoviesApi.Services
{
    public class MoviesService : IMoviesService
    {
        private readonly ApplicationDbContext _dbContext;

        public MoviesService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<Movie>> GetAllAsync(byte genreId = 0)
        {
           return await _dbContext.Movies
                .Where(M=>M.GenreId == genreId || genreId==0)
                .OrderByDescending(M => M.Rate)
                .Include(M => M.Genre)
                .ToListAsync();
        }

        public async Task<Movie> GetByIdAsync(int id)
        {
            return await _dbContext.Movies.Include(M => M.Genre).SingleOrDefaultAsync(M => M.Id == id);
        }
        public async Task<Movie> AddAsync(Movie movie)
        {
            await _dbContext.AddAsync(movie);
            _dbContext.SaveChanges();

            return movie;
        }
        public  Movie Update(Movie movie)
        {
            _dbContext.Update(movie);
            _dbContext.SaveChanges();

            return movie;
        }

        public Movie Delete(Movie movie)
        {
            _dbContext.Remove(movie);
            _dbContext.SaveChanges();

            return movie; ;
        }

       
    }
}
