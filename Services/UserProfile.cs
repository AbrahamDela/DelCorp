using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DelCorp.Services;

[Table("profiles")]
public class UserProfile : BaseModel
{
    [PrimaryKey("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [Column("email")]
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [Column("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [Column("profile_image_url")]
    [JsonPropertyName("profile_image_url")]
    public string? ProfileImageUrl { get; set; }

    [Column("created_at")]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
