namespace GraphAPI.Models
{
    public class ChildrenNodesResponse
    {
        public string node_name { get; set; }
        public Dictionary<string, string> children_nodes { get; set; }

        public ChildrenNodesResponse()
        {
            children_nodes = new Dictionary<string, string>();
        }
    }
    public class ChildAndEdgeType
    {
        public long child_id { get; set; }
        public long edge_type_id { get; set; }
    }
}
