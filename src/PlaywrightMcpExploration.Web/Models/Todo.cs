using System.ComponentModel.DataAnnotations;

namespace PlaywrightMcpExploration.Web.Models;

public class Todo
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }
}
