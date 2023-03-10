using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MoviesController : ControllerBase
    {
       
        private new List<string> _allowedExtentions= new List<string> {".jpg",".png" };
        private long maxAllowedPosterSize = 1048576;
        private readonly IMoviesService _moviesService;
        private readonly IGenreService genreService;
        private readonly IMapper mapper;

        public MoviesController(IMoviesService moviesService
            ,IGenreService genreService
            ,IMapper mapper
            )
        {
            _moviesService = moviesService;
            this.genreService = genreService;
            this.mapper = mapper;
        }

       
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies=await _moviesService.GetAllAsync();
             var dto =mapper.Map<IEnumerable<Movie>,IEnumerable<MovieDetailsDto>>(movies);

            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _moviesService.GetByIdAsync(id);
            if (movie == null) return NotFound();


            var dto = mapper.Map<Movie,MovieDetailsDto>(movie);

            return Ok(dto);
        }


        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _moviesService.GetAllAsync(genreId);

            var dto = mapper.Map<IEnumerable<Movie>, IEnumerable<MovieDetailsDto>>(movies);

            return Ok(dto);
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MoviesDto moviesDto)
        {
            if (moviesDto.Poster == null)
                return BadRequest("Poster is required");

            if (!_allowedExtentions.Contains(Path.GetExtension(moviesDto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed");
            if (moviesDto.Poster.Length > maxAllowedPosterSize)
                return BadRequest("Max Allowed Size for poster is 1MB");

            var Genre = await genreService.GetByIdAsync(moviesDto.GenreId);
            if (Genre==null)
                return BadRequest("invalid genre Id");


            using var dataStream = new MemoryStream();
            await moviesDto.Poster.CopyToAsync(dataStream);

            var movie = mapper.Map<Movie>(moviesDto);
            movie.Poster=dataStream.ToArray();

            await _moviesService.AddAsync(movie);

            return Ok(movie);
        }

        [Authorize("Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id,[FromForm] MoviesDto moviesDto)
        {
            // Get Movie From Db
            var movie = await _moviesService.GetByIdAsync(id);
            if (movie == null) return NotFound($"No Movie was found with ID= {id}");

            var Genre = await genreService.GetByIdAsync(moviesDto.GenreId);
            if (Genre == null)
                return BadRequest("invalid genre Id");

            if (moviesDto.Poster!=null)
            {
                if (!_allowedExtentions.Contains(Path.GetExtension(moviesDto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed");
                if (moviesDto.Poster.Length > maxAllowedPosterSize)
                    return BadRequest("Max Allowed Size for poster is 1MB");

               
                using var dataStream = new MemoryStream();
                await moviesDto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }

            movie.Title = moviesDto.Title;
            movie.GenreId = moviesDto.GenreId;
            movie.Year = moviesDto.Year;
            movie.Storeline = moviesDto.Storeline;
            movie.Rate = moviesDto.Rate;

            _moviesService.Update(movie);

            return Ok(movie);

        }

        [Authorize("Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesService.GetByIdAsync(id);

            if (movie == null) return NotFound($"No Movie was found with ID= {id}");

            _moviesService.Delete(movie);


            return Ok(movie);
        }
    }
}
