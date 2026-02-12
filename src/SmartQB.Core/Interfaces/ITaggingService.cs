using System.Threading.Tasks;
using SmartQB.Core.Entities;

namespace SmartQB.Core.Interfaces;

public interface ITaggingService
{
    Task BackfillTagAsync(Tag tag);
    Task TagQuestionAsync(int questionId);
}
