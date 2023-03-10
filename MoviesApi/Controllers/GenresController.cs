using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Dtos;
using MoviesApi.Models;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _genreService.GetAllAsync();

            return Ok(genres);
        }

        [Authorize("Admin")]
        [HttpPost]
        public async Task<IActionResult> AddAsync(GenreDto genreDto)
        {
            var genra=new Genre() { Name=genreDto.Name};
        
            await _genreService.AddAsync(genra);

            return Ok(genra);
        }
        [Authorize("Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(byte id,[FromBody]GenreDto genreDto)
        {
            var genre = await _genreService.GetByIdAsync(id);

            if (genre == null) return NotFound($"No Genre was found with ID : {id}");

            genre.Name = genreDto.Name;

            _genreService.Update(genre);

            return Ok(genre);
        
        }

        [Authorize("Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(byte id) 
        {
            var genre = await _genreService.GetByIdAsync(id);

            if (genre == null) return NotFound($"No Genre was found with ID : {id}");

            _genreService.Delete(genre);

            return Ok(genre);
        }
    }
}
