using Fantasy.Backend.Data;
using Fantasy.Shared.Entites;
using Fantasy.Shared.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Fantasy.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly DataContext _context;
        private ActionResponse<Country> actionResponse;

        public CountriesController(DataContext context)
        {
            _context = context;
            actionResponse = new ActionResponse<Country>();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(await _context.Countries.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);
            if (country == null)
            {
                actionResponse.Status = StatusCodes.Status404NotFound;
                actionResponse.WasSuccess = false;
                actionResponse.Message = StatusCodes.Status404NotFound.ToString();
                return NotFound(actionResponse);
            }
            actionResponse.Status = StatusCodes.Status200OK;
            actionResponse.WasSuccess = true;
            actionResponse.Message = StatusCodes.Status200OK.ToString();
            actionResponse.Result = country;
            return Ok(actionResponse.Result);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] Country country)
        {
            _context.Add(country);
            await _context.SaveChangesAsync();
            return Ok(country);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Remove(country);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(Country country)
        {
            _context.Update(country);
            await _context.SaveChangesAsync();
            return Ok(country);
        }
    }
}