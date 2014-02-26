using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.Install.Models
{
    /// <summary>
    /// Model to give to the front-end to collect the information for each step
    /// </summary>
    [DataContract(Name = "step", Namespace = "")]
    public abstract class InstallSetupStep<T> : InstallSetupStep
    {
        protected InstallSetupStep()
        {
            var att = GetType().GetCustomAttribute<InstallSetupStepAttribute>(false);
            if (att == null)
            {
                throw new InvalidOperationException("Each step must be attributed");
            }
            _attribute = att;
        }

        private readonly InstallSetupStepAttribute _attribute;

        /// <summary>
        /// Defines the step model type on the server side so we can bind it
        /// </summary>
        [IgnoreDataMember]
        public override Type StepType
        {
            get { return typeof(T); }
        }

        public abstract IDictionary<string, object> Execute(T model);

        [IgnoreDataMember]
        public bool HasUIElement
        {
            get { return View.IsNullOrWhiteSpace() == false; }
        }

        public override string View
        {
            get { return _attribute.View; }
        }
    }

    [DataContract(Name = "step", Namespace = "")]
    public abstract class InstallSetupStep
    {
        protected InstallSetupStep()
        {
            var att = GetType().GetCustomAttribute<InstallSetupStepAttribute>(false);
            if (att == null)
            {
                throw new InvalidOperationException("Each step must be attributed");
            }
            Name = att.Name;
            View = att.View;
        }

        [DataMember(Name = "name")]
        public string Name { get; private set; }

        [DataMember(Name = "view")]
        public virtual string View { get; private set; }

        /// <summary>
        /// Defines what order this step needs to execute on the server side since the 
        /// steps might be shown out of order on the front-end
        /// </summary>
        [IgnoreDataMember]
        public int ServerOrder { get; set; }

        /// <summary>
        /// Defines the step model type on the server side so we can bind it
        /// </summary>
        [IgnoreDataMember]
        public abstract Type StepType { get; }

    }
}