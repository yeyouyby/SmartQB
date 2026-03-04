using System;
using System.Threading.Tasks;
using SmartQB.Core.Entities;

namespace SmartQB.Core.Interfaces;

public interface ITaggingService
{
    event EventHandler QuestionProcessed;

    Task BackfillTagAsync(Tag tag);
    Task TagQuestionAsync(int questionId);
}
