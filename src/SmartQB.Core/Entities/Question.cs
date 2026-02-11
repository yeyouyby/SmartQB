using System.Collections.Generic;

namespace SmartQB.Core.Entities;

public class Question
{
    public int Id { get; set; }
    public required string Content { get; set; } // LaTeX Markdown
    public string? LogicDescriptor { get; set; } // AI 提取的解题思路（用于向量化）
    public double Difficulty { get; set; }
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
