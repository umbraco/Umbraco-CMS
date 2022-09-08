using Examine;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IExamineIndexModelFactory
{
    ExamineIndexModel Create(IIndex index);
}
