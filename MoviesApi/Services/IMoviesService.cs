namespace MoviesApi.Services
{
    public interface IMoviesService
    {
        Task<IEnumerable<Movie>> GetAllAsync(byte genreId=0);

        Task<Movie> GetByIdAsync(int id);
        Task<Movie> AddAsync(Movie movie);

        Movie Update(Movie movie);
        Movie Delete (Movie movie);



    }
}
