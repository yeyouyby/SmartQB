using System.Collections.Generic;
using System.Threading.Tasks;
using SmartQB.Core.Entities;

namespace SmartQB.Core.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetQuestionsAsync(int? tagId = null);
    Task<List<Tag>> GetAllTagsAsync();
}