using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace GraphAPI.Models
{
    public class edge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [SwaggerIgnore]
        public long edgeid { get; set; }
        public long edgetypeid { get; set; }
        public long head_node { get; set; }
        public long tail_node { get; set; }
    }
}