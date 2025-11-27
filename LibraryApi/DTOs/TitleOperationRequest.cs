using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

/// <summary>
/// Request DTO for title manipulation operations
/// </summary>
public class TitleOperationRequest
{
    /// <summary>
    /// The book title to operate on
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for generating title replicas
/// </summary>
public class TitleReplicaRequest : TitleOperationRequest
{
    /// <summary>
    /// Number of times to repeat the title
    /// </summary>
    [Range(0, 100, ErrorMessage = "Count must be between 0 and 100")]
    public int Count { get; set; }
}
