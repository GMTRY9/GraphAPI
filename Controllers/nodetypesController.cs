using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphAPI.Data;
using GraphAPI.Models;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class nodetypesController : ControllerBase
    {
        private readonly GraphAPIContext _context;

        public nodetypesController(GraphAPIContext context)
        {
            _context = context;
        }

        // GET: api/nodetypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<nodetype>>> Getnodetype()
        {
            return await _context.nodetype.ToListAsync();
        }

        // GET: api/nodetypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<nodetype>> Getnodetype(long id)
        {
            var nodetype = await _context.nodetype.FindAsync(id);

            if (nodetype == null)
            {
                return NotFound();
            }

            return nodetype;
        }

        // PUT: api/nodetypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putnodetype(long id, nodetype nodetype)
        {
            if (id != nodetype.nodetypeid)
            {
                return BadRequest();
            }

            _context.Entry(nodetype).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!nodetypeExists(id))
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

        // POST: api/nodetypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<nodetype>> Postnodetype(nodetype nodetype)
        {
            _context.nodetype.Add(nodetype);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getnodetype", new { id = nodetype.nodetypeid }, nodetype);
        }

        // DELETE: api/nodetypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletenodetype(long id)
        {
            var nodetype = await _context.nodetype.FindAsync(id);
            if (nodetype == null)
            {
                return NotFound();
            }

            _context.nodetype.Remove(nodetype);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool nodetypeExists(long id)
        {
            return _context.nodetype.Any(e => e.nodetypeid == id);
        }
    }
}
