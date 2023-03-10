namespace MoviesApi.Services
{
    public interface IGenreService
    {
        Task<IEnumerable<Genre>> GetAllAsync();
        Task<Genre> GetByIdAsync(byte id);

        Task<Genre> AddAsync(Genre genre);
        Genre Update(Genre genre);
        Genre Delete(Genre genre);
    }
}

