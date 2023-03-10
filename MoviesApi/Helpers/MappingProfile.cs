using AutoMapper;

namespace MoviesApi.Helpers
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie,MovieDetailsDto>().ReverseMap();

            CreateMap<MoviesDto, Movie>()
                .ForMember(src => src.Poster, opt => opt.Ignore());
        }
    }
}
