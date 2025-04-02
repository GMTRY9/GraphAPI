using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GraphAPI.Data;
using GraphAPI.Models;
using Microsoft.EntityFrameworkCore.Query;

namespace GraphAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class nodesController : ControllerBase
    {
        private readonly GraphAPIContext _context;
        private GraphArrangement GraphArrangement = new GraphArrangement(new Dictionary<long, long>());

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

        // GET: api/nodes/displaychildren/5/1
        [HttpGet("displaychildren/{id}/{search_depth}")]
        public async Task<ActionResult<Dictionary<string, ChildrenNodesResponse>>> GetChildrenNodes(long id, int search_depth)
        {
            Dictionary<string, ChildrenNodesResponse> payload_response = new Dictionary<string, ChildrenNodesResponse>();
            List<long> node_ids = [id];

            int current_search_depth = 0;
            int search_pointer = 0;
            int layer_end_pointer;

            if (!nodeExists(id))
            {
                return NotFound();
            }

            while (current_search_depth <= search_depth)
            {
                layer_end_pointer = node_ids.Count;
                while (search_pointer < layer_end_pointer) {
                    long current_node_id = node_ids[search_pointer];

                    ChildrenNodesResponse node_section = new ChildrenNodesResponse();

                    node_section.node_name = (await _context.node.FindAsync(current_node_id)).name;

                    List<ChildAndEdgeType> child_and_edges = await GetChildrenAndEdgetypes(current_node_id);

                    foreach (var child_and_edge in child_and_edges)
                    {   
                        long child_id = child_and_edge.child_id;
                        long edge_id = child_and_edge.edge_type_id;

                        node_ids.Add(child_id);
                        node_section.children_nodes.Add(child_id.ToString(), edge_id.ToString());
                    }

                    node_section.x = GraphArrangement.getNodePos(current_node_id).X;
                    node_section.y = GraphArrangement.getNodePos(current_node_id).Y;

                    payload_response.Add(current_node_id.ToString(), node_section);
                    search_pointer++;
                }
                current_search_depth++;
            }

            return Ok(payload_response);
        }
        private async Task<List<long>> GetChildren(long id)
        {
            List<long> children = await _context.edge
                .Where(t => t.headnodeid == id)
                .Select(h => h.tailnodeid)
                .ToListAsync();
            return children;
        }

        private async Task<List<ChildAndEdgeType>> GetChildrenAndEdgetypes(long id)
        {
               var children = await _context.edge
                .Where(t => t.headnodeid == id)
                .Select(h => new ChildAndEdgeType
                {
                    child_id = h.tailnodeid,
                    edge_type_id = h.edgetypeid
                })
                // For loop?
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
                .Where(t => t.tailnodeid == id)
                .Select(h => h.headnodeid)
                .ToListAsync();
            return children;
        }

        //[HttpGet("generic/")]
        //public async Task<ActionResult<List<object>>> GetGenericObject()
        //{
        //    List<>//something in list, but it could be any type so god knows what to put here
        //    var generic_object = await // some form of instance e.g _context.something
        //        .Where(s => new GenericObject
        //        {
        //            // whatever conditions you want to apply in either the form of a dict or a list???
        //        }
        //        )
        //        .
        //        Select(s => new GenericObject
        //        {
        //            // whatever fields you want to return either in the form of a dict or a list???
        //        })
        //        .ToListAsync();
        //    return nodes;
        //}


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
                children_to_search = (await GetChildren(current_node));
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
            if (partial_node.nodetypeid != 0)
            {
                if (!_context.nodetype.Any(e => e.nodetypeid == id))
                {
                    return BadRequest();
                }
                node.nodetypeid = partial_node.nodetypeid;
            }
            if (partial_node.classification != 0)
            {
                node.classification = partial_node.classification;
            }
            if (partial_node.copyrightowner != "")
            {
                node.copyrightowner = partial_node.copyrightowner;
            }
            if (partial_node.version != "")
            {
                node.version = partial_node.version;
            }
            if (partial_node.payload.ToString().Length > 2)
            {
                node.payload = partial_node.payload;
            }
            System.Diagnostics.Debug.WriteLine(node.ToString());
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
                new edge { edgetypeid = edgetype_id, headnodeid = headnode_id, tailnodeid = node.nodeid }
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
                //deleted_edge_ids.Concat(edges_removed);
                deleted_edge_ids.AddRange(edges_removed);
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
                .Where(t => t.tailnodeid == node_id | t.headnodeid == node_id)
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
