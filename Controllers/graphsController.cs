using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphAPI.Data;
using GraphAPI.Models;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class graphsController : ControllerBase
    {
        private readonly GraphAPIContext _context;

        public graphsController(GraphAPIContext context)
        {
            _context = context;
        }

        // GET: api/graphs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<graph>>> Getgraph()
        {
            return await _context.graph.ToListAsync();
        }

        // GET: api/graphs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<graph>> Getgraph(long id)
        {
            var graph = await _context.graph.FindAsync(id);

            if (graph == null)
            {
                return NotFound();
            }

            return graph;
        }

        // PUT: api/graphs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putgraph(long id, graph graph)
        {
            if (id != graph.graphid)
            {
                return BadRequest();
            }

            _context.Entry(graph).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!graphExists(id))
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

        // PUT: api/graphs/edit/5
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Editgraph(long id, graph partial_graph)
        {
            if (!graphExists(id))
            {
                return BadRequest();
            }

            var graph = await _context.graph.FindAsync(id);

            if (partial_graph.name != "")
            {
                graph.name = partial_graph.name;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!graphExists(id))
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

        // POST: api/graphs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<graph>> Postgraph(graph graph)
        {
            _context.graph.Add(graph);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getgraph", new { id = graph.graphid }, graph);
        }

        // DELETE: api/graphs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletegraph(long id)
        {
            var graph = await _context.graph.FindAsync(id);
            if (graph == null)
            {
                return NotFound();
            }

            _context.graph.Remove(graph);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool graphExists(long id)
        {
            return _context.graph.Any(e => e.graphid == id);
        }
    }
}
