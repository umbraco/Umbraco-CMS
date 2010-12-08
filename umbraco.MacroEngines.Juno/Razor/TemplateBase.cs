namespace umbraco.MacroEngines.Razor
{
    using System.Text;
    using System.IO;
    using System.Globalization;
    using System.Dynamic;
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a base implementation of a template.
    /// </summary>
    public abstract class TemplateBase : ITemplate
    {
        #region Fields
        private TextWriter builder = new StringWriter(CultureInfo.InvariantCulture);

        #endregion

        #region Properties
        /// <summary>
        /// Gets the parsed result of the template.
        /// </summary>
        public string Result { get { return builder.ToString(); } }
        #endregion

        #region Methods
        /// <summary>
        /// Clears the template.
        /// </summary>
        public void Clear()
        {
            builder = new StringWriter(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Executes the template.
        /// </summary>
        public virtual void Execute() { }

        /// <summary>
        /// Writes the specified object to the template.
        /// </summary>
        /// <param name="object"></param>
        public void Write(object @object)
        {
            if (@object == null)
                return;

            builder.Write(@object);
        }

        /// <summary>
        /// Writes a literal to the template.
        /// </summary>
        /// <param name="literal"></param>
        public void WriteLiteral(string literal)
        {
            if (literal == null)
                return;

            builder.Write(literal);
        }
        #endregion
    }

    /// <summary>
    /// Provides a base implementation of a template.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    public abstract class TemplateBase<TModel> : TemplateBase, ITemplate<TModel>
    {
        #region Properties
        public TModel Model { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        #endregion

    }

    /// <summary>
    /// Inherits form TemplateBase and provides an anonymous type implementation
    /// </summary>
    public abstract class TemplateBaseDynamic : TemplateBase, ITemplateDynamic
    {

        dynamic model;

        /// <summary>
        /// Gets or sets an anonymous type model
        /// </summary>
        public dynamic Model
        {
            get
            {
                return model;
            }
            set
            {
                model = new RazorDynamicObject() { Model = value };
            }
        }


        /// <summary>
        /// Dynamic object that we'll utilize to return anonymous type parameters
        /// </summary>
        class RazorDynamicObject : DynamicObject
        {
            internal object Model { get; set; }
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                bool isDynamicNode = false;
                object tempResult = null;
                try
                {
                    if (Model.GetType().FullName == "umbraco.MacroEngines.DynamicNode")
                    {
                        isDynamicNode = ((DynamicObject)Model).TryGetMember(binder, out tempResult);
                    }

                }
                catch
                {
                    result = "";
                }
                if (!isDynamicNode)
                {
                    tempResult = Model.GetType().InvokeMember(binder.Name,
                                                          System.Reflection.BindingFlags.GetProperty |
                                                          System.Reflection.BindingFlags.Instance |
                                                          System.Reflection.BindingFlags.Public |
                                                          System.Reflection.BindingFlags.NonPublic,
                                                          null,
                                                          Model,
                                                          null);
                }
                result = tempResult;
                return true;
            }
        }

    }
}