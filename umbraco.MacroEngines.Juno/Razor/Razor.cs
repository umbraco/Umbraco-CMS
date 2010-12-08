namespace umbraco.MacroEngines.Razor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Process razor templates.
    /// </summary>
    public static class Razor
    {
        #region Fields
        private static RazorCompiler Compiler;
        private static readonly IDictionary<string, ITemplate> Templates;
        #endregion

        #region Constructor
        /// <summary>
        /// Statically initialises the <see cref="Razor"/> type.
        /// </summary>
        static Razor()
        {
            Compiler = new RazorCompiler(new CSharpRazorProvider());
            Templates = new Dictionary<string, ITemplate>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets an <see cref="ITemplate"/> for the specified template.
        /// </summary>
        /// <param name="template">The template to parse.</param>
        /// <param name="modelType">The model to use in the template.</param>
        /// <param name="name">[Optional] The name of the template.</param>
        /// <returns></returns>
        private static ITemplate GetTemplate(string template, Type modelType, string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (Templates.ContainsKey(name))
                    return Templates[name];
            }

            var instance = Compiler.CreateTemplate(template, modelType);

            if (!string.IsNullOrEmpty(name))
            {
                if (!Templates.ContainsKey(name))
                    Templates.Add(name, instance);
            }

            return instance;
        }

        /// <summary>
        /// Parses the specified template using the specified model.
        /// </summary>
        /// <typeparam name="T">The model type.</typeparam>
        /// <param name="template">The template to parse.</param>
        /// <param name="model">The model to use in the template.</param>
        /// <param name="name">[Optional] A name for the template used for caching.</param>
        /// <returns>The parsed template.</returns>
        public static string Parse<T>(string template, T model, string name = null)
        {
            var instance = GetTemplate(template, typeof(T), name);
            if (instance is ITemplate<T>)
                ((ITemplate<T>)instance).Model = model;
            else if (instance is ITemplateDynamic)
                ((ITemplateDynamic)instance).Model = model;

            instance.Execute();
            return instance.Result;
        }

        /// <summary>
        /// Sets the razor provider used for compiling templates.
        /// </summary>
        /// <param name="provider">The razor provider.</param>
        public static void SetRazorProvider(IRazorProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            Compiler = new RazorCompiler(provider);
        }
        #endregion
    }
}