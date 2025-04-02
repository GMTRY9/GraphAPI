using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace GraphAPI.Models
{
    public class node
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerIgnore]
        public long nodeid { get; set; }

        public string name { get; set; }

        public long graphid { get; set; }

        public long nodetypeid { get; set; }

        public long classification { get; set; }

        public string copyrightowner { get; set; }

        public string version { get; set; }

        [Column(TypeName = "jsonb")]
        public JsonDocument payload { get; set; }
    }
}
