using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphAPI.Data;
using GraphAPI.Models;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class edgetypesController : ControllerBase
    {
        private readonly GraphAPIContext _context;

        public edgetypesController(GraphAPIContext context)
        {
            _context = context;
        }

        // GET: api/edgetypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<edgetype>>> Getedgetype()
        {
            return await _context.edgetype.ToListAsync();
        }

        // GET: api/edgetypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<edgetype>> Getedgetype(long id)
        {
            var edgetype = await _context.edgetype.FindAsync(id);

            if (edgetype == null)
            {
                return NotFound();
            }

            return edgetype;
        }

        // PUT: api/edgetypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putedgetype(long id, edgetype edgetype)
        {
            if (id != edgetype.edgetypeid)
            {
                return BadRequest();
            }

            _context.Entry(edgetype).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!edgetypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/edgetypes/edit/5
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Editedgetype(long id, edgetype partial_edgetype)
        {
            if (!edgetypeExists(id))
            {
                return BadRequest();
            }

            var edgetype = await _context.edgetype.FindAsync(id);

            if (partial_edgetype.name != "")
            {
                edgetype.name = partial_edgetype.name;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!edgetypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/edgetypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<edgetype>> Postedgetype(edgetype edgetype)
        {
            _context.edgetype.Add(edgetype);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getedgetype", new { id = edgetype.edgetypeid }, edgetype);
        }

        // DELETE: api/edgetypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteedgetype(long id)
        {
            var edgetype = await _context.edgetype.FindAsync(id);
            if (edgetype == null)
            {
                return NotFound();
            }

            _context.edgetype.Remove(edgetype);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool edgetypeExists(long id)
        {
            return _context.edgetype.Any(e => e.edgetypeid == id);
        }
    }
}
