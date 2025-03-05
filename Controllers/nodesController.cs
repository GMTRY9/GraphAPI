using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphAPI.Data;
using GraphAPI.Models;
using System.Security.AccessControl;
using static NpgsqlTypes.NpgsqlTsQuery;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class nodesController : ControllerBase
    {
        private readonly GraphAPIContext _context;

        public nodesController(GraphAPIContext context)
        {
            _context = context;
        }

        // GET: api/nodes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<node>>> Getnode()
        {
            List<node> nodes = await _context.node.ToListAsync();
            return nodes;

        }

        // GET: api/nodes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<node>> Getnode(long id)
        {
            var node = await _context.node.FindAsync(id);

            if (node == null)
            {
                return NotFound();
            }

            return node;
        }

        // GET: api/nodes/children
        [HttpGet("children/{id}")]
        public async Task<ActionResult<List<long>>> GetChildrenNodes(long id)
        {
            if (!nodeExists(id))
            {
                return NotFound();
            }

            var children = await _context.edge
                .Where(t => t.head_node == id)
                .Select(h => h.tail_node)
                .ToListAsync();

            return children;
        }

        // GET: api/nodes/parents
        [HttpGet("parents/{id}")]
        public async Task<ActionResult<List<long>>> GetParentNodes(long id)
        {
            if (!nodeExists(id))
            {
                return NotFound();
            }

            var children = await _context.edge
                .Where(t => t.tail_node == id)
                .Select(h => h.head_node)
                .ToListAsync();

            return children;
        }

        // GET: api/nodes/deletionnodes/5
        [HttpGet("deletionnodes/{id}")]
        public async Task<ActionResult<List<long>>> GetDeletionNodes(long id)
        {
            List<long> parents_to_search = new List<long>();
            parents_to_search.Add(id);

            List<long> children_to_search = new List<long>();
            List<long> deletion_node_ids = new List<long>();

            while (parents_to_search.Count > 0)
            {
                long current_node = parents_to_search[0];
                children_to_search = (await GetChildrenNodes(current_node)).Value;
                foreach (long child_id in children_to_search)
                {
                    List<long> parents = (await GetParentNodes(child_id)).Value;
                    if (parents.Count < 2)
                    {
                        deletion_node_ids.Add(child_id);
                        parents_to_search.Add(child_id);
                    } 
                }
                
                parents_to_search.RemoveAt(0);
            }

            return deletion_node_ids;

        }

        // PUT: api/nodes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Putnode(long id, node node)
        {
            if (id != node.nodeid)
            {
                return BadRequest();
            }

            _context.Entry(node).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!nodeExists(id))
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

        // PUT: api/nodes/edit/5
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Editnode(long id, node partial_node)
        {
            if (!nodeExists(id))
            {
                return BadRequest();
            }

            var node = await _context.node.FindAsync(id);

            if (partial_node.name != "")
            {
                node.name = partial_node.name;
            }
            if (partial_node.graphid != 0)
            {
                if (!_context.graph.Any(e => e.graphid == id)) {
                    return BadRequest();
                }
                node.graphid = partial_node.graphid;
            }
            if (partial_node.nodetype != 0)
            {
                if (!_context.nodetype.Any(e => e.nodetypeid == id))
                {
                    return BadRequest();
                }
                node.nodetype = partial_node.nodetype;
            }
            if (partial_node.classification != 0)
            {
                node.classification = partial_node.classification;
            }
            if (partial_node.copywriteowner != "")
            {
                node.copywriteowner = partial_node.copywriteowner;
            }
            if (partial_node.version != "")
            {
                node.version = partial_node.version;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!nodeExists(id))
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

        // POST: api/nodes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<node>> Postnode(node node)
        {
            _context.node.Add(node);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Getnode", new { id = node.nodeid }, node);
        }

        //POST: api/nodes/children
        [HttpPost("children/{headnode_id}/{edgetype_id}")]
        public async Task<ActionResult<node>> Postchildnode(long headnode_id, long edgetype_id, node node)
        {
            if (!nodeExists(headnode_id))
            {
                return NotFound();
            }
            else if (!_context.edgetype.Any(e => e.edgetypeid == edgetype_id))
            {
                return NotFound();
            }

            await Postnode(node);

            _context.edge.Add(
                new edge { edgetypeid = edgetype_id, head_node = headnode_id, tail_node = node.nodeid }
            );

            await _context.SaveChangesAsync();

            return CreatedAtAction("Getnode", new { id = node.nodeid }, node);
        }

        // DELETE: api/nodes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletenode(long id)
        {
            List<long> deleted_node_ids = new List<long>();
            List<long> deleted_edge_ids = new List<long>();

            var root_node = await _context.node.FindAsync(id);

            if (root_node == null)
            {
                return NotFound();
            }

            List<long> wholly_dependent_nodes = (await GetDeletionNodes(id)).Value;
            deleted_node_ids = wholly_dependent_nodes;

            foreach (long node_id in wholly_dependent_nodes)
            {
                var node = await _context.node.FindAsync(node_id);
                _context.node.Remove(node);
                var edges_removed = await EdgeRemoval(node_id);
                deleted_edge_ids.Concat(edges_removed);
            };

            _context.node.Remove(root_node);

            deleted_node_ids.Add(id);

            var removed_root_edges = await EdgeRemoval(id);
            deleted_edge_ids.Concat(removed_root_edges);

            await _context.SaveChangesAsync();

            Dictionary<string, List<long>> payload = new Dictionary<string, List<long>>();
            payload["deleted_node_ids"] = deleted_node_ids;
            payload["deleted_edge_ids"] = deleted_edge_ids;

            return Ok(payload); // EDGE ID NO WORKEY BUT THE LOGIC WORK
        }

        private async Task<List<long>> EdgeRemoval(long node_id)
        {
            var edges = await _context.edge
                .Where(t => t.tail_node == node_id | t.head_node == node_id)
                .Select(k => k.edgeid)
                .ToListAsync();

            foreach (long edge_id in edges)
            {
                edge edge_to_remove = await _context.edge.FindAsync(edge_id);
                _context.edge.Remove(edge_to_remove);
            }

            Console.WriteLine(edges);
            return edges;
        }

        private bool nodeExists(long id)
        {
            return _context.node.Any(e => e.nodeid == id);
        }
    }
}
