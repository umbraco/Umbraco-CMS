using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Editors
{
    internal class EditorValidationResolver : LazyManyObjectsResolverBase<EditorValidationResolver, IEditorValidator>
    {
        public EditorValidationResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> migrations)
            : base(serviceProvider, logger, migrations, ObjectLifetimeScope.Application)
        {
        }

        public virtual IEnumerable<IEditorValidator> EditorValidators
        {
            get { return Values; }
        }

        public IEnumerable<ValidationResult> Validate(object model)
        {
            return EditorValidators
                .Where(x => model.GetType() == x.ModelType)
                .WhereNotNull()
                .SelectMany(x => x.Validate(model));
        }
    }
}