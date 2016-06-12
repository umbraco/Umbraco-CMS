using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IContentUrlRuleService
    {
        void Save(ContentUrlRule rule);

        void Delete(int ruleId);

        void DeleteContentRules(int contentId);

        ContentUrlRule GetMostRecentRule(string url);

        IEnumerable<ContentUrlRule> GetRules(int contentId);

        IEnumerable<ContentUrlRule> GetAllRules(long pageIndex, int pageSize, out long total);

        IEnumerable<ContentUrlRule> GetAllRules(int rootContentId, long pageIndex, int pageSize, out long total);
    }
}
