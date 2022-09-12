using Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IExamineIndexViewModelFactory
{
    ExamineIndexViewModel Create(IIndex index);
}
