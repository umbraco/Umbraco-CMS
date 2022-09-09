using Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IExamineIndexModelFactory
{
    ExamineIndexViewModel Create(IIndex index);
}
