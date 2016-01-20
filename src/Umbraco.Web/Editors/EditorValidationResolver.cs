using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Validate(object model, EditorValidationErrors editorValidations)
        {
            foreach (var validator in EditorValidators.Where(x => x.GetType() == x.ModelType))
            {
                validator.Validate(model, editorValidations);
            }
        }
    }
}