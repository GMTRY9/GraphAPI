using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace GraphAPI.Models
{
    public class nodetype
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerIgnore]
        public long nodetypeid { get; set; }

        public string name { get; set; }

        [Column(TypeName = "jsonb")]
        public JsonDocument fields { get; set; }

        [Column(TypeName = "jsonb")]
        public JsonDocument settings { get; set; }

    }
}
