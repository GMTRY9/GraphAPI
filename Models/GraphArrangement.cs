using System.Numerics;
using Newtonsoft.Json.Linq;

namespace GraphAPI.Models
{
    public class GraphArrangement 
    {
        Dictionary<long, Vector2> NodesPos = new Dictionary<long, Vector2>();
        Dictionary<long, long> NodeEdges = new Dictionary<long, long>();

        public GraphArrangement(Dictionary<long, long> node_edges)
        {
            NodeEdges = node_edges;
        }

        public Vector2 getNodePos(long node_id)
        {
            Vector2 value;
            if (NodesPos.TryGetValue(node_id, out value))
            {
                return value;
            }
            Random rnd = new Random();
            return new Vector2(rnd.Next(-400, 400), rnd.Next(-400, 400));
        }
    }
}

class GraphNode {
    public long Id { get; set; }
    public Vector2 position { get; set; }

    public GraphNode(int id)
    {
        Id = id;
    }
}