using MoviesApi.Services;

namespace MoviesApi.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            //Inject Application Services
            services.AddScoped(typeof(IGenreService), typeof(GenreService));
            services.AddScoped(typeof(IMoviesService), typeof(MoviesService));
            services.AddScoped(typeof(IAuthService), typeof(AuthService));
            services.AddAutoMapper(typeof(Program));


            return services;
        }
    }
}
