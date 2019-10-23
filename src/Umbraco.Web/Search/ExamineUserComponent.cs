﻿using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// An abstract class for custom index authors to inherit from
    /// </summary>
    public abstract class ExamineUserComponent : IComponent
    {
        private readonly IMainDom _mainDom;

        public ExamineUserComponent(IMainDom mainDom)
        {
            _mainDom = mainDom;
        }

        /// <summary>
        /// Initialize the component, eagerly exits if ExamineComponent.ExamineEnabled == false
        /// </summary>
        public void Initialize()
        {
            if (!_mainDom.IsMainDom) return;

            InitializeComponent();
        }

        /// <summary>
        /// Abstract method which executes to initialize this component if ExamineComponent.ExamineEnabled == true
        /// </summary>
        protected abstract void InitializeComponent();

        public virtual void Terminate()
        {
        }
    }
}
