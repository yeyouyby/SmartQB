using System.Collections.Generic;

namespace SmartQB.Core.Entities;

public class Tag
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Definition { get; set; } // 标签的语义定义，用于 AI 判定
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
