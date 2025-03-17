using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphAPI.Data;
using GraphAPI.Models;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class edgesController : ControllerBase
    {
        private readonly GraphAPIContext _context;

        public edgesController(GraphAPIContext context)
        {
            _context = context;
        }

        // GET: api/edges
        [HttpGet]
        public async Task<ActionResult<IEnumerable<edge>>> GetAlledges()
        {
            return await _context.edge.ToListAsync();
        }

        // GET: api/edges/5
        [HttpGet("{id}")]
        public async Task<ActionResult<edge>> Getedge(long id)
        {
            var edge = await _context.edge.FindAsync(id);

            if (edge == null)
            {
                return NotFound();
            }

            return edge;
        }

        // PUT: api/edges/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putedge(long id, edge edge)
        {
            if (id != edge.edgeid)
            {
                return BadRequest();
            }

            _context.Entry(edge).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!edgeExists(id))
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

        // PUT: api/edges/edit/5
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Editedge(long id, edge partial_edge)
        {
            if (!edgeExists(id))
            {
                return BadRequest();
            }

            var edge = await _context.edge.FindAsync(id);

            if (partial_edge.edgetypeid != 0)
            {
                if (!_context.edgetype.Any(e => e.edgetypeid == id)) {
                    return BadRequest();
                }

                edge.edgetypeid = partial_edge.edgetypeid;
            }

            if (partial_edge.headnodeid != 0)
            {
                edge.headnodeid = partial_edge.headnodeid;
            }

            if (partial_edge.tailnodeid != 0)
            {
                edge.tailnodeid = partial_edge.tailnodeid;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!edgeExists(id))
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

        // POST: api/edges
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<edge>> Postedge(edge edge)
        {
            _context.edge.Add(edge);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getedge", new { id = edge.edgeid }, edge);
        }

        // DELETE: api/edges/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deleteedge(long id)
        {
            var edge = await _context.edge.FindAsync(id);
            if (edge == null)
            {
                return NotFound();
            }

            _context.edge.Remove(edge);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool edgeExists(long id)
        {
            return _context.edge.Any(e => e.edgeid == id);
        }
    }
}
